# Ziyaretçi Yönetim Web API

ASP.NET Core (.NET 9) ile geliştirilmiş, ziyaretçi kayıtlarını ve giriş-çıkış işlemlerini yöneten bir Web API projesidir. İşlemler basit bir web arayüzü üzerinden de gerçekleştirilebilir.

## Kullanılan Teknolojiler

- C# / ASP.NET Core (.NET 9)
- Entity Framework Core InMemory
- HTML, CSS ve JavaScript
- Asenkron veritabanı işlemleri (async/await)

## Proje Özellikleri

- **Ziyaretçi Kaydı:** Kimlik numarası, tam ad ve şirket bilgileriyle ziyaretçi oluşturulur. Aynı kimlik numarasıyla tekrar kayıt yapılması engellenir.
- **Giriş İşlemi:** Ziyaretçinin giriş yaptığı kapı ve giriş zamanı kaydedilir. Açık giriş kaydı bulunan ziyaretçi tekrar giriş yapamaz.
- **Çıkış İşlemi:** Ziyaretçinin çıkış zamanı kaydedilir. Açık giriş kaydı olmadan çıkış yapılamaz.
- **İçerideki Ziyaretçiler:** İçeride bulunan ziyaretçiler ve içeride kalma süreleri listelenir.
- **Kapı İstatistikleri:** Belirtilen tarih aralığında kapılardan yapılan giriş sayıları hesaplanır.
- **Veri Doğrulama:** Zorunlu alanlar kontrol edilir. Başlangıç tarihinin bitiş tarihinden sonra olması reddedilir.

## Proje Yapısı

- `Program.cs`: API adresleri, uygulama yapılandırması ve veri modelleri.
- `ZiyaretciService.cs`: Ziyaretçi kaydı, giriş-çıkış ve istatistik işlemlerinin iş kuralları.
- `wwwroot/index.html`: Web arayüzünün HTML yapısı.
- `wwwroot/style.css`: Arayüz tasarımı.
- `wwwroot/script.js`: Arayüzün API ile iletişimi.
- `README.md`: Proje açıklamaları ve çalıştırma bilgileri.

## Kurulum ve Çalıştırma

Bilgisayarda **.NET 9 SDK** kurulu olmalıdır.

Proje klasöründe terminal açılarak aşağıdaki komutlar çalıştırılır:

```powershell
dotnet restore
dotnet build
dotnet run
```

Uygulama yerel geliştirme ortamında aşağıdaki adreste çalıştırılmıştır:

`http://localhost:5264`

Bu adres tarayıcıda açıldığında ziyaretçi yönetim arayüzü görüntülenir.

**Not:** Port numarası başlatma ayarlarına göre değişebilir.

## Web Arayüzü

Web arayüzünde aşağıdaki işlemler yapılabilir:

- Yeni ziyaretçi kaydetme
- Ziyaretçi girişi ve çıkışı yapma
- İçerideki ziyaretçileri görüntüleme
- Tarih aralığı seçerek kapı istatistiklerini görüntüleme
- Başarılı ve hatalı işlemlerin mesajlarını görme

## API Adresleri

| Metot | API Adresi | Açıklama |
|---|---|---|
| POST | `/api/ziyaretciler` | Yeni ziyaretçi oluşturur |
| POST | `/api/ziyaretciler/{id}/giris?kapi=A` | Ziyaretçi girişini kaydeder |
| POST | `/api/ziyaretciler/{id}/cikis` | Ziyaretçi çıkışını kaydeder |
| GET | `/api/ziyaretciler/iceridekiler` | İçerideki ziyaretçileri listeler |
| GET | `/api/istatistik/kapi-ziyaretleri` | Kapı giriş istatistiklerini getirir |

Kapı istatistiklerinde `baslangic` ve `bitis` parametreleri kullanılır.

Örnek:

`/api/istatistik/kapi-ziyaretleri?baslangic=2026-10-01&bitis=2026-10-31T23:59:59`

## Örnek Ziyaretçi Kaydı

Ziyaretçi oluşturmak için örnek JSON verisi:

```json
{
  "kimlikNumarasi": "12345678901",
  "tamAd": "Ahmet Yilmaz",
  "sirket": "ABC Teknoloji"
}
```

## Veritabanı ve Önemli Notlar

- Veriler Entity Framework Core InMemory veritabanında tutulur.
- Uygulama yeniden başlatıldığında kayıtlar kaybolur.
- Giriş ve çıkış zamanları UTC olarak kaydedilir.
- Kapı istatistikleri giriş kayıtları üzerinden hesaplanır.
- İş kuralları API adreslerinden ayrı olarak `ZiyaretciService.cs` dosyasında tutulur.

## Yapılan Manuel Testler

API üzerinde PowerShell ile aşağıdaki işlemler test edilmiştir:

- Ziyaretçi oluşturma
- Giriş kaydı oluşturma
- Kapı giriş istatistiklerini görüntüleme

Web arayüzünde aşağıdaki işlemler test edilmiştir:

- Ziyaretçi kaydetme
- Giriş kaydı oluşturma
- Aynı ziyaretçinin ikinci girişini engelleme
- İçerideki ziyaretçileri listeleme
- Çıkış kaydı oluşturma
- Açık giriş kaydı olmadan çıkışı reddetme
- Kapı bazında ziyaret sayılarını görüntüleme

Yapılan manuel testlerde beklenen sonuçlar alınmıştır.