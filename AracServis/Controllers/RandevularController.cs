using AracServis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracServis.Controllers
{
    [Authorize(Roles = "İşletme Sahibi,Çalışan")]
    public class RandevularController : Controller
    {
        private readonly AracServisDbContext _context;

        public RandevularController(AracServisDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                                           .Include(r => r.Musteri)
                                           .Include(r => r.DurumBilgisi) // HOCANIN İSTEDİĞİ LOOKUP BAĞLANTISI (JOIN)
                                           .OrderByDescending(r => r.TalepTarihi)
                                           .ToListAsync();
            return View(randevular);
        }

        [HttpPost]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null && randevu.Durum == RandevuDurumu.OnayBekliyor)
            {
                randevu.Durum = RandevuDurumu.Islemde;

                var yeniIsEmri = new IsEmirleri
                {
                    MusteriId = randevu.MusteriId,
                    AracPlaka = randevu.AracPlaka,
                    AcilisTarihi = DateTime.Now,
                    Durum = IsEmriDurumu.Bekliyor,
                    IsDeleted = false
                };

                _context.IsEmirleris.Add(yeniIsEmri);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(randevu.SikayetAciklamasi))
                {
                    var detay = new IsEmriDetaylari
                    {
                        IsEmriId = yeniIsEmri.IsEmriId,
                        YapilanIslem = "MÜŞTERİ ŞİKAYETİ: " + randevu.SikayetAciklamasi,
                        Fiyat = 0
                    };
                    _context.IsEmriDetaylaris.Add(detay);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Randevu onaylandı! Araç garaja aktarıldı.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null && randevu.Durum == RandevuDurumu.OnayBekliyor)
            {
                randevu.Durum = RandevuDurumu.Reddedildi;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Randevu talebi reddedildi.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Musteri)
                .Include(r => r.DurumBilgisi)
                .FirstOrDefaultAsync(m => m.RandevuId == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu sistemden tamamen silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}