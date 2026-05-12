namespace AracServis.Models
{
    /// <summary>
    /// Yönetici ve Çalışan ana sayfasında (Dashboard) 
    /// tüm verileri tek bir seferde göstermek için kullanılan model.
    /// </summary>
    public class DashboardViewModel
    {
        // Atölyedeki iş emirlerinin sayısal özetleri
        public int BekleyenSayisi { get; set; }
        public int IslemdeSayisi { get; set; }
        public int TamamlananSayisi { get; set; }
        public int TeslimEdilenSayisi { get; set; }

        // Hesaplanan Özellik: Şu an dükkanda fiziksel olarak bulunan araç sayısı
        // (Teslim edilenler hariç tutulur)
        public int ToplamAktifArac => BekleyenSayisi + IslemdeSayisi + TamamlananSayisi;

        // Dashboard üzerindeki işletme ismini ve ayarları dinamik çekmek için
        public SistemAyarlari? Ayarlar { get; set; }

        // Müşterilerin evinden/telefonundan gönderdiği ve henüz onay bekleyen randevu talepleri
        // Bu liste sayesinde Dashboard "Canlı" bir komuta merkezine dönüşür.
        public List<Randevu> BekleyenTalepler { get; set; } = new List<Randevu>();
    }
}