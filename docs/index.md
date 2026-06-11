# Introduction

## What is Rez?

Rez consists of two main parts:

- A specification for a simple templating language
- A library implementing the parsing and resolving of Rez template text in .NET applications

Rez takes heavy inspiration from [Mustache](https://mustache.github.io/), a popular templating
language used in many web applications.

One of the key reasons for creating Rez instead of using an existing solution was the need for
terse, nestable tokens that are still easy to read.

## A 10-second example

```csharp
var resolver = new Resolver();
resolver.AddSource(new ResolverSource(new Dictionary<string, string> { { "name", "World" } }));

var greeting = resolver.Resolve("Hello, {name}!");

// greeting: "Hello, World!"
```

## Key features

- **Terse, readable syntax** — variables are written as `{name}` and functions as `{name(args)}`
- **Nesting** — placeholders can be nested (e.g., `{variable{number}}`) and are resolved
  inside-out, left-to-right
- **Recursion** — resolved values may themselves contain placeholders, which are resolved in turn
  (with a hard limit of 4096 recursions to prevent infinite loops)
- **Escaping** — braces can be escaped with a backslash (`\{notAVariable\}`) to exclude them
  from resolution
- **Pluggable sources** — variables and functions are supplied by any number of
  `IResolverSource` implementations, queried in order with first-match-wins semantics
- **Graceful fallback** — placeholders that cannot be resolved are left in the output unchanged
- **High performance** — the resolution pipeline operates on pooled buffers and spans to minimize
  allocations
- **Configuration integration** — the optional `Invex.Rez.Configuration` package resolves
  templates inside `Microsoft.Extensions.Configuration` values transparently

## Packages

| Package | Description |
| --- | --- |
| `Invex.Rez` | The core templating library: resolver, sources, and store |
| `Invex.Rez.Configuration` | Integration with `Microsoft.Extensions.Configuration` and dependency injection |

Both packages target `net10.0`, `net9.0`, `net8.0`, and `netstandard2.0`.

## Where did the name come from?

The name Rez is a play on the word "res" (short for "resolution"), which is the process of
resolving a template. The name was chosen as it is short and memorable and is not too similar to
other names in the .NET ecosystem.

## Where to go next

- [Getting Started](getting-started.md) — install the package and resolve your first template
- [Syntax Guide](syntax.md) — the complete template language reference
- [Examples](examples.md) — worked examples from simple to advanced
- [Developer Guide](developer-guide.md) — the library's abstractions and how to extend them
- [Configuration Integration](configuration.md) — resolving templates in app configuration
- [API Reference](../api/index.md) — generated reference documentation for every public type


