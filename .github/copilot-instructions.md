# Copilot Instructions

Guidance for AI agents working in **Invex.Rez** — a small, focused C# library implementing the
parsing and resolving of Rez template text (`{variable}` / `{function(args)}`) in .NET
applications, plus an integration package for `Microsoft.Extensions.Configuration`. Keep changes
focused and defer to the linked docs for detail.

## What's in the repo

| Project | Role | Target frameworks |
|---------|------|-------------------|
| `Invex.Rez` | The core library: `IResolver`/`Resolver`, `IResolverSource`/`ResolverSource`, `IResolverStore`/`ResolverStore`, `FunctionCall`, and the internal compiler pipeline | `net10.0;net9.0;net8.0;netstandard2.0` |
| `Invex.Rez.Configuration` | `ConfigResolverSource`, `IResolvableConfig`, and the `AddResolvableConfiguration()` DI extension | `net10.0;net9.0;net8.0;netstandard2.0` |
| `Invex.Rez.Tests` | NUnit test suite covering both the public surface and the internal compiler | `net10.0;net9.0;net8.0;net48` |
| `_atom` | Atom build definition (`IBuild.cs`) that generates the GitHub Actions workflows | `net10.0` |

Sources live under `src/`, tests under `tests/`, the Atom build definition under `_atom/`, and
the DocFX documentation site is configured by `docfx.json` with content in `docs/`, `api/`,
`index.md`, and `toc.yml`.

## Build & language specifics

- **.NET 10 SDK** is required (`global.json`); the library packages also target `netstandard2.0`
  and tests also run on `net48`, so changes must compile against older surface areas — the
  **Polyfill**, `System.Buffers`, and `System.Memory` packages cover the gaps. Use
  `#if NET8_0_OR_GREATER` conditionals where APIs differ (see `Resolver.FunctionRegex` and the
  `GetValueOrDefault` call sites for the established pattern).
- C# `LangVersion` 14, `ImplicitUsings` and `Nullable` enabled, `TreatWarningsAsErrors` on.
- Global usings live in each project's `_usings.cs` — add shared usings there, not per-file.
  `Invex.Rez/_usings.cs` also declares `InternalsVisibleTo("Invex.Rez.Tests")`.
- `GenerateDocumentationFile` is on. `CS1591` is in the repo-wide `NoWarn`, so missing XML docs
  won't fail the build — but the convention is that **every public type and member has
  comprehensive XML docs anyway**. Keep them accurate to the implementation.

Build and test the whole solution:

```shell
dotnet build Invex.Rez.slnx
dotnet test Invex.Rez.slnx
```

Build the docs site:

```shell
docfx docfx.json          # add --serve to preview locally
```

## Architecture overview

Two layers, deliberately separated:

- **Public surface** (`Invex.Rez.Abstraction` + `Invex.Rez.Implementation`):
  - `IResolver` / `Resolver` — resolves a template by querying registered sources **in
    registration order, first non-null result wins**. For each placeholder it first tries the
    function shape (`name(args)`), then falls back to a variable lookup.
  - `IResolverSource` / `ResolverSource` — an immutable map of variables and functions.
  - `IResolverStore` / `ResolverStore` — a mutable source; adding an existing name **replaces**
    it.
  - `FunctionCall` — carries the raw argument text (`Args`) to a function delegate. Functions own
    all argument parsing; Rez never splits or trims.
- **Internal compiler** (`Invex.Rez.Compiler`): `Executor` drives a
  lex → tokenize → render loop until the output stabilizes, operating on `ArrayPool`-rented
  buffers and spans. `Lexer` splits text into literals/braces (backslash escapes), `Tokenizer`
  pairs braces innermost-first into variable tokens, `Renderer` substitutes one placeholder per
  pass.

The Configuration package wraps `IConfigurationRoot`/`IConfigurationSection` so values are
resolved **on read** and pass through **unmodified on write**.

### Behavioral contracts (do not break these)

- **Null in, null out**: `Resolve(null)` returns `null`.
- **Unresolvable placeholders are left in the output unchanged** — missing variables never throw.
- **Sources are queried in registration order**; first non-null result wins.
- **Resolution is recursive and inside-out, left-to-right**, with a hard limit of **4096**
  passes — exceeding it throws.
- **Escapes are preserved**: `\{example\}` resolves to `\{example\}` (backslashes are *not*
  stripped from the output). Don't "fix" this to strip them — the test suite pins it.
- **Function delegates must never return null**; sources return `null` from
  `ResolveVariable`/`ResolveFunction` only to defer to the next source.
