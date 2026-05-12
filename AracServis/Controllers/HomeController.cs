using AracServis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracServis.Controllers
{
    // Giriş yapmayan kimse bu Controller'a erişemez!
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AracServisDbContext _context;

        public HomeController(AracServisDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcının rolünü doğrudan yeni Auth sisteminden çekiyoruz
            var isPatron = User.IsInRole("İşletme Sahibi");
            var isCalisan = User.IsInRole("Çalışan");

            // Yönetici ve Çalışan İstatistikleri
            if (isPatron || isCalisan)
            {
                // ENUM KULLANIMI: String yerine statüleri kesin (Enum) olarak kullanıyoruz!
                ViewBag.BekleyenRandevular = await _context.Randevular
                                                           .CountAsync(r => r.Durum == RandevuDurumu.OnayBekliyor);

                ViewBag.GarajdakiAraclar = await _context.IsEmirleris
                                                         .CountAsync(i => i.Durum == IsEmriDurumu.Bekliyor ||
                                                                          i.Durum == IsEmriDurumu.Islemde);

                // IsDeleted yazmıyoruz, Müşteri rolünü çekiyoruz.
                ViewBag.ToplamMusteri = await _context.Kullanicilars
                                                      .CountAsync(k => k.Rol.RolAdi == "Müşteri");

                var ciro = await _context.IsEmriDetaylaris.SumAsync(d => d.Fiyat);
                ViewBag.ToplamCiro = ciro.ToString("N2");
            }

            return View();
        }
    }
}