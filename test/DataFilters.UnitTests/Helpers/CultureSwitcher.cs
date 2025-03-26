namespace DataFilters.UnitTests.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// A helper class to control the <see cref="CultureInfo.CurrentCulture"/> value during tests.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CultureSwitcher : IDisposable
{
    /// <summary>
    /// The current culture used by the current instance.
    /// </summary>
    public CultureInfo CurrentCulture { get; private set; }

    /// <summary>
    /// Gets/Sets the default culture used throughout the lifecycle of the current instance.
    /// </summary>
    public CultureInfo DefaultCulture { get; set; }

    /// <summary>
    /// Bulds a new <see cref="CultureSwitcher"/> instance.
    /// </summary>
    public CultureSwitcher()
    {
        CurrentCulture = CultureInfo.CurrentCulture;
        DefaultCulture = CurrentCulture;
    }

    /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
    /// to the specified <paramref name="cultureToUse"/>.
    /// </summary>
    /// <param name="cultureToUse">Name of the culture to use when running the specified <paramref name="action"/>.</param>
    /// <param name="action">The action to run with the specified <paramref name="cultureToUse"/>.</param>
    /// <remarks>
    /// <paramref name="cultureToUse"/> will be used as Default
    /// </remarks>
    public void Run(string cultureToUse, Action action) => Run(CultureInfo.CreateSpecificCulture(cultureToUse), action);

    /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/> and
    /// <see cref="CultureInfo.DefaultThreadCurrentCulture"/> to the specified <paramref name="cultureToUse"/>.
    /// </summary>
    /// <param name="cultureToUse">The culture to use when running the specified <paramref name="action"/>.</param>
    /// <param name="action">The action to run</param>
    public void Run(CultureInfo cultureToUse, Action action)
    {
        CurrentCulture = cultureToUse;
        CultureInfo.CurrentCulture = CurrentCulture;
        action.Invoke();
        Dispose();
    }

    /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
    /// to <see cref="DefaultCulture"/>.
    /// </summary>
    /// <param name="action">Action to run with</param>
    /// <remarks>
    /// <see cref="CultureInfo.CurrentCulture"/> and <see cref="CultureInfo.DefaultThreadCurrentCulture"/> will be reverted
    /// to <see cref="DefaultCulture"/> when the current instance get disposed.
    /// </remarks>
    public void Run(Action action) => Run(DefaultCulture.Name, action);

    ///<inheritdoc/>
    public void Dispose() => CultureInfo.CurrentCulture = DefaultCulture;
}