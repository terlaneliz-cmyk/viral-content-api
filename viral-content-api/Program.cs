using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ViralContentApi.Data;
using ViralContentApi.Middleware;
using ViralContentApi.Models;
using ViralContentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
var jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
var jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Viral Content API",
        Version = "v1",
        Description = "API for managing viral content posts."
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Enter your API key in the x-api-key header.",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        },
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
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SendGridEmailSettings>(
    builder.Configuration.GetSection("NotificationProviders:SendGrid"));

builder.Services.Configure<StripeBillingSettings>(
    builder.Configuration.GetSection("StripeBilling"));

builder.Services.Configure<StripeUrlsSettings>(
    builder.Configuration.GetSection("StripeUrls"));

builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAiContentService, AiContentService>();
builder.Services.AddScoped<AiTopicProfileBuilder>();
builder.Services.AddScoped<AiStrategyProfileBuilder>();
builder.Services.AddScoped<AiProfileTextBuilder>();
builder.Services.AddScoped<AiEngagementTextBuilder>();
builder.Services.AddScoped<AiContentFormatter>();
builder.Services.AddScoped<AiHookBuilder>();
builder.Services.AddScoped<AiBodyBuilder>();
builder.Services.AddScoped<IAiUsageLimitService, AiUsageLimitService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminOpsService, AdminOpsService>();
builder.Services.AddScoped<IPlansService, PlansService>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
builder.Services.AddScoped<IStripeWebhookApplicationService, StripeWebhookApplicationService>();
builder.Services.AddScoped<IProcessedWebhookEventService, ProcessedWebhookEventService>();
builder.Services.AddScoped<IWebhookEventLogService, WebhookEventLogService>();
builder.Services.AddScoped<IBillingHealthService, BillingHealthService>();
builder.Services.AddScoped<INotificationHealthService, NotificationHealthService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<IBillingEventLogService, BillingEventLogService>();
builder.Services.AddScoped<BillingNotificationOrchestrator>();
builder.Services.Configure<WebhookMaintenanceSettings>(
    builder.Configuration.GetSection("WebhookMaintenance"));

builder.Services.AddScoped<IWebhookMaintenanceService, WebhookMaintenanceService>();
builder.Services.AddHostedService<WebhookMaintenanceBackgroundService>();
builder.Services.AddHostedService<SubscriptionExpirationBackgroundService>();

var notificationProvider = builder.Configuration["NotificationProvider:Provider"] ?? "Stub";

if (notificationProvider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IBillingEmailService, SendGridBillingEmailService>();
}
else
{
    builder.Services.AddScoped<IBillingEmailService, StubBillingEmailService>();
}

var billingProvider = builder.Configuration["BillingProvider:Provider"] ?? "Stub";

if (billingProvider.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IBillingProviderService, StripeBillingProviderService>();
}
else
{
    builder.Services.AddScoped<IBillingProviderService, StubBillingProviderService>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("OK"));

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}