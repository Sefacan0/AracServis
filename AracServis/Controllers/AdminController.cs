using AracServis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AracServis.Controllers
{
    // BÜYÜK DEVRİM: Bu etiketi koyduğumuz an, aşağıdaki hiçbir metoda patron harici kimse ulaşamaz!
    [Authorize(Roles = "İşletme Sahibi")]
    public class AdminController : Controller
    {
        private readonly AracServisDbContext _context;

        public AdminController(AracServisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> OnayBekleyenler()
        {
            // BÜYÜK DOKUNUŞ: Red nedeni DOLU olanları listeden uçuruyoruz!
            var bekleyenler = await _context.Kullanicilars
                                            .Include(k => k.Rol)
                                            .Where(k => k.IsApproved == false && k.RedNedeni == null)
                                            .OrderByDescending(k => k.KullaniciId)
                                            .ToListAsync();

            return View(bekleyenler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalisanOnayla(int id)
        {
            var kullanici = await _context.Kullanicilars.Include(u => u.Rol).FirstOrDefaultAsync(u => u.KullaniciId == id);
            if (kullanici != null)
            {
                kullanici.IsApproved = true;
                kullanici.RedNedeni = null; // Eğer önceden reddedilmişse nedeni temizliyoruz
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{kullanici.AdSoyad} sisteme başarıyla dahil edildi.";
            }
            return RedirectToAction(nameof(OnayBekleyenler));
        }

        // BÜYÜK DOKUNUŞ: Red nedeni parametresi eklendi ve sistem güncellendi!
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciReddet(int id, string redNedeni)
        {
            var kullanici = await _context.Kullanicilars.FindAsync(id);
            if (kullanici != null)
            {
                kullanici.IsApproved = false;
                kullanici.RedNedeni = redNedeni; // Patronun girdiği mesaj veritabanına yazılır
                kullanici.IsDeleted = false; // Silmiyoruz ki, girmeye çalıştığında mesajı okuyabilsin!

                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Kullanıcı başvurusu reddedildi ve neden belirtildi.";
            }
            return RedirectToAction(nameof(OnayBekleyenler));
        }

        [HttpGet]
        public async Task<IActionResult> KullaniciYonetimi()
        {
            var aktifKullanicilar = await _context.Kullanicilars
                                          .Include(k => k.Rol)
                                          .Where(k => k.IsApproved == true)
                                          .OrderBy(k => k.Rol.RolAdi)
                                          .ToListAsync();

            return View(aktifKullanicilar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasifeAl(int id)
        {
            var aktifKullaniciIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() == aktifKullaniciIdStr)
            {
                TempData["ErrorMessage"] = "Hata! Kendi yönetici hesabınızı pasife alamazsınız.";
                return RedirectToAction(nameof(KullaniciYonetimi));
            }

            var kullanici = await _context.Kullanicilars.FindAsync(id);
            if (kullanici != null)
            {
                kullanici.IsDeleted = true;
                kullanici.IsApproved = false;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = $"{kullanici.AdSoyad} adlı kişinin sistem erişimi kesildi.";
            }

            return RedirectToAction(nameof(KullaniciYonetimi));
        }

        [HttpGet]
        public async Task<IActionResult> Ayarlar()
        {
            var ayarlar = await _context.SistemAyarlaris.FirstOrDefaultAsync();
            if (ayarlar == null)
            {
                ayarlar = new SistemAyarlari { IsletmeAdi = "Araç Servis Otomasyonu", ArkaPlanRengi = "#f8f9fa" };
                _context.SistemAyarlaris.Add(ayarlar);
                await _context.SaveChangesAsync();
            }
            return View(ayarlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ayarlar(SistemAyarlari model, IFormFile? yeniLogo)
        {
            var mevcutAyarlar = await _context.SistemAyarlaris.FirstOrDefaultAsync();
            if (mevcutAyarlar == null) return NotFound();

            if (yeniLogo != null)
            {
                string? logoYolu = await ImageHelper.KaydetVeSikistir(yeniLogo, "img/sistem");
                if (logoYolu != null) mevcutAyarlar.LogoYolu = logoYolu;
            }

            mevcutAyarlar.IsletmeAdi = model.IsletmeAdi;
            mevcutAyarlar.ArkaPlanRengi = model.ArkaPlanRengi;
            mevcutAyarlar.WpNumarasi = model.WpNumarasi;
            mevcutAyarlar.GuncellemeTarihi = System.DateTime.Now;

            HttpContext.Session.SetString("IsletmeAdi", mevcutAyarlar.IsletmeAdi);

            if (!string.IsNullOrEmpty(mevcutAyarlar.LogoYolu))
                HttpContext.Session.SetString("LogoYolu", $"/{mevcutAyarlar.LogoYolu}");

            if (!string.IsNullOrEmpty(mevcutAyarlar.WpNumarasi))
                HttpContext.Session.SetString("WpNumarasi", mevcutAyarlar.WpNumarasi);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İşletme ayarları başarıyla güncellendi!";
            return RedirectToAction(nameof(Ayarlar));
        }
    }
}