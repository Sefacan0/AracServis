using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    // Enum yapısının arka planda int çalışacağını garanti ediyoruz
    public enum RandevuDurumu : int
    {
        [Display(Name = "Onay Bekliyor")]
        OnayBekliyor = 1,

        [Display(Name = "İşlemde (Garaja Alındı)")]
        Islemde = 2,

        [Display(Name = "Reddedildi")]
        Reddedildi = 3
    }

    [Table("Randevular")]
    public class Randevu
    {
        [Key]
        public int RandevuId { get; set; }

        public int MusteriId { get; set; }

        [Required(ErrorMessage = "Araç plakası zorunludur.")]
        [StringLength(20)]
        [RegularExpression(@"^(0[1-9]|[1-7][0-9]|8[0-1])\s?[a-zA-Z]{1,3}\s?[0-9]{2,4}$", ErrorMessage = "Lütfen geçerli bir Türkiye plakası giriniz. (Örn: 34 ABC 123)")]
        public string AracPlaka { get; set; } = null!;

        [Required(ErrorMessage = "Şikayet açıklaması zorunludur.")]
        [StringLength(150, ErrorMessage = "Şikayet metni en fazla 150 karakter olabilir.")]
        public string SikayetAciklamasi { get; set; } = null!;

        public string? ArizaGorselYolu { get; set; }

        public DateTime TalepTarihi { get; set; } = DateTime.Now;

        // TİP: RandevuDurumu (int altyapılı)
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.OnayBekliyor;

        [ForeignKey("Durum")]
        public virtual RandevuDurum? DurumBilgisi { get; set; }

        public int? SorumluCalisanId { get; set; }

        [ForeignKey("MusteriId")]
        public virtual Kullanicilar? Musteri { get; set; }

        [ForeignKey("SorumluCalisanId")]
        public virtual Kullanicilar? SorumluCalisan { get; set; }
    }
}