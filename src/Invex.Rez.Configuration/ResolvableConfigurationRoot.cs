namespace Invex.Rez.Configuration;

/// <summary>
///     The default implementation of <see cref="IResolvableConfig" />.<br />
///     Wraps an <see cref="IConfigurationRoot" /> and resolves Rez templates in values on read;
///     writes pass through to the underlying configuration unmodified.
/// </summary>
internal sealed class ResolvableConfigurationRoot(IConfigurationRoot target, IResolver resolver) : IResolvableConfig
{
    public IResolver Resolver { get; } = resolver;

    public string? Resolve(string? value) =>
        Resolver.Resolve(value);

    public IConfigurationSection GetSection(string key) =>
        new ResolvableConfigurationSection(target.GetSection(key), Resolver);

    public IEnumerable<IConfigurationSection> GetChildren() =>
        target
            .GetChildren()
            .Select(x => new ResolvableConfigurationSection(x, Resolver));

    public IChangeToken GetReloadToken() =>
        target.GetReloadToken();

    public string? this[string key]
    {
        get => Resolver.Resolve(target[key]);
        set => target[key] = value;
    }

    public void Reload() =>
        target.Reload();

    public IEnumerable<IConfigurationProvider> Providers => target.Providers;
}
