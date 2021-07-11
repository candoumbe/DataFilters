namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// a <see cref="PropertyName"/> holds the name of a property a filter is build against.
    /// </summary>
#if NETSTANDARD1_3
    public class PropertyName : IEquatable<PropertyName>
#else
    public record PropertyName
#endif
    {
        /// <summary>
        /// Name of the property a filter is applied to
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Builds a new <see cref="PropertyName"/> with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="name"/> is <c>string.Empty</c> or contains only whitespaces.</exception>
        public PropertyName(string name)
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

#if NETSTANDARD1_3
        ///<inheritdoc/>
        public bool Equals(PropertyName other) => Name == other?.Name;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as PropertyName);

        ///<inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{nameof(PropertyName)}[{Name}]";
#endif
    }
}
