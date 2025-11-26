# 🔐 Güvenlik Yapılandırması

## ⚠️ ÖNEMLİ UYARI

Bu proje hassas bilgiler içerir. Lütfen aşağıdaki adımları takip edin:

## 📋 Kurulum Adımları

1. **appsettings.json Oluşturun**

```bash
cd src/SmartReceipt.API
cp appsettings.Example.json appsettings.json
```

2. **Kendi Bilgilerinizi Girin**

`appsettings.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SmartReceiptDb;Username=postgres;Password=GERÇEK_ŞİFRENİZ"
  },
  "OpenAI": {
    "ApiKey": "GERÇEK_GEMİNİ_API_KEY",
    "Model": "gemini-1.5-flash",
    "MaxTokens": 4096,
    "Temperature": 0.1
  }
}
```

## 🔑 API Key Nasıl Alınır?

### Google Gemini API Key

1. https://aistudio.google.com/app/apikey adresine gidin
2. Google hesabınızla giriş yapın
3. "Create API Key" butonuna tıklayın
4. Oluşturulan API key'i kopyalayın
5. `appsettings.json` dosyasına yapıştırın

## 🚫 Asla GitHub'a Yüklemeyin

- ❌ `appsettings.json` (gerçek key'ler)
- ❌ `appsettings.Development.json` (gerçek key'ler)
- ❌ `.env` dosyaları
- ✅ `appsettings.Example.json` (örnek template)

## 🛡️ Güvenlik Kontrol Listesi

- [ ] `appsettings.json` .gitignore'da
- [ ] API key değiştirildi (eğer yanlışlıkla paylaşıldıysa)
- [ ] Veritabanı şifresi güçlü
- [ ] Production'da farklı şifreler kullanıldı
- [ ] Environment variables kullanıldı (production için)

## 📧 Güvenlik Sorunu Bildirimi

Bir güvenlik açığı bulursanız, lütfen public issue açmak yerine direkt benimle iletişime geçin.

