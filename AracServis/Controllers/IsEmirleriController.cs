using AracServis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AracServis.Controllers
{
    // YETKİ FİLTRESİ: Sadece Patron ve Çalışan girebilir.
    [Authorize(Roles = "İşletme Sahibi,Çalışan")]
    public class IsEmirleriController : Controller
    {
        private readonly AracServisDbContext _context;

        public IsEmirleriController(AracServisDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. GARAJDAKİ ARAÇLARIN LİSTESİ (INDEX)
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var aktifIsEmirleri = await _context.IsEmirleris
                                          .Include(i => i.Musteri)
                                          .Include(i => i.DurumBilgisi) // HOCANIN İSTEDİĞİ LOOKUP BAĞLANTISI (JOIN)
                                          .OrderByDescending(i => i.AcilisTarihi)
                                          .ToListAsync();

            return View(aktifIsEmirleri);
        }

        // =========================================================
        // 2. ARACIN DURUMUNU GÜNCELLEME (ENUM İLE)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumGuncelle(int id, IsEmriDurumu yeniDurum)
        {
            var isEmri = await _context.IsEmirleris.FindAsync(id);
            if (isEmri != null)
            {
                isEmri.Durum = yeniDurum;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Araç durumu başarıyla güncellendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // 3. İŞ EMRİ DETAYLARI (Çalışanın Parça Girdiği Ekran)
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var isEmirleri = await _context.IsEmirleris
                .Include(i => i.Musteri)
                .Include(i => i.IsEmriDetaylaris)
                .Include(i => i.DurumBilgisi) // HOCANIN İSTEDİĞİ LOOKUP BAĞLANTISI (JOIN)
                .FirstOrDefaultAsync(m => m.IsEmriId == id);

            if (isEmirleri == null) return NotFound();

            ViewBag.ToplamFatura = isEmirleri.IsEmriDetaylaris.Sum(d => d.Fiyat);

            return View(isEmirleri);
        }

        // =========================================================
        // 4. ARACA YENİ İŞLEM / PARÇA EKLEME (ENUM KONTROLÜ İLE)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IslemEkle(int isEmriId, string yapilanIslem, decimal fiyat)
        {
            var isEmri = await _context.IsEmirleris.FindAsync(isEmriId);
            if (isEmri == null) return NotFound();

            if (isEmri.Durum != IsEmriDurumu.Islemde)
            {
                TempData["ErrorMessage"] = "Sadece 'İşlemde' olan araçlara parça ekleyebilirsiniz!";
                return RedirectToAction(nameof(Details), new { id = isEmriId });
            }

            var yeniDetay = new IsEmriDetaylari
            {
                IsEmriId = isEmriId,
                YapilanIslem = yapilanIslem,
                Fiyat = fiyat,
                IsDeleted = false
            };

            _context.IsEmriDetaylaris.Add(yeniDetay);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İşlem/Parça başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = isEmriId });
        }

        // =========================================================
        // 5. MANUEL YENİ İŞ EMRİ OLUŞTURMA (GET & POST)
        // =========================================================
        public IActionResult Create()
        {
            ViewData["MusteriId"] = new SelectList(_context.Kullanicilars.Where(k => k.Rol.RolAdi == "Müşteri"), "KullaniciId", "AdSoyad");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IsEmriId,MusteriId,AracPlaka")] IsEmirleri isEmirleri)
        {
            if (ModelState.IsValid)
            {
                isEmirleri.Durum = IsEmriDurumu.Bekliyor;
                isEmirleri.AcilisTarihi = DateTime.Now;
                isEmirleri.IsDeleted = false;

                _context.Add(isEmirleri);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İş emri başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["MusteriId"] = new SelectList(_context.Kullanicilars.Where(k => k.Rol.RolAdi == "Müşteri"), "KullaniciId", "AdSoyad", isEmirleri.MusteriId);
            return View(isEmirleri);
        }

        // =========================================================
        // 6. İŞ EMRİ SİLME / ARŞİVE KALDIRMA (SOFT DELETE)
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var isEmirleri = await _context.IsEmirleris
                .Include(i => i.Musteri)
                .Include(i => i.DurumBilgisi)
                .FirstOrDefaultAsync(m => m.IsEmriId == id);

            if (isEmirleri == null) return NotFound();

            return View(isEmirleri);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isEmirleri = await _context.IsEmirleris.FindAsync(id);
            if (isEmirleri != null)
            {
                isEmirleri.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "İş emri arşive kaldırıldı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}