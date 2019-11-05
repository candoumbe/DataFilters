using DataFilters.Grammar.Parsing;
using DataFilters.Grammar.Syntax;
using Microsoft.Extensions.Primitives;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using static DataFilters.FilterOperator;

#if !STRING_SEGMENT
using static System.StringSplitOptions;
#endif

namespace DataFilters
{
    /// <summary>
    /// Extensions methods class which allow to build expression out of a <see cref="IFilter"/> instances.
    /// </summary>
    public static class FilterExtensions
    {
#if STRING_SEGMENT
        /// <summary>
        /// Builds a <see cref="IFilter{T}"/> from <paramref name="queryString"/>
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
        public static IFilter ToFilter<T>(this string queryString)
            => new StringSegment(queryString).ToFilter<T>();

#endif
        /// <summary>
        /// Builds a <see cref="IFilter{T}"/> from <paramref name="queryString"/>
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
#if STRING_SEGMENT
        public static IFilter ToFilter<T>(this StringSegment queryString)
        {
            string localQueryString = queryString.Value;
#else
        public static IFilter ToFilter<T>(this string queryString)
        {
            string localQueryString = queryString;
#endif
            static IFilter ConvertExpressionToFilter(FilterExpression expression, string propertyName, TypeConverter tc)
            {
                IFilter filter = Filter.True;
                switch (expression)
                {
                    case ConstantExpression constant:
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
                        filter = new CompositeFilter
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
                        filter = new CompositeFilter
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
                        filter = new CompositeFilter
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
                            filter = new CompositeFilter { Logic = FilterLogic.Or, Filters = filters };
                        }
                        break;
                    case RangeExpression range:
                        if (range.Min != default && range.Max != default)
                        {
                            filter = new CompositeFilter
                            {
                                Logic = FilterLogic.And,
                                Filters = new IFilter[]
                                {
                                new Filter(propertyName, GreaterThanOrEqual, tc.ConvertFrom(range.Min.Value)),
                                new Filter(propertyName, LessThanOrEqualTo, tc.ConvertFrom(range.Max.Value))
                                }
                            };
                        }
                        else if (range.Min != default)
                        {
                            filter = new Filter(propertyName, GreaterThanOrEqual, tc.ConvertFrom(range.Min.Value));
                        }
                        else
                        {
                            filter = new Filter(propertyName, LessThanOrEqualTo, tc.ConvertFrom(range.Max.Value));
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
                FilterTokenizer tokenizer = new FilterTokenizer();
                TokenList<FilterToken> tokens = tokenizer.Tokenize(localQueryString);

                (PropertyNameExpression Property, FilterExpression Expression)[] expressions = FilterTokenParser.Criteria.Parse(tokens);

                if (expressions.Once())
                {
                    PropertyInfo pi = typeof(T).GetRuntimeProperties()
                         .SingleOrDefault(x => x.CanRead && x.Name == expressions.Single().Property.Name);

                    if (pi != null)
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);
                        filter = ConvertExpressionToFilter(expressions.Single().Expression, pi.Name, tc);
                    }
                }
                else
                {
                    IList<IFilter> filters = new List<IFilter>();

                    foreach ((PropertyNameExpression property, FilterExpression Expression) in expressions)
                    {
                        PropertyInfo pi = typeof(T).GetRuntimeProperties()
                             .SingleOrDefault(x => x.CanRead && x.Name == property.Name);

                        if (pi != null)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);
                            filters.Add(ConvertExpressionToFilter(Expression, property.Name, tc));
                        }
                    }

                    filter = new CompositeFilter { Logic = FilterLogic.And, Filters = filters };
                }
            }

            return filter;
        }
    }
}
