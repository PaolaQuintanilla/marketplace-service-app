using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Application.DTOs.Providers;

public record ProviderDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ServiceId,
    string ServiceName,
    string Description,
    decimal HourlyRate,
    double Rating);

public record CreateProviderRequest(
    [Required] Guid ServiceId,
    [Required, MaxLength(1000)] string Description,
    [Range(0, 100000)] decimal HourlyRate);
