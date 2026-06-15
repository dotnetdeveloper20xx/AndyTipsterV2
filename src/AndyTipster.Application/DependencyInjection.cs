using Microsoft.Extensions.DependencyInjection;

namespace AndyTipster.Application;

/// <summary>
/// Registers Application layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application-layer services (validators, use-case handlers) will be registered here
        return services;
    }
}
