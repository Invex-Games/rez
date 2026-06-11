namespace Invex.Rez.Compiler;

/// <summary>
///     Splits raw template text into a flat sequence of <see cref="Lexeme" />s:
///     literal runs, open braces, and close braces.
/// </summary>
internal static class Lexer
{
    /// <summary>
    ///     Lexes the input text into <paramref name="output" />.
    /// </summary>
    /// <param name="input">The text to lex.</param>
    /// <param name="output">The buffer that receives the produced lexemes.</param>
    /// <returns>The number of lexemes written to <paramref name="output" />.</returns>
    /// <remarks>
    ///     A backslash escapes the next character, causing braces to be lexed as literal text.
    ///     The backslash itself is preserved in the literal lexeme.
    /// </remarks>
    public static int Lex(ReadOnlySpan<char> input, Span<Lexeme> output)
    {
        var lexemeCount = 0;
        var checkpointIndex = 0;
        var workingIndex = 0;
        var escaped = false;

        while (workingIndex < input.Length)
        {
            switch (input[workingIndex])
            {
                case '\\' when !escaped:
                {
                    escaped = true;
                    workingIndex++;

                    continue;
                }

                case '{' when !escaped:
                {
                    if (workingIndex > checkpointIndex)
                        output[lexemeCount++] = Lexeme.CreateLiteral(checkpointIndex, workingIndex - checkpointIndex);

                    output[lexemeCount++] = Lexeme.CreateOpenBrace(workingIndex);
                    workingIndex++;
                    checkpointIndex = workingIndex;

                    continue;
                }

                case '}' when !escaped:
                {
                    if (workingIndex > checkpointIndex)
                        output[lexemeCount++] = Lexeme.CreateLiteral(checkpointIndex, workingIndex - checkpointIndex);

                    output[lexemeCount++] = Lexeme.CreateCloseBrace(workingIndex);
                    workingIndex++;
                    checkpointIndex = workingIndex;

                    continue;
                }

                default:
                {
                    escaped = false;
                    workingIndex++;

                    continue;
                }
            }
        }

        if (workingIndex > checkpointIndex)
            output[lexemeCount++] = Lexeme.CreateLiteral(checkpointIndex, workingIndex - checkpointIndex);

        return lexemeCount;
    }
}
