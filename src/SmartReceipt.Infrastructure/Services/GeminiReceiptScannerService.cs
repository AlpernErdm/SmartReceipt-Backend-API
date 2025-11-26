using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Infrastructure.Services;

public class GeminiReceiptScannerService : IAiReceiptScannerService
{
    private readonly OpenAiOptions _options;
    private readonly ILogger<GeminiReceiptScannerService> _logger;
    private readonly HttpClient _httpClient;

    public GeminiReceiptScannerService(
        IOptions<OpenAiOptions> options,
        ILogger<GeminiReceiptScannerService> logger,
        HttpClient httpClient)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClient;
        
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ReceiptScanResultDto> ScanReceiptAsync(IFormFile imageFile, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fiş tarama işlemi başlatılıyor (Gemini). Dosya: {FileName}, Boyut: {FileSize} bytes", 
                imageFile.FileName, imageFile.Length);

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();
            var base64Image = Convert.ToBase64String(imageBytes);
            
            var mimeType = imageFile.ContentType ?? "image/jpeg";

            _logger.LogDebug("Görsel Base64'e çevrildi. MimeType: {MimeType}, Base64 uzunluğu: {Length}", 
                mimeType, base64Image.Length);

            return await ScanReceiptFromBase64Async(base64Image, mimeType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fiş tarama sırasında hata oluştu");
            return new ReceiptScanResultDto
            {
                IsSuccess = false,
                ErrorMessage = $"Fiş tarama hatası: {ex.Message}"
            };
        }
    }

    public Task<ReceiptScanResultDto> ScanReceiptFromBase64Async(string base64Image, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<ReceiptScanResultDto> ScanReceiptFromBase64Async(
        string base64Image, 
        string mimeType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gemini API'ye istek gönderiliyor...");

            var apiUrl = $"https://generativelanguage.googleapis.com/v1/models/{_options.Model}:generateContent?key={_options.ApiKey}";

            var fullPrompt = GetSystemPrompt() + "\n\n" + GetUserPrompt();

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                text = fullPrompt
                            },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = mimeType,
                                    data = base64Image
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = _options.Temperature,
                    maxOutputTokens = _options.MaxTokens
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("Gemini API isteği hazırlandı. Model: {Model}, MaxTokens: {MaxTokens}", 
                _options.Model, _options.MaxTokens);

            var response = await _httpClient.PostAsync(apiUrl, httpContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API hatası. StatusCode: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"Gemini API hatası ({response.StatusCode}): {errorContent}"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Gemini API yanıtı alındı: {Response}", responseJson);

            var apiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Candidates == null || apiResponse.Candidates.Count == 0)
            {
                _logger.LogWarning("Gemini API'den geçersiz yanıt alındı");
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "API'den geçerli yanıt alınamadı"
                };
            }

            var firstCandidate = apiResponse.Candidates[0];
            if (firstCandidate.Content?.Parts == null || firstCandidate.Content.Parts.Count == 0)
            {
                _logger.LogWarning("Gemini yanıtında içerik bulunamadı");
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "API yanıtında içerik yok"
                };
            }

            var messageContent = firstCandidate.Content.Parts[0].Text;
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogWarning("Gemini yanıtı boş");
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "API yanıtı boş"
                };
            }

            _logger.LogDebug("Fiş verisi JSON: {Json}", messageContent);

            var cleanedJson = messageContent.Trim();
            if (cleanedJson.StartsWith("```json"))
            {
                cleanedJson = cleanedJson.Substring(7);
            }
            else if (cleanedJson.StartsWith("```"))
            {
                cleanedJson = cleanedJson.Substring(3);
            }
            
            if (cleanedJson.EndsWith("```"))
            {
                cleanedJson = cleanedJson.Substring(0, cleanedJson.Length - 3);
            }
            
            cleanedJson = cleanedJson.Trim();

            var result = JsonSerializer.Deserialize<ReceiptScanResultDto>(cleanedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                _logger.LogError("JSON deserialize hatası");
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "JSON parse edilemedi"
                };
            }

            result.IsSuccess = true;
            result.RawOcrText = messageContent;

            _logger.LogInformation("Fiş başarıyla tarandı (Gemini). Mağaza: {StoreName}, Toplam: {TotalAmount}, Ürün Sayısı: {ItemCount}", 
                result.StoreName, result.TotalAmount, result.Items.Count);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP isteği sırasında hata oluştu");
            return new ReceiptScanResultDto
            {
                IsSuccess = false,
                ErrorMessage = $"HTTP hatası: {ex.Message}"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON işleme hatası");
            return new ReceiptScanResultDto
            {
                IsSuccess = false,
                ErrorMessage = $"JSON hatası: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısı sırasında beklenmeyen hata oluştu");
            return new ReceiptScanResultDto
            {
                IsSuccess = false,
                ErrorMessage = $"Beklenmeyen hata: {ex.Message}"
            };
        }
    }

    private static string GetSystemPrompt()
    {
        return """
            Sen uzman bir fiş okuma asistanısın. Gönderilen fiş görselini analiz et. 
            Mağaza adı, tarih, toplam tutar, KDV ve kalemleri çıkar. 
            
            Önemli Kurallar:
            - Eğer tarih okunamazsa bugünün tarihini ver
            - Para birimini tespit et (TL, TRY, USD, EUR vb.)
            - Tüm fiyatları ondalık sayı olarak ver (örn: 25.50)
            - Ürünleri genel kategorilere ayır: Gıda, İçecek, Temizlik, Kişisel Bakım, Kırtasiye, Elektronik, Giyim, Ev, Diğer
            - KDV tutarını ayrıca belirt
            - Tarih formatı: ISO 8601 (YYYY-MM-DDTHH:mm:ss)
            
            Cevabı SADECE ve SADECE saf JSON formatında ver. 
            Markdown kullanma.
            """;
    }

    private static string GetUserPrompt()
    {
        return """
            Bu fiş görselini analiz et ve aşağıdaki JSON formatında döndür:
            
            {
                "storeName": "Mağaza Adı",
                "receiptDate": "2024-11-26T14:30:00",
                "totalAmount": 150.50,
                "taxAmount": 12.50,
                "currency": "TRY",
                "items": [
                    {
                        "productName": "Ürün Adı",
                        "quantity": 2,
                        "unitPrice": 25.00,
                        "totalPrice": 50.00,
                        "category": "Gıda"
                    }
                ]
            }
            
            Para birimi için TRY, USD, EUR gibi ISO 4217 kodlarını kullan.
            Sadece JSON döndür, başka hiçbir açıklama ekleme.
            """;
    }
}

#region Gemini API Response Models

internal class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate> Candidates { get; set; } = new();

    [JsonPropertyName("usageMetadata")]
    public GeminiUsageMetadata? UsageMetadata { get; set; }
}

internal class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

internal class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = new();

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

internal class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

internal class GeminiUsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; set; }

    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; set; }

    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; set; }
}

#endregion

