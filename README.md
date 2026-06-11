# Rez

A library to implement the parsing and resolving of template text in .NET applications.

## The Basics

Rez consists of two main parts:

- A specification for a simple templating language
- A library implementing the parsing and resolving of Rez template text in .NET applications

Rez takes heavy inspiration from [Mustache](https://mustache.github.io/), a popular templating
language used in many web applications. One of the key reasons for creating Rez instead of using
an existing solution was the need for terse, nestable tokens that are still easy to read.

```csharp
var resolver = new Resolver();
resolver.AddSource(new ResolverSource(new Dictionary<string, string> { { "name", "World" } }));

var greeting = resolver.Resolve("Hello, {name}!");

// greeting: "Hello, World!"
```

## Features

- **Variables** — `{name}` is replaced with its value
- **Functions** — `{repeat(abc,3)}` invokes a function with the raw argument text
- **Nesting** — `{variable{number}}` resolves inside-out, left-to-right
- **Recursion** — resolved values may themselves contain placeholders (limit: 4096 recursions)
- **Escaping** — `\{literal\}` excludes braces from resolution (the backslashes are preserved in the output)
- **Pluggable sources** — layer any number of variable/function sources, first match wins
- **Graceful fallback** — unresolvable placeholders are left in the output unchanged
- **High performance** — pooled buffers and span-based parsing minimize allocations
- **Configuration integration** — resolve templates inside `Microsoft.Extensions.Configuration`
  values

## Installation

```shell
dotnet add package Invex.Rez
```

For `Microsoft.Extensions.Configuration` integration:

```shell
dotnet add package Invex.Rez.Configuration
```

Both packages target `net10.0`, `net9.0`, `net8.0`, and `netstandard2.0`.

## Quick example

> appsettings.json

```json
{
  "animal1": "fox",
  "animal2": "dog",
  "color": "brown",
  "description": "lazy"
}
```

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var resolver = new Resolver();
resolver.AddSource(new ConfigResolverSource(config));

var output = resolver.Resolve("The quick {color} {animal1} jumped over the {description} {animal2}.");

// output: "The quick brown fox jumped over the lazy dog."
```

## Documentation

- [Introduction](docs/index.md) — what Rez is and why it exists
- [Getting Started](docs/getting-started.md) — install and resolve your first template
- [Syntax Guide](docs/syntax.md) — the complete template language reference
- [Examples](docs/examples.md) — worked examples from simple to advanced
- [Developer Guide](docs/developer-guide.md) — abstractions, custom sources, and how resolution works
- [Configuration Integration](docs/configuration.md) — resolving templates in app configuration
- [API Reference](api/index.md) — generated reference for every public type

## Where did the name come from?

The name Rez is a play on the word "res" (short for "resolution"), which is the process of
resolving a template. The name was chosen as it is short and memorable and is not too similar to
other names in the .NET ecosystem.

## License

See [LICENSE.txt](LICENSE.txt).

