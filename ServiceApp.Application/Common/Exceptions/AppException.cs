namespace ServiceApp.Application.Common.Exceptions;

/// <summary>Base class for expected, handled application errors mapped to HTTP responses.</summary>
public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }
}

/// <summary>404 — a requested resource does not exist.</summary>
public sealed class NotFoundException(string message) : AppException(message)
{
    public override int StatusCode => 404;
}

/// <summary>409 — the request conflicts with current state (e.g. duplicate email).</summary>
public sealed class ConflictException(string message) : AppException(message)
{
    public override int StatusCode => 409;
}

/// <summary>400 — the request is invalid for a domain reason.</summary>
public sealed class ValidationException(string message) : AppException(message)
{
    public override int StatusCode => 400;
}

/// <summary>401 — authentication failed.</summary>
public sealed class UnauthorizedException(string message) : AppException(message)
{
    public override int StatusCode => 401;
}

/// <summary>403 — authenticated but not allowed to perform the action.</summary>
public sealed class ForbiddenException(string message) : AppException(message)
{
    public override int StatusCode => 403;
}
