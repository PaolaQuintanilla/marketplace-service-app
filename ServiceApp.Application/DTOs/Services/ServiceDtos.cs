using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Application.DTOs.Services;

public record ServiceDto(Guid Id, string Name, string Category, string? Description);

public record CreateServiceRequest(
    [Required, MaxLength(120)] string Name,
    [Required, MaxLength(80)] string Category,
    [MaxLength(500)] string? Description);
