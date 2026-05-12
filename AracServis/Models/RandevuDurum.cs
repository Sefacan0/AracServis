using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    [Table("RandevuDurumlari")]
    public class RandevuDurum
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Otomatik artmasın, sabit (1,2,3) vereceğiz
        // BÜYÜK DOKUNUŞ: int yerine kendi Enum türümüzü yazdık!
        public RandevuDurumu DurumId { get; set; }

        [Required]
        [StringLength(50)]
        public string DurumAdi { get; set; } = null!;
    }
}