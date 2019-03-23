using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
        private const char _aMPERSAND_STAR = '&';

        private const string _sTAR_STRING = "*";
        private const char _sTAR_CHAR = '*';

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
            StringSegment localQueryString = queryString;
            IFilter InternalToFilter(StringSegment keyPart, StringSegment input, bool preceededByStar, bool followedByStar)
#else
        public static IFilter ToFilter<T>(this string queryString)
        {
            string localQueryString = queryString;
            IFilter InternalToFilter(string keyPart, string input, bool preceededByStar, bool followedByStar)
#endif
            {
                Filter localFilter = null;
                if (!preceededByStar)
                {
                    localFilter = !followedByStar
#if STRING_SEGMENT
                        ? new Filter(keyPart.Value, EqualTo, input.Value)
                        : new Filter(keyPart.Value, StartsWith, input.Value);
#else
                        ? new Filter(keyPart, EqualTo, input)
                        : new Filter(keyPart, StartsWith, input);
#endif
                }
                else
                {
#if STRING_SEGMENT
                    localFilter = !followedByStar
                                    ? new Filter(keyPart.Value, EndsWith, input.Value)
                                    : new Filter(keyPart.Value, Contains, input.Value);
#else
                    localFilter = !followedByStar
                                    ? new Filter(keyPart, EndsWith, input)
                                    : new Filter(keyPart, Contains, input);
#endif
                }
                Debug.Assert(localFilter != null);
                return localFilter;
            }

            if (queryString == default)
            {
                throw new ArgumentNullException(nameof(queryString));
            }

            IFilter filter = new Filter(field: null, @operator: default, value: null);

            bool isEmptyQueryString =
#if STRING_SEGMENT
            queryString == StringSegment.Empty;
#else
            string.IsNullOrWhiteSpace(queryString);
#endif

            if (!isEmptyQueryString)
            {
#if STRING_SEGMENT
                IEnumerable<StringSegment> queryStringParts = localQueryString.Split(new[] { _aMPERSAND_STAR })
                    .Where(segment => segment != StringSegment.Empty);
#else
                IEnumerable<string> queryStringParts = localQueryString.Split(new[] { _aMPERSAND_STAR }, RemoveEmptyEntries);
#endif
                if (queryStringParts.Once())
                {
#if STRING_SEGMENT
                    IEnumerable<StringSegment> keyValueParts = queryStringParts.Single().Split(new[] { '=' })
                        .Where(segment => segment != StringSegment.Empty);
#else
                    IEnumerable<string> keyValueParts = queryStringParts.Single().Split(new[] { '=' }, RemoveEmptyEntries);
#endif

                    if (keyValueParts.Exactly(_ => true, 2))
                    {
#if STRING_SEGMENT
                        StringSegment keyPart = keyValueParts.First();
                        StringSegment valuePart = keyValueParts.Last()
                            .Value
                            .Replace("!!", string.Empty)
                            .Replace("**", _sTAR_STRING);
#else
                        string keyPart = keyValueParts.First();
                        string valuePart = keyValueParts.Last()
                            .Replace("!!", string.Empty)
                            .Replace("**", _sTAR_STRING);
#endif

                        PropertyInfo pi = typeof(T).GetRuntimeProperties()
                            .SingleOrDefault(x => x.CanRead && x.Name == keyPart);

                        if (pi != null)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);

#if STRING_SEGMENT
                            bool valuePartIsNegation = valuePart.StartsWith("!", StringComparison.InvariantCultureIgnoreCase);
#else

                            bool valuePartIsNegation = valuePart.StartsWith("!");
#endif
                            if (valuePartIsNegation)
                            {
#if STRING_SEGMENT
                                StringSegment localValue = valuePart.Subsegment(1);
                                filter = $"{keyPart}={localValue.Value}".ToFilter<T>().Negate();
#else
                                string localValue = valuePart.Substring(1);
                                filter = $"{keyPart}={localValue}".ToFilter<T>().Negate();
#endif
                            }
                            else if (valuePart.Like("*?,?*"))
                            {
#if STRING_SEGMENT
                                IEnumerable<StringSegment> segments = valuePart.Split(new[] { ',' })
                                                            .Where(segment => segment != StringSegment.Empty);
                                IEnumerable<IFilter> filters = segments
                                    .Select(segment => $"{keyPart}={segment.Value}".ToFilter<T>());
#else
                                IEnumerable<string> segments = valuePart.Split(new[] { ',' }, RemoveEmptyEntries);
                                IEnumerable<IFilter> filters = segments
                                    .Select(segment => $"{keyPart}={segment}".ToFilter<T>());
#endif

                                filter = new CompositeFilter
                                {
                                    Logic = FilterLogic.And,
                                    Filters = filters
                                };
                            }
                            else if (valuePart.Like("*?|?*"))
                            {
#if STRING_SEGMENT
                                IEnumerable<StringSegment> segments = valuePart.Split(new[] { '|' })
                                                            .Where(segment => segment != StringSegment.Empty);
                                IEnumerable<IFilter> filters = segments
                                    .Select(segment => $"{keyPart}={segment.Value}".ToFilter<T>());
#else
                                IEnumerable<string> segments = valuePart.Split(new[] { '|' }, RemoveEmptyEntries);
                                IEnumerable<IFilter> filters = segments
                                    .Select(segment => $"{keyPart}={segment}".ToFilter<T>());
#endif
                                filter = new CompositeFilter
                                {
                                    Logic = FilterLogic.Or,
                                    Filters = filters
                                };
                            }
                            else if (valuePart.Like(@"*\[*\]*"))
                            {
#if STRING_SEGMENT
                                IEnumerable<StringSegment> values = FlattenValues(valuePart);
#else
                                IEnumerable<string> values = FlattenValues(valuePart);
#endif
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

#if STRING_SEGMENT
                                filter = $"{keyPart}={string.Join("|", values.Select(x => x.Value))}".ToFilter<T>();
#else
                                filter = $"{keyPart}={string.Join("|", values)}".ToFilter<T>();
#endif
                            }
                            else
                            {
#if STRING_SEGMENT
                                ReadOnlySpan<char> span = valuePart.AsSpan();
                                if (span.Contains("*".AsSpan(), StringComparison.OrdinalIgnoreCase)
                                    || span.Contains("?".AsSpan(), StringComparison.OrdinalIgnoreCase))
                                {
                                    if (span.Contains("*".AsSpan(), StringComparison.OrdinalIgnoreCase) && !span.Contains("?".AsSpan(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        IEnumerable<StringSegment> segments = valuePart.Split(new[] { '*' })
                                                                            .Where(segment => segment != StringSegment.Empty);
#else
                                if(valuePart.Contains("*") || valuePart.Contains("?"))
                                {
                                    if(valuePart.Contains(_sTAR_CHAR) && !valuePart.Contains("?"))
                                    {
                                        IEnumerable<string> segments = valuePart.Split(new[] { _sTAR_CHAR }, RemoveEmptyEntries);
#endif
                                        switch (segments.Count())
                                        {
                                            case 1:
                                                filter = new Filter(
#if STRING_SEGMENT
                                                    field: keyPart.Value,
                                                    @operator: valuePart.StartsWith("*", StringComparison.OrdinalIgnoreCase)
                                                        ? EndsWith
                                                        : StartsWith,
                                                    value: tc.ConvertFrom(segments.Single().Value)
#else
                                                    field: keyPart,
                                                    @operator: valuePart.StartsWith(_sTAR_STRING, StringComparison.OrdinalIgnoreCase)
                                                        ? EndsWith
                                                        : StartsWith,
                                                    value: tc.ConvertFrom(segments.Single())
#endif
                                                );
                                                break;
                                            default:
                                                IList<IFilter> filters = new List<IFilter>(segments.Count());

                                                IList<int> starPositions = new List<int>();
                                                for (int i = 0; i < valuePart.Length; i++)
                                                {
                                                    if (valuePart[i] == _sTAR_CHAR)
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
#if STRING_SEGMENT
                                                    StringSegment val = segments.ElementAt(i);
#else
                                                    string val = segments.ElementAt(i);
#endif
                                                    if (i == 0)
                                                    {
                                                        filters.Add(InternalToFilter(keyPart, val, preceededByStar: index > 0, followedByStar: true));
                                                    }
                                                    else if (i < segments.Count() - 1)
                                                    {
                                                        filters.Add(InternalToFilter(keyPart, val, preceededByStar: index > 0 && valuePart[index] == _sTAR_CHAR, followedByStar: true));
                                                    }
                                                    else
                                                    {
                                                        filters.Add(InternalToFilter(keyPart, val, preceededByStar: true, followedByStar: valuePart[valuePart.Length - 1] == _sTAR_CHAR));
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
#if STRING_SEGMENT
                                    IEnumerable<StringSegment> segments = valuePart.Split(new[] { '-' })
                                                                    .Where(segment => segment != StringSegment.Empty);
#else
                                    IEnumerable<string> segments = valuePart.Split(new[] { '-' }, RemoveEmptyEntries);
#endif
                                    if (segments.Count() == 2)
                                    {
                                        filter = new CompositeFilter
                                        {
                                            Logic = FilterLogic.And,
                                            Filters = new[]
                                            {
#if STRING_SEGMENT
                                                new Filter(field : keyPart.Value, @operator : FilterOperator.GreaterThanOrEqual, value : tc.ConvertFrom(segments.First().Value)),
                                                new Filter(field : keyPart.Value, @operator : LessThanOrEqualTo, value : tc.ConvertFrom(segments.Last().Value))
#else
                                                new Filter(field : keyPart, @operator : FilterOperator.GreaterThanOrEqual, value : tc.ConvertFrom(segments.First())),
                                                new Filter(field : keyPart, @operator : LessThanOrEqualTo, value : tc.ConvertFrom(segments.Last()))
#endif
                                            }
                                        };
                                    }
                                    else
                                    {
                                        FilterOperator op = valuePart.Like("*?-")
                                            ? FilterOperator.GreaterThanOrEqual
                                            : LessThanOrEqualTo;
#if STRING_SEGMENT
                                        filter = new Filter(field: keyPart.Value, @operator: op, value: tc.ConvertFrom(segments.Single().Value));
#else
                                        filter = new Filter(field: keyPart, @operator: op, value: tc.ConvertFrom(segments.Single()));
#endif
                                    }
                                }
                                else
                                {
#if STRING_SEGMENT
                                    object value = tc.ConvertFrom(valuePart.ToString());
                                    filter = new Filter(field: keyPart.ToString(), @operator: EqualTo, value: value);
#else
                                    object value = tc.ConvertFrom(valuePart);
                                    filter = new Filter(field: keyPart, @operator: EqualTo, value: value);
#endif
                                }
                            }
                        }
                    }
                }
                else
                {
                    IList<IFilter> filters = new List<IFilter>(queryStringParts.Count());

#if STRING_SEGMENT
                    foreach (StringSegment queryStringPart in queryStringParts)
#else
                    foreach(string queryStringPart in queryStringParts)
#endif
                    {
                        filters.Add(queryStringPart.ToFilter<T>());
                    }

                    filter = new CompositeFilter { Logic = FilterLogic.And, Filters = filters };
                }
            }

            return filter;
        }

#if STRING_SEGMENT
        private static IEnumerable<StringSegment> FlattenValues(StringSegment valuePart)
#else
        private static IEnumerable<string> FlattenValues(string valuePart)
#endif
        {
            int variableStart = valuePart.IndexOf('[');
            int variableEnd = valuePart.IndexOf(']', variableStart + 1);

            char[] variables = valuePart.Substring(variableStart + 1, variableEnd - variableStart - 1)
                .ToCharArray();
#if STRING_SEGMENT
            StringSegment startString = valuePart.Substring(0, variableStart);
            StringSegment endString = variableEnd < valuePart.Length
                ? valuePart.Subsegment(variableEnd + 1)
                : StringSegment.Empty;

            IList<StringSegment> values = new List<StringSegment>(variables.Length);
#else
            string startString = valuePart.Substring(0, variableStart);
            string endString = variableEnd < valuePart.Length
                ? valuePart.Substring(variableEnd + 1)
                : string.Empty;

            IList<string> values = new List<string>(variables.Length);
#endif
            for (int i = 0; i < variables.Length; i++)
            {
                values.Add($"{startString}{variables[i]}{endString}");
            }

            return values;
        }
    }
}
