using System.Net;
using System.Net.Http.Json;
using ServiceApp.Application.DTOs.Auth;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Tests.Integration;

public class AuthApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Register_WithValidRequest_ReturnsTokenAndUser()
    {
        var client = factory.CreateClient();
        var email = ApiTestExtensions.UniqueEmail("new-user");

        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Alice", email, "Secret123!", UserRole.Client));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(email, body.User.Email);
        Assert.Equal("Client", body.User.Role);
    }

    [Fact]
    public async Task Register_AsAdmin_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Eve", ApiTestExtensions.UniqueEmail("admin"), "Secret123!", UserRole.Admin));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var email = ApiTestExtensions.UniqueEmail("dupe");
        await client.RegisterAsync("First", email, "Secret123!", UserRole.Client);

        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Second", email, "Secret123!", UserRole.Client));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Succeeds()
    {
        var client = factory.CreateClient();
        var email = ApiTestExtensions.UniqueEmail("login");
        await client.RegisterAsync("Bob", email, "Secret123!", UserRole.Client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Secret123!"));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var email = ApiTestExtensions.UniqueEmail("badpass");
        await client.RegisterAsync("Carol", email, "Secret123!", UserRole.Client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Bookings_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        // /api/bookings/mine is [Authorize] with no token attached.
        var response = await client.GetAsync("/api/bookings/mine");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
