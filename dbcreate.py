import pyodbc
import time
import logging
import subprocess
import sys

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    filename='db_creation.log'
)
console = logging.StreamHandler()
console.setLevel(logging.INFO)
logging.getLogger('').addHandler(console)
logger = logging.getLogger(__name__)

SERVER = "localhost" 
PORT = "1433"
USERNAME = "sa"
PASSWORD = "YourPassword123" 
DATABASE_NAME = "AtikBildirimFormu"

# Connection string for master database (to create a new database)
MASTER_CONN_STR = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={SERVER},{PORT};DATABASE=master;UID={USERNAME};PWD={PASSWORD};TrustServerCertificate=yes;Connection Timeout=30"

# Connection string for application database (once created)
APP_CONN_STR = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={SERVER},{PORT};DATABASE={DATABASE_NAME};UID={USERNAME};PWD={PASSWORD};TrustServerCertificate=yes;Connection Timeout=30"

def check_docker_container():
    """Check if the SQL Server docker container is running"""
    try:
        # Check if we have docker command
        subprocess.run(["docker", "--version"], check=True, stdout=subprocess.PIPE)
        
        # Check container status, use the specific container ID
        result = subprocess.run(
            ["docker", "ps", "--filter", "id=ddd11bc19d271b2e356a906bbc833462a00c14ceb8866ca9783e36ae1c9ddfcb", "--format", "{{.Status}}"],
            check=True, 
            stdout=subprocess.PIPE, 
            text=True
        )
        
        status = result.stdout.strip()
        
        if not status:
            logger.error("SQL Server container not found or not running.")
            print("\nExpected container with ID ddd11bc19d271b2e356a906bbc833462a00c14ceb8866ca9783e36ae1c9ddfcb")
            print("Check if the container is still running:")
            print('docker ps | findstr sqlserver')
            return False
            
        if status.startswith("Up"):
            logger.info("SQL Server container is running")
            return True
        else:
            logger.warning(f"SQL Server container status: {status}")
            
            # Try to start the container
            logger.info("Attempting to start the container...")
            subprocess.run(["docker", "start", "sqlserver"], check=True)
            
            # Wait a bit and check again
            time.sleep(5)
            result = subprocess.run(
                ["docker", "ps", "--filter", "name=sqlserver", "--format", "{{.Status}}"],
                check=True, 
                stdout=subprocess.PIPE, 
                text=True
            )
            
            status = result.stdout.strip()
            if status.startswith("Up"):
                logger.info("Successfully started SQL Server container")
                return True
            else:
                logger.error("Failed to start SQL Server container. Check docker logs.")
                print("\nCheck container logs with:")
                print("docker logs sqlserver")
                return False
                
    except subprocess.CalledProcessError as e:
        if "command 'docker'" in str(e):
            logger.warning("Docker command not found. Assuming non-Docker environment.")
            return True  # Proceed anyway, might be a direct SQL Server
        else:
            logger.error(f"Error checking Docker container: {str(e)}")
            return False
    except Exception as e:
        logger.error(f"Unexpected error checking container: {str(e)}")
        return False

def wait_for_sql_server():
    """Wait for SQL Server to become available"""
    max_attempts = 15  # Increased attempts for Docker startup
    attempt = 0
    
    while attempt < max_attempts:
        try:
            conn = pyodbc.connect(MASTER_CONN_STR, timeout=5)
            conn.close()
            logger.info("SQL Server is available!")
            return True
        except pyodbc.OperationalError as e:
            attempt += 1
            error_msg = str(e)
            
            # More specific error handling
            if "server name not found" in error_msg.lower():
                logger.warning(f"SQL Server not found. Ensure it's running at {SERVER}:{PORT}")
            elif "connection refused" in error_msg.lower():
                logger.warning(f"Connection refused. SQL Server may still be starting.")
            elif "login timeout" in error_msg.lower():
                logger.warning(f"Login timeout. SQL Server may be busy starting up.")
            elif "login failed" in error_msg.lower():
                logger.error(f"Login failed. Check your username and password.")
                return False  # Don't retry for credential errors
            else:
                logger.warning(f"SQL Server not available yet: {error_msg}")
            
            logger.warning(f"Attempt {attempt}/{max_attempts}. Waiting...")
            time.sleep(10 if attempt < 5 else 20)  # Wait longer after several attempts
        except Exception as e:
            attempt += 1
            logger.warning(f"Unknown error connecting to SQL Server: {str(e)}")
            logger.warning(f"Attempt {attempt}/{max_attempts}. Waiting...")
            time.sleep(10)
    
    logger.error("Could not connect to SQL Server after multiple attempts")
    return False

