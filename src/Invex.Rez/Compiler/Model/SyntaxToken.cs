namespace Invex.Rez.Compiler.Model;

/// <summary>
///     A syntactic unit produced by the <see cref="Tokenizer" />: literal text, or a variable to be resolved.
/// </summary>
/// <param name="Type">The kind of token - one of <see cref="Literal" /> or <see cref="Variable" />.</param>
/// <param name="Start">
///     The zero-based index of the token's first character in the source text.
///     For variables, this is the character after the open brace.
/// </param>
/// <param name="Length">
///     The number of characters the token spans in the source text.
///     For variables, this excludes the enclosing braces.
/// </param>
internal readonly record struct SyntaxToken(int Type, int Start, int Length)
{
    public const int Literal = 0;
    public const int Variable = 1;

    public static SyntaxToken CreateLiteral(int valueStart, int valueLength) =>
        new(Literal, valueStart, valueLength);

    public static SyntaxToken CreateVariable(int valueStart, int valueLength) =>
        new(Variable, valueStart, valueLength);
}
