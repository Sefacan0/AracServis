using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    [Table("IsEmriDurumlari")]
    public class IsEmriDurum
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        // BÜYÜK DOKUNUŞ: int yerine kendi Enum türümüzü yazdık
        public IsEmriDurumu DurumId { get; set; }

        [Required]
        [StringLength(50)]
        public string DurumAdi { get; set; } = null!;
    }
}