def create_database():
    """Create the database if it doesn't exist"""
    try:
        # Set autocommit=True to avoid transaction issues with CREATE DATABASE
        conn = pyodbc.connect(MASTER_CONN_STR, autocommit=True)
        cursor = conn.cursor()
        
        # Check if database exists
        cursor.execute(f"SELECT DB_ID('{DATABASE_NAME}')")
        result = cursor.fetchone()
        
        if result and result[0]:
            logger.info(f"Database '{DATABASE_NAME}' already exists")
        else:
            # Create database - no transaction needed with autocommit=True
            logger.info(f"Creating database '{DATABASE_NAME}'...")
            cursor.execute(f"CREATE DATABASE {DATABASE_NAME}")
            # No need for conn.commit() since autocommit is True
            logger.info(f"Database '{DATABASE_NAME}' created successfully")
        
        cursor.close()
        conn.close()
        return True
    except Exception as e:
        logger.error(f"Error creating database: {str(e)}")
        return False

def create_tables():
    """Create necessary tables for the application"""
    try:
        conn = pyodbc.connect(APP_CONN_STR)
        cursor = conn.cursor()
        
        # Create Atik Cinsi enum table
        cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AtikCinsi]') AND type in (N'U'))
        BEGIN
            CREATE TABLE AtikCinsi (
                Id INT PRIMARY KEY,
                Name NVARCHAR(50) NOT NULL
            )
            
            INSERT INTO AtikCinsi (Id, Name) VALUES 
                (0, 'Kati'),
                (1, 'Toz'),
                (2, 'Sivi'),
                (3, 'AkiskanMacun')
        END
        """)
        
        # Create AtikBildirimFormlari table
        cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AtikBildirimFormlari]') AND type in (N'U'))
        BEGIN
            CREATE TABLE AtikBildirimFormlari (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                KayitNo NVARCHAR(20) NOT NULL,
                GonderenKisim NVARCHAR(200) NOT NULL,
                GonderenKisi NVARCHAR(200) NOT NULL,
                Tarih DATETIME NOT NULL DEFAULT GETDATE(),
                AtikCinsi INT NOT NULL FOREIGN KEY REFERENCES AtikCinsi(Id),
                AtikIsmi NVARCHAR(500) NOT NULL,
                SapmaDkHtf NVARCHAR(500) NULL,
                MiktarKg DECIMAL(18, 4) NOT NULL,
                MiktarAdet INT NULL,
                AmbalajVarilAdedi INT NULL,
                AmbalajFiciAdedi INT NULL,
                AmbalajIbcAdedi INT NULL,
                AmbalajTorbaAdedi INT NULL,
                AmbalajKutuAdedi INT NULL,
                AmbalajPaletAdedi INT NULL,
                Durum NVARCHAR(50) NOT NULL DEFAULT 'Hazırlanıyor',
                KisimAtikSorumlusuId NVARCHAR(100) NULL,
                UsmPersoneli NVARCHAR(200) NULL,
                UsmPersonelId NVARCHAR(100) NULL,
                OnayTarihi DATETIME NULL
            )
            
            CREATE NONCLUSTERED INDEX IX_AtikBildirimFormlari_KayitNo 
            ON AtikBildirimFormlari(KayitNo)
            
            CREATE NONCLUSTERED INDEX IX_AtikBildirimFormlari_Tarih
            ON AtikBildirimFormlari(Tarih)
            
            CREATE NONCLUSTERED INDEX IX_AtikBildirimFormlari_Durum 
            ON AtikBildirimFormlari(Durum)
        END
        """)
        
        conn.commit()
        logger.info("Tables created/verified successfully")
        
        cursor.close()
        conn.close()
        return True
    except Exception as e:
        logger.error(f"Error creating tables: {str(e)}")
        return False

