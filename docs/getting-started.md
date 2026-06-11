# Getting Started

## Installation

Install the core package from NuGet:

```shell
dotnet add package Invex.Rez
```

If you want to resolve templates inside `Microsoft.Extensions.Configuration` values, also install
the configuration package:

```shell
dotnet add package Invex.Rez.Configuration
```

Both packages target `net10.0`, `net9.0`, `net8.0`, and `netstandard2.0`, so they can be used from
modern .NET, .NET Framework, and anything else that supports .NET Standard 2.0.

## Your first template

Resolving a template takes three steps:

1. Instantiate an implementation of `IResolver` (e.g., `Resolver`).
2. Add one or more sources of variables and functions (`IResolverSource` implementations).
3. Call `Resolve()`, passing in the text to be resolved.

```csharp
using Invex.Rez.Abstraction;
using Invex.Rez.Implementation;

var resolver = new Resolver();

resolver.AddSource(new ResolverSource(new Dictionary<string, string>
{
    { "name", "World" }
}));

var greeting = resolver.Resolve("Hello, {name}!");

// greeting: "Hello, World!"
```

## Adding functions

Functions are written in templates as `{name(args)}`. A function is a
`Func<FunctionCall, string>` — it receives the raw argument text via `FunctionCall.Args` and
returns the result:

```csharp
var resolver = new Resolver();

resolver.AddSource(new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>
{
    { "shout", call => (call.Args ?? string.Empty).ToUpperInvariant() + "!!!" }
}));

var output = resolver.Resolve("{shout(hello)}");

// output: "HELLO!!!"
```

> [!NOTE]
> The argument text between the parentheses is passed to the function as a single string.
> The function itself is responsible for any parsing (e.g., splitting on commas) — no splitting
> or trimming is performed by Rez.

## Mutable sources with `ResolverStore`

`ResolverSource` is immutable — its contents are fixed at construction time. When you need to add
and remove variables or functions at runtime, use `ResolverStore`:

```csharp
var store = new ResolverStore();
var resolver = new Resolver().AddSource(store);

store.AddVariable("user", "Alice");
store.AddFunction("now", _ => DateTime.UtcNow.ToString("O"));

var line = resolver.Resolve("[{now()}] {user} logged in");

store.RemoveVariable("user");
```

Adding a variable or function with a name that already exists **replaces** the existing entry.

## Multiple sources and ordering

A resolver can have any number of sources. When resolving a placeholder, sources are queried
**in the order they were added**, and the first non-null result wins:

```csharp
var overrides = new ResolverStore();
var defaults  = new ResolverSource(new Dictionary<string, string>
{
    { "env", "production" }
});

var resolver = new Resolver()
    .AddSource(overrides)   // queried first
    .AddSource(defaults);   // queried second

resolver.Resolve("{env}");  // "production"

overrides.AddVariable("env", "development");

resolver.Resolve("{env}");  // "development" — the override wins
```

This makes it easy to layer sources: put overrides first and fallbacks last.

## What happens when something can't be resolved?

Placeholders that no source can resolve are left in the output unchanged:

```csharp
resolver.Resolve("Hello, {unknown}!");

// "Hello, {unknown}!"
```

This means a missing variable never throws — the template degrades gracefully.

## Next steps

- Learn the full template language in the [Syntax Guide](syntax.md)
- See more complete scenarios in [Examples](examples.md)
- Understand the abstractions and write custom sources in the [Developer Guide](developer-guide.md)
- Resolve templates inside app configuration with [Configuration Integration](configuration.md)

