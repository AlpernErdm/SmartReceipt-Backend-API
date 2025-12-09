# SmartReceipt Backend API

AI destekli fiş okuma, abonelik ve ödeme yönetimi platformu - Backend API

## 🚀 Teknolojiler
- **.NET 9.0**, **ASP.NET Core Web API**
- **JWT Authentication**, **FluentValidation**, **MediatR (CQRS)**
- **Entity Framework Core**, **PostgreSQL**
- **Google Gemini AI** (fiş görsel analizi)
- **Iyzipay SDK** (iyzico ödemeleri, 3D secure/token ile)
- **Mapster** (mapping), **BCrypt.Net** (hashing)

## ⭐ Öne Çıkan Özellikler
- Abonelik planları, kullanım kotaları, iptal/yükseltme
- iyzico ödeme entegrasyonu (token/3DS), ödeme geçmişi, iade
- Gelişmiş analitik: kategori, trend, mağaza, vergi, karşılaştırma
- Raporlama: PDF/Excel/CSV (basit çıktı), bütçe takibi
- Premium: çoklu para birimi, ML kategori önerisi, tekrar/fraud tespiti
- Webhook desteği (ödeme ve abonelik olayları)
- Mobil: `mobile/` içinde React Native başlangıç projesi

## 📁 Proje Yapısı (Clean Architecture)
```
backend/
├── src/
│   ├── SmartReceipt.API/           # API (Controllers, Middleware)
│   ├── SmartReceipt.Application/   # CQRS, DTO, Validators
│   ├── SmartReceipt.Domain/        # Entities, Enums
│   └── SmartReceipt.Infrastructure/# Db, Payments, AI, Services
└── SmartReceipt.sln
```

## 🛠️ Kurulum
### Gereksinimler
- .NET 9.0 SDK, PostgreSQL 14+
- Google Gemini API key
- iyzico sandbox key'leri (PaymentSettings altında)

### Adımlar
1) Bağımlılıklar
```bash
cd backend
dotnet restore
```
2) Konfigürasyon
```bash
cd src/SmartReceipt.API
cp appsettings.Example.json appsettings.json
```
`appsettings.json` içinde doldur:
- `ConnectionStrings:DefaultConnection`
- `JwtSettings` (issuer/audience/key)
- `Gemini:ApiKey`
- `PaymentSettings:Iyzico:{ApiKey,SecretKey,BaseUrl}` (sandbox bilgileri)

3) Veritabanı
```bash
cd src/SmartReceipt.Infrastructure
dotnet ef database update --startup-project ../SmartReceipt.API
```
Abonelik planları startup sırasında otomatik seed edilir.

4) Çalıştırma
```bash
cd ../SmartReceipt.API
dotnet run
```
API: `https://localhost:5001` (Swagger: `/swagger`)

## 📡 API Endpoints (özet)
- **Auth**: `POST /api/Auth/register`, `POST /api/Auth/login`, `POST /api/Auth/refresh-token`, `GET /api/Auth/me`, `POST /api/Auth/logout`
- **Receipts**: `GET /api/Receipts`, `GET /api/Receipts/{id}`, `POST /api/Receipts/scan` (Gemini AI), `POST /api/Receipts` (manuel)
- **Subscriptions**: `GET /api/Subscriptions/plans`, `GET /api/Subscriptions/current`, `GET /api/Subscriptions/usage`, `POST /api/Subscriptions/subscribe`, `POST /api/Subscriptions/cancel`
- **Payments**: `POST /api/Payments` (iyzico token/3DS destekli), `POST /api/Payments/refund`, `GET /api/Payments/{id}`
- **Analytics**: `GET /api/Analytics/category`, `GET /api/Analytics/trends?period=1`, `GET /api/Analytics/stores`, `GET /api/Analytics/tax`, `GET /api/Analytics/comparison`
- **Reports**: `POST /api/Reports/pdf`, `POST /api/Reports/excel`, `POST /api/Reports/csv`
- **Webhooks**: `POST /api/Webhooks/iyzico` (payment/subscription event işleme)
- **Health**: `GET /health`

🔒 Endpoint'ler JWT ister: `Authorization: Bearer {token}`

## 🏗️ Mimari Prensipler
- Clean Architecture, CQRS (MediatR), DI
- Global exception middleware + validation pipeline
- EF Core + konfigurasyon sınıfları + seed

## 🔧 Geliştirme
- Migration: `cd src/SmartReceipt.Infrastructure && dotnet ef migrations add Name --startup-project ../SmartReceipt.API`
- Build: `dotnet build`
- Test: `dotnet test`

## 📦 Deployment
### Docker
```bash
docker build -t smartreceipt-backend .
docker run -p 5001:5001 smartreceipt-backend
```
Gerekli env: `ASPNETCORE_ENVIRONMENT`, `ConnectionStrings__DefaultConnection`, `PaymentSettings__Iyzico__ApiKey`, `PaymentSettings__Iyzico__SecretKey`, `PaymentSettings__Iyzico__BaseUrl`, `Gemini__ApiKey`, `JwtSettings__Key`

## 📱 Mobil
`mobile/` klasöründe React Native başlangıç projesi ve kurulum dokümanı (`mobile/INSTALLATION_GUIDE.md`).

## 📝 Lisans
MIT License

