using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApp.Application.DTOs.Services;
using ServiceApp.Application.Interfaces;

namespace ServiceApp.API.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(IServiceCatalogService services) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll(CancellationToken ct)
        => Ok(await services.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await services.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Create(CreateServiceRequest request, CancellationToken ct)
    {
        var created = await services.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
