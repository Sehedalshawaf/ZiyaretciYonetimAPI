using Microsoft.EntityFrameworkCore;

namespace ZiyaretciAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ZiyaretciDb"));
        builder.Services.AddScoped<ZiyaretciService>();

        var app = builder.Build();
        if (app.Environment.IsDevelopment()) app.MapOpenApi();
        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapPost("/api/ziyaretciler", async (ZiyaretciService service, Ziyaretci ziyaretci) =>
        {
            var result = await service.ZiyaretciEkleAsync(ziyaretci);
            return result.Success
                ? Results.Created($"/api/ziyaretciler/{ziyaretci.Id}", ziyaretci)
                : Results.BadRequest(result.ErrorMessage);
        });

        app.MapPost("/api/ziyaretciler/{id}/giris", async (ZiyaretciService service, int id, string kapi) =>
        {
            var result = await service.GirisYapAsync(id, kapi);
            return result.Success ? Results.Ok("Giriş kaydı oluşturuldu.") : Results.BadRequest(result.ErrorMessage);
        });

        app.MapPost("/api/ziyaretciler/{id}/cikis", async (ZiyaretciService service, int id) =>
        {
            var result = await service.CikisYapAsync(id);
            return result.Success ? Results.Ok("Çıkış kaydı oluşturuldu.") : Results.BadRequest(result.ErrorMessage);
        });

        app.MapGet("/api/ziyaretciler/iceridekiler", async (ZiyaretciService service) =>
            Results.Ok(await service.IceridekileriGetirAsync()));

        app.MapGet("/api/istatistik/kapi-ziyaretleri", async (ZiyaretciService service, DateTime baslangic, DateTime bitis) =>
        {
            if (baslangic > bitis)
                return Results.BadRequest("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
            var istatistik = await service.KapiZiyaretIstatistikleriAsync(baslangic, bitis);
            return Results.Ok(istatistik);
        });

        app.Run();
    }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Ziyaretci> Ziyaretciler => Set<Ziyaretci>();
    public DbSet<GirisKaydi> GirisKayitlari => Set<GirisKaydi>();
}

public class Ziyaretci
{
    public int Id { get; set; }
    public string KimlikNumarasi { get; set; } = string.Empty;
    public string TamAd { get; set; } = string.Empty;
    public string Sirket { get; set; } = string.Empty;
}

public class GirisKaydi
{
    public int Id { get; set; }
    public int ZiyaretciId { get; set; }
    public string Kapi { get; set; } = string.Empty;
    public DateTime GirisZamani { get; set; } = DateTime.UtcNow;
    public DateTime? CikisZamani { get; set; }
}

public class IceridekiZiyaretciDto
{
    public int ZiyaretciId { get; set; }
    public string TamAd { get; set; } = string.Empty;
    public string Kapi { get; set; } = string.Empty;
    public DateTime GirisZamani { get; set; }
    public string IcerideKalmaSuresi { get; set; } = string.Empty;
}


