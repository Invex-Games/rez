namespace Invex.Rez.Abstraction;

/// <summary>
///     Provides a map of variables and functions to be used by an <see cref="IResolver" />,
///     as well as the ability to add and remove variables and functions at any time.
/// </summary>
[PublicAPI]
public interface IResolverStore : IResolverSource
{
    /// <summary>
    ///     Adds a variable to the store.<br />
    ///     If a variable with the same name already exists, its value is replaced.
    /// </summary>
    /// <param name="name">The name of the variable to add.</param>
    /// <param name="value">
    ///     The value of the variable to add.<br />
    ///     Note: the value should never be null; instead, use an empty string for a blank result.
    /// </param>
    /// <remarks>
    ///     The braces that denote a variable in a template are not included in the name -
    ///     for the template <c>{color}</c>, the name is <c>"color"</c>.
    /// </remarks>
    void AddVariable(string name, string value);

    /// <summary>
    ///     Adds a function to the store.<br />
    ///     If a function with the same name already exists, it is replaced.
    /// </summary>
    /// <param name="name">The name of the function to add.</param>
    /// <param name="function">
    ///     The function to add.<br />
    ///     Note: the function should never return null; instead, return an empty string for a blank result.
    /// </param>
    /// <remarks>
    ///     The braces, parentheses, and arguments that denote a function in a template are not included
    ///     in the name - for the template <c>{repeat(abc,3)}</c>, the name is <c>"repeat"</c>.
    /// </remarks>
    void AddFunction(string name, Func<FunctionCall, string> function);

    /// <summary>
    ///     Removes a variable from the store.<br />
    ///     If the variable was not added, this method does nothing.
    /// </summary>
    /// <param name="name">The name of the variable to remove.</param>
    /// <remarks>
    ///     The braces that denote a variable in a template are not included in the name -
    ///     for the template <c>{color}</c>, the name is <c>"color"</c>.
    /// </remarks>
    void RemoveVariable(string name);

    /// <summary>
    ///     Removes a function from the store.<br />
    ///     If the function was not added, this method does nothing.
    /// </summary>
    /// <param name="name">The name of the function to remove.</param>
    /// <remarks>
    ///     The braces, parentheses, and arguments that denote a function in a template are not included
    ///     in the name - for the template <c>{repeat(abc,3)}</c>, the name is <c>"repeat"</c>.
    /// </remarks>
    void RemoveFunction(string name);
}
