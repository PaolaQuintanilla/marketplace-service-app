using AutoMapper;
using ServiceApp.Application.Common.Exceptions;
using ServiceApp.Application.DTOs.Auth;
using ServiceApp.Application.Interfaces;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Enums;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IMapper mapper) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Role == UserRole.Admin)
            throw new ValidationException("You cannot self-register as an administrator.");

        var email = Normalize(request.Email);

        if (await unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException($"An account with email '{request.Email}' already exists.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(Normalize(request.Email), cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var token = tokenGenerator.GenerateToken(user);
        return new AuthResponse(token.Token, token.ExpiresAt, mapper.Map<UserDto>(user));
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
