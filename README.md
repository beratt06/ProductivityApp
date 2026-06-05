# ⚡ ProductivityApp — Üretkenlik ve Alışkanlık Takip Portalı

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)](https://dotnet.microsoft.com/download)
[![Database](https://img.shields.io/badge/Database-SQLite-blue.svg)](https://www.sqlite.org/)
[![UI Style](https://img.shields.io/badge/UI-Glassmorphism-orange.svg)](#)

ProductivityApp; kişilerin zaman yönetimini optimize etmelerini, hedeflerini planlamalarını, yeni alışkanlıklar kazanmalarını ve Pomodoro tekniğiyle odaklanma sürelerini ölçerek verimliliklerini artırmalarını sağlayan modern bir kişisel asistan portalıdır.

Cam şeffaflığı efekti sunan **Glassmorphism** stili koyu temalı arayüzü ile kullanıcı dostu bir deneyim sağlar.

---

## ✨ Özellikler ve Modüller

*   **🔐 Güvenli Oturum Yönetimi:** Sunucu tabanlı Session mekanizması ile kimlik doğrulama. Şifreler `PBKDF2` algoritması ile tuzlanarak hash'lenir.
*   **🔔 Akıllı Hatırlatıcı Paneli:** Son teslim tarihi yarın olan görevleri ve bugün henüz tamamlanmamış alışkanlıkları Dashboard üzerinde otomatik olarak uyarır.
*   **📝 Görev Yönetimi (Tasks):** Görev ekleme, listeleme, arama ve kategorilere göre filtreleme. Sayfa yenilenmeden durum/öncelik güncellemeleri (AJAX).
*   **🔄 İşlem Geri Alma (Undo):** Silinen görevlerin bellek içi bir LIFO Yığın (Stack) üzerinden tek tıkla geri alınabilmesi.
*   **🏃 Alışkanlık Takibi (Habits):** Emojili alışkanlık kartları oluşturma, haftalık hedef frekansı ve ardışık gün (Streak) hesaplama algoritması.
*   **⏱️ Pomodoro Sayacı (Focus Mode):** JavaScript tabanlı 25 dk çalışma / 5 dk mola zamanlayıcısı. Seans bittiğinde asenkron veritabanı kaydı.
*   **📊 İstatistik & Raporlama (Stats):** Chart.js ile odaklanma analitiği grafikleri ve son 30 günün alışkanlık tamamlama yoğunluğunu gösteren **Alışkanlık Isı Haritası (Heatmap)**.

---

## 🛠️ Mimari & Kullanılan Yapılar

*   **ASP.NET Core 8.0 Razor Pages (MVVM):** Sayfa tabanlı backend (`.cshtml.cs`) ve frontend (`.cshtml`) tasarımı.
*   **Entity Framework Core & SQLite:** İlişkisel veritabanı modeli, otomatik veri besleme (`DatabaseSeeder`) ve Cascade silme ilişkileri.
*   **Asenkron Haberleşme (Fetch API):** Kullanıcı etkileşimlerini sayfa yenilemeden sunucuya işleyen AJAX mimarisi.
*   **Dependency Injection (DI):** Servis yaşam döngülerinin (Scoped/Singleton) yönetimi.

---

## 🚀 Başlatma ve Çalıştırma

Uygulamayı yerel bilgisayarınızda çalıştırmak için iki yöntem kullanabilirsiniz:

### Yöntem 1: Dotnet CLI (Terminal) ile Çalıştırma
Bilgisayarınızda [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) kurulu olmalıdır.

```bash
# Proje dizinine gidin
cd ProductivityApp

# Bağımlılıkları geri yükleyin ve derleyin
dotnet restore
dotnet build

# Uygulamayı başlatın
dotnet run
```

Uygulama çalıştıktan sonra tarayıcınızdan şu adrese gidin:
👉 **[http://127.0.0.1:5080](http://127.0.0.1:5080)**

### Yöntem 2: Derlenmiş `.exe` Dosyası ile Çalıştırma (Kurulumsuz)
1.  Proje ana dizinindeki `ProductivityApp.zip` dosyasını bir klasöre ayıklayın.
2.  `ProductivityApp.exe` dosyasına çift tıklayarak çalıştırın.
3.  Tarayıcınızdan **`http://127.0.0.1:5080`** adresine gidin.

---

## 🔑 Demo Giriş Bilgileri

Uygulama ilk kez çalıştırıldığında veritabanı (`app.db`) otomatik olarak oluşturulur ve test verileriyle beslenir. Giriş için aşağıdaki bilgileri kullanabilirsiniz:

*   **Kullanıcı Adı:** `admin`
*   **Şifre:** `admin123`

---

## 📄 Akademik Rapor Oluşturma

Projeyle ilgili hocaya teslim edilecek Word (.docx) rapor şablonunu markdown dosyasından otomatik üretmek isterseniz:

1. Gerekli Python kütüphanesini yükleyin:
   ```bash
   pip install -r requirements.txt
   ```
2. Rapor oluşturma betiğini çalıştırın:
   ```bash
   python ProductivityApp/docs/convert_md_to_docx.py
   ```
3. Oluşan `proje_raporu_hazir.docx` dosyasını açıp kendi kişisel bilgilerinizi doldurarak çıktısını alabilirsiniz.
