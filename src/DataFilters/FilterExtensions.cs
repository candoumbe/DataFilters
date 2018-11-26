using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static DataFilters.FilterOperator;
using static System.Linq.Expressions.Expression;
using static System.StringSplitOptions;

namespace DataFilters
{
    /// <summary>
    /// Extensions methods class which allow to build expression out of a <see cref="IFilter"/> instances.
    /// </summary>
    public static class FilterExtensions
    {
        /// <summary>
        /// Builds an <see cref="Expression{Func{T}}"/> tree from a <see cref="IFilter"/> instance.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="filter"><see cref="IFilter"/> instance to build an <see cref="Expression{TDelegate}"/> tree from.</param>
        /// <returns><see cref="Expression{TDelegate}"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="filter"/> is <c>null</c>.</exception>
        public static Expression<Func<T, bool>> ToExpression<T>(this IFilter filter)
        {
            object ConvertObjectToDateTime(object source, Type targetType)
            {
                object dateTime = null;

                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(source?.ToString(), out DateTime result))
                    {
                        dateTime = result;
                    }
                    else if (targetType == typeof(DateTime?))
                    {
                        dateTime = null;
                    }
                }
                else if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
                {
                    if (DateTimeOffset.TryParse(source?.ToString(), out DateTimeOffset result))
                    {
                        dateTime = result;
                    }
                    else if (targetType == typeof(DateTimeOffset?))
                    {
                        dateTime = null;
                    }
                }

