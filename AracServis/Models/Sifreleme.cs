using Microsoft.CodeAnalysis.Scripting;

namespace AracServis.Models
{
    public static class Sifreleme
    {
        // Şifreyi BCrypt (Dinamik Salt) ile geri döndürülemez güvenli bir metne çevirir
        public static string HashSifre(string sifre)
        {
            // Şifreyi otomatik salt üreterek hashler (Kırılmasını engeller)
            return BCrypt.Net.BCrypt.HashPassword(sifre);
        }

        // Girilen düz şifrenin, veritabanındaki hashli şifreyle eşleşip eşleşmediğini doğrular
        public static bool SifreDogrula(string girilenSifre, string veritabanindakiHash)
        {
            // BCrypt, metnin içindeki salt'ı otomatik okur ve karşılaştırmayı kendi yapar
            return BCrypt.Net.BCrypt.Verify(girilenSifre, veritabanindakiHash);
        }
    }
}