using System.ComponentModel.DataAnnotations;
namespace AracServis.Models;

public partial class Kullanicilar
{
    public int KullaniciId { get; set; }

    [Required(ErrorMessage = SabitMesajlar.AdSoyadZorunlu)]
    public string AdSoyad { get; set; } = null!;

    [Required(ErrorMessage = SabitMesajlar.KullaniciAdiZorunlu)]
    [StringLength(20, MinimumLength = 4, ErrorMessage = SabitMesajlar.KullaniciAdiKisa)]
    public string KullaniciAdi { get; set; } = null!;

    [Required(ErrorMessage = SabitMesajlar.SifreZorunlu)]
    [StringLength(20, MinimumLength = 6, ErrorMessage = SabitMesajlar.SifreKisa)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W_]).+$", ErrorMessage = SabitMesajlar.SifreKurali)]
    public string Sifre { get; set; } = null!;

    public int? RolId { get; set; }
    public bool? IsApproved { get; set; }
    public bool? IsDeleted { get; set; }
    public string? OnayKodu { get; set; }

    // BÜYÜK DOKUNUŞ: Patronun red sebebini tutacak alan!
    public string? RedNedeni { get; set; }

    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();
    public virtual Roller? Rol { get; set; }
}