using MafiaMMORPG.Application.Configuration;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Infrastructure.Jobs;
using MafiaMMORPG.Infrastructure.Repositories;
using MafiaMMORPG.Infrastructure.Services;
using MafiaMMORPG.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

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
            .AddStackExchangeRedis(configuration["Redis:ConnectionString"] ?? "redis:6379");

        // Redis Connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;    // dev için önerilir
            return ConnectionMultiplexer.Connect(options);
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Default") ?? "Host=postgres;Database=mmrpg;Username=postgres;Password=postgres", tags: new[] { "ready" })
            .AddRedis(configuration["Redis:ConnectionString"] ?? "redis:6379", tags: new[] { "ready" });

        // Problem Details
        services.AddProblemDetails();

        // Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Auth endpoints - stricter limits
            options.AddPolicy("AuthPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // PvP endpoints - medium limits
            options.AddPolicy("PvPPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

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

        // Hosted Services
        services.AddHostedService<DbReadyHostedService>();

        // Repository Pattern
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IMatchmakingService, MatchmakingService>();
        services.AddScoped<ICombatService, CombatService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<ISeasonService, SeasonService>();
   

        // Quartz Jobs
        services.AddQuartz(q =>
        {
            // Season Close Job
            var seasonCloseJobKey = new JobKey("SeasonCloseJob");
            q.AddJob<SeasonCloseJob>(opts => opts.WithIdentity(seasonCloseJobKey));

            q.AddTrigger(t => t
                .ForJob(seasonCloseJobKey)
                .WithIdentity("SeasonCloseTrigger")
                .WithSchedule(CronScheduleBuilder
                    .CronSchedule(configuration["Quartz:SeasonsCloseCron"] ?? "0 59 23 L * ?")
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"))
                    .WithMisfireHandlingInstructionFireAndProceed()));

            // Match Cleanup Job
            var matchCleanupJobKey = new JobKey("MatchCleanupJob");
            q.AddJob<MatchCleanupJob>(opts => opts.WithIdentity(matchCleanupJobKey));

            q.AddTrigger(t => t
                .ForJob(matchCleanupJobKey)
                .WithIdentity("MatchCleanupTrigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .RepeatForever()));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
