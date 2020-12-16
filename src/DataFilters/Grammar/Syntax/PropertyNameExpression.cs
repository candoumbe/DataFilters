using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Expression that holds the name of a property a filter is build against
    /// </summary>
    public class PropertyNameExpression : FilterExpression, IEquatable<PropertyNameExpression>
    {
        /// <summary>
        /// Name of the property a filter is applied to
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Builds a new <see cref="PropertyNameExpression"/> with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="name"/> is <c>string.Empty</c> or contains only whitespaces.</exception>
        public PropertyNameExpression(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            Name = name;
        }

        public bool Equals(PropertyNameExpression other) => Name == other?.Name;

        public override bool Equals(object obj) => Equals(obj as PropertyNameExpression);

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => $"{nameof(PropertyNameExpression)}[{Name}]";
    }
}
