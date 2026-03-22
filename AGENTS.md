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
