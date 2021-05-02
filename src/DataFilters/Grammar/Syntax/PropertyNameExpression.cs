using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// a <see cref="FilterExpression"/> that holds the name of a property a filter is build against.
    /// </summary>
    public sealed class PropertyNameExpression : FilterExpression, IEquatable<PropertyNameExpression>
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

        ///<inheritdoc/>
        public bool Equals(PropertyNameExpression other) => Name == other?.Name;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as PropertyNameExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{nameof(PropertyNameExpression)}[{Name}]";
    }
}
