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
    }
}