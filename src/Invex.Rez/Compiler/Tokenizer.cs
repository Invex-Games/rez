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
    ///     <br />
    ///     Open braces are tracked on an explicit stack rather than by recursion, so arbitrarily deep nesting
    ///     or long runs of unmatched braces cannot overflow the call stack.
    /// </remarks>
    public static int TokenizeLexemes(Span<Lexeme> lexemes, Span<SyntaxToken> tokens)
    {
        Frame[]? frames = null;
        var frameCount = 0;
        var tokenIndex = 0;

        try
        {
            for (var lexemeIndex = 0; lexemeIndex < lexemes.Length; lexemeIndex++)
            {
                var lexeme = lexemes[lexemeIndex];

                switch (lexeme.Type)
                {
                    case Lexeme.OpenBrace:
                    {
                        frames ??= ArrayPool<Frame>.Shared.Rent(lexemes.Length);
                        frames[frameCount++] = new(lexemeIndex, tokenIndex, false);

                        break;
                    }

                    case Lexeme.CloseBrace when frameCount > 0:
                    {
                        // The variable spans from the lexeme after the open brace up to this close brace.
                        var firstLexeme = lexemes[frames![--frameCount].LexemeIndex + 1];

                        tokens[tokenIndex++] =
                            SyntaxToken.CreateVariable(firstLexeme.Start, lexeme.Start - firstLexeme.Start);

                        if (frameCount > 0)
                            frames[frameCount - 1] = frames[frameCount - 1] with { HasVariable = true };

                        break;
                    }

                    default:
                    {
                        tokens[tokenIndex++] = SyntaxToken.CreateLiteral(lexeme.Start, lexeme.Length);

                        break;
                    }
                }
            }

            // Unmatched open braces, innermost first. A brace with any nested variable keeps its tokens so the
            // variables are evaluated first; otherwise everything from the brace to the end becomes one literal.
            var unmatchedLength = 0;
            var lengthFromIndex = lexemes.Length;

            while (frameCount > 0)
            {
                var frame = frames![--frameCount];

                for (var i = frame.LexemeIndex; i < lengthFromIndex; i++)
                    unmatchedLength += lexemes[i].Length;

                lengthFromIndex = frame.LexemeIndex;

                if (frame.HasVariable)
                {
                    if (frameCount > 0)
                        frames[frameCount - 1] = frames[frameCount - 1] with { HasVariable = true };

                    continue;
                }

                tokenIndex = frame.TokenIndex;
                tokens[tokenIndex++] = SyntaxToken.CreateLiteral(lexemes[frame.LexemeIndex].Start, unmatchedLength);
            }

            return tokenIndex;
        }
        finally
        {
            if (frames is not null)
                ArrayPool<Frame>.Shared.Return(frames);
        }
    }

    /// <summary>
    ///     An open brace awaiting its matching close brace.
    /// </summary>
    /// <param name="LexemeIndex">The index of the open brace lexeme.</param>
    /// <param name="TokenIndex">The index of the first token produced after the open brace.</param>
    /// <param name="HasVariable">Whether a variable token has been produced inside this brace.</param>
    private readonly record struct Frame(int LexemeIndex, int TokenIndex, bool HasVariable);
}
