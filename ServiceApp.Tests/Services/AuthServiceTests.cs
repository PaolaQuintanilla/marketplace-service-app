using Moq;
using ServiceApp.Application.Common.Exceptions;
using ServiceApp.Application.DTOs.Auth;
using ServiceApp.Application.Interfaces;
using ServiceApp.Application.Services;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Enums;
using ServiceApp.Domain.Interfaces;
using ServiceApp.Tests.TestSupport;

namespace ServiceApp.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _sut = new AuthService(_uow.Object, _hasher.Object, _tokens.Object, TestMapper.Create());
    }

    // ---- RegisterAsync ----

    [Fact]
    public async Task RegisterAsync_WhenRoleIsAdmin_ThrowsValidationException()
    {
        var request = new RegisterRequest("Eve", "eve@example.com", "Secret123!", UserRole.Admin);

        await Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(request));
        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        _users.Setup(r => r.EmailExistsAsync("taken@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);
        var request = new RegisterRequest("Bob", "taken@example.com", "Secret123!", UserRole.Client);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterAsync(request));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_HashesPasswordNormalizesEmailAndPersists()
    {
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("Secret123!")).Returns("HASHED");
        _tokens.Setup(t => t.GenerateToken(It.IsAny<User>()))
               .Returns(new TokenResult("jwt-token", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        User? saved = null;
        _users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .Callback<User, CancellationToken>((u, _) => saved = u)
              .Returns(Task.CompletedTask);

        // Email has surrounding whitespace and mixed case; name has whitespace.
        var request = new RegisterRequest("  Alice  ", "  Alice@Example.COM ", "Secret123!", UserRole.Client);

        var response = await _sut.RegisterAsync(request);

        Assert.NotNull(saved);
        Assert.Equal("Alice", saved!.Name);                 // trimmed
        Assert.Equal("alice@example.com", saved.Email);     // normalized (trim + lowercase)
        Assert.Equal("HASHED", saved.PasswordHash);         // stored the hash, never the plaintext
        Assert.Equal(UserRole.Client, saved.Role);

        Assert.Equal("jwt-token", response.Token);
        Assert.Equal("alice@example.com", response.User.Email);
        Assert.Equal("Client", response.User.Role);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- LoginAsync ----

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        var request = new LoginRequest("ghost@example.com", "whatever");

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordDoesNotMatch_ThrowsUnauthorizedException()
    {
        var user = new User { Email = "a@example.com", PasswordHash = "HASHED" };
        _users.Setup(r => r.GetByEmailAsync("a@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("bad", "HASHED")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.LoginAsync(new LoginRequest("a@example.com", "bad")));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUser()
    {
        var user = new User { Name = "Alice", Email = "a@example.com", PasswordHash = "HASHED", Role = UserRole.Provider };
        _users.Setup(r => r.GetByEmailAsync("a@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Secret123!", "HASHED")).Returns(true);
        _tokens.Setup(t => t.GenerateToken(user))
               .Returns(new TokenResult("jwt-token", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        var response = await _sut.LoginAsync(new LoginRequest("a@example.com", "Secret123!"));

        Assert.Equal("jwt-token", response.Token);
        Assert.Equal("Alice", response.User.Name);
        Assert.Equal("Provider", response.User.Role);
    }
}
