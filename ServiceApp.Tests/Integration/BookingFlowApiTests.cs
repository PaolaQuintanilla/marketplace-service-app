using System.Net;
using System.Net.Http.Json;
using ServiceApp.Application.DTOs.Bookings;
using ServiceApp.Application.DTOs.Providers;
using ServiceApp.Application.DTOs.Services;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Tests.Integration;

/// <summary>
/// End-to-end happy path over real HTTP: register a client and a provider, create a provider
/// profile, book it, and have the provider confirm — plus the key authorization guard.
/// </summary>
public class BookingFlowApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task FullBookingLifecycle_ClientBooks_ProviderConfirms()
    {
        // --- Arrange: a provider user with a profile, and a separate client ---
        var providerClient = factory.CreateClient();
        var providerAuth = await providerClient.RegisterAsync(
            "Pat Provider", ApiTestExtensions.UniqueEmail("provider"), "Secret123!", UserRole.Provider);
        providerClient.Authenticate(providerAuth.Token);

        var services = await providerClient.GetFromJsonAsync<List<ServiceDto>>("/api/services");
        var serviceId = services!.First().Id;

        var profileResponse = await providerClient.PostAsJsonAsync(
            "/api/providers", new CreateProviderRequest(serviceId, "20 years of plumbing.", 45m));
        profileResponse.EnsureSuccessStatusCode();
        var profile = (await profileResponse.Content.ReadFromJsonAsync<ProviderDto>())!;

        var clientHttp = factory.CreateClient();
        var clientAuth = await clientHttp.RegisterAsync(
            "Cleo Client", ApiTestExtensions.UniqueEmail("client"), "Secret123!", UserRole.Client);
        clientHttp.Authenticate(clientAuth.Token);

        // --- Act 1: the client creates a booking ---
        var createResponse = await clientHttp.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(profile.Id, DateTime.UtcNow.AddDays(2), "Kitchen sink leak"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var booking = (await createResponse.Content.ReadFromJsonAsync<BookingDto>())!;
        Assert.Equal("Pending", booking.Status);
        Assert.Equal("Kitchen sink leak", booking.Notes);

        // --- Act 2: the provider confirms it ---
        var confirmResponse = await providerClient.PatchAsJsonAsync(
            $"/api/bookings/{booking.Id}/status", new { status = "Confirmed" });

        confirmResponse.EnsureSuccessStatusCode();
        var confirmed = (await confirmResponse.Content.ReadFromJsonAsync<BookingDto>())!;
        Assert.Equal("Confirmed", confirmed.Status);

        // --- Assert: the client sees it under "mine" ---
        var mine = await clientHttp.GetFromJsonAsync<List<BookingDto>>("/api/bookings/mine");
        Assert.Contains(mine!, b => b.Id == booking.Id && b.Status == "Confirmed");
    }

    [Fact]
    public async Task ClientCannotConfirmBooking_ReturnsForbidden()
    {
        // Provider with a profile.
        var providerClient = factory.CreateClient();
        var providerAuth = await providerClient.RegisterAsync(
            "Provider Two", ApiTestExtensions.UniqueEmail("provider2"), "Secret123!", UserRole.Provider);
        providerClient.Authenticate(providerAuth.Token);

        var services = await providerClient.GetFromJsonAsync<List<ServiceDto>>("/api/services");
        var profileResponse = await providerClient.PostAsJsonAsync(
            "/api/providers", new CreateProviderRequest(services!.First().Id, "Electrician.", 60m));
        var profile = (await profileResponse.Content.ReadFromJsonAsync<ProviderDto>())!;

        // Client books it.
        var clientHttp = factory.CreateClient();
        var clientAuth = await clientHttp.RegisterAsync(
            "Client Two", ApiTestExtensions.UniqueEmail("client2"), "Secret123!", UserRole.Client);
        clientHttp.Authenticate(clientAuth.Token);
        var createResponse = await clientHttp.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(profile.Id, DateTime.UtcNow.AddDays(1), null));
        var booking = (await createResponse.Content.ReadFromJsonAsync<BookingDto>())!;

        // The client (not the provider) tries to confirm → 403.
        var confirmResponse = await clientHttp.PatchAsJsonAsync(
            $"/api/bookings/{booking.Id}/status", new { status = "Confirmed" });

        Assert.Equal(HttpStatusCode.Forbidden, confirmResponse.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_InThePast_ReturnsBadRequest()
    {
        var providerClient = factory.CreateClient();
        var providerAuth = await providerClient.RegisterAsync(
            "Provider Three", ApiTestExtensions.UniqueEmail("provider3"), "Secret123!", UserRole.Provider);
        providerClient.Authenticate(providerAuth.Token);

        var services = await providerClient.GetFromJsonAsync<List<ServiceDto>>("/api/services");
        var profileResponse = await providerClient.PostAsJsonAsync(
            "/api/providers", new CreateProviderRequest(services!.First().Id, "Gardener.", 30m));
        var profile = (await profileResponse.Content.ReadFromJsonAsync<ProviderDto>())!;

        var clientHttp = factory.CreateClient();
        var clientAuth = await clientHttp.RegisterAsync(
            "Client Three", ApiTestExtensions.UniqueEmail("client3"), "Secret123!", UserRole.Client);
        clientHttp.Authenticate(clientAuth.Token);

        var response = await clientHttp.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(profile.Id, DateTime.UtcNow.AddDays(-1), null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
