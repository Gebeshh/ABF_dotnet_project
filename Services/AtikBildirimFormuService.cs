using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestNo_9999999.Data;
using TestNo_9999999.Models;

namespace TestNo_9999999.Services
{
    public class AtikBildirimFormuService
    {
        private readonly AppDbContext _context;

        public AtikBildirimFormuService(AppDbContext context)
        {
            _context = context;
        }

        // Generate a unique ABF record number
        public async Task<string> GenerateKayitNoAsync()
        {
            // Format: ABF-YYYY-MM-NNNN (Year-Month-Sequential Number)
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            
            // Find the highest sequence number for the current month
            var prefix = $"ABF-{year}-{month:D2}-";
            var lastForm = await _context.AtikBildirimFormlari
                .Where(a => a.KayitNo.StartsWith(prefix))
                .OrderByDescending(a => a.KayitNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1; // Start with 0001 if no forms exist for this month
            if (lastForm != null)
            {
                // Parse the last number from the KayitNo and increment
                var parts = lastForm.KayitNo.Split('-');
                if (parts.Length == 4 && int.TryParse(parts[3], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"ABF-{year}-{month:D2}-{nextNumber:D4}";
        }

        // Get all forms for export to Excel
        public async Task<List<AtikBildirimFormu>> GetAllForExcelExportAsync()
        {
            return await _context.AtikBildirimFormlari
                .OrderByDescending(a => a.Tarih)
                .ToListAsync();
        }
        
        // Get forms by approval status
        public async Task<List<AtikBildirimFormu>> GetFormsByStatusAsync(string status)
        {
            return await _context.AtikBildirimFormlari
                .Where(a => a.Durum == status)
                .OrderByDescending(a => a.Tarih)
                .ToListAsync();
        }
    }
}