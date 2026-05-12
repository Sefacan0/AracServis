using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AracServis.Models;
using Microsoft.AspNetCore.Authorization;

namespace AracServis.Controllers
{
    // Sadece personelin detay eklemesini/silmesini garantiye alıyoruz
    [Authorize(Roles = "İşletme Sahibi,Çalışan")]
    public class IsEmriDetaylarisController : Controller
    {
        private readonly AracServisDbContext _context;

        public IsEmriDetaylarisController(AracServisDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var detaylar = _context.IsEmriDetaylaris.Include(i => i.IsEmri);
            return View(await detaylar.ToListAsync());
        }

        public IActionResult Create(int? isEmriId)
        {
            if (isEmriId == null) return NotFound();
            ViewBag.IsEmriId = isEmriId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IsEmriId,YapilanIslem,Fiyat")] IsEmriDetaylari isEmriDetaylari)
        {
            if (ModelState.IsValid)
            {
                isEmriDetaylari.IsDeleted = false;
                _context.Add(isEmriDetaylari);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İşlem faturaya başarıyla eklendi.";
                return RedirectToAction("Details", "IsEmirleri", new { id = isEmriDetaylari.IsEmriId });
            }
            return View(isEmriDetaylari);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var isEmriDetaylari = await _context.IsEmriDetaylaris.FindAsync(id);
            if (isEmriDetaylari == null) return NotFound();

            return View(isEmriDetaylari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetayId,IsEmriId,YapilanIslem,Fiyat,IsDeleted")] IsEmriDetaylari isEmriDetaylari)
        {
            if (id != isEmriDetaylari.DetayId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(isEmriDetaylari);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "İşlem bilgileri güncellendi.";
                return RedirectToAction("Details", "IsEmirleri", new { id = isEmriDetaylari.IsEmriId });
            }
            return View(isEmriDetaylari);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isEmriDetaylari = await _context.IsEmriDetaylaris.FindAsync(id);
            if (isEmriDetaylari == null) return NotFound();

            // CS0266 HATASININ ÇÖZÜMÜ: Eğer değer null gelirse, sistemi çöktürmek yerine id'yi 0 kabul ediyoruz.
            int gecerliIsEmriId = isEmriDetaylari.IsEmriId ?? 0;

            isEmriDetaylari.IsDeleted = true;
            await _context.SaveChangesAsync();

            TempData["WarningMessage"] = "İşlem faturadan kaldırıldı.";
            return RedirectToAction("Details", "IsEmirleri", new { id = gecerliIsEmriId });
        }
    }
}