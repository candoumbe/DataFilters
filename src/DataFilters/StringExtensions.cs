using DataFilters;

using FluentValidation.Results;

using System.Linq;

using DataFilters.Grammar.Parsing;
using DataFilters.Grammar.Syntax;
using Superpower;
using Superpower.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using static DataFilters.FilterOperator;
using DataFilters.Casing;

#if STRING_SEGMENT
using Microsoft.Extensions.Primitives;
#endif

using static DataFilters.SortDirection;
#if NETSTANDARD2_0 || NETSTANDARD2_1
using System.Runtime.InteropServices;
#endif

namespace System
{
    /// <summary>
    /// String extensions methods
    /// </summary>
    public static class StringExtensions
    {
        private static char Separator => ',';

        /// <summary>
        /// Converts <paramref name="sortString"/> to a <see cref="ISort{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">Type of the element to which the <see cref="ISort{T}"/> will be generated from</typeparam>
        /// <param name="sortString"></param>
        /// <exception cref="ArgumentOutOfRangeException">when <paramref name="sortString"/> is <c>null</c> or whitespace</exception>
        /// <exception cref="InvalidSortExpressionException">when <paramref name="sortString"/> is not a valid sort expression.</exception>
        public static ISort<T> ToSort<T>(this string sortString) => sortString.ToSort<T>(PropertyNameResolutionStrategy.Default);

        /// <summary>
        /// Converts <paramref name="sortString"/> to a <see cref="ISort{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">Type of the element to which the <see cref="ISort{T}"/> will be generated from</typeparam>
        /// <param name="sortString"></param>
        /// <param name="propertyNameResolutionStrategy">The transformation to apply to each property name.</param>
        /// <exception cref="ArgumentOutOfRangeException">when <paramref name="sortString"/> is <c>null</c> or whitespace</exception>
        /// <exception cref="InvalidSortExpressionException">when <paramref name="sortString"/> is not a valid sort expression.</exception>
        public static ISort<T> ToSort<T>(this string sortString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
        {
            if (string.IsNullOrWhiteSpace(sortString) || sortString?.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sortString), "cannot be be null or whitespace only");
            }

            SortValidator validator = new();
            ValidationResult validationResult = validator.Validate(sortString);

            if (!validationResult.IsValid)
            {
                throw new InvalidSortExpressionException(sortString);
            }

#if NETSTANDARD2_0 || NETSTANDARD2_1
            ReadOnlyMemory<string> sorts = sortString.Split(new []{ Separator }, StringSplitOptions.RemoveEmptyEntries)
                                                     .AsMemory();

#else
            string[] sorts = sortString.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

#endif
            ISort<T> sort = null;

            if (sorts.Length > 1)
            {
#if NETSTANDARD2_0 || NETSTANDARD2_1
                sort = new MultiSort<T>(MemoryMarshal.ToEnumerable(sorts).Select(s => s.ToSort<T>(propertyNameResolutionStrategy) as Sort<T>).ToArray());
#else
                sort = new MultiSort<T>(sorts.Select(s => s.ToSort<T>(propertyNameResolutionStrategy) as Sort<T>).ToArray());
#endif
            }
            else if (sortString.StartsWith("+"))
            {
#if NETSTANDARD1_3 || NETSTANDARD2_0
                sort = new Sort<T>(propertyNameResolutionStrategy.Handle(sortString.Substring(1)));
#else
                sort = new Sort<T>(propertyNameResolutionStrategy.Handle(sortString[1..]));
#endif
            }
                else if (sortString.StartsWith("-"))
            {
#if NETSTANDARD1_3 || NETSTANDARD2_0
                sort = new Sort<T>(propertyNameResolutionStrategy.Handle(sortString.Substring(1)),
                           direction: Descending);
#else
                sort = new Sort<T>(propertyNameResolutionStrategy.Handle(sortString[1..]),
                           direction: Descending);
#endif
            }
            else
            {
                sort = new Sort<T>(propertyNameResolutionStrategy.Handle(sortString));
            }

