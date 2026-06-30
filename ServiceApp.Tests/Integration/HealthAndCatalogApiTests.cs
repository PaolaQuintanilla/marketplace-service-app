using System.Net;
using System.Net.Http.Json;
using ServiceApp.Application.DTOs.Services;

namespace ServiceApp.Tests.Integration;

public class HealthAndCatalogApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetServices_IsAnonymous_AndReturnsSeededCatalog()
    {
        var client = factory.CreateClient();

        var services = await client.GetFromJsonAsync<List<ServiceDto>>("/api/services");

        Assert.NotNull(services);
        // DbSeeder seeds Plumbing/Cleaning/Electrical/Gardening on first boot.
        Assert.Contains(services!, s => s.Name == "Plumbing");
        Assert.True(services!.Count >= 4);
    }

    [Fact]
    public async Task PostService_WithoutAdminToken_IsRejected()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/services", new CreateServiceRequest("Painting", "Home", "Walls and ceilings."));

        // Anonymous caller hitting an [Authorize(Roles="Admin")] action → 401 Unauthorized.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
