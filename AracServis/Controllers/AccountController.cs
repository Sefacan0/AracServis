using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AracServis.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AracServis.Controllers
{
    public class AccountController : Controller
    {
        private readonly AracServisDbContext _context;

        public AccountController(AracServisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var rollerListesi = _context.Rollers.ToList();
            var musteriRolu = rollerListesi.FirstOrDefault(r => r.RolAdi == "Müşteri");
            ViewData["RolId"] = new SelectList(rollerListesi, "RolId", "RolAdi", musteriRolu?.RolId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("AdSoyad,KullaniciAdi,Sifre,RolId")] Kullanicilar kullanici)
        {
            var secilebilirRoller = await _context.Rollers.ToListAsync();

            if (kullanici.RolId <= 0)
            {
                var musteriRolu = secilebilirRoller.FirstOrDefault(r => r.RolAdi == "Müşteri");
                if (musteriRolu != null) kullanici.RolId = musteriRolu.RolId;
            }

            if (!ModelState.IsValid)
            {
                ViewData["RolId"] = new SelectList(secilebilirRoller, "RolId", "RolAdi", kullanici.RolId);
                return View(kullanici);
            }

            var varMi = await _context.Kullanicilars.AnyAsync(u => u.KullaniciAdi == kullanici.KullaniciAdi);
            if (varMi)
            {
                TempData["ErrorMessage"] = "Bu kullanıcı adı zaten kullanılıyor!";
                ViewData["RolId"] = new SelectList(secilebilirRoller, "RolId", "RolAdi", kullanici.RolId);
                return View(kullanici);
            }

            var secilenRol = await _context.Rollers.FindAsync(kullanici.RolId);

            if (secilenRol != null && secilenRol.RolAdi == "İşletme Sahibi")
            {
                var yoneticiVarMi = await _context.Kullanicilars.AnyAsync(u => u.Rol.RolAdi == "İşletme Sahibi");
                if (yoneticiVarMi)
                {
                    TempData["ErrorMessage"] = "Sistemde zaten bir İşletme Sahibi hesabı mevcut!";
                    ViewData["RolId"] = new SelectList(secilebilirRoller, "RolId", "RolAdi", kullanici.RolId);
                    return View(kullanici);
                }
                kullanici.IsApproved = true;
                TempData["SuccessMessage"] = "Yönetici hesabı başarıyla oluşturuldu.";
            }
            else
            {
                kullanici.IsApproved = false;
                TempData["WarningMessage"] = "Kayıt başarılı. İşletme sahibinin hesabınızı onaylamasını bekleyiniz.";
            }

            kullanici.Sifre = Sifreleme.HashSifre(kullanici.Sifre);
            kullanici.IsDeleted = false;

            _context.Add(kullanici);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var ayarlar = await _context.SistemAyarlaris.FirstOrDefaultAsync();
            if (ayarlar != null)
            {
                ViewBag.IsletmeAdi = ayarlar.IsletmeAdi;
                ViewBag.LogoYolu = !string.IsNullOrEmpty(ayarlar.LogoYolu) ? "/" + ayarlar.LogoYolu.TrimStart('/') : "";
            }

            // GİRİŞ EKRANI İÇİN: Sadece Patronun onayladığı (OnaylandiMi == true) yorumlar gelir.
            var yorumlar = await _context.Yorumlars
                                           .Include(y => y.Musteri)
                                           .Where(y => y.OnaylandiMi == true)
                                           .OrderByDescending(y => y.Puan)
                                           .Take(5)
                                           .ToListAsync();

            ViewBag.MusteriYorumlari = yorumlar;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre)
        {
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                TempData["ErrorMessage"] = "Kullanıcı adı ve şifre boş bırakılamaz!";
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Kullanicilars
                                     .Include(u => u.Rol)
                                     .FirstOrDefaultAsync(u => u.KullaniciAdi == kullaniciAdi);

            if (user != null && Sifreleme.SifreDogrula(sifre, user.Sifre))
            {
                // =========================================================
                // BÜYÜK DOKUNUŞ: KULLANICI RED NEDENİ SORGULAMASI
                // =========================================================
                if (user.IsApproved == false)
                {
                    // Eğer patron reddedip mesaj yazmışsa:
                    if (!string.IsNullOrEmpty(user.RedNedeni))
                    {
                        TempData["ErrorMessage"] = $"BAŞVURUNUZ REDDEDİLDİ! Neden: {user.RedNedeni}";
                    }
                    // Eğer henüz patron bakmamışsa, beklemedeyse:
                    else
                    {
                        TempData["WarningMessage"] = "Hesabınız şu an patron onayı bekliyor. Lütfen daha sonra tekrar deneyin.";
                    }
                    return RedirectToAction(nameof(Login));
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.KullaniciId.ToString()),
                    new Claim(ClaimTypes.Name, user.AdSoyad),
                    new Claim(ClaimTypes.Role, user.Rol?.RolAdi ?? "")
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("Cookies", principal);

                var ayarlar = await _context.SistemAyarlaris.FirstOrDefaultAsync();
                if (ayarlar != null)
                {
                    HttpContext.Session.SetString("IsletmeAdi", ayarlar.IsletmeAdi ?? "Araç Servis");

                    if (!string.IsNullOrEmpty(ayarlar.LogoYolu))
                        HttpContext.Session.SetString("LogoYolu", $"/{ayarlar.LogoYolu.TrimStart('/')}");

                    if (!string.IsNullOrEmpty(ayarlar.WpNumarasi))
                        HttpContext.Session.SetString("WpNumarasi", ayarlar.WpNumarasi);
                }

                HttpContext.Session.SetInt32("KullaniciID", user.KullaniciId);
                HttpContext.Session.SetString("AdSoyad", user.AdSoyad);
                HttpContext.Session.SetString("RolAdi", user.Rol?.RolAdi ?? "");

                TempData["SuccessMessage"] = $"Hoş geldiniz, {user.AdSoyad}.";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Hatalı kullanıcı adı veya şifre!";
            return RedirectToAction(nameof(Login));
        }


        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("Cookies");

            TempData["SuccessMessage"] = "Başarıyla çıkış yapıldı.";
            return RedirectToAction(nameof(Login));
        }
    }
}