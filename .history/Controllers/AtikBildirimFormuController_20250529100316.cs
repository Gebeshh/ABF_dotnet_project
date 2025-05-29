using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestNo_9999999.Data;
using TestNo_9999999.Models;
using TestNo_9999999.Services;

namespace TestNo_9999999.Controllers
{
    [Authorize] 
    public class AtikBildirimFormuController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AtikBildirimFormuService _abfService;
        private readonly ExcelExportService _excelService;

        public AtikBildirimFormuController(
            AppDbContext context, 
            AtikBildirimFormuService abfService,
            ExcelExportService excelService)
        {
            _context = context;
            _abfService = abfService;
            _excelService = excelService;
        }

        // GET: AtikBildirimFormu
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUSM = User.IsInRole("USM");
            var isDepo = User.IsInRole("DepoYetkilisi");
            var isAdmin = User.IsInRole("Admin");

            try
            {
                // Veritabanı sorgularını daha güvenli hale getirin
                var forms = isUSM || isDepo || isAdmin
                    ? await _context.AtikBildirimFormlari.AsNoTracking().OrderByDescending(a => a.Tarih).ToListAsync()
                    : await _context.AtikBildirimFormlari
                        .AsNoTracking()
                        .Where(a => a.KisimAtikSorumlusuId == userId)
                        .OrderByDescending(a => a.Tarih)
                        .ToListAsync();

                return View(forms);
            }
            catch (Exception ex)
            {
                // Geçici bir çözüm olarak boş liste döndür
                TempData["ErrorMessage"] = "Veritabanı bağlantısında bir sorun oluştu. Lütfen sistem yöneticisine başvurun.";
                return View(new List<AtikBildirimFormu>());
            }
        }

        // GET: AtikBildirimFormu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atikBildirimFormu = await _context.AtikBildirimFormlari
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (atikBildirimFormu == null)
            {
                return NotFound();
            }

            return View(atikBildirimFormu);
        }

        // GET: AtikBildirimFormu/Create
        public async Task<IActionResult> Create()
        {
            // Pre-fill form with user information
            var userName = User.Identity != null ? User.Identity.Name : "Unknown User";
            var department = User.FindFirstValue("Department"); 
            if (string.IsNullOrEmpty(department))
            {
                // Department claim bulunamadı, alternatif çözüm
                department = "Bilinmeyen Departman";
            }
            
            // Generate a real kayıt no to show in the form
            string kayitNo = await _abfService.GenerateKayitNoAsync();
            
            var model = new AtikBildirimFormu
            {
                Tarih = DateTime.Now,
                GonderenKisi = userName != null ? userName : "Bilinmeyen Kullanıcı",
                GonderenKisim = department,
                KayitNo = kayitNo,
                Durum = "Onay Bekliyor"
            };

            return View(model);
        }

        // POST: AtikBildirimFormu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AtikBildirimFormu atikBildirimFormu)
        {
            if (string.IsNullOrEmpty(atikBildirimFormu.Durum))
            {
                atikBildirimFormu.Durum = "Onay Bekliyor";
                ModelState.ClearValidationState(nameof(AtikBildirimFormu.Durum));
                ModelState.MarkFieldValid(nameof(AtikBildirimFormu.Durum));
            }
            
            // We're already setting the KayitNo in the GET action, so we don't need to generate it here
            // But check if it's valid, if not, generate a new one
            if (string.IsNullOrEmpty(atikBildirimFormu.KayitNo) || 
                !atikBildirimFormu.KayitNo.StartsWith("ABF-"))
            {
                atikBildirimFormu.KayitNo = await _abfService.GenerateKayitNoAsync();
            }
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı kimliği bulunamadı.");
                return View(atikBildirimFormu);
            }
            atikBildirimFormu.KisimAtikSorumlusuId = userId;
            
            if (ModelState.IsValid)
            {
                _context.Add(atikBildirimFormu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Form başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            // Hataları bulalım ve kullanıcıya bilgi verelim
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            
            return View(atikBildirimFormu);
        }

        // GET: AtikBildirimFormu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atikBildirimFormu = await _context.AtikBildirimFormlari.FindAsync(id);
            if (atikBildirimFormu == null)
            {
                return NotFound();
            }
            
            // Only allow editing of forms that are not approved
            if (atikBildirimFormu.Durum != "Hazırlanıyor")
            {
                TempData["ErrorMessage"] = "Onaylanmış formlar düzenlenemez.";
                return RedirectToAction(nameof(Details), new { id = atikBildirimFormu.Id });
            }

            // Check if user is authorized to edit this form
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (atikBildirimFormu.KisimAtikSorumlusuId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(atikBildirimFormu);
        }

        // POST: AtikBildirimFormu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AtikBildirimFormu atikBildirimFormu)
        {
            if (id != atikBildirimFormu.Id)
            {
                return NotFound();
            }

            // Re-check if the form can be edited
            var existingForm = await _context.AtikBildirimFormlari.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            if (existingForm == null)
            {
                return NotFound();
            }

            if (existingForm.Durum != "Hazırlanıyor")
            {
                TempData["ErrorMessage"] = "Onaylanmış formlar düzenlenemez.";
                return RedirectToAction(nameof(Details), new { id = atikBildirimFormu.Id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve original values that shouldn't change
                    atikBildirimFormu.KayitNo = existingForm.KayitNo;
                    atikBildirimFormu.Tarih = existingForm.Tarih;
                    atikBildirimFormu.GonderenKisi = existingForm.GonderenKisi;
                    atikBildirimFormu.GonderenKisim = existingForm.GonderenKisim;
                    atikBildirimFormu.KisimAtikSorumlusuId = existingForm.KisimAtikSorumlusuId;

                    _context.Update(atikBildirimFormu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtikBildirimFormuExists(atikBildirimFormu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(atikBildirimFormu);
        }

        // POST: AtikBildirimFormu/Submit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var form = await _context.AtikBildirimFormlari.FindAsync(id);
            if (form == null)
            {
                return NotFound();
            }

            // Check if user is authorized to submit this form
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (form.KisimAtikSorumlusuId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (form.Durum == "Hazırlanıyor")
            {
                form.Durum = "Onay Bekliyor";
                _context.Update(form);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Form başarıyla onaya gönderildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bu form zaten onaya gönderilmiş.";
            }

            return RedirectToAction(nameof(Details), new { id = form.Id });
        }

        // POST: AtikBildirimFormu/Approve/5
        [HttpPost]
        [Authorize(Roles = "USM")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var form = await _context.AtikBildirimFormlari.FindAsync(id);
            if (form == null)
            {
                return NotFound();
            }

            if (form.Durum == "Onay Bekliyor")
            {
                form.Durum = "Onaylandı";
                form.UsmPersoneli = User.Identity?.Name ?? "Unknown User";
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı kimliği bulunamadı.";
                    return RedirectToAction(nameof(Details), new { id = form.Id });
                }
                form.UsmPersonelId = userId;
                form.OnayTarihi = DateTime.Now;

                if (form != null)
                {
                    _context.Update(form);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Form başarıyla onaylandı.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Bu form onaylanamaz.";
            }

            return RedirectToAction(nameof(Details), new { id = form?.Id ?? 0 });
        }

        // GET: AtikBildirimFormu/ExportToExcel
        [Authorize(Roles = "USM")]
        public async Task<IActionResult> ExportToExcel()
        {
            var forms = await _abfService.GetAllForExcelExportAsync();
            var excelData = _excelService.ExportToExcel(forms);

            return File(
                excelData,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AtikBildirimFormlari_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        private bool AtikBildirimFormuExists(int id)
        {
            return _context.AtikBildirimFormlari.Any(e => e.Id == id);
        }
    }
}