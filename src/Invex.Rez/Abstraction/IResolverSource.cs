namespace Invex.Rez.Abstraction;

/// <summary>
///     Provides a map of variables and functions to be used by an <see cref="IResolver" />.
/// </summary>
[PublicAPI]
public interface IResolverSource
{
    /// <summary>
    ///     Resolves a variable with the given name.
    /// </summary>
    /// <param name="name">The name of the variable to resolve.</param>
    /// <returns>
    ///     The value of the variable, or <see langword="null" /> if this source cannot resolve the variable.<br />
    ///     Returning <see langword="null" /> allows the next source registered with the <see cref="IResolver" /> to be queried.
    /// </returns>
    /// <remarks>
    ///     The braces that denote a variable in a template are not included in the name -
    ///     for the template <c>{color}</c>, the name is <c>"color"</c>.
    /// </remarks>
    string? ResolveVariable(string name);

    /// <summary>
    ///     Resolves a function with the given name.
    /// </summary>
    /// <param name="name">The name of the function to resolve.</param>
    /// <returns>
    ///     A delegate that produces the function's result from a <see cref="FunctionCall" />,
    ///     or <see langword="null" /> if this source cannot resolve the function.<br />
    ///     Returning <see langword="null" /> allows the next source registered with the <see cref="IResolver" /> to be queried.
    /// </returns>
    /// <remarks>
    ///     The braces, parentheses, and arguments that denote a function in a template are not included
    ///     in the name - for the template <c>{repeat(abc,3)}</c>, the name is <c>"repeat"</c>.
    /// </remarks>
    Func<FunctionCall, string>? ResolveFunction(string name);
}