- The compiler must remain allocation-light: pooled buffers, span slicing, no per-pass string
  churn beyond what resolution requires.

## Key design rules

- Consumers depend on the `Invex.Rez.Abstraction` interfaces; the compiler namespace stays
  `internal` (tests reach it via `InternalsVisibleTo`).
- Keep the public surface minimal — this library does one thing. Push back on scope creep.
- Annotate every new public type with `[PublicAPI]` — the in-repo
  `Invex.RepoUtils.PublicApiAnalyzers` flags anything missing, and warnings are errors.
- Behavior changes to the template language must be reflected in **three places**: the XML docs,
  the `docs/syntax.md` reference, and the test suite. They are kept deliberately in sync.

## Atom workflows

The GitHub Actions workflow YAML under `.github/workflows/` is **generated** from the Atom build
definition in `_atom/IBuild.cs` (workflows: `Validate`, `Build`, `Dependabot Enable auto-merge`,
`Cleanup Prereleases`, plus the Dependabot config).

Whenever you change anything that affects the workflows — targets, workflow definitions,
triggers, options, or params/secrets — regenerate the YAML:

```shell
atom gen
```

(equivalently `dotnet run --project _atom -- gen`). Commit the regenerated `.github/workflows/`
files alongside your `_atom/` changes; never hand-edit the generated YAML.

A drift between `_atom/IBuild.cs` and the committed YAML should be treated as a missing
`atom gen` run.

## Conventions

- Add XML doc comments to all public types and members. Match the existing `<summary>` /
  `<param>` / `<remarks>` / `<example>` style, and keep docs **accurate to the implementation**
  (e.g. exact escaping and replace-vs-noop semantics).
- Use Conventional Commits — the prefix drives versioning:

  | Prefix | Version bump |
  |--------|--------------|
  | `breaking:` / `major:` | Major |
  | `feat:` / `feature:` / `minor:` | Minor |
  | `fix:` / `patch:` | Patch |
  | `semver-none` / `semver-skip` | No bump |

- When adding user-facing features, update the relevant `docs/` page and `README.md`. The README
  is packed into the NuGet package.

## Testing

- Tests use **NUnit** with **Shouldly** (plus **FakeItEasy** and **Verify.NUnit** available).
- Tests mirror the source layout: `tests/Invex.Rez.Tests/Compiler/` covers the internal pipeline
  (`Executor`, `Lexer`, `Tokenizer`, `Renderer`), `tests/Invex.Rez.Tests/Implementation/` covers
  the public types.
- Prefer `[TestCase]`-driven tests showing input → expected output, following the existing style
  (e.g., `ExecutorTests.Resolve_VariableReference_Returns_Value`).
- The Validate workflow's `CheckPrForBreakingChanges` target inspects changes to
  `tests/**/*.verified.txt` on PRs — if Verify snapshot tests are added, treat unexpected diffs
  in `*.verified.txt` files as unintentional API/behavior changes. To accept a valid change,
  overwrite the `*.verified.txt` with the matching `*.received.txt`, delete the received file,
  and re-run `dotnet test`.
- CI runs the test matrix across `net8.0`/`net9.0`/`net10.0` on Ubuntu and Windows, plus `net48`
  on Windows — keep tests free of platform- and TFM-specific assumptions.

## Adding a feature to the template language

1. Update the internal compiler (`Lexer`/`Tokenizer`/`Renderer`/`Executor`) — keep the
   one-substitution-per-pass model and pooled-buffer approach intact.
2. Add `[TestCase]`-style tests at both the compiler level (`Compiler/`) and the public level
   (`Implementation/RezzerTests.cs`).
3. Update the XML docs on the affected public members.
4. Update `docs/syntax.md` (the language reference) and any other affected `docs/` pages.
5. If the public surface changed, update the README feature list.

## Defer to the docs

For anything beyond the above, prefer these over duplicating detail:

- `README.md` — package overview and quick start.
- `docs/index.md` — what Rez is and why it exists.
- `docs/getting-started.md` — installation, first template, sources and ordering.
- `docs/syntax.md` — the complete template language reference (the source of truth for
  language behavior, including escaping and not-found semantics).
- `docs/examples.md` — worked examples from simple to advanced.
- `docs/developer-guide.md` — abstractions, custom sources, pipeline internals, thread safety.
- `docs/configuration.md` — `ConfigResolverSource`, `IResolvableConfig`, and DI registration.
- `api/index.md` — API reference landing page (generated reference from XML docs).

