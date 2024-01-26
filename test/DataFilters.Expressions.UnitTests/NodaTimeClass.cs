namespace DataFilters.Expressions.UnitTests;

using System.Diagnostics.CodeAnalysis;
using NodaTime;

[ExcludeFromCodeCoverage]
public class NodaTimeClass
{
    public LocalDate LocalDate { get; set; }

    public LocalDateTime LocalDateTime { get; set; }

    public Instant Instant { get; set; }
}
