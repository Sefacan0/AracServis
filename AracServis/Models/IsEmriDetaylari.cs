namespace AracServis.Models;

public partial class IsEmriDetaylari
{
    public int DetayId { get; set; }

    public int? IsEmriId { get; set; }

    public string YapilanIslem { get; set; } = null!;

    public decimal Fiyat { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual IsEmirleri? IsEmri { get; set; }
}
