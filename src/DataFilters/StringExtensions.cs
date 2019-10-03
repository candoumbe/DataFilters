using DataFilters;
using FluentValidation.Results;
using static DataFilters.SortDirection;

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

            ISort<T> sort = null;
#if NETSTANDARD2_0 || NETSTANDARD2_1
            Span<string> sorts = sortString.Split(new []{ Separator }, StringSplitOptions.RemoveEmptyEntries)
                .AsSpan();

#else
            string[] sorts = sortString.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

#endif

            if (sorts.Length > 1)
            {
                MultiSort<T> multiSort = new MultiSort<T>();

                foreach (string item in sorts)
                {
                    multiSort.Add(item.ToSort<T>() as Sort<T>);
                }

                sort = multiSort;
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