            return sort;
        }

#if STRING_SEGMENT
        /// <summary>
        /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/>
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
        /// <param name="propertyNameResolutionStrategy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
        public static IFilter ToFilter<T>(this string queryString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
            => new StringSegment(queryString).ToFilter<T>(propertyNameResolutionStrategy);

        /// <summary>
        /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> using <see cref="PropertyNameResolutionStrategy.Default"/>
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
        /// <returns>a <see cref="IFilter"/> that correspond to the <paramref name="queryString"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
        public static IFilter ToFilter<T>(this string queryString) => ToFilter<T>(queryString, PropertyNameResolutionStrategy.Default);

#endif
        /// <summary>
        /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> with the specified <paramref name="propertyNameResolutionStrategy"/>.
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
        /// <param name="propertyNameResolutionStrategy"></param>
        /// <returns>The corresponding <see cref="IFilter"/></returns>
        /// <exception cref="ArgumentNullException">either <paramref name="queryString"/> or <paramref name="propertyNameResolutionStrategy"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
#if STRING_SEGMENT
        public static IFilter ToFilter<T>(this StringSegment queryString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
        {
            string localQueryString = queryString.Value;
#else
        public static IFilter ToFilter<T>(this string queryString, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
        {
            string localQueryString = queryString;
#endif
            static IFilter ConvertExpressionToFilter(FilterExpression expression, string propertyName, TypeConverter tc)
            {
                IFilter filter = Filter.True;
                switch (expression)
                {
                    case ConstantValueExpression constant:
                        filter = new Filter(propertyName, EqualTo, tc.ConvertFrom(constant.Value));
                        break;
                    case StartsWithExpression startsWith:
                        filter = new Filter(propertyName, StartsWith, startsWith.Value);
                        break;
                    case EndsWithExpression endsWith:
                        filter = new Filter(propertyName, EndsWith, endsWith.Value);
                        break;
                    case ContainsExpression endsWith:
                        filter = new Filter(propertyName, Contains, endsWith.Value);
                        break;
                    case NotExpression not:
                        filter = ConvertExpressionToFilter(not.Expression, propertyName, tc).Negate();
                        break;
                    case OrExpression orExpression:
                        filter = new MultiFilter
                        {
                            Logic = FilterLogic.Or,
                            Filters = new IFilter[]
                            {
                                ConvertExpressionToFilter(orExpression.Left, propertyName, tc),
                                ConvertExpressionToFilter(orExpression.Right, propertyName, tc)
                            }
                        };
                        break;
                    case AndExpression andExpression:
                        filter = new MultiFilter
                        {
                            Logic = FilterLogic.And,
                            Filters = new IFilter[]
                            {
                                ConvertExpressionToFilter(andExpression.Left, propertyName, tc),
                                ConvertExpressionToFilter(andExpression.Right, propertyName, tc)
                            }
                        };
                        break;
                    case RegularExpression regex:
                        filter = new MultiFilter
                        {
                            Logic = FilterLogic.Or,
                            //Filters = regex.Value.Select(c => ConvertExpressionToFilter(new ConstantExpression($"{regex.Before?.Value ?? string.Empty}{c}{regex.After?.Value ?? string.Empty}"), propertyName, tc))
                        };
                        break;
                    case GroupExpression group:
                        filter = ConvertExpressionToFilter(group.Expression, propertyName, tc);
                        break;
                    case OneOfExpression oneOf:
                        FilterExpression[] possibleValues = oneOf.Values.ToArray();
                        if (oneOf.Values.Exactly(1))
                        {
                            filter = ConvertExpressionToFilter(possibleValues[0], propertyName, tc);
                        }
                        else
                        {
                            IList<IFilter> filters = new List<IFilter>(possibleValues.Length);

                            foreach (FilterExpression item in possibleValues)
                            {
                                filters.Add(ConvertExpressionToFilter(item, propertyName, tc));
                            }
                            filter = new MultiFilter { Logic = FilterLogic.Or, Filters = filters };
                        }
                        break;
                    case RangeExpression range:

                        static (ConstantValueExpression constantExpression, bool included) ConvertBounderyExpressionToConstantExpression(BoundaryExpression input)
                            => input?.Expression switch
                            {
                                ConstantValueExpression ce => (ce, input.Included),
                                DateTimeExpression { Time: null } dateTime => (new ConstantValueExpression($"{dateTime.Date.Year:D4}-{dateTime.Date.Month:D2}-{dateTime.Date.Day:D2}"), input.Included),
                                DateTimeExpression { Date: null } dateTime => (new ConstantValueExpression($"{dateTime.Time.Hours:D2}:{dateTime.Time.Minutes}:{dateTime.Time.Seconds}"), input.Included),
                                DateTimeExpression { Date: var date, Time: { Offset: null } } dateTime => (new ConstantValueExpression($"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}T{dateTime.Time.Hours:D2}:{dateTime.Time.Minutes:D2}:{dateTime.Time.Seconds:D2}"), input.Included),
                                DateTimeExpression { Date: var date, Time: { Offset: var offset } } dateTime => (new ConstantValueExpression($"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}T{dateTime.Time.Hours:D2}:{dateTime.Time.Minutes:D2}:{dateTime.Time.Seconds:D2}{offset}"), input.Included),
                                DateExpression date => (new ConstantValueExpression($"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}"), input.Included),
                                TimeExpression time => (new ConstantValueExpression($"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}"), input.Included),
                                AsteriskExpression or null => default, // because this is equivalent to an unbounded range
                                _ => throw new ArgumentOutOfRangeException($"Unsupported boundary type {input.GetType()}")
                            };

                        (ConstantValueExpression constantExpression, bool included) min = ConvertBounderyExpressionToConstantExpression(range.Min);
                        (ConstantValueExpression constantExpression, bool included) max = ConvertBounderyExpressionToConstantExpression(range.Max);

                        FilterOperator minOperator = min.included ? GreaterThanOrEqual : GreaterThan;
                        FilterOperator maxOperator = max.included ? LessThanOrEqualTo : LessThan;

                        if (min != default && max != default)
                        {
                            filter = new MultiFilter
                            {
                                Logic = FilterLogic.And,
                                Filters = new IFilter[]
                                {
                                    new Filter(propertyName, minOperator, tc.ConvertFrom(min.constantExpression.Value)),
                                    new Filter(propertyName, maxOperator, tc.ConvertFrom(max.constantExpression.Value))
                                }
                            };
                        }
                        else if (min != default)
                        {
                            filter = new Filter(propertyName, minOperator, tc.ConvertFrom(min.constantExpression.Value));
                        }
                        else
                        {
                            filter = new Filter(propertyName, maxOperator, tc.ConvertFrom(max.constantExpression.Value));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported '{expression.GetType()}'s expression type.");
                }

                return filter;
            }

            if (localQueryString == default)
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

                if (expressions.Once())
                {
                    (PropertyName property, FilterExpression expression) = expressions[0];

                    PropertyInfo pi = typeof(T).GetRuntimeProperties()
                                               .SingleOrDefault(x => x.CanRead && x.Name == propertyNameResolutionStrategy.Handle(property.Name));

                    if (pi is not null)
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);
                        filter = ConvertExpressionToFilter(expression, pi.Name, tc);
                    }
                }
                else
                {
                    IList<IFilter> filters = new List<IFilter>();

                    foreach ((PropertyName property, FilterExpression expression) in expressions)
                    {
                        PropertyInfo pi = typeof(T).GetRuntimeProperties()
                                                   .SingleOrDefault(x => x.CanRead && x.Name == property.Name);

                        if (pi is not null)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);
                            filters.Add(ConvertExpressionToFilter(expression, property.Name, tc));
                        }
                    }

                    filter = new MultiFilter { Logic = FilterLogic.And, Filters = filters };
                }
            }

            return filter;
        }

        /// <summary>
        /// Builds a <see cref="IFilter"/> from <paramref name="queryString"/> using <see cref="DefaultPropertyNameResolutionStrategy"/>.
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString">A query string (without any leading <c>?</c> character)</param>
        /// <returns>The corresponding <see cref="IFilter"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
#if STRING_SEGMENT
        public static IFilter ToFilter<T>(this StringSegment queryString) => ToFilter<T>(queryString.Value, PropertyNameResolutionStrategy.Default);

#else
        public static IFilter ToFilter<T>(this string queryString) => ToFilter<T>(queryString, PropertyNameResolutionStrategy.Default);
#endif
    }
}
