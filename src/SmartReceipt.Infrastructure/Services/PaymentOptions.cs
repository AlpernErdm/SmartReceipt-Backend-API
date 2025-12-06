namespace SmartReceipt.Infrastructure.Services;

public class PaymentOptions
{
    public const string SectionName = "PaymentSettings";

    public string IyzicoApiKey { get; set; } = string.Empty;
    
    public string IyzicoSecretKey { get; set; } = string.Empty;
    
    public string IyzicoBaseUrl { get; set; } = "https://api.iyzipay.com";
    
    public bool IyzicoUseSandbox { get; set; } = true;
    
    public string StripeApiKey { get; set; } = string.Empty;
    
    public string StripeSecretKey { get; set; } = string.Empty;
    
    public string StripeWebhookSecret { get; set; } = string.Empty;
    
    public string PayPalClientId { get; set; } = string.Empty;
    
    public string PayPalClientSecret { get; set; } = string.Empty;
    
    public bool PayPalUseSandbox { get; set; } = true;
    
    public string DefaultCurrency { get; set; } = "TRY";
}

