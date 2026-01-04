| Bilgi | Detay |
| :--- | :--- |
| **Ad Soyad** | Selameddin Tirit |
| **Öğrenci No** | 240541035 |
| **Bölüm** | Yazılım Mühendisliği (A) |
| **Fakülte** | Teknoloji Fakültesi |




# 🏦 FaceID - Banka Tipi Yüz Tanıma Sistemi

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![OpenCV](https://img.shields.io/badge/OpenCV-5C3EE8?style=for-the-badge&logo=opencv&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)

> **Görüntü İşleme, Matematiksel Analiz ve Makine Öğrenmesi Tekniklerini Birleştiren Güvenli Biyometrik Kimlik Doğrulama Sistemi**

---

## 📋 Proje Özeti

FaceID, bankacılık sektörü standartlarında güvenlik sağlayan, .NET 8 ve EmguCV (OpenCV) teknolojileri ile geliştirilmiş profesyonel bir yüz tanıma sistemidir. Bu proje, sadece yüz tanıma yapmakla kalmayıp; **Haar Cascade** algoritması ile gerçek zamanlı yüz algılama, **LBPH (Local Binary Patterns Histograms)** makine öğrenmesi algoritması ile biyometrik tanıma ve **Matris Matematiği** tabanlı görüntü işleme tekniklerini birleştirerek kurumsal düzeyde bir güvenlik çözümü sunmaktadır.

Sistem, **State Machine (Durum Makinesi)** yapısı ile çoklu açılı yüz kayıt süreci, **Repository Pattern** ile veritabanı soyutlaması, **Service-Oriented Architecture** ile modüler mimari ve **SOLID** prensiplerine uygun temiz kod yapısı içermektedir.

---

## ✨ Temel Özellikler

### 🔍 Gerçek Zamanlı Yüz Algılama
- **Haar Cascade** algoritması kullanılarak yüksek performanslı yüz tespiti
- OpenCV'nin optimize edilmiş cascade classifier'ı ile milisaniyeler içinde algılama
- Çoklu yüz algılama desteği

### 🎯 Akıllı Kayıt Sihirbazı (State Machine)
- **Durum Makinesi (State Machine)** yapısı ile yapılandırılmış kayıt süreci
- Üç aşamalı profil kaydı:
  - **LookingFront:** Düz bakış açısından 5 fotoğraf
  - **LookingRight:** Sağa bakış açısından 5 fotoğraf
  - **LookingLeft:** Sola bakış açısından 5 fotoğraf
- Her kullanıcı için toplam **15 fotoğraf** ile kapsamlı veri seti oluşturma
- Otomatik durum geçişleri ve görsel geri bildirim

### 🧠 Biyometrik Tanıma (LBPH Algoritması)
- **LBPH (Local Binary Patterns Histograms)** makine öğrenmesi algoritması
- Işık değişimlerine karşı dayanıklı yerel ikili desen analizi
- Eğitimli model ile gerçek zamanlı kimlik doğrulama
- Ayarlanabilir threshold değeri ile hassas tanıma kontrolü
- Matris tabanlı görüntü işleme ile standartlaştırılmış veri seti

### 🗄️ Kurumsal Veritabanı Entegrasyonu
- **PostgreSQL** ve **Npgsql** ile güvenli veritabanı bağlantısı
- Connection pooling ile performans optimizasyonu
- Repository Pattern ile veri erişim katmanı soyutlaması
- Parametreli sorgular ile SQL injection koruması
- Otomatik tablo oluşturma ve seed data yönetimi

### 🏗️ Temiz Mimari ve Tasarım Desenleri
- **Service-Oriented Architecture (SOA)** ile modüler yapı
- **Repository Pattern** ile veri erişim soyutlaması
- **SOLID Prensipleri:**
  - Single Responsibility: Her servis tek bir sorumluluğa sahip
  - Open/Closed: Genişlemeye açık, değişikliğe kapalı
  - Liskov Substitution: Arayüz uyumluluğu
  - Interface Segregation: Minimal arayüz bağımlılıkları
  - Dependency Inversion: Üst seviye modüller alt seviye modüllere bağımlı değil

### ⚡ Performans ve Güvenilirlik
- **Thread-safe UI** güncellemeleri (Async/Await, BeginInvoke)
- Login debounce mekanizması ile aşırı API çağrılarının önlenmesi
- Frame interval optimizasyonu (her 10 frame'de bir tanıma)
- Memory leak önleme (IDisposable pattern)
- Hata yönetimi ve graceful degradation

---

## 🛠️ Teknoloji Yığını

| Kategori | Teknoloji | Versiyon | Açıklama |
|----------|-----------|----------|----------|
| **Framework** | .NET | 8.0 | Modern, yüksek performanslı çalışma zamanı |
| **UI Framework** | Windows Forms | - | Masaüstü uygulama arayüzü |
| **Görüntü İşleme** | Emgu.CV | 4.9.0.5494 | OpenCV'nin .NET wrapper'ı |
| **Makine Öğrenmesi** | Emgu.CV.Face | 4.9.0.5494 | Yüz tanıma algoritmaları (LBPH) |
| **Veritabanı** | PostgreSQL | - | İlişkisel veritabanı yönetim sistemi |
| **ORM/Bağlantı** | Npgsql | 10.0.1 | PostgreSQL .NET provider |
| **Algoritma** | Haar Cascade | - | Yüz algılama algoritması |
| **Algoritma** | LBPH | - | Local Binary Patterns Histograms |

---

## 📦 Kurulum ve Gereksinimler

### Sistem Gereksinimleri

- **İşletim Sistemi:** Windows 10/11 (x64 veya ARM64)
- **.NET Runtime:** .NET 8.0 SDK veya üzeri
- **IDE:** Visual Studio 2022 (önerilen) veya Visual Studio Code
- **Veritabanı:** PostgreSQL 12 veya üzeri
- **Kamera:** USB webcam veya entegre kamera

### Adım 1: Projeyi Klonlayın

```bash
git clone https://github.com/kullaniciadi/FaceID.git
cd FaceID
```

### Adım 2: PostgreSQL Veritabanını Hazırlayın

1. PostgreSQL'in kurulu olduğundan emin olun
2. PostgreSQL'e bağlanın ve bir veritabanı oluşturun:

```sql
CREATE DATABASE FaceID_DB;
```

### Adım 3: Veritabanı Bağlantı String'ini Güncelleyin

`FaceID/DatabaseService.cs` dosyasındaki bağlantı string'ini kendi PostgreSQL ayarlarınıza göre güncelleyin:

```18:18:FaceID/DatabaseService.cs
    private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=postgres;Database=FaceID_DB";
```

**⚠️ ÖNEMLİ:** `Password` kısmını kendi PostgreSQL şifrenizle değiştirin!

### Adım 4: Haar Cascade Dosyasını Yerleştirin

`haarcascade_frontalface_default.xml` dosyasını şu konuma kopyalayın:

```
FaceID/bin/Debug/net8.0-windows/haarcascade_frontalface_default.xml
```

**Dosyayı nereden bulabilirsiniz?**
- OpenCV resmi repository: [GitHub - opencv/data/haarcascades](https://github.com/opencv/opencv/tree/master/data/haarcascades)
- Emgu.CV örnek projeleri
- Alternatif: Proje içinde `bin/Debug/net8.0-windows/` klasöründe zaten mevcut olabilir

### Adım 5: NuGet Paketlerini Yükleyin

Projeyi Visual Studio'da açtığınızda NuGet paketleri otomatik olarak geri yüklenecektir. Manuel yükleme için:

```bash
dotnet restore
```

Veya Visual Studio'da: **Tools > NuGet Package Manager > Restore NuGet Packages**

### Adım 6: Projeyi Derleyin ve Çalıştırın

```bash
dotnet build
dotnet run --project FaceID/FaceID.csproj
```

Veya Visual Studio'da **F5** tuşuna basarak projeyi çalıştırın.

---

## 🚀 Kullanım

### Yüz Kaydı Yapma

1. Uygulamayı başlatın
2. **"Kamerayı Başlat"** butonuna tıklayın
3. **"Kayıt Başlat"** butonuna tıklayın
4. Ekrandaki talimatları takip edin:
   - **Adım 1:** Kameraya düz bakın (5 fotoğraf otomatik çekilecek)
   - **Adım 2:** Kafanızı hafifçe sağa çevirin (5 fotoğraf otomatik çekilecek)
   - **Adım 3:** Kafanızı hafifçe sola çevirin (5 fotoğraf otomatik çekilecek)
5. Kayıt tamamlandığında sistem otomatik olarak modeli eğitecektir

**Not:** Her kullanıcı için toplam 15 fotoğraf kaydedilir ve `TrainedFaces/` klasörüne `User_{ID}_{Sıra}.bmp` formatında kaydedilir.

### Yüz Tanıma ile Giriş Yapma

1. Uygulama başlatıldığında, eğer `TrainedFaces/` klasöründe eğitim verisi varsa model otomatik olarak eğitilir
2. Kamerayı başlatın
3. Kameraya bakın
4. Sistem yüzünüzü algıladığında ve tanıdığında otomatik olarak giriş yapacaktır
5. Başarılı giriş sonrası **Dashboard** ekranı açılacaktır

### Dashboard

Dashboard ekranında:
- Kullanıcı bilgileri (ID, İsim, Bakiye, Rol)
- Gerçek zamanlı kamera görüntüsü
- Çıkış yapma özelliği

---

## 📁 Proje Yapısı

```
FaceID/
├── FaceID/
│   ├── CameraService.cs              # Kamera yakalama servisi
│   ├── FaceDetectionService.cs       # Haar Cascade yüz algılama
│   ├── FaceRecognitionService.cs     # LBPH yüz tanıma servisi
│   ├── FaceRegistrationService.cs    # State Machine kayıt servisi
│   ├── DatabaseService.cs            # PostgreSQL bağlantı yönetimi
│   ├── UserRepository.cs             # Repository Pattern veri erişimi
│   ├── User.cs                       # Kullanıcı veri modeli
│   ├── RegistrationState.cs          # State Machine durum enum'u
│   ├── Form1.cs                      # Ana form (UI katmanı)
│   ├── DashboardForm.cs              # Dashboard formu
│   ├── Program.cs                    # Uygulama giriş noktası
│   └── FaceID.csproj                 # Proje dosyası
├── docs/
│   └── sprint-reports/               # Sprint durum raporları
│       ├── SPRINT1_DURUM_RAPORU.md
│       ├── SPRINT2_DURUM_RAPORU.md
│       ├── SPRINT3_DURUM_RAPORU.md
│       └── SPRINT4_DURUM_RAPORU.md
└── README.md                         # Bu dosya
```

---

## 🔬 Teknik Detaylar

### Haar Cascade Yüz Algılama

**Haar Cascade**, Viola-Jones algoritmasına dayanan, makine öğrenmesi tabanlı bir nesne algılama yöntemidir. Sistem, önceden eğitilmiş `haarcascade_frontalface_default.xml` dosyasını kullanarak görüntülerdeki yüzleri tespit eder.

**Parametreler:**
- Scale Factor: 1.1 (her ölçekte %10 artış)
- Min Neighbors: 5 (doğruluk için minimum komşu sayısı)
- Min Size: 30x30 piksel (minimum yüz boyutu)

### LBPH Yüz Tanıma Algoritması

**LBPH (Local Binary Patterns Histograms)**, yerel ikili desen histogramları kullanarak yüz tanıma yapan bir algoritmadır.

**Algoritma Parametreleri:**
- `radius: 1` - LBP yarıçapı
- `neighbors: 8` - Komşu piksel sayısı
- `gridX: 8` - X ekseni grid sayısı
- `gridY: 8` - Y ekseni grid sayısı
- `threshold: 100.0` - Tanıma eşik değeri (Distance <= 100 ise tanındı)

**Eğitim Süreci:**
1. `TrainedFaces/` klasöründeki tüm görüntüler okunur
2. Her görüntü grayscale'e çevrilir ve 100x100 piksele yeniden boyutlandırılır
3. Dosya adından kullanıcı ID'si çıkarılır (`User_{ID}_{Sıra}.bmp` formatı)
4. LBPH modeli eğitilir

**Tanıma Süreci:**
1. Yüz görüntüsü algılanır
2. Görüntü işlenir (grayscale, 100x100 boyutlandırma)
3. `Predict()` metodu ile tahmin yapılır
4. Distance değeri threshold ile karşılaştırılır
5. Distance <= 100 ise kullanıcı tanındı kabul edilir

### State Machine (Durum Makinesi) Yapısı

Kayıt süreci, **Finite State Machine** yapısı ile yönetilir:

```
Idle → LookingFront → LookingRight → LookingLeft → Completed → Idle
```

Her durumda 5 fotoğraf çekilir ve durum otomatik olarak bir sonraki duruma geçer.

---

## 🧪 Test ve Doğrulama

Sistem, aşağıdaki senaryolarda test edilmiştir:

- ✅ Çoklu kullanıcı kaydı ve tanıma
- ✅ Farklı ışık koşullarında çalışma
- ✅ Hızlı yüz hareketlerine karşı dayanıklılık
- ✅ Veritabanı bağlantı hatalarında graceful degradation
- ✅ Eksik dosya durumlarında uygulama çökmesi önleme
- ✅ Thread-safe UI güncellemeleri

---

## 📝 Lisans

Bu proje eğitim ve araştırma amaçlı geliştirilmiştir. Kullanım ve dağıtım hakları proje sahibine aittir.

---

## 👨‍💻 Geliştirici Notları

### Kod Standartları

- Tüm kod açıklamaları Türkçe yapılmıştır
- SOLID prensiplerine uygun mimari kullanılmıştır
- Gereksiz yorumlar eklenmemiş, sadece önemli kısımlara açıklama yapılmıştır
- Kod çalışır durumda ve eksik bırakılmamıştır

### Performans Notları

- Frame interval optimizasyonu ile CPU kullanımı minimize edilmiştir
- Connection pooling ile veritabanı bağlantıları optimize edilmiştir
- Memory leak önleme mekanizmaları uygulanmıştır

### Güvenlik Notları

- Tüm SQL sorguları parametreli olarak yapılmıştır (SQL injection koruması)
- Connection string'ler kod içinde sabitlenmiştir (production'da app.config kullanılmalı)
- Veritabanı şifreleri kod içinde saklanmamalıdır

---

## 🤝 Katkıda Bulunma

1. Bu repository'yi fork edin
2. Feature branch oluşturun (`git checkout -b feature/YeniOzellik`)
3. Değişikliklerinizi commit edin (`git commit -am 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/YeniOzellik`)
5. Pull Request oluşturun

---

## 📧 İletişim

Sorularınız veya önerileriniz için issue açabilirsiniz.

---

**⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!**

