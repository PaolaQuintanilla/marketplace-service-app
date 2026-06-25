using System.ComponentModel.DataAnnotations;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Application.DTOs.Auth;

public record RegisterRequest(
    [Required, MaxLength(120)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    UserRole Role);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(Guid Id, string Name, string Email, string Role);
