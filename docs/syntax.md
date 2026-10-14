# Syntax Guide

This page is the complete reference for the Rez template language: variables, functions, nesting,
recursion and escaping. Use it when writing templates or when you need to know exactly how a
template will resolve.

In Rez, variables and functions are enclosed within curly braces:

- Variables: `{variableName}`
- Functions: `{functionName()}` or `{functionName(parameters)}`

All examples on this page use the [example data](#example-data-for-this-page) listed at the bottom.

## Plain text

Any plain text that doesn't get resolved into variables or functions will always resolve as itself:

```text
in  > here are some words
out > here are some words
```

## Variables

### Resolve a variable

Insert a variable using curly braces:

```text
in  > {variable1}
out > variableValue1
```

### Resolve nested variables

Variables can be nested and will always be resolved from the innermost to the outermost,
left to right:

```text
in  > {variable{number2}}
--- > {variable2}
out > variableValue2
```

Multiple placeholders in one template are resolved left to right:

```text
in  > {variable1} {variable2} {variable3}
--- > variableValue1 {variable2} {variable3}
--- > variableValue1 variableValue2 {variable3}
out > variableValue1 variableValue2 variableValue3
```

### Recursive variables

A variable's value may itself contain placeholders, which are resolved in turn:

```text
in  > {chain1}
--- > {chain2}
out > variableValue1

Variables:
chain1 = {chain2}
chain2 = variableValue1
```

> [!NOTE]
> Recursion is limited to a depth of 4096 to prevent infinite loops. Depth counts how many times
> a resolved value leads to another resolution (`{chain1}` above has depth 2), not how many
> placeholders a template contains, so a template with any number of independent placeholders is
> fine. A cycle whose output keeps changing (e.g., `a = {b}` and `b = {a}`) or a value that keeps
> expanding (e.g., `a = x{a}`) exceeds the limit and throws a `RezResolutionException`. A variable that resolves to its own placeholder (e.g., `a = {a}`) does not:
> the text stops changing, so `{a}` is returned unchanged.

### When a variable is not found

When a variable is not found, it resolves as the input, including the curly braces:

```text
in  > {variable4}
out > {variable4}
```

## Functions

### Resolve a function

Call a function with curly braces and parentheses:

```text
in  > {date()}
out > 2023-04-05
```

Functions can also accept parameters:

```text
in  > {fancyFunction(ooh!)}
out > ***ooh!***
```

### Resolve a function with multiple parameters

Call a function with multiple parameters by separating them with commas:

```text
in  > wo{repeatFunction(lo,2)}
out > wololo
```

> [!IMPORTANT]
> The entire text between the parentheses is passed to the function as a single string —
> the function itself decides how to split and interpret it.

Whitespace is NOT ignored in function parameters, and is treated as part of the parameter:

```text
in  > {andFunction(apple,banana)}
out > apple&banana

in  > {andFunction(apple, banana)}
out > apple& banana
```

> [!TIP]
> This usually doesn't matter for parameters that are numbers, but it's good to not get into the
> habit of adding whitespace to function parameters, as is the case with many programming
> languages.

### Nesting variables in function parameters

Because placeholders resolve inside-out, variables can be used as function parameters:

```text
in  > {fancyFunction({variable1})}
--- > {fancyFunction(variableValue1)}
out > ***variableValue1***
```

### When a function is not found

When a function is not found, it resolves as the input, including the curly braces and
parentheses:

```text
in  > {notAFunction(ooh!)}
out > {notAFunction(ooh!)}
```

## Escaping

Use a backslash before a brace to prevent it from being treated as a placeholder delimiter:

```text
in  > \{variable1\}
out > \{variable1\}
```

This works for functions too:

```text
in  > \{escapedFunction(parameter)\}
out > \{escapedFunction(parameter)\}
```

> [!NOTE]
> The backslash is **preserved** in the output — escaping prevents resolution, it does not
> strip the escape characters. This means escaping remains stable across repeated resolves:
> the escaped text never resolves, no matter how many times the output is fed back through a
> resolver. Strip the backslashes yourself if your final output should not contain them.

Escaped braces inside an unescaped placeholder become part of the name itself:

```text
in  > \{{example\}}
out > \{value

Variables:
example\} = value
```

(The first `\{` is literal text; the unescaped `{` opens a placeholder whose name is
`example\}`, which is closed by the final unescaped `}`.)

### Escaping in JSON files

If writing templates inside `.json` files, you will need to escape the backslashes as well:

```json
{
  "myTemplate": "\\{escapedVariable\\}"
}
```

## Example data for this page

Variables used in examples:

```text
variable1: variableValue1
variable2: variableValue2
variable3: variableValue3
number1:   1
number2:   2
number3:   3
```

Functions used in examples:

```text
date():                      [the current date]
fancyFunction(input):        ***input***
andFunction(input1,input2):  input1&input2
repeatFunction(input,count): [repeat input count times]
```

## See also

- [Getting Started](getting-started.md): register variables and functions in code
- [Examples](examples.md): templates applied to complete scenarios
- [Developer Guide](developer-guide.md#how-resolution-works): how the resolution pipeline produces these results
- <xref:Invex.Rez.Abstraction.IResolver> and <xref:Invex.Rez.Abstraction.FunctionCall>
