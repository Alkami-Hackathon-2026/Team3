using System;

namespace Alkami.Monitoring
{

    /// <summary>
    /// Defines alias for <see cref="IMonitorMinion"/> implementation to be used in filtering rules.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MonitorMinionAliasAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="MonitorMinionAliasAttribute"/> instance.
        /// </summary>
        /// <param name="alias">The alias to set.</param>
        public MonitorMinionAliasAttribute(string alias)
        {
            Alias = alias;
        }

        /// <summary>
        /// The alias of the monitor minion.
        /// </summary>
        public string Alias { get; }
    }
}
