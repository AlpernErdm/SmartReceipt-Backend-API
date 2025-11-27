using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartReceipt.API.Middleware;
using SmartReceipt.Application;
using SmartReceipt.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication yapılandırması
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret key is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Development için
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Debug logging için events ekle
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>()
                .LogError("Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>()
                .LogInformation("Token validated for user: {UserId}", 
                    context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>()
                .LogWarning("Authentication challenge: {Error}", context.Error);
            return Task.CompletedTask;
        }
    };
});

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

    // JWT Authentication için Swagger yapılandırması
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
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

// Authentication middleware'i Authorization'dan önce ve Routing'den sonra ekleyin
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Logger.LogInformation("SmartReceipt API başlatılıyor...");
app.Logger.LogInformation("Swagger UI: {Url}", app.Environment.IsDevelopment() ? "https://localhost:5001" : "Disabled");

app.Run();
