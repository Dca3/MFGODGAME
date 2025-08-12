using MafiaMMORPG.Web.Extensions;
using MafiaMMORPG.Web.Endpoints;
using MafiaMMORPG.Web.Hubs;
using MafiaMMORPG.Web.Services;

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthentication();
app.UseAuthorization();

// Health check
app.MapHealthChecks("/health");

// Map all endpoints
app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapInventoryEndpoints();
app.MapQuestEndpoints();
app.MapPvpEndpoints();
app.MapLeaderboardEndpoints();

// SignalR Hub
app.MapHub<DuelHub>("/duelHub");

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seedService.SeedAsync();
}

app.Run();
