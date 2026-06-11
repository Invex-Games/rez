namespace Invex.Rez.Compiler;

/// <summary>
///     Converts a flat sequence of <see cref="Lexeme" />s into <see cref="SyntaxToken" />s,
///     pairing braces into variable tokens and treating everything else as literals.
/// </summary>
internal static class Tokenizer
{
    /// <summary>
    ///     Tokenizes the given lexemes into <paramref name="tokens" />.
    /// </summary>
    /// <param name="lexemes">The lexemes to tokenize.</param>
    /// <param name="tokens">The buffer that receives the produced tokens.</param>
    /// <returns>The number of tokens written to <paramref name="tokens" />.</returns>
    /// <remarks>
    ///     For nested braces (e.g., <c>{outer{inner}}</c>), the innermost pair produces the variable token,
    ///     allowing inner placeholders to be resolved before outer ones across successive
    ///     <see cref="Executor" /> passes. Unmatched open braces with no nested variables fall back to literals.
    /// </remarks>
    public static int TokenizeLexemes(Span<Lexeme> lexemes, Span<SyntaxToken> tokens)
    {
        var lexemeIndex = 0;
        var tokenIndex = 0;

        while (lexemeIndex < lexemes.Length)
        {
            var result = lexemes[lexemeIndex].Type == Lexeme.OpenBrace
                ? TokenizeNonLiteralLexeme(lexemes[lexemeIndex..], tokens[tokenIndex..])
                : TokenizeLiteralLexeme(lexemes[lexemeIndex..], tokens[tokenIndex..]);

            lexemeIndex += result.ConsumedLexemes;
            tokenIndex += result.ProducedTokens;
        }

        return tokenIndex;
    }

    /// <summary>
    ///     Emits a literal token for a single literal lexeme.
    /// </summary>
    private static SyntaxResult TokenizeLiteralLexeme(Span<Lexeme> lexemes, Span<SyntaxToken> tokens)
    {
        var lexeme = lexemes[0];
        tokens[0] = SyntaxToken.CreateLiteral(lexeme.Start, lexeme.Length);

        return new(1, 1);
    }

    /// <summary>
    ///     Tokenizes a span of lexemes beginning with an open brace, recursing into any nested braces.
    ///     Produces a variable token when a matching close brace is found; otherwise falls back to literals.
    /// </summary>
    private static SyntaxResult TokenizeNonLiteralLexeme(Span<Lexeme> lexemes, Span<SyntaxToken> tokens)
    {
        if (lexemes.Length == 1)
        {
            tokens[0] = SyntaxToken.CreateLiteral(lexemes[0].Start, lexemes[0].Length);

            return new(1, 1);
        }

        var firstLexeme = lexemes[1];

        var lexemeIndex = 1;
        var tokenIndex = 0;

        while (lexemeIndex < lexemes.Length)
        {
            var lexeme = lexemes[lexemeIndex];

            if (lexeme.Type == Lexeme.CloseBrace)
            {
                tokens[tokenIndex++] = SyntaxToken.CreateVariable(firstLexeme.Start, lexeme.Start - firstLexeme.Start);

                return new(lexemeIndex + 1, tokenIndex);
            }

            var result = lexeme.Type == Lexeme.OpenBrace
                ? TokenizeNonLiteralLexeme(lexemes[lexemeIndex..], tokens[tokenIndex..])
                : TokenizeLiteralLexeme(lexemes[lexemeIndex..], tokens[tokenIndex..]);

            lexemeIndex += result.ConsumedLexemes;
            tokenIndex += result.ProducedTokens;
        }

        // Handle case where there is no closing brace

        for (var i = 0; i < tokenIndex; i++)

            // If there are any nested non-literals, then they will be evaluated first so we can just return
            if (tokens[i].Type == SyntaxToken.Variable)
                return new(lexemes.Length, tokenIndex);

        var charLength = 0;

        foreach (var lexeme in lexemes)
            charLength += lexeme.Length;

        // If there are no nested non-literals, then we can just treat the whole thing as a literal
        tokens[0] = SyntaxToken.CreateLiteral(lexemes[0].Start, charLength);

        return new(lexemes.Length, 1);
    }
}
