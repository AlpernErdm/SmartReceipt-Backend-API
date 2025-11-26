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

public class OpenAiReceiptScannerService : IAiReceiptScannerService
{
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiReceiptScannerService> _logger;
    private readonly HttpClient _httpClient;
    private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAiReceiptScannerService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiReceiptScannerService> logger,
        HttpClient httpClient)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClient;
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ReceiptScanResultDto> ScanReceiptAsync(IFormFile imageFile, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fiş tarama işlemi başlatılıyor. Dosya: {FileName}, Boyut: {FileSize} bytes", 
                imageFile.FileName, imageFile.Length);

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();
            var base64Image = Convert.ToBase64String(imageBytes);
            
            var contentType = imageFile.ContentType ?? "image/jpeg";
            var dataUrl = $"data:{contentType};base64,{base64Image}";

            _logger.LogDebug("Görsel Base64'e çevrildi. ContentType: {ContentType}, Base64 uzunluğu: {Length}", 
                contentType, base64Image.Length);

            return await ScanReceiptFromBase64Async(dataUrl, cancellationToken);
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

    public async Task<ReceiptScanResultDto> ScanReceiptFromBase64Async(string base64Image, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("OpenAI API'ye istek gönderiliyor...");

            var systemPrompt = GetSystemPrompt();
            var userPrompt = GetUserPrompt();

            var requestBody = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = userPrompt
                            },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = base64Image
                                }
                            }
                        }
                    }
                },
                max_tokens = _options.MaxTokens,
                temperature = _options.Temperature,
                response_format = new
                {
                    type = "json_object"
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("API isteği hazırlandı. Model: {Model}, MaxTokens: {MaxTokens}", 
                _options.Model, _options.MaxTokens);

            var response = await _httpClient.PostAsync(OpenAiApiUrl, httpContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI API hatası. StatusCode: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"OpenAI API hatası ({response.StatusCode}): {errorContent}"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("OpenAI API yanıtı alındı: {Response}", responseJson);

            var apiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Choices == null || apiResponse.Choices.Count == 0)
            {
                _logger.LogWarning("OpenAI API'den geçersiz yanıt alındı");
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "API'den geçerli yanıt alınamadı"
                };
            }

            var messageContent = apiResponse.Choices[0].Message?.Content;
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogWarning("OpenAI yanıtı boş");
                return new ReceiptScanResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "API yanıtı boş"
                };
            }

            _logger.LogDebug("Fiş verisi JSON: {Json}", messageContent);

            var result = JsonSerializer.Deserialize<ReceiptScanResultDto>(messageContent, new JsonSerializerOptions
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

            _logger.LogInformation("Fiş başarıyla tarandı. Mağaza: {StoreName}, Toplam: {TotalAmount}, Ürün Sayısı: {ItemCount}", 
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
            _logger.LogError(ex, "OpenAI API çağrısı sırasında beklenmeyen hata oluştu");
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
            Markdown kullanma (```json ... ``` yapma).
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

#region OpenAI API Response Models

internal class OpenAiResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAiChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; set; }
}

internal class OpenAiChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAiMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

internal class OpenAiMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

internal class OpenAiUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

#endregion
