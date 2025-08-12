using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Web.DTOs;

namespace MafiaMMORPG.Tests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Use in-memory database for testing
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_Refresh_Flow_Should_Work()
    {
        // Arrange
        var registerRequest = new RegisterRequest("testuser", "test@example.com", "Test123!");

        // Act 1: Register
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registerResult);
        Assert.NotNull(registerResult.AccessToken);
        Assert.NotNull(registerResult.RefreshToken);

        // Act 2: Login
        var loginRequest = new LoginRequest("testuser", "Test123!");
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AccessToken);
        Assert.NotNull(loginResult.RefreshToken);

        // Act 3: Use access token to call /me
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var meResponse = await _client.GetAsync("/me");
        meResponse.EnsureSuccessStatusCode();

        // Act 4: Refresh token
        var refreshRequest = new RefreshRequest(loginResult.RefreshToken);
        var refreshResponse = await _client.PostAsJsonAsync("/auth/refresh", refreshRequest);
        refreshResponse.EnsureSuccessStatusCode();

        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshResult);
        Assert.NotNull(refreshResult.AccessToken);
        Assert.NotNull(refreshResult.RefreshToken);
        Assert.NotEqual(loginResult.RefreshToken, refreshResult.RefreshToken); // Should be different
    }

    [Fact]
    public async Task Me_Endpoints_Should_Require_Authentication()
    {
        // Act: Call /me without authentication
        var response = await _client.GetAsync("/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Stats_Allocation_Should_Work()
    {
        // Arrange: Register and login
        var registerRequest = new RegisterRequest("testuser2", "test2@example.com", "Test123!");
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResult!.AccessToken);

        // Act 1: Get initial stats
        var statsResponse = await _client.GetAsync("/me/stats");
        statsResponse.EnsureSuccessStatusCode();

        var initialStats = await statsResponse.Content.ReadFromJsonAsync<dynamic>();
        var initialFreePoints = (int)initialStats!.FreePoints;

        // Act 2: Allocate stats
        var allocateRequest = new AllocateStatsRequest(1, 1, 1, 1);
        var allocateResponse = await _client.PostAsJsonAsync("/me/stats/allocate", allocateRequest);
        allocateResponse.EnsureSuccessStatusCode();

        // Act 3: Get updated stats
        var updatedStatsResponse = await _client.GetAsync("/me/stats");
        updatedStatsResponse.EnsureSuccessStatusCode();

        var updatedStats = await updatedStatsResponse.Content.ReadFromJsonAsync<dynamic>();
        var updatedFreePoints = (int)updatedStats!.FreePoints;

        // Assert
        Assert.Equal(initialFreePoints - 4, updatedFreePoints);
    }
}
