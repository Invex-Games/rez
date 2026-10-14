# Introduction

Rez is a small templating language and a .NET library that resolves it. This page explains what
problem Rez solves, when it is (and is not) the right tool, and the handful of concepts the rest of
the documentation builds on.

## What is Rez?

Rez consists of two parts:

- A specification for a simple templating language, where variables are written as `{name}` and
  functions as `{name(args)}`.
- A library implementing the parsing and resolving of Rez template text in .NET applications.

Rez takes heavy inspiration from [Mustache](https://mustache.github.io/), a popular templating
language used in many web applications. One of the key reasons for creating Rez instead of using an
existing solution was the need for terse, nestable tokens that are still easy to read: in Rez,
`{greeting{locale}}` first resolves `{locale}` and then looks up the resulting variable name.

## When to use Rez

Rez fits well when:

- **Text is short and mostly static.** Application strings, log and notification messages, file
  names, URLs and connection strings with a few substituted values.
- **Values come from several places.** Defaults, per-environment overrides, user input and
  configuration can each be a separate source, layered in priority order.
- **Configuration values reference each other.** With `Invex.Rez.Configuration`, a value such as
  `"{BaseUrl}/orders"` is resolved transparently when it is read.
- **Missing values should not be fatal.** A placeholder that cannot be resolved stays in the output
  as written, so partially resolved text is still readable.

Rez is not a good fit when:

- **You need control flow.** There are no loops, conditionals or sections; anything like that has
  to live inside a function you write.
- **You generate markup or code.** Rez does no HTML encoding or escaping of resolved values; use a
  view engine such as Razor, or a full template engine, for those.
- **You need structured arguments.** Function arguments arrive as one raw string, so typed or
  quoted arguments must be parsed by your function.

## Key concepts at a glance

| Concept | What it is |
| --- | --- |
| Template | Any string. Text outside braces is copied through unchanged. |
| Variable | `{name}`, replaced with a value supplied by a source. |
| Function | `{name(args)}`, replaced with the result of a delegate that receives the raw argument text as a <xref:Invex.Rez.Abstraction.FunctionCall>. |
| Resolver | An <xref:Invex.Rez.Abstraction.IResolver> (by default <xref:Invex.Rez.Implementation.Resolver>) that resolves a template against its sources. |
| Source | An <xref:Invex.Rez.Abstraction.IResolverSource> that supplies variables and functions by name, or `null` to defer to the next source. |
| Store | An <xref:Invex.Rez.Abstraction.IResolverStore>, a source whose entries can be added and removed at runtime. |
| Escaping | A backslash before a brace (`\{`, `\}`) stops it from opening or closing a placeholder. |

## Design principles

- **Terse and readable.** Placeholders are a pair of braces and a name, with nothing else to learn
  for the common case.
- **Inside-out, one step at a time.** The innermost placeholder resolves first, then the template is
  resolved again until nothing changes. Nesting and recursion fall out of this rule, with a hard
  limit of 4096 passes so a cycle such as `a = {b}`, `b = {a}` throws instead of looping forever.
- **First match wins.** Sources are queried in the order they were added, so layering overrides and
  defaults is a matter of registration order.
- **Degrade gracefully.** Unknown variables and functions never throw; the placeholder is left in
  the output unchanged, and `null` input resolves to `null`.
- **Functions own their arguments.** Rez passes the text between the parentheses untouched, without
  splitting or trimming, so each function decides its own argument convention.
- **Small and allocation-conscious.** The resolver works on pooled buffers and spans to minimize
  allocations.

## Packages

| Package | Description |
| --- | --- |
| `Invex.Rez` | The core templating library: resolver, sources, and store |
| `Invex.Rez.Configuration` | Integration with `Microsoft.Extensions.Configuration` and dependency injection |

Both packages target `net10.0` and `netstandard2.0`.

## Next steps

- [Getting Started](getting-started.md): install the package and resolve your first template
- [Syntax Guide](syntax.md): the complete template language reference
- [Examples](examples.md): worked examples from simple to advanced
- [Configuration Integration](configuration.md): resolving templates in app configuration
- [Developer Guide](developer-guide.md): the library's abstractions and how to extend them
- [API Reference](../api/index.md): generated reference documentation for every public type
