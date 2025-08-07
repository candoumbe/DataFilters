using System.Diagnostics.CodeAnalysis;

namespace DataFilters.TestObjects
{
    [ExcludeFromCodeCoverage]
    public class Model
    {
        public string PascalCaseProperty { get; set; }

#pragma warning disable IDE1006 // Styles d'affectation de noms
        public string snake_case_property { get; set; }

        public string CAPITALCASINGPROPERTY { get; set; }
#pragma warning restore IDE1006 // Styles d'affectation de noms
    }
}