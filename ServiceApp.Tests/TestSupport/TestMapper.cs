using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceApp.Application.Mapping;

namespace ServiceApp.Tests.TestSupport;

/// <summary>
/// Builds a real <see cref="IMapper"/> from the production <see cref="MappingProfile"/>, so
/// service tests exercise the actual mapping configuration instead of a stub.
/// </summary>
public static class TestMapper
{
    public static IMapper Create()
    {
        // AutoMapper 16 requires an ILoggerFactory; the null factory keeps tests quiet.
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        return config.CreateMapper();
    }
}
