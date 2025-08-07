
using System.Diagnostics.CodeAnalysis;
using NodaTime;

namespace DataFilters.Expressions.UnitTests;
[ExcludeFromCodeCoverage]
public class NodaTimeClass
{
    public LocalDate LocalDate { get; set; }

    public LocalDateTime LocalDateTime { get; set; }

    public Instant Instant { get; set; }
}
