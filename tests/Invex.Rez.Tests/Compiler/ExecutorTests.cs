namespace Invex.Rez.Tests.Compiler;

[TestFixture]
[SuppressMessage("ReSharper", "ConvertToLocalFunction")]
public class ExecutorTests
{
    private static readonly Func<string, string> NoMatch = s => $"{{{s}}}";

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("example")]
    [TestCase("{example}")]
    [TestCase("{{example}}")]
    [TestCase("{{example()}}")]
    [TestCase("{{example(something)}}")]
    [TestCase("{{{example}}}")]
    [TestCase("{{{example(something)}}}")]
    [TestCase(@"\{{{example\}}}")]
    [TestCase(@"\{{{example(something)\}}}")]
    public void Execute_NoMatch_Returns_Input(string? input)
    {
        var result = Executor.Execute(input, NoMatch);

        result.ShouldBe(input);
    }

    [TestCase("{example}", "example", "value", "value")]
    [TestCase("{example}", "example", "{example}", "{example}")]
    [TestCase("{{example}}", "example", "value", "{value}")]
    [TestCase("{example}", "example", "{{value}}", "{{value}}")]
    [TestCase(@"\{example\}", "example", "value", @"\{example\}")]
    [TestCase(@"\{{example\}}", "example\\}", "value", @"\{value")]
    public void Resolve_VariableReference_Returns_Value(string input, string key, string value, string expected)
    {
        var result = Executor.Execute(input,
            s => s == key
                ? value
                : NoMatch(s));

        result.ShouldBe(expected);
    }

    [Test]
    public void Resolve_MultipleVariables_Returns_Value()
    {
        Func<string, string> resolution = s => s switch
        {
            "variableName1" => "value1",
            "variableName2" => "value2",
            _ => NoMatch(s),
        };

        var result = Executor.Execute("prefix_{variableName1}_middle_{variableName2}_postfix", resolution);

        result.ShouldBe("prefix_value1_middle_value2_postfix");
    }

    [Test]
    public void Resolve_NestedVariable_Returns_Value()
    {
        Func<string, string> resolution = s => s switch
        {
            "key1" => "{key2}",
            "key2" => "{key3}",
            "key3" => "value",
            _ => NoMatch(s),
        };

        var result = Executor.Execute("{key1}", resolution);

        result.ShouldBe("value");
    }

    [TestCase(4095)]
    [TestCase(4096)]
    [TestCase(4097)]
    [TestCase(100_000)]
    public void Execute_LongTemplate_Returns_Value(int length)
    {
        var padding = new string('a', length);
        Func<string, string> resolution = s => s == "key" ? "value" : NoMatch(s);

        var result = Executor.Execute($"{padding}{{key}}{padding}", resolution);

        result.ShouldBe($"{padding}value{padding}");
    }

    [TestCase(4095)]
    [TestCase(4096)]
    [TestCase(4097)]
    [TestCase(100_000)]
    public void Execute_LongResolvedValue_Returns_Value(int length)
    {
        var value = new string('v', length);
        Func<string, string> resolution = s => s == "key" ? value : NoMatch(s);

        var result = Executor.Execute("prefix_{key}_postfix", resolution);

        result.ShouldBe($"prefix_{value}_postfix");
    }

    [Test]
    public void Execute_LongNestedResolution_Returns_Value()
    {
        var chunk = new string('c', 3000);

        Func<string, string> resolution = s => s switch
        {
            "outer" => "{inner}-{inner}-{inner}",
            "inner" => chunk,
            _ => NoMatch(s),
        };

        var result = Executor.Execute("{outer}", resolution);

        result.ShouldBe($"{chunk}-{chunk}-{chunk}");
    }

    [Test]
    public void Execute_ManyPlaceholders_Returns_Value()
    {
        var input = string.Concat(Enumerable.Repeat("{a}{b}", 1000));
        Func<string, string> resolution = s => s switch
        {
            "a" => "1",
            "b" => "2",
            _ => NoMatch(s),
        };

        var result = Executor.Execute(input, resolution);

        result.ShouldBe(string.Concat(Enumerable.Repeat("12", 1000)));
    }

    [Test]
    public void Execute_SelfReference_Stabilises()
    {
        var calls = 0;

        Func<string, string> resolution = s =>
        {
            calls++;

            return s == "a" ? "{a}" : NoMatch(s);
        };

        var result = Executor.Execute("{a}", resolution);

        result.ShouldBe("{a}");
        calls.ShouldBe(1);
    }

    [Test]
    public void Execute_ChangingCycle_Throws_RezResolutionException()
    {
        Func<string, string> resolution = s => s switch
        {
            "a" => "{b}",
            "b" => "{a}",
            _ => NoMatch(s),
        };

        Should.Throw<RezResolutionException>(() => Executor.Execute("{a}", resolution));
    }

    [TestCase(10_000)]
    public void Execute_ManyFlatPlaceholders_DoesNotThrow(int count)
    {
        var input = string.Concat(Enumerable.Repeat("{a}", count));
        Func<string, string> resolution = s => s == "a" ? "1" : NoMatch(s);

        var result = Executor.Execute(input, resolution);

        result.ShouldBe(new('1', count));
    }

    [Test]
    public void Execute_ValueWithManyPlaceholders_DoesNotThrow()
    {
        var value = string.Concat(Enumerable.Repeat("{b}", Executor.MaxResolutionDepth + 100));
        Func<string, string> resolution = s => s switch
        {
            "a" => value,
            "b" => "2",
            _ => NoMatch(s),
        };

        var result = Executor.Execute("{a}", resolution);

        result.ShouldBe(new('2', Executor.MaxResolutionDepth + 100));
    }

    [Test]
    public void Execute_ChainAtMaxDepth_Returns_Value()
    {
        Func<string, string> resolution = s =>
        {
            var n = int.Parse(s);

            return n < Executor.MaxResolutionDepth ? $"{{{n + 1}}}" : "done";
        };

        Executor.Execute("{1}", resolution).ShouldBe("done");
    }

    [Test]
    public void Execute_ChainBeyondMaxDepth_Throws_RezResolutionException()
    {
        Func<string, string> resolution = s =>
        {
            var n = int.Parse(s);

            return n <= Executor.MaxResolutionDepth ? $"{{{n + 1}}}" : "done";
        };

        Should.Throw<RezResolutionException>(() => Executor.Execute("{1}", resolution));
    }

    [Test]
    public void Execute_GrowingExpansion_Throws_RezResolutionException()
    {
        Func<string, string> resolution = s => s == "a" ? "x{a}" : NoMatch(s);

        Should.Throw<RezResolutionException>(() => Executor.Execute("{a}", resolution));
    }

    [Test]
    public void Execute_NestedPlaceholderInResolvedValue_Returns_Value()
    {
        Func<string, string> resolution = s => s switch
        {
            "outer" => "{key{n}}",
            "n" => "1",
            "key1" => "ok",
            _ => NoMatch(s),
        };

        Executor.Execute("{outer}", resolution).ShouldBe("ok");
    }

    [TestCase(100_000)]
    public void Execute_ManyUnmatchedOpenBraces_Returns_Input(int count)
    {
        var input = new string('{', count);

        Executor.Execute(input, NoMatch).ShouldBe(input);
    }

    [TestCase(100_000)]
    public void Execute_UnmatchedOpenBracesAroundVariable_Returns_Value(int count)
    {
        var braces = new string('{', count);
        Func<string, string> resolution = s => s == "a" ? "value" : NoMatch(s);

        Executor.Execute($"{braces}{{a}}", resolution).ShouldBe($"{braces}value");
    }

    [TestCase(20_000)]
    public void Execute_DeeplyNestedBraces_Returns_Input(int depth)
    {
        var input = $"{new string('{', depth)}a{new string('}', depth)}";

        Executor.Execute(input, _ => null).ShouldBe(input);
    }
}
