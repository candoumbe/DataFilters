using FluentValidation.Results;
using DataFilters;
using System.Linq;
using DataFilters.Grammar.Parsing;
using DataFilters.Grammar.Syntax;
using Superpower;
using Superpower.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using static DataFilters.FilterOperator;
using DataFilters.Casing;

#if STRING_SEGMENT
using Microsoft.Extensions.Primitives;
#endif

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static DataFilters.OrderDirection;

#if NET7_0_OR_GREATER
    using System.Diagnostics;
#endif

namespace System;

/// <summary>
/// String extensions methods
/// </summary>
public static class StringExtensions
{
    private static char Separator => ',';

    /// <summary>
    /// Converts <paramref name="sortString"/> to a <see cref="IOrder{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of the element to which the <see cref="IOrder{T}"/> will be generated from</typeparam>
    /// <param name="sortString"></param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="sortString"/> is <see langword="null"/> or whitespace</exception>
    /// <exception cref="InvalidOrderExpressionException">when <paramref name="sortString"/> is not a valid sort expression.</exception>
    public static IOrder<T> ToSort<T>(this string sortString) => sortString.ToOrder<T>(PropertyNameResolutionStrategy.Default);

    /// <summary>
    /// Converts <paramref name="sortString"/> to a <see cref="IOrder{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of the element to which the <see cref="IOrder{T}"/> will be generated from</typeparam>
    /// <param name="sortString"></param>
    /// <param name="propertyNameResolutionStrategy">The transformation to apply to each property name.</param>
    /// <exception cref="ArgumentOutOfRangeException">when <paramref name="sortString"/> is <see langword="null"/> or whitespace</exception>
    /// <exception cref="InvalidOrderExpressionException">when <paramref name="sortString"/> is not a valid sort expression.</exception>
    public static IOrder<T> ToOrder<T>(this string sortString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
    {
        if (string.IsNullOrWhiteSpace(sortString) || sortString?.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortString), "cannot be be null or whitespace only");
        }

        OrderValidator validator = new();
        ValidationResult validationResult = validator.Validate(sortString);

        if (!validationResult.IsValid)
        {
            throw new InvalidOrderExpressionException(sortString);
        }

        ReadOnlyMemory<string> sorts = sortString.Split([Separator], StringSplitOptions.RemoveEmptyEntries)
            .AsMemory();

        IOrder<T> sort = null;

        if (sorts.Length > 1)
        {
            sort = new MultiOrder<T>(MemoryMarshal.ToEnumerable(sorts)
                                         .Select(s => s.ToOrder<T>(propertyNameResolutionStrategy) as Order<T>).ToArray());
        }
        else if (sortString.StartsWith("+"))
        {
            sort = new Order<T>(propertyNameResolutionStrategy.Handle(sortString[1..]));
        }
        else if (sortString.StartsWith("-"))
        {
            sort = new Order<T>(propertyNameResolutionStrategy.Handle(sortString[1..]),
                                direction: Descending);
        }
        else
        {
            sort = new Order<T>(propertyNameResolutionStrategy.Handle(sortString));
        }

