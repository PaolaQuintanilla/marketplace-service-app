using System.Net.Http.Headers;
using System.Net.Http.Json;
using ServiceApp.Application.DTOs.Auth;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Tests.Integration;

/// <summary>Small helpers to keep the API tests readable (register/login, attach the bearer token).</summary>
public static class ApiTestExtensions
{
    public static async Task<AuthResponse> RegisterAsync(
        this HttpClient client, string name, string email, string password, UserRole role)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest(name, email, password, role));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    /// <summary>Sets the Authorization header on the client to the given JWT.</summary>
    public static void Authenticate(this HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    /// <summary>A unique email so tests sharing a factory (and its in-memory DB) don't collide.</summary>
    public static string UniqueEmail(string prefix) => $"{prefix}-{Guid.NewGuid():N}@example.com";
}
