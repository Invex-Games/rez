namespace Invex.Rez.Tests.Compiler;

[TestFixture]
public class LexerTests
{
    [TestCase(" ")]
    [TestCase("example")]
    [TestCase("e x a m p l e")]
    [TestCase("   example   ")]
    [TestCase(@"\example\")]
    [TestCase(@"\{example\}")]
    public void Lex_Literal(string input)
    {
        var inputBuffer = input.AsSpan();
        Span<Lexeme> outputBuffer = stackalloc Lexeme[input.Length];

        var result = Lexer.Lex(inputBuffer, outputBuffer);

        result.ShouldBe(1);

        outputBuffer[0]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[0]
            .Start
            .ShouldBe(0);

        outputBuffer[0]
            .Length
            .ShouldBe(input.Length);
    }

    [Test]
    public void Lex_OpenBrace_Single()
    {
        const string input = "{";
        var inputBuffer = input.AsSpan();
        Span<Lexeme> outputBuffer = stackalloc Lexeme[input.Length];

        var result = Lexer.Lex(inputBuffer, outputBuffer);

        result.ShouldBe(1);

        outputBuffer[0]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[0]
            .Start
            .ShouldBe(0);

        outputBuffer[0]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Lex_CloseBrace_Single()
    {
        const string input = "}";
        var inputBuffer = input.AsSpan();
        Span<Lexeme> outputBuffer = stackalloc Lexeme[input.Length];

        var result = Lexer.Lex(inputBuffer, outputBuffer);

        result.ShouldBe(1);

        outputBuffer[0]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[0]
            .Start
            .ShouldBe(0);

        outputBuffer[0]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Lex_OpenBrace_Multiple()
    {
        const string input = "{{";
        var inputBuffer = input.AsSpan();
        Span<Lexeme> outputBuffer = stackalloc Lexeme[input.Length];

        var result = Lexer.Lex(inputBuffer, outputBuffer);

        result.ShouldBe(2);

        outputBuffer[0]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[0]
            .Start
            .ShouldBe(0);

        outputBuffer[0]
            .Length
            .ShouldBe(1);

        outputBuffer[1]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[1]
            .Start
            .ShouldBe(1);

        outputBuffer[1]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Lex_CloseBrace_Multiple()
    {
        const string input = "}}";
        var inputBuffer = input.AsSpan();
        Span<Lexeme> outputBuffer = stackalloc Lexeme[input.Length];

        var result = Lexer.Lex(inputBuffer, outputBuffer);

        result.ShouldBe(2);

        outputBuffer[0]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[0]
            .Start
            .ShouldBe(0);

        outputBuffer[0]
            .Length
            .ShouldBe(1);

        outputBuffer[1]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[1]
            .Start
            .ShouldBe(1);

        outputBuffer[1]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Lex_Mix()
    {
        const string input = @"{var1{{var2}{var3}}var4}\{{\}}";
        var inputBuffer = input.AsSpan();
        Span<Lexeme> outputBuffer = stackalloc Lexeme[input.Length];

        var result = Lexer.Lex(inputBuffer, outputBuffer);

        result.ShouldBe(16);

        outputBuffer[0]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[0]
            .Start
            .ShouldBe(0);

        outputBuffer[0]
            .Length
            .ShouldBe(1);

        outputBuffer[1]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[1]
            .Start
            .ShouldBe(1);

        outputBuffer[1]
            .Length
            .ShouldBe(4);

        outputBuffer[2]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[2]
            .Start
            .ShouldBe(5);

        outputBuffer[2]
            .Length
            .ShouldBe(1);

        outputBuffer[3]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[3]
            .Start
            .ShouldBe(6);

        outputBuffer[3]
            .Length
            .ShouldBe(1);

        outputBuffer[4]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[4]
            .Start
            .ShouldBe(7);

        outputBuffer[4]
            .Length
            .ShouldBe(4);

        outputBuffer[5]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[5]
            .Start
            .ShouldBe(11);

        outputBuffer[5]
            .Length
            .ShouldBe(1);

        outputBuffer[6]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[6]
            .Start
            .ShouldBe(12);

        outputBuffer[6]
            .Length
            .ShouldBe(1);

        outputBuffer[7]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[7]
            .Start
            .ShouldBe(13);

        outputBuffer[7]
            .Length
            .ShouldBe(4);

        outputBuffer[8]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[8]
            .Start
            .ShouldBe(17);

        outputBuffer[8]
            .Length
            .ShouldBe(1);

        outputBuffer[9]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[9]
            .Start
            .ShouldBe(18);

        outputBuffer[9]
            .Length
            .ShouldBe(1);

        outputBuffer[10]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[10]
            .Start
            .ShouldBe(19);

        outputBuffer[10]
            .Length
            .ShouldBe(4);

        outputBuffer[11]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[11]
            .Start
            .ShouldBe(23);

        outputBuffer[11]
            .Length
            .ShouldBe(1);

        outputBuffer[12]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[12]
            .Start
            .ShouldBe(24);

        outputBuffer[12]
            .Length
            .ShouldBe(2);

        outputBuffer[13]
            .Type
            .ShouldBe(Lexeme.OpenBrace);

        outputBuffer[13]
            .Start
            .ShouldBe(26);

        outputBuffer[13]
            .Length
            .ShouldBe(1);

        outputBuffer[14]
            .Type
            .ShouldBe(Lexeme.Literal);

        outputBuffer[14]
            .Start
            .ShouldBe(27);

        outputBuffer[14]
            .Length
            .ShouldBe(2);

        outputBuffer[15]
            .Type
            .ShouldBe(Lexeme.CloseBrace);

        outputBuffer[15]
            .Start
            .ShouldBe(29);

        outputBuffer[15]
            .Length
            .ShouldBe(1);
    }
}
