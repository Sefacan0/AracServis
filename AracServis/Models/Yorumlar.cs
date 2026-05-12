using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    [Table("Yorumlar")]
    public class Yorum
    {
        [Key]
        public int YorumId { get; set; }

        public int MusteriId { get; set; }

        [Required]
        public int IsEmriId { get; set; } // Hangi işlem/araç için yorum yapıldı?

        [Required(ErrorMessage = "Lütfen bir yorum yazın.")]
        [StringLength(500, ErrorMessage = "Yorumunuz en fazla 500 karakter olabilir.")]
        public string YorumMetni { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Puan { get; set; } // Yıldız sistemi için

        public DateTime YorumTarihi { get; set; } = DateTime.Now;

        // İŞLETME SAHİBİ CEVABI İÇİN ALANLAR
        public string? IsletmeCevabi { get; set; }
        public DateTime? CevapTarihi { get; set; }

        // Küfür/Hakaret kontrolü için yönetici onayından geçmeden ana ekranda yayınlanmasın
        public bool OnaylandiMi { get; set; } = false;

        [ForeignKey("MusteriId")]
        public virtual Kullanicilar? Musteri { get; set; }

        [ForeignKey("IsEmriId")]
        public virtual IsEmirleri? IsEmri { get; set; }
    }
}