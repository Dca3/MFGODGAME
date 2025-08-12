using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Infrastructure.Services;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Application.Configuration;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Web.Services;
using MafiaMMORPG.Infrastructure.Repositories;

namespace MafiaMMORPG.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<BalanceOptions>(configuration.GetSection("Balance"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        // Identity
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings?.Key ?? "default_key");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings?.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings?.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            // SignalR için JWT token'ı query'den alma
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/duelHub"))
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // SignalR + Redis
        services.AddSignalR(o => o.EnableDetailedErrors = true)
            .AddStackExchangeRedis(configuration["Redis:ConnectionString"]);

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Default"))
            .AddRedis(configuration["Redis:ConnectionString"]);

        // Problem Details
        services.AddProblemDetails();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Mafia MMRPG API", Version = "v1" });
            
            var bearer = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer {token}'"
            };
            
            c.AddSecurityDefinition("Bearer", bearer);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearer, Array.Empty<string>() } });
        });

        // Services
        services.AddScoped<IStatFormulaService, StatFormulaService>();
        services.AddScoped<JwtService>();
        services.AddScoped<SeedService>();

        // Repository Pattern
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IMatchmakingService, MatchmakingService>();
        services.AddScoped<ICombatService, CombatService>();

        return services;
    }
}