                return dateTime;
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), $"{nameof(filter)} cannot be null");
            }
            Expression<Func<T, bool>> filterExpression = null;

            switch (filter)
            {
                case Filter df:
                    {
                        if (df.Field == null)
                        {
                            filterExpression = _ => true;
                        }
                        else
                        {
                            Type type = typeof(T);
                            ParameterExpression pe = Parameter(type, "item");

                            string[] fields = df.Field.Split(new[] { '.' });
                            MemberExpression property = null;
                            foreach (string field in fields)
                            {
                                property = property == null
                                    ? Property(pe, field)
                                    : Property(property, field);
                            }

                            Expression body;
                            Type memberType = (property.Member as PropertyInfo)?.PropertyType;
                            ConstantExpression constantExpression = memberType == typeof(DateTime) || memberType == typeof(DateTime?) || memberType == typeof(DateTimeOffset) || memberType == typeof(DateTimeOffset?)
                                ? Constant(ConvertObjectToDateTime(df.Value, memberType), memberType)
                                : Constant(df.Value, memberType);

                            switch (df.Operator)
                            {
                                case NotEqualTo:
                                    // 
                                    body = NotEqual(property, constantExpression);
                                    break;
                                case IsNull:
                                    body = Equal(property, Constant(null));
                                    break;
                                case IsNotNull:
                                    body = NotEqual(property, Constant(null));
                                    break;
                                case FilterOperator.LessThan:
                                    body = LessThan(property, constantExpression);
                                    break;
                                case FilterOperator.GreaterThan:
                                    body = GreaterThan(property, constantExpression);
                                    break;
                                case FilterOperator.GreaterThanOrEqual:
                                    body = GreaterThanOrEqual(property, constantExpression);
                                    break;
                                case StartsWith:
                                    body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), constantExpression);
                                    break;
                                case EndsWith:
                                    body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), constantExpression);
                                    break;
                                case Contains:
                                    body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression);
                                    break;
                                case IsEmpty:
                                    body = Equal(property, Constant(string.Empty));
                                    break;
                                case IsNotEmpty:
                                    body = NotEqual(property, Constant(string.Empty));
                                    break;
                                default:
                                    body = Equal(property, constantExpression);
                                    break;
                            }

                            filterExpression = Lambda<Func<T, bool>>(body, pe);
                        }

                        break;
                    }

                case CompositeFilter dcf:
                    {
                        Expression<Func<T, bool>> expression = null;
                        // local function that can combine two expressions using either AND or OR operators
                        Func<Expression<Func<T, bool>>, Expression<Func<T, bool>>, Expression<Func<T, bool>>> expressionMerger;

                        if (dcf.Logic == FilterLogic.And)
                        {
                            expressionMerger = (first, second) => first.AndAlso(second);
                        }
                        else
                        {
                            expressionMerger = (first, second) => first.OrElse(second);
                        }

                        foreach (IFilter item in dcf.Filters)
                        {
                            expression = expression == null
                                ? item.ToExpression<T>()
                                : expressionMerger(expression, item.ToExpression<T>());
                        }

                        filterExpression = expression;
                        break;
                    }
            }

            return filterExpression;
        }

        public static IFilter ToFilter<T>(this string queryString) => new StringSegment(queryString).ToFilter<T>();

        /// <summary>
        /// Builds a <see cref="IFilter{T}"/> from <paramref name="queryString"/>
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="queryString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
        public static IFilter ToFilter<T>(this StringSegment queryString)
        {
            StringSegment localQueryString = queryString;
            IFilter InternalToFilter(StringSegment keyPart, StringSegment input, bool preceededByStar, bool followedByStar)
            {
                Filter localFilter = null;
                if (!preceededByStar)
                {
                    if (!followedByStar)
                    {
                        localFilter = new Filter(keyPart.ToString(), EqualTo, input.Value);
                    }
                    else
                    {
                        localFilter = new Filter(keyPart.ToString(), StartsWith, input.Value);
                    }
                }
                else
                {
                    if (!followedByStar)
                    {
                        localFilter = new Filter(keyPart.Value, EndsWith, input.Value);
                    }
                    else
                    {
                        localFilter = new Filter(keyPart.Value, Contains, input.Value);
                    }
                }
                Debug.Assert(localFilter != null);
                return localFilter;
            }

            if (queryString == default)
            {
                throw new ArgumentNullException(nameof(queryString));
            }

            IFilter filter = new Filter(field: null, @operator: default, value: null);
            Uri fakeuri = new UriBuilder
            {
                Host = "localhost",
                Query = queryString.ToString()
            }.Uri;

            if (queryString != StringSegment.Empty)
            {
                IEnumerable<StringSegment> queryStringParts = localQueryString.Split(new[] { '&'})
                    .Where(segment => segment != StringSegment.Empty);
                if (queryStringParts.Once())
                {
                    StringTokenizer keyValueParts = queryStringParts.Single().Split(new[] { '=' });

                    if (keyValueParts.Exactly(_ => true, 2))
                    {
                        StringSegment keyPart = keyValueParts.First();
                        StringSegment valuePart = keyValueParts.Last()
                            .Value
                            .Replace("!!", string.Empty)
                            .Replace("**", "*");

                        PropertyInfo pi = typeof(T).GetRuntimeProperties()
                            .SingleOrDefault(x => x.CanRead && x.Name == keyPart);

                        if (pi != null)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);

                            if (valuePart.StartsWith("!", StringComparison.InvariantCultureIgnoreCase))
                            {
                                StringSegment localValue = valuePart.Subsegment(1);
                                filter = $"{keyPart}={localValue.ToString()}".ToFilter<T>().Negate();
                            }
                            else if (valuePart.Like("*?,?*"))
                            {
                                IEnumerable<StringSegment> segments = valuePart.Split(new[] { ',' })
                                    .Where(segment => segment != StringSegment.Empty);

                                IList<IFilter> filters = new List<IFilter>(segments.Count());
                                foreach (StringSegment segment in segments)
                                {
                                    filters.Add($"{keyPart}={segment.Value}".ToFilter<T>());
                                }
                                filter = new CompositeFilter
                                {
                                    Logic = FilterLogic.And,
                                    Filters = filters
                                };
                            }
                            else if (valuePart.Like("*?|?*"))
                            {
                                StringTokenizer segments = valuePart.Split(new[] { '|' });

                                IList<IFilter> filters = new List<IFilter>();

                                foreach (StringSegment value in segments)
                                {
                                    filters.Add($"{keyPart}={value.Value}".ToFilter<T>());
                                }
                                filter = new CompositeFilter
                                {
                                    Logic = FilterLogic.Or,
                                    Filters = filters
                                };
                            }
                            else if (valuePart.Like(@"*\[*\]*"))
                            {
                                IEnumerable<StringSegment> values = FlattenValues(valuePart);
                                while (values.Any(val => val.Like(@"*\[*\]*")))
                                {
                                    values = values
                                        .Where(val => !val.Like(@"*\[*\]*"))
                                        .Concat(
                                            values
                                                .Where(val => val.Like(@"*\[*\]*"))
                                                .Select(FlattenValues)
                                                .SelectMany(x => x)
                                        );
                                }

                                filter = $"{keyPart}={string.Join("|", values.Select(x => x.Value))}".ToFilter<T>();
                            }
                            else
                            {
                                ReadOnlySpan<char> span = valuePart.AsSpan();
                                if (span.Contains("*".AsSpan(), StringComparison.OrdinalIgnoreCase) || span.Contains("?".AsSpan(), StringComparison.OrdinalIgnoreCase))
                                {
                                    if (span.Contains("*".AsSpan(), StringComparison.OrdinalIgnoreCase) && !span.Contains("?".AsSpan(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        IEnumerable<StringSegment> segments = valuePart.Split(new[] { '*' })
                                            .Where(segment => segment != StringSegment.Empty);
                                        switch (segments.Count())
                                        {
                                            case 1:
                                                filter = new Filter(
                                                    field: keyPart.ToString(),
                                                    @operator: valuePart.StartsWith("*", StringComparison.OrdinalIgnoreCase)
                                                        ? EndsWith
                                                        : StartsWith,
                                                    value: tc.ConvertFrom(segments.Single().ToString())
                                                );
                                                break;
                                            default:
                                                IList<IFilter> filters = new List<IFilter>(segments.Count());

                                                IList<int> starPositions = new List<int>();
                                                for (int i = 0; i < valuePart.Length; i++)
                                                {
                                                    if (valuePart[i] == '*')
                                                    {
                                                        starPositions.Add(i);
                                                    }
                                                }

                                                int index = 0;
                                                for (int i = 0; i < segments.Count(); i++)
                                                {
                                                    if (starPositions.Any(pos => pos == 0))
                                                    {
                                                        index++;
                                                    }
                                                    StringSegment val = segments.ElementAt(i);
                                                    if (i == 0)
                                                    {
                                                        filters.Add(InternalToFilter(keyPart, val, preceededByStar: index > 0, followedByStar: true));
                                                    }
                                                    else if (i < segments.Count() - 1)
                                                    {
                                                        filters.Add(InternalToFilter(keyPart, val, preceededByStar: index > 0 && valuePart[index] == '*', followedByStar: true));
                                                    }
                                                    else
                                                    {
                                                        filters.Add(InternalToFilter(keyPart, val, preceededByStar: true, followedByStar: valuePart[valuePart.Length - 1] == '*'));
                                                    }
                                                    index += val.Length;
                                                }

                                                filter = new CompositeFilter { Logic = FilterLogic.And, Filters = filters };
                                                break;
                                        }
                                    }
                                }
                                else if (valuePart.Like("*-*"))
                                {
                                    IEnumerable<StringSegment> segments = valuePart.Split(new[] { '-' })
                                        .Where(segment => segment != StringSegment.Empty);
                                    if (segments.Count() == 2)
                                    {
                                        filter = new CompositeFilter
                                        {
                                            Logic = FilterLogic.And,
                                            Filters = new[]
                                            {
                                            new Filter(field : keyPart.ToString(), @operator : FilterOperator.GreaterThanOrEqual, value : tc.ConvertFrom(segments.First().ToString())),
                                            new Filter(field : keyPart.ToString(), @operator : LessThanOrEqualTo, value : tc.ConvertFrom(segments.ElementAt(1).ToString())),
                                        }
                                        };
                                    }
                                    else
                                    {
                                        FilterOperator op = valuePart.Like("*?-")
                                            ? FilterOperator.GreaterThanOrEqual
                                            : LessThanOrEqualTo;
                                        filter = new Filter(field: keyPart.ToString(), @operator: op, value: tc.ConvertFrom(segments.Single().ToString()));
                                    }
                                }
                                else
                                {
                                    object value = tc.ConvertFrom(valuePart.ToString());
                                    filter = new Filter(field: keyPart.ToString(), @operator: EqualTo, value: value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    IList<IFilter> filters = new List<IFilter>(queryStringParts.Count());

                    foreach (StringSegment queryStringPart in queryStringParts)
                    {
                        filters.Add(queryStringPart.ToFilter<T>());
                    }

                    filter = new CompositeFilter { Logic = FilterLogic.And, Filters = filters };
                }
            }

            return filter;
        }

        private static IEnumerable<StringSegment> FlattenValues(StringSegment valuePart)
        {
            int variableStart = valuePart.IndexOf('[');
            int variableEnd = valuePart.IndexOf(']', variableStart + 1);

            char[] variables = valuePart.Substring(variableStart + 1, variableEnd - variableStart - 1)
                .ToCharArray();
            StringSegment startString = valuePart.Substring(0, variableStart);
            StringSegment endString = variableEnd < valuePart.Length
                ? valuePart.Subsegment(variableEnd + 1)
                : StringSegment.Empty;

            IList<StringSegment> values = new List<StringSegment>(variables.Length);
            for (int i = 0; i < variables.Length; i++)
            {
                values.Add($"{startString}{variables[i]}{endString}");
            }

            return values;
        }
    }
}
