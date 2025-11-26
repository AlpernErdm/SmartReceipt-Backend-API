using Microsoft.OpenApi.Models;
using SmartReceipt.API.Middleware;
using SmartReceipt.Application;
using SmartReceipt.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SmartReceipt API",
        Description = "AI Destekli Fiş Okuma ve Finans Takip API'si",
        Contact = new OpenApiContact
        {
            Name = "SmartReceipt Team",
            Email = "info@smartreceipt.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartReceipt API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseExceptionHandling();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Logger.LogInformation("SmartReceipt API başlatılıyor...");
app.Logger.LogInformation("Swagger UI: {Url}", app.Environment.IsDevelopment() ? "https://localhost:5001" : "Disabled");

app.Run();