def insert_sample_data(count=5):
    """Insert sample data for testing"""
    try:
        conn = pyodbc.connect(APP_CONN_STR)
        cursor = conn.cursor()
        
        # Check if we already have data
        cursor.execute("SELECT COUNT(*) FROM AtikBildirimFormlari")
        existing_count = cursor.fetchone()[0]
        
        if existing_count > 0:
            logger.info(f"Sample data already exists ({existing_count} records). Skipping insertion.")
            cursor.close()
            conn.close()
            return True
            
        # Insert sample forms
        current_year = time.strftime("%Y")
        
        for i in range(1, count+1):
            kayit_no = f"ABF-{current_year}-{i:04d}"
            gonderen_kisim = f"Test Kısım {i}"
            gonderen_kisi = f"Test Kullanıcı {i}"
            atik_cinsi = i % 4  # Rotate through the enum values
            atik_ismi = f"Test Atık {i}"
            sapma_dk_htf = "Test Sapma" if i % 2 == 0 else None
            miktar_kg = 10.5 * i
            miktar_adet = i * 5 if i % 2 == 0 else None
            
            cursor.execute("""
            INSERT INTO AtikBildirimFormlari 
            (KayitNo, GonderenKisim, GonderenKisi, Tarih, AtikCinsi, 
             AtikIsmi, SapmaDkHtf, MiktarKg, MiktarAdet,
             AmbalajVarilAdedi, AmbalajFiciAdedi, AmbalajIbcAdedi,
             AmbalajTorbaAdedi, AmbalajKutuAdedi, AmbalajPaletAdedi,
             Durum, KisimAtikSorumlusuId)
            VALUES
            (?, ?, ?, GETDATE(), ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 'Hazırlanıyor', 'test-user-id')
            """, 
            (kayit_no, gonderen_kisim, gonderen_kisi, atik_cinsi, 
             atik_ismi, sapma_dk_htf, miktar_kg, miktar_adet,
             i if i % 2 == 0 else None,  # Varil
             i+1 if i % 3 == 0 else None,  # Fıçı
             i+2 if i % 4 == 0 else None,  # IBC
             i+3 if i % 2 == 1 else None,  # Torba
             i+4 if i % 3 == 1 else None,  # Kutu
             i+5 if i % 4 == 1 else None  # Palet
            ))
        
        conn.commit()
        logger.info(f"Inserted {count} sample records")
        
        cursor.close()
        conn.close()
        return True
    except Exception as e:
        logger.error(f"Error inserting sample data: {str(e)}")
        return False

def test_connection():
    """Test the connection to the application database"""
    try:
        conn = pyodbc.connect(APP_CONN_STR)
        cursor = conn.cursor()
        
        cursor.execute("SELECT @@VERSION")
        version = cursor.fetchone()[0]
        logger.info(f"Successfully connected to SQL Server: {version}")
        
        # Get table count
        cursor.execute("SELECT COUNT(*) FROM AtikBildirimFormlari")
        count = cursor.fetchone()[0]
        logger.info(f"Number of records in AtikBildirimFormlari: {count}")
        
        cursor.close()
        conn.close()
        return True
    except Exception as e:
        logger.error(f"Error testing connection: {str(e)}")
        return False

def main():
    """Main function to orchestrate database creation"""
    logger.info("Starting database creation process...")
    
    # Check Docker container first
    if not check_docker_container():
        logger.error("Please fix the SQL Server container issues before proceeding.")
        return
    
    # Then wait for SQL Server to be ready
    if not wait_for_sql_server():
        return
    
    # Create database
    if not create_database():
        return
    
    # Then create tables and add sample data
    if not create_tables():
        return
    
    # Optional: Insert sample data
    insert_sample_data(10)
    
    # Test the connection and configuration
    test_connection()
    
    logger.info("Database setup completed successfully!")

if __name__ == "__main__":
    main()