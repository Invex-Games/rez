namespace Invex.Rez.Implementation;

/// <summary>
///     The default implementation of <see cref="IResolver" />.<br />
///     Resolves variables (e.g., <c>{name}</c>) and functions (e.g., <c>{name(args)}</c>) in a Rez template
///     by querying registered <see cref="IResolverSource" /> instances in the order they were added.
/// </summary>
/// <remarks>
///     Resolution is recursive: if a resolved value itself contains variables or functions, they are resolved
///     as well, up to a maximum depth - exceeding it (e.g., via a self-referencing variable) throws an exception.
/// </remarks>
[PublicAPI]
public sealed
    #if NET8_0_OR_GREATER
    partial
    #endif
    class Resolver : IResolver
{
    #if NET9_0_OR_GREATER
    [GeneratedRegex(@"^(.+?)\((.*)\)$", RegexOptions.Compiled)]
    private static partial Regex FunctionRegex { get; }

    #elif NET8_0_OR_GREATER
    [GeneratedRegex(@"^(.+?)\((.*)\)$", RegexOptions.Compiled)]
    private static partial Regex FunctionRegexGenerated();

    private static Regex FunctionRegex => FunctionRegexGenerated();

    #else
    private static Regex FunctionRegex { get; } = new(@"^(.+?)\((.*)\)$", RegexOptions.Compiled);

    #endif

    private readonly List<IResolverSource> _sources = [];

    /// <inheritdoc />
    public IResolver AddSource(IResolverSource source)
    {
        if (!_sources.Contains(source))
            _sources.Add(source);

        return this;
    }

    /// <inheritdoc />
    public IResolver RemoveSource(IResolverSource source)
    {
        _sources.Remove(source);

        return this;
    }

    /// <inheritdoc />
    public string? Resolve(string? input) =>
        input is null
            ? input
            : Executor.Execute(input, ResolveParsedMember);

    private string? ResolveParsedMember(string input)
    {
        var functionMatch = FunctionRegex.Match(input);

        string? result = null;

        if (functionMatch.Success)
            result = ResolveFunction(functionMatch.Groups[1].Value, new(functionMatch.Groups[2].Value));

        result ??= ResolveVariable(input);

        return result;
    }

    private string? ResolveFunction(string name, FunctionCall functionCall) =>
        _sources
            .Select(source => source
                .ResolveFunction(name)
                ?.Invoke(functionCall))
            .FirstOrDefault(result => result is not null);

    private string? ResolveVariable(string input) =>
        _sources
            .Select(source => source.ResolveVariable(input))
            .FirstOrDefault(result => result is not null);
}
