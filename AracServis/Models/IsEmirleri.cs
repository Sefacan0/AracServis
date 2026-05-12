using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    // =========================================================================
    // ENUM DEVRİMİ: İş Emri Durumları
    // =========================================================================
    public enum IsEmriDurumu
    {
        [Display(Name = "Bekliyor")]
        Bekliyor = 1,

        [Display(Name = "İşlemde")]
        Islemde = 2,

        [Display(Name = "Tamamlandı")]
        Tamamlandi = 3,

        [Display(Name = "Teslim Edildi")]
        TeslimEdildi = 4
    }

    public partial class IsEmirleri
    {
        public int IsEmriId { get; set; }

        public int? MusteriId { get; set; }

        [Required(ErrorMessage = "Araç plakası zorunludur.")]
        [StringLength(20)]
        public string AracPlaka { get; set; } = null!;

        public IsEmriDurumu Durum { get; set; } = IsEmriDurumu.Bekliyor;

        // =========================================================
        // İŞTE EKSİK OLAN VE HATAYA SEBEP OLAN BAĞLANTI (LOOKUP) BURASI!
        // =========================================================
        [ForeignKey("Durum")]
        public virtual IsEmriDurum DurumBilgisi { get; set; }

        public DateTime? AcilisTarihi { get; set; }

        public bool? IsDeleted { get; set; }

        public virtual ICollection<IsEmriDetaylari> IsEmriDetaylaris { get; set; } = new List<IsEmriDetaylari>();

        public virtual Kullanicilar? Musteri { get; set; }
    }
}