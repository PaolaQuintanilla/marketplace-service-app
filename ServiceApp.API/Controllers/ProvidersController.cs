using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApp.API.Extensions;
using ServiceApp.Application.DTOs.Providers;
using ServiceApp.Application.Interfaces;

namespace ServiceApp.API.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController(IProviderService providers) : ControllerBase
{
    /// <summary>Search providers, optionally filtered by service, ordered by rating.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProviderDto>>> Search([FromQuery] Guid? serviceId, CancellationToken ct)
        => Ok(await providers.SearchAsync(serviceId, ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProviderDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await providers.GetByIdAsync(id, ct));

    /// <summary>Creates a provider profile for the current user (promotes them to Provider).</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProviderDto>> CreateProfile(CreateProviderRequest request, CancellationToken ct)
    {
        var created = await providers.CreateProfileAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
