namespace AracServis.Helpers
{
    public static class ImageHelper
    {
        public static async Task<string?> KaydetVeSikistir(IFormFile dosya, string klasorYolu)
        {
            try
            {
                if (dosya == null || dosya.Length == 0) return null;

                // Dosya uzantısını al (.jpg, .png vb.)
                var uzanti = Path.GetExtension(dosya.FileName).ToLower();
                // Benzersiz bir isim oluştur (Örn: logo_638492.png)
                var yeniDosyaAdi = Guid.NewGuid().ToString() + uzanti;

                // Kaydedilecek tam yolu belirle (wwwroot/img/sistem/...)
                var tamYol = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", klasorYolu, yeniDosyaAdi);

                // Klasör yoksa oluştur
                var dir = Path.GetDirectoryName(tamYol);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

                // Dosyayı kaydet
                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await dosya.CopyToAsync(stream);
                }

                // Veritabanına yazılacak yolu döndür
                return "/" + klasorYolu + "/" + yeniDosyaAdi;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}