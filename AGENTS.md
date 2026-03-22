# AI Agent Instructions

## Project overview

DataFilters is a .NET library (C#) that converts strings into generic `IFilter` / `IOrder` objects using a syntax inspired by Lucene and elastic queries. It targets .NET SDK 10.0 (see `global.json`).

### Solution structure

| Path | Role |
|------|------|
| `src/DataFilters/` | Core library — parsing, `IFilter`, `Filter`, `MultiFilter`, `Order` |
| `src/Datafilters.Expressions/` | Converts `IFilter` → `Expression<Func<T,bool>>` |
| `src/DataFilters.Queries/` | Converts `IFilter` → `IWhereClause` (SQL) |
| `test/DataFilters.UnitTests/` | Core unit tests |
| `test/DataFilters.Expressions.UnitTests/` | Expression unit tests |
| `test/DataFilters.Queries.UnitTests/` | Query unit tests |
| `test/DataFilters.PerformanceTests/` | Benchmarks |
| `test/DataFilters.TestObjects/` | Shared test models |
| `build/` | Nuke build project |

## Build & test

```bash
./build.sh restore   # restore packages & tools
./build.sh tests     # run unit tests
./build.sh           # full build
```

Build system: [Nuke](https://nuke.build/). Entrypoint: `build/Build.cs`.

## Coding conventions

These rules are **mandatory** — see `CONTRIBUTING.md` for full details.

1. **No `var`** except with anonymous types. Always use explicit types.
2. **Single exit point** per method — assign to a result variable and return once at the end.
3. **Usings**: place `System` directives first; remove unused usings.
4. **Indentation**: follow the existing style (spaces, consistent alignment).
5. **Tests**: use xunit + FluentAssertions + FsCheck. Test projects reference `tests.props`.
6. **Semantic Versioning**: the project follows SemVer 2.0. Version zero — public API is not yet stable.

### Examples

#### Variable declarations

```csharp
// ✅ DO — explicit type
Filter filter = new Filter("Nickname", FilterOperator.EqualTo, "Batman");
IList<IFilter> filters = new List<IFilter>();
string name = "Bruce";

// ❌ DON'T — var is forbidden
var filter = new Filter("Nickname", FilterOperator.EqualTo, "Batman");
var filters = new List<IFilter>();
var name = "Bruce";
```

#### Single exit point

```csharp
// ✅ DO — one return at the end
public string GetLabel(FilterOperator op)
{
    string result;
    if (op == FilterOperator.EqualTo)
    {
        result = "eq";
    }
    else
    {
        result = "other";
    }

    return result;
}

// ❌ DON'T — multiple returns
public string GetLabel(FilterOperator op)
{
    if (op == FilterOperator.EqualTo)
    {
        return "eq";
    }

    return "other";
}
```

#### Usings order

```csharp
// ✅ DO — System first, then others alphabetically
using System;
using System.Collections.Generic;
using System.Linq;
using DataFilters;
using FluentAssertions;

// ❌ DON'T — System not first, or unused usings left in
using DataFilters;
using System;
using System.IO; // unused
```

#### Writing tests

```csharp
// ✅ DO — xunit + FluentAssertions, [UnitTest] category, TheoryData for cases
[UnitTest]
public class MyTests
{
    public static TheoryData<string, FilterOperator> Cases => new()
    {
        { "eq", FilterOperator.EqualTo },
        { "contains", FilterOperator.Contains },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Should_parse_operator(string input, FilterOperator expected)
    {
        // Act
        FilterOperator result = Parse(input);

        // Assert
        result.Should().Be(expected);
    }
}

// ❌ DON'T — NUnit, Assert.Equal, missing category
[Test]
public void TestParse()
{
    var result = Parse("eq");
    Assert.AreEqual(FilterOperator.EqualTo, result);
}
```

## Testing expectations

- Every new feature or bug fix must have corresponding unit tests.
- Mutation testing (Stryker) is configured — aim for high mutation score.
- Test data generators live in `Generators.cs` files alongside tests.

## Branching

- Default branch: `develop`.
- Production branch: `main`.
- Create topic branches off `develop`.

## What NOT to do

- Do not add unnecessary abstractions, helpers, or wrappers.
- Do not introduce dependencies without justification.
- Do not modify build infrastructure unless explicitly asked.
- Do not use `var` (except anonymous types).
- Do not write methods with multiple return statements.

### Anti-pattern examples

```csharp
// ❌ DON'T — unnecessary wrapper/helper
public static class FilterHelper
{
    public static Filter Eq(string field, object value)
        => new(field, FilterOperator.EqualTo, value);
}

// ✅ DO — use the types directly
Filter filter = new Filter("Age", FilterOperator.EqualTo, 30);
```

```csharp
// ❌ DON'T — over-engineer with defensive checks on internal code
public void Process(Filter filter)
{
    if (filter == null) throw new ArgumentNullException(nameof(filter));
    if (string.IsNullOrEmpty(filter.Field)) throw new ArgumentException("Field required");
    // ...
}

// ✅ DO — trust internal callers, validate only at system boundaries
public void Process(Filter filter)
{
    // filter is guaranteed non-null by the calling pipeline
    string field = filter.Field;
    // ...
}
```
