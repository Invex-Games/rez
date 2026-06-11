namespace Invex.Rez.Configuration;

/// <summary>
///     An <see cref="IResolverSource" /> that resolves variables from an <see cref="IConfigurationRoot" />.<br />
///     Functions are not supported by this source.
/// </summary>
[PublicAPI]
public sealed class ConfigResolverSource : IResolverSource
{
    private readonly IConfigurationRoot _configuration;

    /// <summary>
    ///     Creates a new <see cref="ConfigResolverSource" /> with the given <see cref="IConfigurationRoot" />.
    /// </summary>
    /// <param name="configuration">
    ///     The <see cref="IConfigurationRoot" /> to resolve variables from.<br />
    ///     Nested configuration keys (e.g., <c>"Section": { "Key": ... }</c>) are addressed using
    ///     the standard colon-delimited form (e.g., <c>{Section:Key}</c>).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration" /> is null.</exception>
    public ConfigResolverSource(IConfigurationRoot configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public string? ResolveVariable(string name) =>
        _configuration[name];

    /// <summary>
    ///     Always returns <see langword="null" /> - functions are not supported by this source.
    /// </summary>
    /// <param name="name">The name of the function to resolve.</param>
    /// <returns><see langword="null" />.</returns>
    public Func<FunctionCall, string>? ResolveFunction(string name) =>
        null;
}
