using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using controller.Models.Auth;
using controller.Services.Auth;
var builder = WebApplication.CreateBuilder(args);

// Configuration JWT depuis appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Configuration JwtSettings manquante dans appsettings.json");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Pokemon API", Version = "v1" });
    
    // Configuration Swagger pour JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Autorisation JWT utilisant le schéma Bearer. Exemple: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddPokemonServices();
builder.Services.AddPokemonController();

// Enregistrement des services d'authentification
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IAuthService, AuthService>();

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Pour le développement, mettre true en production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // Pas de tolérance de temps
    };

    // Événements pour le logging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Échec de l'authentification JWT: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var username = context.Principal?.Identity?.Name;
            logger.LogInformation("Token JWT validé pour l'utilisateur: {Username}", username);
            return Task.CompletedTask;
        }
    };
});

// Configuration de l'autorisation
builder.Services.AddAuthorization(options =>
{
    // Politique par défaut : utilisateur authentifié
    options.FallbackPolicy = options.DefaultPolicy;
    
    // Politiques personnalisées
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TrainerOnly", policy => policy.RequireRole("Trainer", "Admin"));
});

// Enregistrer les ActionFilters pour l'injection de dépendance
builder.Services.AddScoped<ExecutionTimeActionFilter>();
builder.Services.AddScoped<ActionStatisticsFilter>();

builder.Services.AddResponseCaching();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

// Ajouter les middlewares de tracing AVANT les autres middlewares
app.UseExecutionTimeTracing();
app.UseRequestStatistics();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}
    app.UseSwagger();
    app.UseSwaggerUI();
app.UseResponseCompression();
app.UseResponseCaching();


app.UseHttpsRedirection();
app.UseRouting();

// Middleware d'authentification et d'autorisation (ordre important!)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
