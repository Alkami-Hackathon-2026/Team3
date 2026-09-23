using System;

namespace Alkami.Monitoring
{
    /// <summary>
    /// Adds Suppression methods to the Monitoring class
    /// </summary>
    public interface IMonitorSuppression
    {
        /// <summary>
        /// Begins a logical operation scope for suppressing data to a specific <see cref="IMonitorMinion"/>
        /// </summary>
        /// <param name="minionType">The minion type to suppress.</param>
        /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
        IDisposable BeginSuppression(string minionType);
    }
}
