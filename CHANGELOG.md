# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### ⚠️ Breaking Changes

- Dropped `net5.0` support
- Dropped `netcoreapp3.1` support

### 🧹 Housekeeping

- Moved pipeline to Ubuntu agent
- Updated build definition to [Candoumbe.Pipelines 0.7.0-rc0003](https://www.nuget.org/packages/Candoumbe.Pipelines/0.7.0-rc0003)
- Updated `build.sh` script by running `nuke :update` command
- Removed explicit `Nuke.Common` dependency from the build project
- Added local file to store encrypted secrets needed when running some CI targets locally

## [0.12.0] / 2022-10-12
### 🚀 New features
- Added `net7.0` support
- Added syntax for `OneOfExpression` using curly braces ([#123](https://github.com/candoumbe/datafilters/issues/123))
- Added `+` operator to combine `ConstantValueExpression` with `AsteriskExpression`
- Added `+` operator to combine `StartsWithExpression` with `EndsWithExpression`
- Added `+` operator to combine `StartsWithExpression` with `ContainsExpression`
- Added `+` operator to combine `StartsWithExpression` with `StartsWithExpression`
- Added `+` operator to combine `StartsWithExpression` with `StringValueExpression` 
- Added `|` operator to combine two `FilterExpression`s into a `OrExpression`
- Added `&` operator to combine two `FilterExpression`s into a `AndExpression`

### ⚠️ Breaking changes
- `AsteriskExpression` default constructor is now private
- Dropped filter service
- Renamed `ISort<T>` to `IOrder<T>`
- Renamed `Sort<T>` to `Order<T>`
- Renamed `MultiSort<T>` to `MultiOrder<T>`
- Renamed `SortValidator<T>` to `OrderValidator<T>`
- Renamed `SortToQueries` class to `OrderExtensions` in `DataFilters.Queries`
- Renamed `SortExtensions` class to `OrderExtensions` in `DataFilters.Expressions`

## [0.12.0] / 2022-10-12
### 🚀 New features
- Added syntax for `OneOfExpression` using curly braces ([#123](https://github.com/candoumbe/datafilters/issues/123))
- Added `+` operator to combine `ConstantValueExpression` with `AsteriskExpression`
- Added `+` operator to combine `StartsWithExpression` with `EndsWithExpression`
- Added `+` operator to combine `StartsWithExpression` with `ContainsExpression`
- Added `+` operator to combine `StartsWithExpression` with `StartsWithExpression`
- Added `+` operator to combine `StartsWithExpression` with `StringValueExpression` 
- Added `|` operator to combine two `FilterExpression`s into a `OrExpression`
- Added `&` operator to combine two `FilterExpression`s into a `AndExpression`

### ⚠️ Breaking changes
- `AsteriskExpression` default constructor is now private
- Dropped filter service
- Renamed `ISort<T>` to `IOrder<T>`
- Renamed `Sort<T>` to `Order<T>`
- Renamed `MultiSort<T>` to `MultiOrder<T>`
- Renamed `SortValidator<T>` to `OrderValidator<T>`
- Renamed `SortToQueries` class to `OrderExtensions` in `DataFilters.Queries`
- Renamed `SortExtensions` class to `OrderExtensions` in `DataFilters.Expressions`

## [0.11.0] / 2022-03-13
- Added [`FilterOptions`](/src/DataFilters/FilterOptions.cs)
- Marked `FilterService` as obsolete.


## [0.10.2] / 2022-03-09
- Fixed `NullReferenceException` thrown when calling `OffsetExpression.Equals(null)` ([#98](https://github.com/candoumbe/DataFilters/issues/98))

## [0.10.1] / 2022-03-09
- Fixed equality implementation of `OffsetExpression` when comparing two `OffsetExpression.Zero` 
when the only difference is the numeric sign

## [0.10.0] / 2022-01-12
 - Added `IFilterService` which caches `IFilter` results. This partially addresses [issue #15](https://github.com/candoumbe/datafilters/issues/15)
 - Fixed `NotSupportedException` thrown when calling `Negate()` on a `IFilter` that contains the `FilterOperator.NotContain` operator

## [0.9.0] / 2021-12-30
- Made `BracketValue` implement `IParseable`, `IHaveComplexity`
- Bumped `Candoumbe.MiscUtilities` to `0.6.3`
- Bumped `Queries.Core` to `0.4.0`
- Added `DateOnly` support ([#63](https://github.com/candoumbe/datafilters/issues/63))
- Added `TimeOnly` support ([#64](https://github.com/candoumbe/datafilters/issues/64))
- Added [`NullableValueBehavior`](/src/Datafilters.Expressions/NullableValueBehavior.cs) ([#68](https://github.com/candoumbe/datafilters/issues/68))

## [0.8.0] / 2021-10-10
- Added `ISimplifiable` marker interface which defines a `FilterExpression.Simplify()` method to rewrite a 
`FilterExpression` to a less complex form.
- Renamed `RangeExpression` to `IntervalExpression` [BREAKING]
- Added `IParseableString` interface which defines :
  - `EscapedParseableString` property that holds a string representation of a `FilterExpression` instance ([#26](https://github.com/candoumbe/datafilters/issues/26)).
  - `OriginalString` property that holds the string representation of a filter BEFORE being escaped.
- Added `Deconstruct` method for `TimeExpression` class.
- Added `BoolValueExpression`
- Added [`NumericValueExpression`](/src/DataFilters/Grammar/Syntax/NumericValueExpression.cs) to improve parsing of numeric values ([#29](https://github.com/candoumbe/datafilters/issues/29))
- Added [`TextExpression`](/src/DataFilters/Grammar/Syntax/TextExpression.cs)
- Made `ConstantValueExpression` abstract [BREAKING]

## [0.7.0] / 2021-06-29
- Fixed missing documentation ([#17](https://github.com/candoumbe/datafilters/issues/17))
- Fixed parsing `*<regex>`, `<regex>*` expressions ([#18](https://github.com/candoumbe/datafilters/issues/18))
- Added complexity measurement of a `FilterExpression` ([#19](https://github.com/candoumbe/datafilters/issues/19))
- Renamed `PropertyNameExpression` to `PropertyName` as its no longer a `FilterExpression` [BREAKING]
- Removed `EqualExpression`
- Added support for `[<start>-<end>]` regex syntax [BREAKING]
- Renamed `RegularExpression` to `BracketExpression` [BREAKING]
- Renamed `RegularValue` to `BracketValue` [BREAKING]
- Renamed `RegularConstantValue` to `ConstantBracketValue` [BREAKING]
- Renamed `RegularRangeValue` to `RangeBracketValue` [BREAKING]
- Replaced `ConstantValueExpression(object)` constructor by : [BREAKING]
  - `ConstantValueExpression(string)`
  - `ConstantValueExpression(int)`
  - `ConstantValueExpression(long)`
  - `ConstantValueExpression(bool)`
  - `ConstantValueExpression(Guid)`
  - `ConstantValueExpression(DateTime)`
  - `ConstantValueExpression(DateTimeOffset)`
  - `ConstantValueExpression(byte)`
  - `ConstantValueExpression(char)`

## [0.6.0] / 2021-05-03
- License changed to [Apache 2.0](https://spdx.org/licenses/Apache-2.0.html)


## [0.5.0] / 2021-05-02
- Introduces [`PropertyNameResolutionStrategy`](/src/DataFilters/Casing/PropertyNameResolutionStrategy.cs) base class as extension point to configure the way to lookup 
for a corresponding property ([#8](https://github.com/candoumbe/datafilters/issues/8))
- Added `ToFilter<T>(string, PropertyNameResolutionStrategy)` overload
- Added `ToSort<T>(string, PropertyNameResolutionStrategy)` overload

## [0.4.1] / 2021-04-28
- Fixes `FileNotFoundException` when calling `StringExtensions.ToFilter<T>` method after a fresh install.

## [0.4.0] / 2021-04-03
- Added support for [offset](/src/DataFilters/Grammar/Syntax/TimeOffset.cs) when parsing [`TimeExpression`](/src/DataFilters/Grammar/Syntax/TimeExpression.cs)s
- Changed [`ConstantValueExpresssion.Value`](/src/DataFilters/Grammar/Syntax/ConstantValueExpression.cs) type from `string` to `object` [BREAKING]
- Added support for [duration](/src/DataFilters/Grammar/Syntax/DurationExpression.cs) expressions
- Removed runtime dependency to [Candoumbe.MiscUtilities](https://www.nuget.org/packages/Candoumbe.MiscUtilities/) package [BREAKING]
- Moved classes from `DataFilters.Converters` to DataFilters.Serizalization` namespace. [BREAKING]
- Added [`Kind`](/src/DataFilters/Grammar/Syntax/DateTimeExpressionKind.cs) property to specify the [`DateTimeExpression`](/src/DataFilters/Grammar/Syntax/DateTimeExpression.cs) [BREAKING]

## [0.3.2] / 2021-01-31
- Fixed parsing of sort expressions where a sub property name was specified.

## [0.3.1] / 2020-12-19
- Fixes `RepositoryUrl` metadata in nuget package

## [0.3.0] / 2020-12-19
- Added support for filtering over complex type collections by introducing sub property syntax
- Moved `FilterExtensions.ToFilter<T>(this StringSegment)` method to `StringExtensions.ToFilter<T>(this StringSegment)` [BREAKING]
- Moved `FilterExtensions.ToFilter<T>(this string)` method to `StringExtensions.ToFilter<T>(this string)` [BREAKING]
- Changed `ConstantExpression` to `ConstantValueExpression` [BREAKING]
- Changed subproperty syntax from `property.subproperty` method to `property["subproperty"]` [BREAKING]
- Fixed parsing [`RangeExpression`](src/DataFilters/Grammar/Syntax/IntervalExpression.cs) with datetime values
- Enabled Source Link

## [0.2.2] / 2020-12-05
- Fixed "contains" parser not working with Guid.
- Replaced "Utilities" dependency with "Candoumbe.MiscUtilities" dependency

## [0.2.0] / 2020-12-04
- Added support for "equals" operator on collections
- Added support for "contains" operator on collections.

[Unreleased]: https://github.com/candoumbe/DataFilters/compare/0.12.0...HEAD
[0.12.0]: https://github.com/candoumbe/DataFilters/compare/0.11.0...0.12.0
[0.11.0]: https://github.com/candoumbe/DataFilters/compare/0.10.2...0.11.0
[0.10.2]: https://github.com/candoumbe/DataFilters/compare/0.10.1...0.10.2
[0.10.1]: https://github.com/candoumbe/DataFilters/compare/0.10.0...0.10.1
[0.10.0]: https://github.com/candoumbe/DataFilters/compare/0.9.0...0.10.0
[0.9.0]: https://github.com/candoumbe/DataFilters/compare/0.8.0...0.9.0
[0.8.0]: https://github.com/candoumbe/DataFilters/compare/0.7.0...0.8.0
[0.7.0]: https://github.com/candoumbe/DataFilters/compare/0.6.0...0.7.0
[0.6.0]: https://github.com/candoumbe/DataFilters/compare/0.5.0...0.6.0
[0.5.0]: https://github.com/candoumbe/DataFilters/compare/0.4.1...0.5.0
[0.4.1]: https://github.com/candoumbe/DataFilters/compare/0.4.0...0.4.1
[0.4.0]: https://github.com/candoumbe/DataFilters/compare/0.3.2...0.4.0
[0.3.2]: https://github.com/candoumbe/DataFilters/compare/0.3.1...0.3.2
[0.3.1]: https://github.com/candoumbe/DataFilters/compare/0.3.0...0.3.1
[0.3.0]: https://github.com/candoumbe/DataFilters/compare/0.2.2...0.3.0
[0.2.2]: https://github.com/candoumbe/DataFilters/compare/0.2.0...0.2.2
[0.2.0]: https://github.com/candoumbe/DataFilters/tree/0.2.0

