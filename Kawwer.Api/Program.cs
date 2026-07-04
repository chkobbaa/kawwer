using System.Text;
using Kawwer.Api.Middleware;
using Kawwer.Api.Realtime;
using Microsoft.AspNetCore.StaticFiles;
using Kawwer.Application;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Infrastructure;
using Kawwer.Infrastructure.Identity;
using Kawwer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----- Services -----
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// Development fallback signing key. Must be set BEFORE AddInfrastructure binds JwtOptions so
// token generation (JwtTokenGenerator) and validation share the same key. Override in production.
if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
{
    builder.Configuration["Jwt:SigningKey"] = "kawwer-development-signing-key-change-me-please-32+";
}

// Development fallback VAPID key pair for Web Push (the PWA). Set BEFORE AddInfrastructure binds
// WebPushOptions. VAPID keys must stay stable (they are baked into every browser subscription), so
// unlike a random secret these are a fixed pair. Override BOTH in production via configuration.
if (string.IsNullOrWhiteSpace(builder.Configuration["WebPush:PublicKey"]))
{
    builder.Configuration["WebPush:PublicKey"] =
        "BG6303GrR2S6Dr1ywiWQoWJ98QTaHI9AvOGizadFNGNTaSVTGwLLvNPt_PZs1PTcrlp1aLYkK-WI3VrvTys_4DU";
}

if (string.IsNullOrWhiteSpace(builder.Configuration["WebPush:PrivateKey"]))
{
    builder.Configuration["WebPush:PrivateKey"] = "s2kuX3_NjHUIRQo9pICitX5Ruzj00O5rP97FbgXcqaQ";
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// IRealtimeNotifier is backed by SignalR, which lives in the API host.
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

// ----- Authentication -----
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Allow the SignalR hub to authenticate via the access_token query string.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials());
});

// ----- Swagger / OpenAPI -----
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Kawwer API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT access token.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ----- Pipeline -----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect to HTTPS outside development. The mobile app talks plain HTTP to the dev host,
// and a 307 redirect would strip the Authorization header from authenticated requests (401s).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Serve the installable PWA (Kawwer.Api/wwwroot): index.html at "/", plus the service worker,
// manifest and icons. The web app uses hash-based routing, so no SPA fallback is needed.
var pwaContentTypes = new FileExtensionContentTypeProvider();
pwaContentTypes.Mappings[".webmanifest"] = "application/manifest+json";
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = pwaContentTypes });

app.UseCors();

// Serve uploaded assets (e.g. profile pictures under /uploads/avatars).
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MatchHub>("/hubs/match");
app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

// Apply migrations on startup. Logged (not fatal) so the host still boots if the DB is unavailable.
await ApplyMigrationsAsync(app);

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    if (app.Configuration.GetValue("Database:AutoMigrate", true) is false)
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<KawwerDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations on startup.");
    }
}

// Exposed so the integration test host (WebApplicationFactory) can reference the entry point.
public partial class Program;
