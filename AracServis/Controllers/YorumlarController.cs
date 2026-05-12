using AracServis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracServis.Controllers
{
    [Authorize]
    public class YorumlarController : Controller
    {
        private readonly AracServisDbContext _context;

        public YorumlarController(AracServisDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "İşletme Sahibi")]
        public async Task<IActionResult> Index()
        {
            var yorumlar = await _context.Yorumlars
                .Include(y => y.Musteri)
                .Include(y => y.IsEmri)
                .OrderByDescending(y => y.YorumTarihi)
                .ToListAsync();

            return View(yorumlar);
        }

        [HttpPost]
        [Authorize(Roles = "İşletme Sahibi")]
        public async Task<IActionResult> Cevapla(int yorumId, string isletmeCevabi)
        {
            var yorum = await _context.Yorumlars.FindAsync(yorumId);
            if (yorum != null && !string.IsNullOrEmpty(isletmeCevabi))
            {
                yorum.IsletmeCevabi = isletmeCevabi;
                yorum.CevapTarihi = DateTime.Now;
                yorum.OnaylandiMi = true; // Patron cevap yazdığı an yorum otomatik yayınlanır!
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yoruma cevabınız iletildi ve yayınlandı!";
            }
            return RedirectToAction(nameof(Index));
        }

        // PATRON İÇİN YENİ: Sadece Onayla (Cevap yazmadan direkt anasayfaya gönder)
        [HttpPost]
        [Authorize(Roles = "İşletme Sahibi")]
        public async Task<IActionResult> YorumOnayla(int id)
        {
            var yorum = await _context.Yorumlars.FindAsync(id);
            if (yorum != null)
            {
                yorum.OnaylandiMi = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum onaylandı, artık anasayfada görünüyor.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "İşletme Sahibi")]
        public async Task<IActionResult> YorumSil(int id)
        {
            var yorum = await _context.Yorumlars.FindAsync(id);
            if (yorum != null)
            {
                _context.Yorumlars.Remove(yorum);
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Yorum sistemden tamamen silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // MÜŞTERİ EKRANI: 14 GÜN KURALI VE DÜZENLEME MANTIĞI
        // =========================================================================
        [HttpPost]
        [Authorize(Roles = "Müşteri")]
        public async Task<IActionResult> YorumYap(int isEmriId, string yorumMetni, int puan)
        {
            var musteriId = HttpContext.Session.GetInt32("KullaniciID");
            if (musteriId == null) return RedirectToAction("Login", "Account");

            var mevcutYorum = await _context.Yorumlars.FirstOrDefaultAsync(y => y.IsEmriId == isEmriId && y.MusteriId == musteriId);

            if (mevcutYorum != null)
            {
                // Müşteri önceden yorum yapmış, 14 gün geçmiş mi kontrol edelim:
                TimeSpan fark = DateTime.Now - mevcutYorum.YorumTarihi;
                if (fark.TotalDays > 14)
                {
                    TempData["ErrorMessage"] = "Yorumunuzu düzenleme süreniz (14 gün) dolmuştur. Yeni değişiklik yapılamaz.";
                    return RedirectToAction("Araclarim", "Musteri");
                }

                // 14 gün geçmemişse eski yorumun üzerine yaz (Update)
                mevcutYorum.YorumMetni = yorumMetni;
                mevcutYorum.Puan = puan;
                mevcutYorum.YorumTarihi = DateTime.Now;
                mevcutYorum.OnaylandiMi = false; // Değiştiği için tekrar patron onayına düşer

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorumunuz güncellendi. Yönetici onayından sonra yayınlanacaktır.";
            }
            else
            {
                // Müşteri bu araca ilk defa yorum yapıyor (Insert)
                var yeniYorum = new Yorum
                {
                    MusteriId = musteriId.Value,
                    IsEmriId = isEmriId,
                    YorumMetni = yorumMetni,
                    Puan = puan,
                    YorumTarihi = DateTime.Now,
                    OnaylandiMi = false
                };

                _context.Yorumlars.Add(yeniYorum);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Değerlendirmeniz alındı! Onaylandıktan sonra yayınlanacaktır.";
            }

            return RedirectToAction("Araclarim", "Musteri");
        }
    }
}