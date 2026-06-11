namespace Invex.Rez.Tests.Implementation;

[TestFixture]
public class RezzerTests
{
    [Test]
    public void Constructor_Creates_Instance()
    {
        var instance = new Resolver();
        instance.ShouldNotBeNull();
    }

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
    public void Resolve_NoMatch_Returns_Input(string? input)
    {
        var resolver = new Resolver();
        var result = resolver.Resolve(input);

        result.ShouldBe(input);
    }

    [TestCase("{example}", "example", "value", "value")]
    [TestCase("{example}", "example", "{example}", "{example}")]
    [TestCase("{{example}}", "example", "value", "{value}")]
    [TestCase("{example}", "example", "{example}", "{example}")]
    [TestCase(@"\\{example\\}", "example", "value", @"\\{example\\}")]
    [TestCase(@"\{{example\}}", "example", "value", @"\{{example\}}")]
    public void Resolve_VariableReference_Returns_Value(string input, string key, string value, string expected)
    {
        var resolver = new Resolver();
        resolver.AddSource(new ResolverSource(new KeyValuePair<string, string>[] { new(key, value) }));

        var result = resolver.Resolve(input);

        result.ShouldBe(expected);
    }

    [Test]
    public void Resolve_MultipleVariableReferences_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new KeyValuePair<string, string>[]
        {
            new("variableName1", "value1"), new("variableName2", "value2"),
        }));

        var result = resolver.Resolve("prefix_{variableName1}_middle_{variableName2}_postfix");

        result.ShouldBe("prefix_value1_middle_value2_postfix");
    }

    [Test]
    public void Resolve_NestedVariableReference_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new KeyValuePair<string, string>[]
        {
            new("variableName1", "variableName2"),
            new("variableName2", "variableName3"),
            new("variableName3", "value"),
        }));

        var result = resolver.Resolve("prefix_{{{variableName1}}}_postfix");

        result.ShouldBe("prefix_value_postfix");
    }

    [Test]
    public void Resolve_FunctionReference_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new List<KeyValuePair<string, Func<FunctionCall, string>>>
        {
            new("functionName",
                input => input.Args == "correctParam"
                    ? "value"
                    : string.Empty),
        }));

        var result = resolver.Resolve("prefix_{functionName(correctParam)}_postfix");

        result.ShouldBe("prefix_value_postfix");
    }

    [Test]
    public void Resolve_MultipleFunctionReferences_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new List<KeyValuePair<string, Func<FunctionCall, string>>>
        {
            new("functionName1",
                input => input.Args == "correctParam"
                    ? "value1"
                    : string.Empty),
            new("functionName2",
                input => input.Args == "correctParam"
                    ? "value2"
                    : string.Empty),
        }));

        var result =
            resolver.Resolve("prefix_{functionName1(correctParam)}_middle_{functionName2(correctParam)}_postfix");

        result.ShouldBe("prefix_value1_middle_value2_postfix");
    }

    [Test]
    public void Resolve_NestedFunctionReference_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new List<KeyValuePair<string, Func<FunctionCall, string>>>
        {
            new("functionName1",
                input => input.Args == "correctParam"
                    ? "functionName2"
                    : string.Empty),
            new("functionName2",
                input => input.Args == "correctParam"
                    ? "value"
                    : string.Empty),
        }));

        var result = resolver.Resolve("prefix_{{functionName1(correctParam)}(correctParam)}_postfix");

        result.ShouldBe("prefix_value_postfix");
    }

    [Test]
    public void Resolve_MultipleNestedFunctionReferences_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new List<KeyValuePair<string, Func<FunctionCall, string>>>
        {
            new("functionName1",
                input => input.Args == "correctParam1"
                    ? "functionName2(correctParam2)"
                    : string.Empty),
            new("functionName2",
                input => input.Args == "correctParam2"
                    ? "functionName3(correctParam3)"
                    : string.Empty),
            new("functionName3",
                input => input.Args == "correctParam3"
                    ? "value"
                    : string.Empty),
        }));

        var result = resolver.Resolve("prefix_{{{functionName1(correctParam1)}}}_postfix");

        result.ShouldBe("prefix_value_postfix");
    }

    [Test]
    public void Resolve_FunctionReferenceWithVariableReference_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource([new("variableName", "correctParam")],
            new List<KeyValuePair<string, Func<FunctionCall, string>>>
            {
                new("functionName",
                    input => input.Args == "correctParam"
                        ? "value"
                        : string.Empty),
            }));

        var result = resolver.Resolve("prefix_{functionName({variableName})}_postfix");

        result.ShouldBe("prefix_value_postfix");
    }

    [Test]
    public void Resolve_FunctionReferenceWithNestedVariableReference_Returns_Value()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(
            [new("variableName1", "{variableName2}"), new("variableName2", "correctParam")],
            new List<KeyValuePair<string, Func<FunctionCall, string>>>
            {
                new("functionName",
                    input => input.Args == "correctParam"
                        ? "value"
                        : string.Empty),
            }));

        var result = resolver.Resolve("prefix_{functionName({variableName1})}_postfix");

        result.ShouldBe("prefix_value_postfix");
    }

    [Test]
    public void Resolve_CircularReferences_Throws_Exception()
    {
        var resolver = new Resolver();

        resolver.AddSource(new ResolverSource(new KeyValuePair<string, string>[]
        {
            new("variableName1", "{variableName2}"), new("variableName2", "{variableName1}"),
        }));

        Should.Throw<Exception>(() => resolver.Resolve("prefix_{variableName1}_postfix"));
    }

    [Test]
    public void Add_Remove_Resolve_Source_Returns_Input()
    {
        var resolver = new Resolver();

        var source = new ResolverSource([new("variableName", "value")],
            new List<KeyValuePair<string, Func<FunctionCall, string>>>
            {
                new("functionName",
                    input => input.Args == "correctParam"
                        ? "value"
                        : string.Empty),
            });

        resolver.AddSource(source);
        resolver.RemoveSource(source);

        var varResult = resolver.Resolve("{variableName}");
        var funcResult = resolver.Resolve("{functionName(correctParam)}");

        varResult.ShouldBe("{variableName}");
        funcResult.ShouldBe("{functionName(correctParam)}");
    }
}
