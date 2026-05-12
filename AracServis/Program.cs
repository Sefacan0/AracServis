using AracServis.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. SESSION SERVİSİ: UI tarafında (Layout'ta) isim yazdırmak veya anlık mesajlar için hala gerekli
builder.Services.AddSession();

// ==============================================================
// 🌟 PROFESYONEL KİMLİK DOĞRULAMA VE HATA YÖNLENDİRME 🌟
// ==============================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";              // Giriş yapmamış biri gizli sayfaya girerse
        options.AccessDeniedPath = "/Error/AccessDenied";  // BÜYÜK DOKUNUŞ: Yetkisi olmayan biri (Çalışan) girerse 403 Hata sayfasına at!
        options.ExpireTimeSpan = TimeSpan.FromHours(12);   // Oturum 12 saat sonra otomatik kapansın
    });

// Veritabanımızı sisteme tanıtıyoruz
builder.Services.AddDbContext<AracServisDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Sistem çökerse özel 500 veya Index sayfasına gönder
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

// BÜYÜK DOKUNUŞ: 404 Sayfa Bulunamadı gibi durum kodlarını Error Controller'ın Index'ine yönlendir!
app.UseStatusCodePagesWithReExecute("/Error/Index");

app.UseHttpsRedirection();
app.UseRouting();

// 2. SESSION KULLANIMINI AKTİF ET
app.UseSession();

// ==============================================================
// ⚠️ HAYATİ SIRALAMA: Önce Kimlik Sor, Sonra Yetkiye Bak!
// ==============================================================
app.UseAuthentication(); // SİSTEM: "Dur bakalım, kimsin sen? Biletini (Cookie) göster."
app.UseAuthorization();  // SİSTEM: "Tamam içeri girdin, ama bu odaya girmeye yetkin var mı?"

app.MapStaticAssets();

// SİTENİN İLK AÇILIŞ SAYFASI:
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();