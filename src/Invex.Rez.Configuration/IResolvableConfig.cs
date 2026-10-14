namespace Invex.Rez.Configuration;

/// <summary>
///     An <see cref="IConfigurationRoot" /> whose values are resolved with an <see cref="IResolver" /> on read,
///     allowing configuration values to contain Rez templates (e.g., <c>"{baseUrl}/api"</c>).
/// </summary>
/// <remarks>
///     Values returned via the indexer, sections, and <see cref="ResolvableConfigurationSection" /> children
///     are resolved automatically; writes pass through to the underlying configuration unmodified.
/// </remarks>
[PublicAPI]
public interface IResolvableConfig : IConfigurationRoot
{
    /// <summary>
    ///     The <see cref="IResolver" /> used to resolve configuration values.
    /// </summary>
    IResolver Resolver { get; }

    /// <summary>
    ///     Resolves variables and functions within the given text using <see cref="Resolver" />.
    /// </summary>
    /// <param name="value">The template text containing variables and/or functions to be resolved.</param>
    /// <returns>The resolved output text, or <see langword="null" /> if <paramref name="value" /> is <see langword="null" />.</returns>
    /// <exception cref="RezResolutionException">
    ///     Thrown by the default <see cref="IResolver" /> if recursion exceeds its depth limit, such as for a cycle like
    ///     <c>a = {b}</c>, <c>b = {a}</c>.
    /// </exception>
    string? Resolve(string? value);
}
