namespace Invex.Rez.Implementation;

/// <summary>
///     The default implementation of <see cref="IResolverStore" /> - a mutable <see cref="IResolverSource" />
///     whose variables and functions can be added and removed at any time.
/// </summary>
/// <remarks>
///     For an immutable source whose contents are fixed at construction time,
///     use <see cref="ResolverSource" /> instead.
/// </remarks>
[PublicAPI]
public sealed class ResolverStore : IResolverStore
{
    private readonly Dictionary<string, Func<FunctionCall, string>> _functions = new();
    private readonly Dictionary<string, string> _variables = new();

    /// <inheritdoc />
    public void AddVariable(string name, string value) =>
        _variables[name] = value;

    /// <inheritdoc />
    public void AddFunction(string name, Func<FunctionCall, string> function) =>
        _functions[name] = function;

    /// <inheritdoc />
    public void RemoveVariable(string name) =>
        _variables.Remove(name);

    /// <inheritdoc />
    public void RemoveFunction(string name) =>
        _functions.Remove(name);

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
