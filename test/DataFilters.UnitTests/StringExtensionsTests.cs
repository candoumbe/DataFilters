using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.SortDirection;

namespace DataFilters.UnitTests
{
    public class StringExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public class SuperHero
        {
            public string Name { get; set;  }

            public string Age { get; set; }
        }

        public StringExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Theory]
        [InlineData(null, "sort expression cannot be null")]
        [InlineData("  ", "sort expression cannot be whitespace only")]
        public void Throws_ArgumentNullException_When_Parameter_IsNull(string source, string reason)
        {
            // Act
            Action action = () => source.ToSort<SuperHero>();

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), $"{nameof(ArgumentOutOfRangeException.ParamName)} must not be null")
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), $"{nameof(ArgumentOutOfRangeException.Message)} must not be null") ;
        }

        [Theory]
        [InlineData("prop1 prop2", "sort expression contains two properties that are not separated by a comma")]
        [InlineData("prop1,prop2,", "sort expression cannot ends with a comma")]
        [InlineData(",prop1,prop2", "sort expression cannot starts with a comma")]
        [InlineData("--prop", "sort expression can start with only one hyphen")]
        public void Throws_InvalidSortExpression_When_Expression_IsNotValid(string invalidExpression, string reason)
        {
            // Act
            Action action = () => invalidExpression.ToSort<SuperHero>();

            // Assert
            action.Should()
                .ThrowExactly<InvalidSortExpression>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), $"{nameof(ArgumentOutOfRangeException.Message)} must not be null");
        }

        public static IEnumerable<object[]> ToSortCases
        {
            get
            {
                yield return new object[]
                {
                    $"{nameof(SuperHero.Name)}",
                    new Sort<SuperHero>(expression : nameof(SuperHero.Name), direction : Ascending)
                };
                yield return new object[]
                {
                    $"+{nameof(SuperHero.Name)}",
                    new Sort<SuperHero>(expression : nameof(SuperHero.Name), direction : Ascending)
                };

                yield return new object[]
                {
                    $"-{nameof(SuperHero.Name)}",
                    new Sort<SuperHero>(expression : nameof(SuperHero.Name), direction : Descending)
                };

                {
                    MultiSort<SuperHero> multiSort = new MultiSort<SuperHero>();
                    multiSort.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));
                    multiSort.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age)));

                    yield return new object[]
                    {
                        $"{nameof(SuperHero.Name)},{nameof(SuperHero.Age)}",
                        multiSort
                    };
                }
                {
                    MultiSort<SuperHero> multiSort = new MultiSort<SuperHero>();
                    multiSort.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));
                    multiSort.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending));

                    yield return new object[]
                    {
                        $"+{nameof(SuperHero.Name)},-{nameof(SuperHero.Age)}",
                        multiSort
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ToSortCases))]
        public void ToSortTests(string sort, ISort<SuperHero> expected)
        {
            _outputHelper.WriteLine($"{nameof(sort)} : '{sort}'");

            // Act
            ISort<SuperHero> actual = sort.ToSort<SuperHero>();

            // Assert
            actual.Should()
                .Be(expected);
        }


        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new Sort<SuperHero>("Name"),
                    new Sort<SuperHero>("Name"),
                    true,
                    $"Two distinct {nameof(Sort<SuperHero>)} instances with same properties must be equal"
                };

                {
                    Sort<SuperHero> sort = new Sort<SuperHero>("Name");
                    yield return new object[]
                    {
                        sort,
                        sort,
                        true,
                        $"A {nameof(Sort<SuperHero>)} instance is equal to itself"
                    };
                }

                yield return new object[]
                {
                    new Sort<SuperHero>("Name", Descending),
                    new Sort<SuperHero>("Name"),
                    false,
                    $"Two distinct {nameof(Sort<SuperHero>)} instances with same {nameof(Sort<SuperHero>.Expression)} but different {nameof(Sort<SuperHero>.Direction)} must not be equal"
                };

                {
                    MultiSort<SuperHero> first = new MultiSort<SuperHero>();
                    first.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));
                    first.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending));

                    yield return new object[]
                    {
                        first,
                        first,
                        true,
                        $"A {nameof(MultiSort<SuperHero>)} instance is equal to itself"
                    };
                }
                {
                    MultiSort<SuperHero> first = new MultiSort<SuperHero>();
                    first.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));
                    first.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending));

                    MultiSort<SuperHero> second = new MultiSort<SuperHero>();
                    second.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));
                    second.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending));

                    yield return new object[]
                    {
                        first,
                        second,
                        true,
                        $"Two distinct {nameof(MultiSort<SuperHero>)} instances holding same data"
                    };
                }

                {
                    MultiSort<SuperHero> first = new MultiSort<SuperHero>();
                    first.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));
                    first.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending));

                    MultiSort<SuperHero> second = new MultiSort<SuperHero>();
                    second.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending));
                    second.Add(new Sort<SuperHero>(expression: nameof(SuperHero.Name)));

                    yield return new object[]
                    {
                        first,
                        second,
                        false,
                        $"Two distinct {nameof(MultiSort<SuperHero>)} instances holding same data but not same order"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void EqualsTests(ISort<SuperHero> first, object second, bool expected, string reason)
        {
            _outputHelper.WriteLine($"{nameof(first)} : '{first}'");

            // Act
            bool actual = first.Equals(second);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
