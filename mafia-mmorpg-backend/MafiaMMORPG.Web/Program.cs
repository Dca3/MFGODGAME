using MafiaMMORPG.Web.Extensions;
using MafiaMMORPG.Web.Endpoints;
using MafiaMMORPG.Web.Hubs;
using MafiaMMORPG.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration loading
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Docker URL binding
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Add all services
builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(); // ProblemDetails
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health/live", new() { Predicate = _ => true });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

// Map all endpoints
app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapInventoryEndpoints();
app.MapQuestEndpoints();
app.MapPvpEndpoints();
app.MapLeaderboardEndpoints();
app.MapAdminEndpoints();

// SignalR Hub
app.MapHub<DuelHub>("/duelHub");

app.Run();

// Make Program class public for testing
public partial class Program { }
