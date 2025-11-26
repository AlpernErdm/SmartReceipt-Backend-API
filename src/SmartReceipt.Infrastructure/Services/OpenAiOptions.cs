namespace SmartReceipt.Infrastructure.Services;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";
    
    public string ApiKey { get; set; } = string.Empty;
    
    public string Model { get; set; } = "gpt-4o";
    
    public int MaxTokens { get; set; } = 4096;
    
    public float Temperature { get; set; } = 0.1f;
}

