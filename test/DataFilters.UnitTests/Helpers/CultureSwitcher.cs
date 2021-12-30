namespace DataFilters.UnitTests.Helpers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System;

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
        /// to the specified <paramref name="newCultureName"/>.
        /// </summary>
        /// <param name="newCultureName"></param>
        /// <param name="action"></param>
        public void Run(string newCultureName, Action action)
        {
            CurrentCulture = CultureInfo.CreateSpecificCulture(newCultureName);
            CultureInfo.CurrentCulture = CurrentCulture;
            action.Invoke();
        }

        /// <summary>
        /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
        /// to <see cref="DefaultCulture"/>.
        /// </summary>
        /// <param name="action">Action to run with</param>
        public void Run(Action action) => Run(DefaultCulture.Name, action);

        ///<inheritdoc/>
        public void Dispose() => CultureInfo.CurrentCulture = DefaultCulture;
    }
}