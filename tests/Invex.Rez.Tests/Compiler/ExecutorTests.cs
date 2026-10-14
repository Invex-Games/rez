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
}
