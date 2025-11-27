# SmartReceipt Backend API

AI destekli fiş okuma ve finans takip sistemi - Backend API

## 🚀 Teknolojiler

- **.NET 9.0** - Modern C# framework
- **ASP.NET Core Web API** - RESTful API
- **JWT Authentication** - Bearer token ile kimlik doğrulama
- **Entity Framework Core** - ORM
- **PostgreSQL** - Veritabanı
- **MediatR** - CQRS pattern
- **FluentValidation** - Validasyon
- **BCrypt.Net** - Password hashing
- **Google Gemini AI** - Fiş görsel analizi

## 📁 Proje Yapısı (Clean Architecture)

```
backend/
├── src/
│   ├── SmartReceipt.API/          # API Layer (Controllers, Middleware)
│   ├── SmartReceipt.Application/  # Application Layer (CQRS, DTOs, Validators)
│   ├── SmartReceipt.Domain/       # Domain Layer (Entities, Enums)
│   └── SmartReceipt.Infrastructure/# Infrastructure Layer (Database, AI Services)
└── SmartReceipt.sln
```

## 🛠️ Kurulum

### Gereksinimler
- .NET 9.0 SDK
- PostgreSQL 14+
- Google Gemini API Key

### Adımlar

1. **Bağımlılıkları yükleyin:**
```bash
cd backend
dotnet restore
```

2. **Yapılandırma dosyasını oluşturun:**

```bash
cd src/SmartReceipt.API
cp appsettings.Example.json appsettings.json
```

3. **API Key ve veritabanı bilgilerini girin:**

`src/SmartReceipt.API/appsettings.json` dosyasını düzenleyin:
- `YOUR_PASSWORD_HERE` yerine PostgreSQL şifrenizi
- `YOUR_API_KEY_HERE` yerine Gemini API key'inizi yazın

**🔑 Gemini API Key nasıl alınır?**
https://aistudio.google.com/app/apikey

**⚠️ ÖNEMLİ:** `appsettings.json` dosyasını asla GitHub'a yüklemeyin!

3. **Veritabanı migration'larını çalıştırın:**
```bash
cd src/SmartReceipt.Infrastructure
dotnet ef database update --startup-project ../SmartReceipt.API
```

4. **Uygulamayı çalıştırın:**
```bash
cd src/SmartReceipt.API
dotnet run
```

API şu adreste çalışacak: `https://localhost:5001`
Swagger UI: `https://localhost:5001`

## 📡 API Endpoints

### Authentication (🔓 Public)

- `POST /api/Auth/register` - Yeni kullanıcı kaydı
- `POST /api/Auth/login` - Kullanıcı girişi (JWT token döner)
- `POST /api/Auth/refresh-token` - Access token yenileme
- `GET /api/Auth/me` 🔒 - Mevcut kullanıcı bilgisi
- `POST /api/Auth/logout` 🔒 - Kullanıcı çıkışı

### Receipts (🔒 Authorization Required)

- `GET /api/Receipts` - Tüm fişleri listele (filtreleme ve sayfalama ile)
- `GET /api/Receipts/{id}` - ID'ye göre fiş detayı
- `POST /api/Receipts/scan` - Fiş görselini AI ile tara ve kaydet
- `POST /api/Receipts` - Manuel fiş oluştur

### Health Check (🔓 Public)
- `GET /health` - Sistem sağlık kontrolü

**Not:** 🔒 işaretli endpoint'ler için `Authorization: Bearer {token}` header'ı gereklidir.

## 🏗️ Mimari Prensipler

- **Clean Architecture** - Katmanlı mimari
- **CQRS Pattern** - Command/Query ayrımı (MediatR)
- **Repository Pattern** - Veri erişim soyutlaması
- **Dependency Injection** - Gevşek bağlı bileşenler
- **Validation Pipeline** - FluentValidation ile otomatik validasyon
- **JWT Authentication** - Token-based kimlik doğrulama
- **Authorization** - Role-based ve claim-based yetkilendirme

## 🔧 Geliştirme

### Migration Oluşturma
```bash
cd src/SmartReceipt.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../SmartReceipt.API
```

### Build
```bash
dotnet build
```

### Test
```bash
dotnet test
```

## 📦 Deployment

### Docker ile Çalıştırma
```bash
docker build -t smartreceipt-backend .
docker run -p 5001:5001 smartreceipt-backend
```

## 📝 Lisans

MIT License

