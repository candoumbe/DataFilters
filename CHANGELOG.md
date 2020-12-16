# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Added support for filtering over complex type collections by introducing sub property syntax
- Moved `FilterExtensions.ToFilter<T>(this StringSegment)` method to `StringExtensions.ToFilter<T>(this StringSegment)` [BREAKING]
- Moved `FilterExtensions.ToFilter<T>(this string)` method to `StringExtensions.ToFilter<T>(this string)` [BREAKING]
- Changed `ConstantExpression` to `ConstantValueExpression` [BREAKING]
- Changed subproperty syntax from `property.subproperty` method to `property["subproperty"]` [BREAKING]
- Fixed parsing [`RangeExpression`](src/DataFilters/Grammar/Syntax/RangeExpression.cs) with date values

## [0.2.0] / 2020-12-04
- Added support for "equals" operator on collections
- Added support for "contains" operator on collections.
