using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AracServis.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace AracServis.Controllers
{
    [Authorize(Roles = "Müşteri")]
    public class MusteriController : Controller
    {
        private readonly AracServisDbContext _context;

        public MusteriController(AracServisDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // --- 1. ARAÇLARIM VE BEKLEYEN TALEPLERİM ---
        // ==========================================
        public async Task<IActionResult> Araclarim()
        {
            var musteriIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int musteriId = int.Parse(musteriIdStr);

            // Garajdaki araçlar (İş Emirleri)
            var araclar = await _context.IsEmirleris
                                        .Include(i => i.DurumBilgisi)
                                        .Where(i => i.MusteriId == musteriId)
                                        .OrderByDescending(i => i.AcilisTarihi)
                                        .ToListAsync();

            // Onay bekleyen yeni randevu talepleri
            ViewBag.BekleyenRandevular = await _context.Randevular
                                                .Where(r => r.MusteriId == musteriId && r.Durum == RandevuDurumu.OnayBekliyor)
                                                .OrderByDescending(r => r.TalepTarihi)
                                                .ToListAsync();

            // KRİTİK DÜZELTME: 14 Gün kuralının ve buton gizlemenin çalışması için müşterinin yorumlarını View'a yolluyoruz!
            ViewBag.MusteriYorumlari = await _context.Yorumlars
                                                .Where(y => y.MusteriId == musteriId)
                                                .ToListAsync();

            return View(araclar);
        }

        // ==========================================
        // --- 2. ARAÇ DETAY VE FATURA GÖRÜNTÜLEME ---
        // ==========================================
        public async Task<IActionResult> AracDetay(int id)
        {
            var musteriIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int musteriId = int.Parse(musteriIdStr);

            var isEmri = await _context.IsEmirleris
                                       .Include(i => i.DurumBilgisi)
                                       .Include(i => i.IsEmriDetaylaris)
                                       .FirstOrDefaultAsync(i => i.IsEmriId == id && i.MusteriId == musteriId);

            if (isEmri == null) return NotFound();

            var gecmisYorum = await _context.Yorumlars
                                            .OrderByDescending(y => y.YorumTarihi)
                                            .FirstOrDefaultAsync(y => y.IsEmriId == id && y.MusteriId == musteriId);

            ViewBag.MusteriYorumu = gecmisYorum;

            return View(isEmri);
        }

        // ==========================================
        // --- 3. YENİ RANDEVU ALMA (GET & POST) ---
        // ==========================================

        // KAYBOLAN GET METODU GERİ GELDİ! (500 Hatasını çözen kısım)
        [HttpGet]
        public IActionResult RandevuAl()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuAl([Bind("AracPlaka,SikayetAciklamasi")] Randevu randevu, IFormFile? GorselDosyasi)
        {
            if (GorselDosyasi != null && GorselDosyasi.Length > 0)
            {
                string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(GorselDosyasi.FileName);
                string klasorYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(klasorYolu)) Directory.CreateDirectory(klasorYolu);
                string tamYol = Path.Combine(klasorYolu, dosyaAdi);

                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await GorselDosyasi.CopyToAsync(stream);
                }
                randevu.ArizaGorselYolu = "/uploads/" + dosyaAdi;
            }

            var musteriIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            randevu.MusteriId = int.Parse(musteriIdStr);
            randevu.TalepTarihi = DateTime.Now;
            randevu.Durum = RandevuDurumu.OnayBekliyor;

            if (ModelState.IsValid)
            {
                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu talebiniz başarıyla iletildi.";
                return RedirectToAction("Araclarim");
            }
            return View(randevu);
        }

        // ==========================================
        // --- 4. RANDEVU GÜNCELLEME (GÖRSEL SİLİNMEYECEK) ---
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> RandevuGuncelle(int id)
        {
            var musteriIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int musteriId = int.Parse(musteriIdStr);

            var randevu = await _context.Randevular
                                .FirstOrDefaultAsync(r => r.RandevuId == id &&
                                                         r.MusteriId == musteriId &&
                                                         r.Durum == RandevuDurumu.OnayBekliyor);

            if (randevu == null) return NotFound();
            return View(randevu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuGuncelle(int id, [Bind("RandevuId,AracPlaka,SikayetAciklamasi,ArizaGorselYolu")] Randevu randevu, IFormFile? GorselDosyasi)
        {
            if (id != randevu.RandevuId) return NotFound();

            var musteriIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int musteriId = int.Parse(musteriIdStr);

            var mevcutRandevu = await _context.Randevular.AsNoTracking()
                                      .FirstOrDefaultAsync(r => r.RandevuId == id && r.MusteriId == musteriId);

            if (mevcutRandevu == null || mevcutRandevu.Durum != RandevuDurumu.OnayBekliyor) return RedirectToAction(nameof(Araclarim));

            if (GorselDosyasi != null && GorselDosyasi.Length > 0)
            {
                string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(GorselDosyasi.FileName);
                string tamYol = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", dosyaAdi);
                using (var stream = new FileStream(tamYol, FileMode.Create)) { await GorselDosyasi.CopyToAsync(stream); }
                randevu.ArizaGorselYolu = "/uploads/" + dosyaAdi;
            }
            else
            {
                randevu.ArizaGorselYolu = mevcutRandevu.ArizaGorselYolu;
            }

            randevu.MusteriId = musteriId;
            randevu.TalepTarihi = DateTime.Now;
            randevu.Durum = RandevuDurumu.OnayBekliyor;

            if (ModelState.IsValid)
            {
                _context.Update(randevu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bilgiler başarıyla güncellendi.";
                return RedirectToAction(nameof(Araclarim));
            }
            return View(randevu);
        }

        // ==========================================
        // --- 5. RANDEVU İPTAL ETME ---
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var musteriIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int musteriId = int.Parse(musteriIdStr);

            var iptalEdilecekRandevu = await _context.Randevular
                                          .FirstOrDefaultAsync(r => r.RandevuId == id && r.MusteriId == musteriId && r.Durum == RandevuDurumu.OnayBekliyor);

            if (iptalEdilecekRandevu != null)
            {
                _context.Randevular.Remove(iptalEdilecekRandevu);
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Randevu talebiniz başarıyla iptal edildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bu randevu işlemde olduğu için iptal edilemez.";
            }

            return RedirectToAction(nameof(Araclarim));
        }
    }
}