using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MafiaMMORPG.Infrastructure.Data;
using Npgsql;

namespace MafiaMMORPG.Web.Services;

public class DbReadyHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<DbReadyHostedService> _log;
    private readonly IHostApplicationLifetime _lifetime;

    public DbReadyHostedService(IServiceProvider sp, ILogger<DbReadyHostedService> log, IHostApplicationLifetime lifetime)
    {
        _sp = sp; _log = log; _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var delayMs = new[] { 500, 1000, 2000, 3000, 5000, 8000 };
        for (var i = 0; i < delayMs.Length; i++)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                _log.LogInformation("Checking database connectivity...");
                await db.Database.OpenConnectionAsync(ct);
                await db.Database.CloseConnectionAsync();
                _log.LogInformation("Database reachable.");
                // Migrate → Seed
                await db.Database.MigrateAsync(ct);
                var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
                await seeder.SeedAsync();
                _log.LogInformation("Migration & Seed completed.");
                return;
            }
            catch (Exception ex) when (ex is NpgsqlException || ex is Npgsql.NpgsqlException || ex is TimeoutException)
            {
                _log.LogWarning(ex, "DB not ready yet. Retry in {Delay}ms (attempt {Attempt}/{Total})", delayMs[i], i+1, delayMs.Length);
                await Task.Delay(delayMs[i], ct);
            }
        }

        _log.LogError("Database is not reachable after retries. Stopping application.");
        // İstersen uygulamayı düşür:
        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
