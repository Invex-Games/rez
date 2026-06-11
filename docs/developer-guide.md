# Developer Guide

Rez allows you to resolve variables and functions embedded within strings, making it easier to
dynamically generate text. This guide explains the library's abstractions, the built-in
implementations, and how to extend Rez with custom sources.

## Architecture at a glance

```
            ┌────────────────────────────────────────────┐
 template ─►│ IResolver (Resolver)                       │─► output
            │                                            │
            │   queries sources in order, first match    │
            │   wins, repeats until output stabilizes    │
            └──────┬─────────────┬─────────────┬─────────┘
                   ▼             ▼             ▼
            IResolverSource  IResolverSource  IResolverSource
            (ResolverSource) (ResolverStore)  (ConfigResolverSource, custom, ...)
```

## Core abstractions

All abstractions live in the `Invex.Rez.Abstraction` namespace.

### `IResolver`

The entry point. Resolves variables and functions in a template into an output string.

| Member | Description |
| --- | --- |
| `AddSource(IResolverSource)` | Registers a source. Order matters — sources are queried in registration order. Returns the resolver for chaining. |
| `RemoveSource(IResolverSource)` | Unregisters a source. |
| `Resolve(string?)` | Resolves the template. Returns `null` for `null` input. Unresolvable placeholders are left intact. |

### `IResolverSource`

Provides variables and functions to a resolver.

| Member | Description |
| --- | --- |
| `ResolveVariable(string name)` | Returns the variable's value, or `null` if this source can't resolve it (deferring to the next source). |
| `ResolveFunction(string name)` | Returns a `Func<FunctionCall, string>`, or `null` if this source can't resolve it. |

Names never include the template delimiters: for `{color}` the name is `"color"`, and for
`{repeat(abc,3)}` the name is `"repeat"`.

### `IResolverStore`

Extends `IResolverSource` with mutation: `AddVariable`, `AddFunction`, `RemoveVariable`,
`RemoveFunction`. Adding an entry whose name already exists replaces it.

### `FunctionCall`

A readonly record struct passed to function delegates. Its single property, `Args`, contains the
raw text between the parentheses — for `{repeat(abc,3)}` that's `"abc,3"`. The function is
responsible for all parsing; Rez performs no splitting or trimming.

## Built-in implementations

All implementations live in the `Invex.Rez.Implementation` namespace.

| Type | Use when... |
| --- | --- |
| `Resolver` | You need the default `IResolver`. |
| `ResolverSource` | Your variables/functions are known up front (immutable, constructor-initialized). |
| `ResolverStore` | You need to add and remove variables/functions at runtime (mutable). |
| `ConfigResolverSource` | Your variables come from `Microsoft.Extensions.Configuration` (in the `Invex.Rez.Configuration` package — see [Configuration Integration](configuration.md)). |

## Using Rez

To resolve the variables and functions in a template:

1. Instantiate an implementation of `IResolver` (e.g., `Resolver`).
2. Add sources for variable and function resolution, in the form of `IResolverSource`
   implementations.
3. Call the `Resolve()` method on the `IResolver` instance, passing in the text to be resolved.

The easiest way is to directly call `Resolver.Resolve()` with some variables:

```csharp
var resolver = new Resolver();
resolver.AddSource(new ResolverSource(new Dictionary<string, string> { { "name", "World" } }));
var input = "Hello, {name}!";

var greeting = resolver.Resolve(input);

// greeting: "Hello, World!"
```

## Writing a custom `IResolverSource`

Implement `IResolverSource` to pull values from anywhere — environment variables, a database,
an HTTP service, computed values, and so on:

```csharp
public sealed class EnvironmentResolverSource : IResolverSource
{
    public string? ResolveVariable(string name) =>
        Environment.GetEnvironmentVariable(name);

    // This source provides no functions
    public Func<FunctionCall, string>? ResolveFunction(string name) =>
        null;
}
```

```csharp
var resolver = new Resolver()
    .AddSource(new EnvironmentResolverSource());

resolver.Resolve("Running on {COMPUTERNAME}");
```

Guidelines for implementations:

- Return `null` from `ResolveVariable`/`ResolveFunction` when the name is unknown — this lets the
  resolver fall through to the next source.
- Never return `null` *from a function delegate* — return an empty string for a blank result.
- Don't include braces, parentheses, or arguments in the names you match against.

## How resolution works

Understanding the pipeline helps explain the behavior described in the
[Syntax Guide](syntax.md):

1. **Lexing** — the input is scanned into literals, open braces, and close braces. A backslash
   escapes the following character, so escaped braces become literal text.
2. **Tokenizing** — braces are paired into variable tokens. For nested braces, the *innermost*
   pair forms the token, which is why nesting resolves inside-out. Unmatched braces fall back to
   literal text.
3. **Rendering** — the first resolvable placeholder (left to right) is substituted with its value.
4. **Repeat** — the pipeline runs again on the new text, until a pass produces no change. This is
   what enables nested and recursive resolution. A hard limit of 4096 passes guards against
   infinite recursion (e.g., a variable that resolves to itself) — exceeding it throws an
   exception.

For each placeholder, the resolver first checks whether the contents match the function shape
(`name(args)`); if a source resolves the function, its delegate is invoked with a `FunctionCall`
containing the argument text. Otherwise the contents are treated as a variable name. If nothing
resolves, the placeholder is left in the output unchanged.

The pipeline operates on pooled buffers (`ArrayPool`) and spans, so resolution does not allocate
per pass.

### Thread safety

`Resolver`, `ResolverSource`, and `ResolverStore` are not synchronized. Concurrent `Resolve()`
calls against a fixed set of sources are safe, but mutating sources (`AddSource`,
`AddVariable`, etc.) concurrently with resolution is not. Configure the resolver up front, or
provide your own synchronization if you need runtime mutation under concurrency.

## Tips and best practices

- **Source ordering matters.** Rez searches sources in the order they were added. If a variable
  or function is found in multiple sources, Rez uses the first one it encounters — put overrides
  first and fallbacks last.
- **Use custom `IResolverSource` implementations** to provide additional variables and functions
  or to override existing ones. This is useful when you want to extend the template language or
  provide domain-specific functionality.
- **Escape deliberately.** When creating text templates, ensure that variables and functions are
  enclosed within curly braces, and use escape characters (`\{`, `\}`) when braces should not be
  resolved. Note that the backslashes are preserved in the output — strip them yourself if the
  final text should not contain them.
- **Prefer `ResolverSource` for fixed data** and `ResolverStore` when values change at runtime.
- **Functions own their argument parsing.** Decide on a convention (e.g., comma-separated, no
  whitespace) and document it for template authors — whitespace is part of the argument text.

By following these guidelines, you'll be able to effectively use Rez to create dynamic text
templates that can be easily maintained and updated.


