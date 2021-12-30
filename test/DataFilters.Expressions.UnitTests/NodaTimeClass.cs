namespace DataFilters.Expressions.UnitTests;

using NodaTime;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class NodaTimeClass
{
    public LocalDate LocalDate { get; set; }

    public LocalDateTime LocalDateTime { get; set; }

    public Instant Instant { get; set; }
}
