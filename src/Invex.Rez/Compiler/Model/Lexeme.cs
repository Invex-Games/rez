namespace Invex.Rez.Compiler.Model;

/// <summary>
///     A lexical unit produced by the <see cref="Lexer" />: a literal run of text, an open brace, or a close brace.
/// </summary>
/// <param name="Type">The kind of lexeme - one of <see cref="Literal" />, <see cref="OpenBrace" />, or <see cref="CloseBrace" />.</param>
/// <param name="Start">The zero-based index of the lexeme's first character in the source text.</param>
/// <param name="Length">The number of characters the lexeme spans in the source text.</param>
internal readonly record struct Lexeme(int Type, int Start, int Length)
{
    public const int Literal = 0;
    public const int OpenBrace = 1;
    public const int CloseBrace = 2;

    public static Lexeme CreateLiteral(int valueStart, int valueLength) =>
        new(Literal, valueStart, valueLength);

    public static Lexeme CreateOpenBrace(int valueStart) =>
        new(OpenBrace, valueStart, 1);

    public static Lexeme CreateCloseBrace(int valueStart) =>
        new(CloseBrace, valueStart, 1);
}
