namespace AracServis.Models
{
    public static class SabitMesajlar
    {
        // --- ŞİFRE VE KAYIT MESAJLARI ---
        public const string SifreKurali = "Şifreniz en az 6 karakter olmalı, en az 1 büyük harf ve 1 noktalama işareti içermelidir!";
        public const string SifreKisa = "Şifreniz en az 6 karakter olmalıdır!";
        public const string KullaniciAdiKisa = "Kullanıcı adı en az 4, en fazla 20 karakter olmalıdır!";
        public const string AdSoyadZorunlu = "Lütfen Ad Soyad alanını boş bırakmayınız.";
        public const string KullaniciAdiZorunlu = "Lütfen kullanıcı adı belirleyiniz.";
        public const string SifreZorunlu = "Lütfen bir şifre belirleyiniz.";

        // --- SİSTEM MESAJLARI ---
        public const string KullaniciMevcut = "Bu kullanıcı adı zaten sistemde kayıtlı. Lütfen başka bir tane seçin.";
        public const string GirisBasarisiz = "Kullanıcı adı veya şifre hatalı!";
        public const string GirisBos = "Kullanıcı adı ve şifre alanları boş bırakılamaz.";
        public const string CalisanOnayBekliyor = "Hesabınız henüz işletme sahibi tarafından onaylanmamış.";
        public const string KayitBasarili = "Kaydınız başarıyla tamamlandı. Sisteme giriş yapabilirsiniz.";
        public const string KayitBasariliCalisan = "Çalışan kaydı oluşturuldu. Sisteme girmek için işletme sahibinin onayı bekleniyor.";
    }
}