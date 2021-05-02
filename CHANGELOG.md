# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Introduces [`PropertyNameResolutionStrategy`](/src/DataFilters/Casing/PropertyNameResolutionStrategy.cs) as extension point to configure the way to lookup 
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
- Moved `FilterExtensions.ToFilter<T>(this string)
- 
` method to `StringExtensions.ToFilter<T>(this string)` [BREAKING]
- Changed `ConstantExpression` to `ConstantValueExpression` [BREAKING]
- Changed subproperty syntax from `property.subproperty` method to `property["subproperty"]` [BREAKING]
- Fixed parsing [`RangeExpression`](src/DataFilters/Grammar/Syntax/RangeExpression.cs) with datetime values
- Enabled Source Link

## [0.2.2] / 2020-12-05
- Fixed "contains" parser not working with Guid.
- Replaced "Utilities" dependency with "Candoumbe.MiscUtilities" dependency

## [0.2.0] / 2020-12-04
- Added support for "equals" operator on collections
- Added support for "contains" operator on collections.

[Unreleased]: https://github.com/candoumbe/DataFilters.git/compare/0.4.1...HEAD
[0.4.1]: https://github.com/candoumbe/DataFilters.git/compare/0.4.0...0.4.1
[0.4.0]: https://github.com/candoumbe/DataFilters.git/compare/0.3.2...0.4.0
[0.3.2]: https://github.com/candoumbe/DataFilters.git/compare/0.3.1...0.3.2
[0.3.1]: https://github.com/candoumbe/DataFilters.git/compare/0.3.0...0.3.1
[0.3.0]: https://github.com/candoumbe/DataFilters.git/compare/0.2.2...0.3.0
[0.2.2]: https://github.com/candoumbe/DataFilters.git/compare/0.2.0...0.2.2
[0.2.0]: https://github.com/candoumbe/DataFilters.git/tree/0.2.0

