namespace DataFilters.Expressions;

/// <summary>
/// Defines the desired behavior when turning a <see cref="IFilter"/> into an <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
/// </summary>
public enum NullableValueBehavior
{
    /// <summary>
    /// The computed expression will not try to prevent <c>NullReferenceException</c> from being thrown
    /// </summary>
    /// <remarks>
    /// This is the default behavior
    /// </remarks>
    NoAction,

    /// <summary>
    /// Null checks will be added when necessary before accessing a property
    /// </summary>
    AddNullCheck
}
