using Microsoft.EntityFrameworkCore;

namespace ZiyaretciAPI;

public class ZiyaretciService
{
    private readonly AppDbContext _context;
    public ZiyaretciService(AppDbContext context) => _context = context;

    public async Task<(bool Success, string? ErrorMessage)> ZiyaretciEkleAsync(Ziyaretci? ziyaretci)
    {
        if (ziyaretci == null) return (false, "Ziyaretçi bilgileri boş olamaz.");
        if (string.IsNullOrWhiteSpace(ziyaretci.KimlikNumarasi)) return (false, "Kimlik numarası zorunludur.");
        if (string.IsNullOrWhiteSpace(ziyaretci.TamAd)) return (false, "Tam ad zorunludur.");
        if (string.IsNullOrWhiteSpace(ziyaretci.Sirket)) return (false, "Şirket bilgisi zorunludur.");
        if (await _context.Ziyaretciler.AnyAsync(z => z.KimlikNumarasi == ziyaretci.KimlikNumarasi))
            return (false, "Bu kimlik numarası zaten kayıtlı.");
        try
        {
            _context.Ziyaretciler.Add(ziyaretci);
            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateException)
        {
            return (false, "Ziyaretçi kaydedilirken veritabanı hatası oluştu.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> GirisYapAsync(int ziyaretciId, string kapi)
    {
        if (ziyaretciId <= 0) return (false, "Geçersiz ziyaretçi ID.");
        if (string.IsNullOrWhiteSpace(kapi)) return (false, "Kapı bilgisi zorunludur.");
        if (await _context.Ziyaretciler.FindAsync(ziyaretciId) == null) return (false, "Ziyaretçi bulunamadı.");
        if (await _context.GirisKayitlari.AnyAsync(g => g.ZiyaretciId == ziyaretciId && g.CikisZamani == null))
            return (false, "Ziyaretçinin zaten açık bir giriş kaydı var.");
        try
        {
            _context.GirisKayitlari.Add(new GirisKaydi
            {
                ZiyaretciId = ziyaretciId,
                Kapi = kapi.Trim(),
                GirisZamani = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateException)
        {
            return (false, "Giriş kaydı oluşturulamadı.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CikisYapAsync(int ziyaretciId)
    {
        if (ziyaretciId <= 0) return (false, "Geçersiz ziyaretçi ID.");
        var acikGiris = await _context.GirisKayitlari
            .FirstOrDefaultAsync(g => g.ZiyaretciId == ziyaretciId && g.CikisZamani == null);
        if (acikGiris == null) return (false, "Bu ziyaretçi için açık giriş kaydı bulunamadı.");
        try
        {
            acikGiris.CikisZamani = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateException)
        {
            return (false, "Çıkış kaydı oluşturulamadı.");
        }
    }

    public async Task<List<IceridekiZiyaretciDto>> IceridekileriGetirAsync()
    {
        var acikKayitlar = await _context.GirisKayitlari.Where(g => g.CikisZamani == null).ToListAsync();
        var sonuc = new List<IceridekiZiyaretciDto>();
        foreach (var kayit in acikKayitlar)
        {
            var ziyaretci = await _context.Ziyaretciler.FindAsync(kayit.ZiyaretciId);
            var sure = DateTime.UtcNow - kayit.GirisZamani;
            sonuc.Add(new IceridekiZiyaretciDto
            {
                ZiyaretciId = kayit.ZiyaretciId,
                TamAd = ziyaretci?.TamAd ?? "Bilinmiyor",
                Kapi = kayit.Kapi,
                GirisZamani = kayit.GirisZamani,
                IcerideKalmaSuresi = $"{(int)sure.TotalHours} saat {sure.Minutes} dakika"
            });
        }
        return sonuc;
    }

    public async Task<Dictionary<string, int>> KapiZiyaretIstatistikleriAsync(DateTime baslangic, DateTime bitis)
    {
        if (baslangic > bitis) return new Dictionary<string, int>();
        // EF Core InMemory, GroupBy + ToDictionaryAsync ifadesini çeviremeyebilir.
        // Veritabanından asenkron çek, ardından bellekte grupla.
        var kayitlar = await _context.GirisKayitlari
            .AsNoTracking()
            .Where(g => g.GirisZamani >= baslangic && g.GirisZamani <= bitis)
            .ToListAsync();
        return kayitlar.GroupBy(g => g.Kapi).ToDictionary(g => g.Key, g => g.Count());
    }
}