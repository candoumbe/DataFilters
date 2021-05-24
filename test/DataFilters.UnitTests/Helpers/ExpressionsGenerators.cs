using System;
using System.Collections.Generic;
using System.Linq;

using DataFilters.Grammar.Syntax;

using FsCheck;

namespace DataFilters.UnitTests.Helpers
{
    public static class ExpressionsGenerators
    {
        public static Arbitrary<DateTimeExpression> GenerateDateTimeExpression()
        {
            return Arb.Default.DateTime()
                                .Filter(dateTime => dateTime.Hour >= 0 && dateTime.Minute >= 0 && dateTime.Second >= 0 && dateTime.Millisecond >= 0)
                                .Convert(convertTo: dateTime => new DateTimeExpression(date: new(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day),
                                                                                                time: new(hours: dateTime.Hour, minutes: dateTime.Minute, seconds: dateTime.Second)),
                                           convertFrom: dateTimeExpression => new DateTime(year: dateTimeExpression.Date.Year,
                                                                                           month: dateTimeExpression.Date.Month,
                                                                                           day: dateTimeExpression.Date.Day,
                                                                                           hour: dateTimeExpression.Time.Hours,
                                                                                           minute: dateTimeExpression.Time.Hours,
                                                                                           second: dateTimeExpression.Time.Seconds)
            );
        }

        public static Arbitrary<TimeExpression> GenerateTimeExpression()
        {
            return Arb.Default.TimeSpan()
                        .Filter(timespan => timespan.Hours >= 0 && timespan.Minutes >= 0 && timespan.Seconds >= 0 && timespan.Milliseconds >= 0)
                        .Convert(convertTo: timespan => new TimeExpression(hours: timespan.Hours,
                                                                                            minutes: timespan.Minutes,
                                                                                            seconds: timespan.Seconds,
                                                                                            milliseconds: timespan.Milliseconds),
                                                  convertFrom: timeExpression => new TimeSpan(hours: timeExpression.Hours,
                                                                                              minutes: timeExpression.Minutes,
                                                                                              seconds: timeExpression.Seconds));
        }

        public static Arbitrary<DateExpression> GenerateDateExpression()
        {
            return Arb.Default.DateTime()
                            .Filter(dateTime => dateTime.Date.Year >= 0 && dateTime.Date.Month >= 0 && dateTime.Date.Day >= 0)
                            .Convert(convertTo: dateTime => new DateExpression(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day),
                                     convertFrom: dateExpression => new DateTime(year: dateExpression.Year, month: dateExpression.Month, day: dateExpression.Day));
        }

        public static Arbitrary<ConstantValueExpression> GenerateConstantValueExpression()
        {
            IList<Gen<object>> generators = new List<Gen<object>>
            {
                Arb.Default.Bool().Generator.Select(item => (object) item),
                Arb.Default.String().Generator.Where(item => !string.IsNullOrEmpty(item)).Select(item => (object) item),
                Arb.Default.Int16().Generator.Select(item => (object) item),
                Arb.Default.Int32().Generator.Select(item => (object) item),
                Arb.Default.Int64().Generator.Select(item => (object) item),
                Arb.Default.DateTime().Generator.Select(item => (object) item),
                Arb.Default.DateTimeOffset().Generator.Select(item => (object) item),
                Arb.Default.Byte().Generator.Select(item => (object) item),
                Arb.Default.Guid().Generator.Select(item => (object) item)
            };

            return Gen.OneOf(generators).Select(generatedValue => new ConstantValueExpression(generatedValue))
                       .ToArbitrary();
        }

        public static Arbitrary<DurationExpression> GenerateDurationExpression()
        {
            Arbitrary<Tuple<Tuple<Tuple<PositiveInt, PositiveInt, PositiveInt>, Tuple<PositiveInt, PositiveInt, PositiveInt>>, PositiveInt>> arb = Arb.Default.PositiveInt().Generator
                                                                                                                                                              .Three()
                                                                                                                                                              .Two()
                                                                                                                                                              .Zip(Arb.Default.PositiveInt().Generator)
                                                                                                                                                              .ToArbitrary();

            return arb.Convert(convertTo: tuple => new DurationExpression(years: tuple.Item1.Item1.Item1.Item,
                                           months: tuple.Item1.Item1.Item2.Item,
                                           weeks: tuple.Item1.Item1.Item3.Item,
                                           days: tuple.Item1.Item2.Item1.Item,
                                           hours: tuple.Item1.Item2.Item2.Item,
                                           minutes: tuple.Item1.Item2.Item3.Item,
                                           seconds: tuple.Item2.Item),
                               convertFrom: _ => default);
        }
    }
}