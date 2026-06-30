using ServiceApp.Infrastructure.Auth;

namespace ServiceApp.Tests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ProducesAHashThatIsNotThePlaintext()
    {
        var hash = _sut.Hash("Secret123!");

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual("Secret123!", hash);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("Secret123!");

        Assert.True(_sut.Verify("Secret123!", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("Secret123!");

        Assert.False(_sut.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // BCrypt salts each hash, so identical passwords must not yield identical hashes.
        var first = _sut.Hash("Secret123!");
        var second = _sut.Hash("Secret123!");

        Assert.NotEqual(first, second);
    }
}
