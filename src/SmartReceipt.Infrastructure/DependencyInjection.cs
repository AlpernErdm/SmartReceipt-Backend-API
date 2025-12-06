using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Infrastructure.Persistence;
using SmartReceipt.Infrastructure.Services;

namespace SmartReceipt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("SmartReceipt.Infrastructure");
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
            
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        });

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<OpenAiOptions>(
            configuration.GetSection(OpenAiOptions.SectionName));
        
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.AddHttpClient<IAiReceiptScannerService, GeminiReceiptScannerService>();
        
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUsageLimitService, UsageLimitService>();
        
        services.Configure<PaymentOptions>(
            configuration.GetSection(PaymentOptions.SectionName));
        services.AddScoped<IPaymentService, IyzicoPaymentService>();
        
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        
        services.AddScoped<IReportService, ReportService>();
        
        services.AddScoped<IWebhookService, WebhookService>();
        
        services.AddScoped<ICurrencyService, CurrencyService>();
        
        services.AddScoped<IMlService, MlService>();
        
        services.AddScoped<IStorageService, StorageService>();

        return services;
    }
}