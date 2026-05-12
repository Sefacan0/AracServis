using Microsoft.AspNetCore.Mvc;

namespace AracServis.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            if (statusCode == 404)
            {
                ViewBag.HataKodu = "404";
                ViewBag.Mesaj = "Aradığınız sayfa bulunamadı veya taşınmış olabilir.";
            }
            else if (statusCode == 403 || statusCode == 401)
            {
                ViewBag.HataKodu = "403";
                ViewBag.Mesaj = "Bu sayfayı görüntülemek için yetkiniz (İşletme Sahibi) bulunmamaktadır!";
            }
            else
            {
                ViewBag.HataKodu = "500";
                ViewBag.Mesaj = "Sistemde beklenmeyen bir hata oluştu. Mühendislerimiz konuyla ilgileniyor.";
            }

            return View();
        }

        [Route("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            ViewBag.HataKodu = "403";
            ViewBag.Mesaj = "Bu sayfayı veya istatistikleri görüntülemek için YÖNETİCİ yetkisine sahip olmalısınız!";
            return View("Index");
        }
    }
}