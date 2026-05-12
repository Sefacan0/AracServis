using Microsoft.EntityFrameworkCore;

namespace AracServis.Models;

public partial class AracServisDbContext : DbContext
{
    public AracServisDbContext() { }

    public AracServisDbContext(DbContextOptions<AracServisDbContext> options)
        : base(options) { }

    public virtual DbSet<IsEmirleri> IsEmirleris { get; set; }
    public virtual DbSet<IsEmirleriLog> IsEmirleriLogs { get; set; }
    public virtual DbSet<IsEmriDetaylari> IsEmriDetaylaris { get; set; }
    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }
    public virtual DbSet<Roller> Rollers { get; set; }
    public virtual DbSet<SistemAyarlari> SistemAyarlaris { get; set; }
    public virtual DbSet<Randevu> Randevular { get; set; }
    public virtual DbSet<Yorum> Yorumlars { get; set; }
    public virtual DbSet<IsEmriDurum> IsEmriDurumlari { get; set; }
    public virtual DbSet<RandevuDurum> RandevuDurumlari { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=AracServisDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. İŞ EMİRLERİ TABLOSU
        modelBuilder.Entity<IsEmirleri>(entity =>
        {
            entity.HasKey(e => e.IsEmriId);
            entity.ToTable("IsEmirleri", tb => tb.HasTrigger("TRG_IsEmirleri_Yedekle"));
            entity.Property(e => e.IsEmriId).HasColumnName("IsEmriID");
            entity.Property(e => e.AcilisTarihi).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.AracPlaka).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Durum).HasDefaultValue(IsEmriDurumu.Bekliyor);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MusteriId).HasColumnName("MusteriID");

            entity.HasOne(d => d.Musteri).WithMany(p => p.IsEmirleris).HasForeignKey(d => d.MusteriId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.DurumBilgisi).WithMany().HasForeignKey(d => d.Durum).HasPrincipalKey(p => p.DurumId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        // 2. İŞ EMİRLERİ LOG TABLOSU
        modelBuilder.Entity<IsEmirleriLog>(entity =>
        {
            entity.HasKey(e => e.LogId);
            entity.ToTable("IsEmirleri_Log");
            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.LogTarihi).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
        });

        // 3. İŞ EMRİ DETAYLARI TABLOSU
        modelBuilder.Entity<IsEmriDetaylari>(entity =>
        {
            entity.HasKey(e => e.DetayId);
            entity.ToTable("IsEmriDetaylari");
            entity.Property(e => e.Fiyat).HasColumnType("decimal(18, 2)");
            entity.HasOne(d => d.IsEmri).WithMany(p => p.IsEmriDetaylaris).HasForeignKey(d => d.IsEmriId);
        });

        // 4. KULLANICILAR TABLOSU
        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId);
            entity.ToTable("Kullanicilar");
            entity.HasIndex(e => e.KullaniciAdi).IsUnique();
            entity.Property(e => e.Sifre).HasMaxLength(255).IsRequired();
            entity.HasOne(d => d.Rol).WithMany(p => p.Kullanicilars).HasForeignKey(d => d.RolId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        // 5. ROLLER TABLOSU
        modelBuilder.Entity<Roller>(entity =>
        {
            entity.HasKey(e => e.RolId);
            entity.ToTable("Roller");
        });

        // 6. RANDEVULAR TABLOSU (HATA BURADAN KAYNAKLIYDI)
        modelBuilder.Entity<Randevu>(entity =>
        {
            entity.HasKey(e => e.RandevuId);
            entity.ToTable("Randevular");

            entity.Property(e => e.AracPlaka).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SikayetAciklamasi).HasMaxLength(150).IsRequired();

            // BURASI ÖNEMLİ: Sütun tipini açıkça INT olarak zorluyoruz
            entity.Property(e => e.Durum)
                  .HasColumnType("int")
                  .HasDefaultValue(RandevuDurumu.OnayBekliyor);

            entity.Property(e => e.TalepTarihi).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            entity.HasOne(d => d.Musteri).WithMany().HasForeignKey(d => d.MusteriId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.SorumluCalisan).WithMany().HasForeignKey(d => d.SorumluCalisanId).OnDelete(DeleteBehavior.ClientSetNull);

            // İlişkiyi sayısal ID üzerinden kuruyoruz
            entity.HasOne(d => d.DurumBilgisi)
                  .WithMany()
                  .HasForeignKey(d => d.Durum)
                  .HasPrincipalKey(p => p.DurumId)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // SEED DATA
        modelBuilder.Entity<IsEmriDurum>().HasData(
            new IsEmriDurum { DurumId = IsEmriDurumu.Bekliyor, DurumAdi = "Bekliyor" },
            new IsEmriDurum { DurumId = IsEmriDurumu.Islemde, DurumAdi = "İşlemde" },
            new IsEmriDurum { DurumId = IsEmriDurumu.Tamamlandi, DurumAdi = "Tamamlandı" },
            new IsEmriDurum { DurumId = IsEmriDurumu.TeslimEdildi, DurumAdi = "Teslim Edildi" }
        );

        modelBuilder.Entity<RandevuDurum>().HasData(
            new RandevuDurum { DurumId = RandevuDurumu.OnayBekliyor, DurumAdi = "Onay Bekliyor" },
            new RandevuDurum { DurumId = RandevuDurumu.Islemde, DurumAdi = "İşlemde (Garaja Alındı)" },
            new RandevuDurum { DurumId = RandevuDurumu.Reddedildi, DurumAdi = "Reddedildi" }
        );

        // GLOBAL FILTERS
        modelBuilder.Entity<IsEmirleri>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<IsEmriDetaylari>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Kullanicilar>().HasQueryFilter(e => e.IsDeleted == false);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}