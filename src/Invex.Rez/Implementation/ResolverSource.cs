namespace Invex.Rez.Implementation;

/// <summary>
///     An immutable <see cref="IResolverSource" /> backed by fixed maps of variables and functions
///     supplied at construction time.
/// </summary>
/// <remarks>
///     Use this when the set of variables and functions is known up front.
///     For a mutable source, use <see cref="ResolverStore" /> instead.
/// </remarks>
[PublicAPI]
public sealed class ResolverSource : IResolverSource
{
    private readonly Dictionary<string, Func<FunctionCall, string>> _functions;
    private readonly Dictionary<string, string> _variables;

    /// <summary>
    ///     Creates a new <see cref="ResolverSource" /> with the given variables and functions.
    /// </summary>
    /// <param name="variables">A map of variable names to their values.</param>
    /// <param name="functions">A map of function names to their implementations.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="variables" /> or <paramref name="functions" /> is null.
    /// </exception>
    public ResolverSource(
        Dictionary<string, string> variables,
        Dictionary<string, Func<FunctionCall, string>> functions)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        _functions = functions ?? throw new ArgumentNullException(nameof(functions));
    }

    /// <summary>
    ///     Creates a new <see cref="ResolverSource" /> with the given variables and functions.
    /// </summary>
    /// <param name="variables">A sequence of variable names and their values.</param>
    /// <param name="functions">A sequence of function names and their implementations.</param>
    /// <exception cref="ArgumentException">Thrown if either sequence contains duplicate names.</exception>
    public ResolverSource(
        IEnumerable<KeyValuePair<string, string>> variables,
        IEnumerable<KeyValuePair<string, Func<FunctionCall, string>>> functions)
    {
        _variables = variables.ToDictionary(x => x.Key, x => x.Value);
        _functions = functions.ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    ///     Creates a new <see cref="ResolverSource" /> with the given variables and no functions.
    /// </summary>
    /// <param name="variables">A map of variable names to their values.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="variables" /> is null.</exception>
    public ResolverSource(Dictionary<string, string> variables)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        _functions = new();
    }

    /// <summary>
    ///     Creates a new <see cref="ResolverSource" /> with the given variables and no functions.
    /// </summary>
    /// <param name="variables">A sequence of variable names and their values.</param>
    /// <exception cref="ArgumentException">Thrown if the sequence contains duplicate names.</exception>
    public ResolverSource(IEnumerable<KeyValuePair<string, string>> variables)
    {
        _variables = variables.ToDictionary(x => x.Key, x => x.Value);
        _functions = new();
    }

    /// <summary>
    ///     Creates a new <see cref="ResolverSource" /> with the given functions and no variables.
    /// </summary>
    /// <param name="functions">A map of function names to their implementations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="functions" /> is null.</exception>
    public ResolverSource(Dictionary<string, Func<FunctionCall, string>> functions)
    {
        _variables = new();
        _functions = functions ?? throw new ArgumentNullException(nameof(functions));
    }

    /// <summary>
    ///     Creates a new <see cref="ResolverSource" /> with the given functions and no variables.
    /// </summary>
    /// <param name="functions">A sequence of function names and their implementations.</param>
    /// <exception cref="ArgumentException">Thrown if the sequence contains duplicate names.</exception>
    public ResolverSource(IEnumerable<KeyValuePair<string, Func<FunctionCall, string>>> functions)
    {
        _variables = new();
        _functions = functions.ToDictionary(x => x.Key, x => x.Value);
    }

    /// <inheritdoc />
    public string? ResolveVariable(string name)
    {
        #if NET8_0_OR_GREATER
        return _variables.GetValueOrDefault(name);
        #else
        return _variables!.GetValueOrDefault(name);
        #endif
    }

    /// <inheritdoc />
    public Func<FunctionCall, string>? ResolveFunction(string name)
    {
        #if NET8_0_OR_GREATER
        return _functions.GetValueOrDefault(name);
        #else
        return _functions!.GetValueOrDefault(name);
        #endif
    }
}
