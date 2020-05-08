using DataFilters;

using FluentValidation.Results;

using System.Linq;

using static DataFilters.SortDirection;
#if NETSTANDARD2_0 || NETSTANDARD2_1
using System.Runtime.InteropServices;
#endif

namespace System
{
    public static class StringExtensions
    {
        private static char Separator => ',';

        /// <summary>
        /// Converts 
        /// </summary>
        /// <typeparam name="T">Type of the element to which the <see cref="ISort"/> will be generated from</typeparam>
        /// <param name="sortString"></param>
        /// <returns></returns>
        public static ISort<T> ToSort<T>(this string sortString)
        {
            if (string.IsNullOrWhiteSpace(sortString) || sortString?.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sortString), "cannot be be null or whitespace only");
            }

            SortValidator validator = new SortValidator();
            ValidationResult validationResult = validator.Validate(sortString);

            if (!validationResult.IsValid)
            {
                throw new InvalidSortExpression(sortString);
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
                sort = new MultiSort<T>(MemoryMarshal.ToEnumerable(sorts).Select(s => s.ToSort<T>() as Sort<T>).ToArray());
#else
                sort = new MultiSort<T>(sorts.Select(s => s.ToSort<T>() as Sort<T>).ToArray());
#endif
            }
            else
            {
                if (sortString.StartsWith("+"))
                {
                    sort = new Sort<T>(sortString.Substring(1));
                }
                else if (sortString.StartsWith("-"))
                {
                    sort = new Sort<T>(sortString.Substring(1), direction: Descending);
                }
                else
                {
                    sort = new Sort<T>(sortString);
                }
            }

            return sort;
        }
    }
}
