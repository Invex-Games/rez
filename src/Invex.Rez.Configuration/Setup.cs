namespace Invex.Rez.Configuration;

/// <summary>
///     Provides extension methods for registering Rez configuration services with dependency injection.
/// </summary>
[PublicAPI]
public static class Setup
{
    /// <summary>
    ///     Registers an <see cref="IResolvableConfig" /> singleton that wraps the application's
    ///     <see cref="IConfigurationRoot" /> and resolves Rez templates in configuration values on read.
    /// </summary>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <returns>The service collection, allowing calls to be chained.</returns>
    /// <remarks>
    ///     Both <see cref="IConfigurationRoot" /> and <see cref="IResolver" /> must be registered
    ///     in the service collection for the <see cref="IResolvableConfig" /> to be constructed.
    /// </remarks>
    public static IServiceCollection AddResolvableConfiguration(this IServiceCollection services) =>
        services.AddSingleton<IResolvableConfig>(x =>
            new ResolvableConfigurationRoot(x.GetRequiredService<IConfigurationRoot>(),
                x.GetRequiredService<IResolver>()));
}
