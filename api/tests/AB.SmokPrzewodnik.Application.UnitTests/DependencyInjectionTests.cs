using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersMediator()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IMediator>());
    }
}
