namespace AracServis.Models;

public partial class IsEmirleriLog
{
    public int LogId { get; set; }

    public int? IsEmriId { get; set; }

    public int? MusteriId { get; set; }

    public string? AracPlaka { get; set; }

    public string? Durum { get; set; }

    public DateTime? AcilisTarihi { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? LogTarihi { get; set; }

    public string? LogTipi { get; set; }
}
