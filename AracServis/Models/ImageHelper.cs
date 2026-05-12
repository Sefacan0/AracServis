using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace AracServis.Models
{
    public static class ImageHelper
    {
        // Metot adını AdminController ile uyumlu hale getirdik: KaydetVeSikistir
        public static async Task<string?> KaydetVeSikistir(IFormFile file, string klasorYolu)
        {
            if (file == null || file.Length == 0) return null;

            // 1. Benzersiz bir dosya adı oluştur (Çakışma olmasın)
            // Uzantıyı .jpg yaparak tüm formatları standartlaştırıyoruz
            string dosyaAdi = Guid.NewGuid().ToString() + ".jpg";

            // 2. Kayıt yolunu belirle (wwwroot/img/sistem gibi)
            string dizinYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", klasorYolu);
            string tamYol = Path.Combine(dizinYolu, dosyaAdi);

            // 3. Klasör yoksa oluştur
            if (!Directory.Exists(dizinYolu))
            {
                Directory.CreateDirectory(dizinYolu);
            }

            // 4. Görsel işleme motorunu çalıştır
            using (var stream = file.OpenReadStream())
            {
                using (var image = await Image.LoadAsync(stream))
                {
                    // OTOMATİK ÖLÇEKLERME: 
                    // 4K olsa bile genişliği 800px yapıyoruz, yükseklik oranı bozulmadan otomatik ayarlanır.
                    // Bu işlem dosya boyutunu %90 oranında düşürür.
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(800, 0), // Yükseklik 0 ise oranı korur
                        Mode = ResizeMode.Max
                    }));

                    // 5. YÜKSEK SIKIŞTIRMA: 
                    // %75 kalite seviyesi insan gözüyle fark edilmez ama veritabanını şişirmeyi engeller.
                    var encoder = new JpegEncoder { Quality = 75 };
                    await image.SaveAsync(tamYol, encoder);
                }
            }

            // 6. Veritabanına kaydedilecek dosya yolunu döndür
            return "/" + klasorYolu + "/" + dosyaAdi;
        }
    }
}