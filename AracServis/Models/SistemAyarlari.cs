using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Tablo ismi eşlemesi için şart!

namespace AracServis.Models
{
    [Table("SistemAyarlari")] // SQL'deki tablo adıyla birebir eşliyoruz
    public class SistemAyarlari
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İşletme adı boş bırakılamaz.")]
        [StringLength(100)]
        public string IsletmeAdi { get; set; } = null!;

        public string? LogoYolu { get; set; }

        [StringLength(20)]
        public string? ArkaPlanRengi { get; set; }

        [StringLength(20)]
        public string? WpNumarasi { get; set; }

        public DateTime? GuncellemeTarihi { get; set; }
    }
}