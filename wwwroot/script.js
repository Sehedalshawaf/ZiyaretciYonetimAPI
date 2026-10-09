
const mesaj = document.getElementById("mesaj");

function mesajGoster(metin, hata = false) {
    mesaj.textContent = metin;
    mesaj.className = hata ? "hata" : "";
}

async function istekYap(adres, ayarlar = {}) {
    const cevap = await fetch(adres, ayarlar);
    const metin = await cevap.text();

    let veri;
    try {
        veri = JSON.parse(metin);
    } catch {
        veri = metin;
    }

    if (!cevap.ok) {
        throw new Error(
            typeof veri === "string"
                ? veri
                : veri?.detail || veri?.title || "İşlem başarısız."
        );
    }

    return veri;
}

// ZİYARETÇİ KAYDET
document.getElementById("ziyaretciForm")
    .addEventListener("submit", async (event) => {
        event.preventDefault();

        const ziyaretci = {
            kimlikNumarasi: document.getElementById("kimlik").value.trim(),
            tamAd: document.getElementById("ad").value.trim(),
            sirket: document.getElementById("sirket").value.trim()
        };

        try {
            const sonuc = await istekYap("/api/ziyaretciler", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(ziyaretci)
            });

            mesajGoster(`Ziyaretçi kaydedildi. ID: ${sonuc.id}`);
            document.getElementById("ziyaretciForm").reset();
            document.getElementById("ziyaretciId").value = sonuc.id;
        } catch (hata) {
            mesajGoster(hata.message, true);
        }
    });

// GİRİŞ YAP
document.getElementById("girisBtn")
    .addEventListener("click", async () => {
        const id = document.getElementById("ziyaretciId").value;
        const kapi = document.getElementById("kapi").value.trim();

        if (!id || !kapi) {
            mesajGoster("Ziyaretçi ID ve kapı bilgisi gereklidir.", true);
            return;
        }

        try {
            await istekYap(
                `/api/ziyaretciler/${encodeURIComponent(id)}/giris?kapi=${encodeURIComponent(kapi)}`,
                { method: "POST" }
            );

            mesajGoster("Giriş kaydı oluşturuldu.");
            await iceridekileriGetir();
        } catch (hata) {
            mesajGoster(hata.message, true);
        }
    });

// ÇIKIŞ YAP
document.getElementById("cikisBtn")
    .addEventListener("click", async () => {
        const id = document.getElementById("ziyaretciId").value;

        if (!id) {
            mesajGoster("Ziyaretçi ID giriniz.", true);
            return;
        }

        try {
            await istekYap(
                `/api/ziyaretciler/${encodeURIComponent(id)}/cikis`,
                { method: "POST" }
            );

            mesajGoster("Çıkış kaydı oluşturuldu.");
            await iceridekileriGetir();
        } catch (hata) {
            mesajGoster(hata.message, true);
        }
    });

// İÇERİDEKİ ZİYARETÇİLER
async function iceridekileriGetir() {
    const tablo = document.getElementById("iceridekilerTablo");

    try {
        const liste = await istekYap(
            "/api/ziyaretciler/iceridekiler"
        );

        tablo.replaceChildren();

        if (liste.length === 0) {
            const satir = tablo.insertRow();
            const hucre = satir.insertCell();
            hucre.colSpan = 4;
            hucre.textContent = "Şu anda içeride ziyaretçi yok.";
            return;
        }

        liste.forEach(ziyaretci => {
            const satir = tablo.insertRow();

            [
                ziyaretci.ziyaretciId,
                ziyaretci.tamAd,
                ziyaretci.kapi,
                ziyaretci.icerideKalmaSuresi
            ].forEach(deger => {
                const hucre = satir.insertCell();
                hucre.textContent = deger;
            });
        });
    } catch (hata) {
        mesajGoster(hata.message, true);
    }
}

document.getElementById("yenileBtn")
    .addEventListener("click", iceridekileriGetir);

// KAPI İSTATİSTİKLERİ
document.getElementById("istatistikBtn")
    .addEventListener("click", async () => {
        const baslangic = document.getElementById("baslangic").value;
        const bitis = document.getElementById("bitis").value;
        const alan = document.getElementById("istatistikSonuc");

        if (!baslangic || !bitis) {
            mesajGoster("Başlangıç ve bitiş tarihlerini seçiniz.", true);
            return;
        }

        if (baslangic > bitis) {
            mesajGoster("Başlangıç tarihi bitişten sonra olamaz.", true);
            return;
        }

        try {
            const sonuc = await istekYap(
                `/api/istatistik/kapi-ziyaretleri?baslangic=${baslangic}&bitis=${bitis}T23:59:59`
            );

            alan.replaceChildren();

            const kapilar = Object.entries(sonuc);

            if (kapilar.length === 0) {
                alan.textContent = "Bu tarih aralığında ziyaret yok.";
                return;
            }

            kapilar.forEach(([kapi, sayi]) => {
                const satir = document.createElement("p");
                satir.textContent = `${kapi} kapısı: ${sayi} ziyaret`;
                alan.appendChild(satir);
            });

            mesajGoster("İstatistikler getirildi.");
        } catch (hata) {
            mesajGoster(hata.message, true);
        }
    });

// SAYFA AÇILDIĞINDA LİSTEYİ YÜKLE
iceridekileriGetir();