        return sort;
    }

    /// <summary>
    /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/>
    /// </summary>
    /// <typeparam name="T">Type of element to filter</typeparam>
    /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
    /// <param name="propertyNameResolutionStrategy"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
    public static IFilter ToFilter<T>(this string queryString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
        => ToFilter<T>(queryString, new FilterOptions() { DefaultPropertyNameResolutionStrategy = propertyNameResolutionStrategy });

/// <summary>
    /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> using <see cref="PropertyNameResolutionStrategy.Default"/>
    /// </summary>
    /// <typeparam name="T">Type of element to filter</typeparam>
    /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
    /// <returns>a <see cref="IFilter"/> that correspond to the <paramref name="queryString"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
    public static IFilter ToFilter<T>(this string queryString) => ToFilter<T>(queryString, PropertyNameResolutionStrategy.Default);

    /// <summary>
    /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> with the specified <paramref name="propertyNameResolutionStrategy"/>.
    /// </summary>
    /// <typeparam name="T">Type of element to filter</typeparam>
    /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
    /// <param name="propertyNameResolutionStrategy"></param>
    /// <returns>The corresponding <see cref="IFilter"/></returns>
    /// <exception cref="ArgumentNullException">either <paramref name="queryString"/> or <paramref name="propertyNameResolutionStrategy"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
    public static IFilter ToFilter<T>(this StringSegment queryString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
        => ToFilter<T>(queryString.Value, new FilterOptions() { DefaultPropertyNameResolutionStrategy = propertyNameResolutionStrategy });

    /// <summary>
    /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> with the specified <paramref name="options"/>.
    /// </summary>
    /// <typeparam name="T">Type of element to filter</typeparam>
    /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
    /// <param name="options"></param>
    /// <returns>The corresponding <see cref="IFilter"/></returns>
    /// <exception cref="ArgumentNullException">either <paramref name="queryString"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
    /// <exception cref="NotSupportedException"><paramref name="queryString"/> contains an unsupported</exception>
    public static IFilter ToFilter<T>(this string queryString, FilterOptions options)
    {
        string localQueryString = queryString ?? throw new ArgumentNullException(nameof(queryString));

        if (localQueryString is null)
        {
            throw new ArgumentNullException(nameof(queryString));
        }

        IFilter filter = Filter.True;
        bool isEmptyQueryString = string.IsNullOrWhiteSpace(localQueryString);

        if (!isEmptyQueryString)
        {
            FilterTokenizer tokenizer = new();
            TokenList<FilterToken> tokens = tokenizer.Tokenize(localQueryString);

            (PropertyName Property, FilterExpression Expression)[] expressions = FilterTokenParser.Criteria.Parse(tokens);
#if NET6_0
            if (HackZone.TryAdd(true, 1) && expressions.AtLeastOnce())
            {
                TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));
            }
#endif
            if (expressions.Once())
            {
                (PropertyName property, FilterExpression expression) = expressions[0];

                string fieldName = options.DefaultPropertyNameResolutionStrategy.Handle(property.Name);
                Debug.Assert(Regex.IsMatch(fieldName, Filter.ValidFieldNamePattern, RegexOptions.Singleline, TimeSpan.FromSeconds(10)), "fieldName is not a valid field name");

                Type fieldType = ComputeTargetedPropertyType(fieldName);

                if (fieldType is not null)
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(fieldType);
                    filter = ConvertExpressionToFilter(fieldName, expression, tc, fieldType);
                }
            }
            else
            {
                IList<IFilter> filters = [];

                foreach ((PropertyName property, FilterExpression expression) in expressions)
                {
                    string fieldName = options.DefaultPropertyNameResolutionStrategy.Handle(property.Name);

                    Type fieldType = ComputeTargetedPropertyType(fieldName);
                    if (fieldType is not null)
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(fieldType);
                        filters.Add(ConvertExpressionToFilter(fieldName, expression, tc, fieldType));
                    }
                }

                filter = new MultiFilter { Logic = options.Logic, Filters = filters };
            }
        }

        return filter;

        static IFilter ConvertExpressionToFilter(string fieldName, FilterExpression expression, TypeConverter tc, Type fieldType)
        {
            IFilter filter;
            switch (expression)
            {
                case ConstantValueExpression constant:

                    filter = new Filter(fieldName,
                                        EqualTo,
                                        tc.ConvertTo(constant.Value.ToStringValue(), fieldType));
                    break;
                case StartsWithExpression startsWith:
                    filter = new Filter(fieldName, StartsWith, startsWith.Value.ToStringValue());
                    break;
                case EndsWithExpression endsWith:
                    filter = new Filter(fieldName, EndsWith, endsWith.Value.ToStringValue());
                    break;
                case ContainsExpression endsWith:
                    filter = new Filter(fieldName, Contains, endsWith.Value.ToStringValue());
                    break;
                case NotExpression not:
                    filter = ConvertExpressionToFilter(fieldName, not.Expression, tc, fieldType).Negate();
                    break;
                case OrExpression orExpression:
                    filter = new MultiFilter
                    {
                        Logic = FilterLogic.Or,
                        Filters =
                        [
                            ConvertExpressionToFilter(fieldName, orExpression.Left, tc, fieldType),
                            ConvertExpressionToFilter(fieldName, orExpression.Right, tc, fieldType)
                        ]
                    };
                    break;
                case AndExpression andExpression:
                    filter = new MultiFilter
                    {
                        Logic = FilterLogic.And,
                        Filters =
                        [
                            ConvertExpressionToFilter(fieldName, andExpression.Left, tc, fieldType),
                            ConvertExpressionToFilter(fieldName, andExpression.Right, tc, fieldType)
                        ]
                    };
                    break;
                case BracketExpression regex:
                    filter = new MultiFilter
                    {
                        Logic = FilterLogic.Or,
                        //Filters = regex.Value.Select(c => ConvertExpressionToFilter(new ConstantExpression($"{regex.Before?.Value ?? string.Empty}{c}{regex.After?.Value ?? string.Empty}"), propertyName, tc))
                    };
                    break;
                case GroupExpression group:
                    filter = ConvertExpressionToFilter(fieldName, group.Expression, tc, fieldType);
                    break;
                case OneOfExpression oneOf:
                    FilterExpression[] possibleValues = [.. oneOf.Values];
                    if (oneOf.Values.Count is 1)
                    {
                        filter = ConvertExpressionToFilter(fieldName, possibleValues[0], tc, fieldType);
                    }
                    else
                    {
                        List<IFilter> filters = new(possibleValues.Length);
                        filters.AddRange(possibleValues.Select(item => ConvertExpressionToFilter(fieldName, item, tc, fieldType)));

                        filter = new MultiFilter { Logic = FilterLogic.Or, Filters = filters };
                    }

                    break;
                case IntervalExpression range:

                    static (ConstantValueExpression constantExpression, bool included) ConvertBoundaryExpressionToConstantExpression(BoundaryExpression input)
                        => input?.Expression switch
                        {
                            NumericValueExpression numeric => (new StringValueExpression(numeric.Value), input.Included),
                            DateTimeExpression { Date: not null, Time: null } dateTime => (new StringValueExpression($"{dateTime.Date.Year:D4}-{dateTime.Date.Month:D2}-{dateTime.Date.Day:D2}"),
                                                                                           input.Included),
                            DateExpression date => (
                                                       new StringValueExpression($"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}"),
                                                       input.Included),
                            DateTimeExpression { Date: null, Time: not null, Offset: null } dateTime => (new StringValueExpression($"0001-01-01T{dateTime.Time.Hours}:{dateTime.Time.Minutes}:{dateTime.Time.Seconds}"),
                                                                                                         input.Included),
                            TimeExpression time => (new StringValueExpression($"0001-01-01T{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}"),
                                                    input.Included),
                            DateTimeExpression { Date: not null, Time: not null, Offset: null } dateTime => (new StringValueExpression($"{dateTime.Date.Year:D4}-{dateTime.Date.Month:D2}-{dateTime.Date.Day:D2}T{dateTime.Time.Hours:D2}:{dateTime.Time.Minutes:D2}:{dateTime.Time.Seconds:D2}.{dateTime.Time.Milliseconds}"),
                                                                                                             input.Included),
                            DateTimeExpression { Date: not null, Time: not null, Offset: not null } dateTime => (new StringValueExpression(dateTime.EscapedParseableString),
                                                                                                                 input.Included),
                            AsteriskExpression or null => default, // because this is equivalent to an unbounded range
#if NET8_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported boundary type {input.Expression.GetType()}")
#else
                            _ => throw new NotSupportedException($"Unsupported boundary type {input.Expression.GetType()}")
#endif
                        };

                    (ConstantValueExpression constantExpression, bool included) min = ConvertBoundaryExpressionToConstantExpression(range.Min);
                    (ConstantValueExpression constantExpression, bool included) max = ConvertBoundaryExpressionToConstantExpression(range.Max);

                    FilterOperator minOperator = min.included ? GreaterThanOrEqual : GreaterThan;
                    FilterOperator maxOperator = max.included ? LessThanOrEqualTo : LessThan;

                    if (min.constantExpression?.Value is not null && max.constantExpression?.Value is not null)
                    {
                        object minValue = min.constantExpression.Value.ToStringValue();
                        object maxValue = max.constantExpression.Value.ToStringValue();
                        filter = new MultiFilter
                        {
                            Logic = FilterLogic.And,
                            Filters =
                            [
                                new Filter(fieldName,
                                           minOperator,
                                           tc.ConvertFrom(minValue)),
                                new Filter(fieldName,
                                           maxOperator,
                                           tc.ConvertFrom(maxValue))
                            ]
                        };
                    }
                    else if (min.constantExpression?.Value is not null)
                    {
                        object minValue = min.constantExpression.Value.ToStringValue();
                        filter = new Filter(fieldName, minOperator, tc.ConvertFrom(minValue));
                    }
                    else
                    {
                        object maxValue = max.constantExpression.Value.ToStringValue();
                        filter = new Filter(fieldName, maxOperator, tc.ConvertFrom(maxValue));
                    }

                    break;
                default:
                    throw new NotSupportedException($"Unsupported '{expression.GetType()}'s expression type.");
            }

            return filter;
        }

        static IReadOnlyList<ReadOnlyMemory<char>> ParsePropertyPath(string input)
        {
            List<ReadOnlyMemory<char>> result = [];
            int start = 0;
            bool insideBrackets = false;
            bool insideQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];

                switch (currentChar)
                {
                    case '[':
                        if (!insideQuotes)
                        {
                            if (!insideBrackets && i > start)
                            {
                                result.Add(input.AsMemory().Slice(start, i - start));
                            }

                            insideBrackets = true;
                        }

                        break;

                    case '"':
                        if (insideBrackets)
                        {
                            if (!insideQuotes)
                            {
                                start = i + 1;
                            }
                            else
                            {
                                result.Add(input.AsMemory(start, i - start));
                            }

                            insideQuotes = !insideQuotes;
                        }

                        break;

                    case ']':
                        if (!insideQuotes && insideBrackets)
                        {
                            insideBrackets = false;
                            start = i + 1;
                        }

                        break;
                }
            }

            return result is { Count: > 0 }
                       ? result
                       : [input.AsMemory()];
        }

        static Type ComputeTargetedPropertyType(string fieldName)
        {
            IReadOnlyList<ReadOnlyMemory<char>> fieldPath = ParsePropertyPath(fieldName);

            PropertyInfo currentProperty;
            Type currentType = typeof(T);
            for (int i = 0; i < fieldPath.Count; i++)
            {
                currentProperty = currentType?.GetProperty(fieldPath[i].ToString());
                if (currentProperty is not null)
                {
                    currentType = currentProperty.PropertyType;
#if NETSTANDARD2_0_OR_GREATER
                    if (typeof(IEnumerable<>).IsAssignableFrom(currentType))
#else
                    if (currentType.IsAssignableTo(typeof(IEnumerable<>)))
#endif
                    {
                        if (currentType.IsGenericType)
                        {
                            currentType = currentType.GetGenericArguments()[0];
                            i++;
                        }
                        else if (currentType.IsArray)
                        {
                            currentType = currentType.GetElementType();
                            i++;
                        }
                    }
                }
            }

            return currentType;
        }
    }

    /// <summary>
    /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> using <see cref="DefaultPropertyNameResolutionStrategy"/>.
    /// </summary>
    /// <typeparam name="T">Type of element to filter</typeparam>
    /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
    /// <returns>The corresponding <see cref="IFilter"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
    public static IFilter ToFilter<T>(this StringSegment queryString) => ToFilter<T>(queryString.Value, PropertyNameResolutionStrategy.Default);
}