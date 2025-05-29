using OfficeOpenXml;
using OfficeOpenXml.Style;
using TestNo_9999999.Models;
using System.Drawing;

namespace TestNo_9999999.Services
{
    public class ExcelExportService
    {
        public byte[] ExportToExcel(List<AtikBildirimFormu> forms)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Atık Bildirim Formları");

                // Add header row
                worksheet.Cells[1, 1].Value = "Kayıt No";
                worksheet.Cells[1, 2].Value = "Tarih";
                worksheet.Cells[1, 3].Value = "Gönderen Kısım";
                worksheet.Cells[1, 4].Value = "Gönderen Kişi";
                worksheet.Cells[1, 5].Value = "Atığın Cinsi";
                worksheet.Cells[1, 6].Value = "Atığın İsmi";
                worksheet.Cells[1, 7].Value = "Sapma-DK-HTF";
                worksheet.Cells[1, 8].Value = "Miktar (Kg)";
                worksheet.Cells[1, 9].Value = "Miktar (Adet)";
                worksheet.Cells[1, 10].Value = "Varil Adedi";
                worksheet.Cells[1, 11].Value = "Fıçı Adedi";
                worksheet.Cells[1, 12].Value = "IBC Adedi";
                worksheet.Cells[1, 13].Value = "Torba Adedi";
                worksheet.Cells[1, 14].Value = "Kutu Adedi";
                worksheet.Cells[1, 15].Value = "Palet Adedi";
                worksheet.Cells[1, 16].Value = "Durum";
                worksheet.Cells[1, 17].Value = "ÜSM Personeli";
                worksheet.Cells[1, 18].Value = "Onay Tarihi";

                // Format header
                using (var range = worksheet.Cells[1, 1, 1, 18])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                }

                // Populate data rows
                for (int i = 0; i < forms.Count; i++)
                {
                    var form = forms[i];
                    int row = i + 2;

                    worksheet.Cells[row, 1].Value = form.KayitNo;
                    worksheet.Cells[row, 2].Value = form.Tarih;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "dd.MM.yyyy";
                    worksheet.Cells[row, 3].Value = form.GonderenKisim;
                    worksheet.Cells[row, 4].Value = form.GonderenKisi;
                    worksheet.Cells[row, 5].Value = form.AtikCinsi.ToString();
                    worksheet.Cells[row, 6].Value = form.AtikIsmi;
                    worksheet.Cells[row, 7].Value = string.IsNullOrEmpty(form.SapmaDkHtf) ? "-" : form.SapmaDkHtf;
                    worksheet.Cells[row, 8].Value = form.MiktarKg;
                    worksheet.Cells[row, 9].Value = form.MiktarAdet;
                    worksheet.Cells[row, 10].Value = form.AmbalajVarilAdedi;
                    worksheet.Cells[row, 11].Value = form.AmbalajFiciAdedi;
                    worksheet.Cells[row, 12].Value = form.AmbalajIbcAdedi;
                    worksheet.Cells[row, 13].Value = form.AmbalajTorbaAdedi;
                    worksheet.Cells[row, 14].Value = form.AmbalajKutuAdedi;
                    worksheet.Cells[row, 15].Value = form.AmbalajPaletAdedi;
                    worksheet.Cells[row, 16].Value = form.Durum;
                    worksheet.Cells[row, 17].Value = form.UsmPersoneli;
                    worksheet.Cells[row, 18].Value = form.OnayTarihi;
                    if (form.OnayTarihi.HasValue)
                    {
                        worksheet.Cells[row, 18].Style.Numberformat.Format = "dd.MM.yyyy";
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }
}