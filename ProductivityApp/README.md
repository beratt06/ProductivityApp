# ProductivityApp

Web tabanlı kişisel verimlilik ve zaman yönetimi uygulaması. Proje ASP.NET Core Razor Pages ile C# olarak geliştirilmiştir ve veriler SQLite veritabanında saklanır.

## Kurulum ve Çalıştırma

Bu bilgisayarda .NET SDK sistem geneline kurulu değildi. Admin şifresi gerektirmemesi için .NET 8 SDK proje kökündeki `.dotnet` klasörüne yerel olarak kuruldu.

```bash
cd "/Users/karatay/Documents/Görsel programlama/ProductivityApp"
DOTNET_CLI_HOME="$PWD/../.dotnet_home" NUGET_PACKAGES="$PWD/.nuget/packages" ../.dotnet/dotnet build
ASPNETCORE_ENVIRONMENT=Development DOTNET_CLI_HOME="$PWD/../.dotnet_home" NUGET_PACKAGES="$PWD/.nuget/packages" ../.dotnet/dotnet bin/Debug/net8.0/ProductivityApp.dll
```

Uygulama adresi:

```text
http://127.0.0.1:5080
```

Demo kullanıcı:

```text
Kullanıcı adı: admin
Şifre: admin123
```

Veritabanı ilk çalıştırmada otomatik oluşur:

```text
ProductivityApp/app.db
```

## Özellikler

- Kullanıcı kayıt ve giriş sistemi
- Dashboard özet kartları
- Görev ekleme, listeleme, güncelleme, silme, arama ve filtreleme
- Son silinen görevi geri alma
- Alışkanlık ekleme, silme ve günlük tamamlandı işaretleme
- Alışkanlık serileri için alev görseli ve küçük animasyonlar
- Pomodoro/odak zamanlayıcısı, özel dakika girişi ve seans kaydı
- Günlük ve haftalık odak süresi özeti
- Responsive koyu tema arayüz

## Ders Kriterleri

- Ana form yapısı: Dashboard ana panel, Görevler, Alışkanlıklar ve Odak modülleri
- Form nesneleri: `form`, `input`, `select`, `button`, `table`, tarih alanı
- Temel sınıflar: `User`, `TaskItem`, `Habit`, `HabitCompletion`, `FocusSession`
- Collection yapıları:
  - `List<T>`: görev, alışkanlık ve seans listeleri
  - `Dictionary<string, int>`: dashboard sayaçları
  - `Stack<int>`: son silinen görevi geri alma
- Eventler:
  - Razor form submit işlemleri
  - JavaScript `click`, `input`, `mouseenter`, `mouseleave`
  - Pomodoro için `setInterval`
- Veri saklama yöntemi: EF Core + SQLite

## Test Notları

Doğrulanan senaryolar:

- `dotnet build` başarılı
- Login sonrası dashboard 200 dönüyor
- Görev ekleme çalışıyor
- Alışkanlık ekleme çalışıyor
- Pomodoro özel dakika ve seans kaydı çalışıyor
- Login ve dashboard ekranları görsel olarak kontrol edildi

Not: Sandbox içinde Kestrel port bağlama izni kısıtlı olduğu için Codex çalıştırırken sunucu dış sandbox izniyle başlatıldı. Normal terminalde aynı komut doğrudan çalışır.
