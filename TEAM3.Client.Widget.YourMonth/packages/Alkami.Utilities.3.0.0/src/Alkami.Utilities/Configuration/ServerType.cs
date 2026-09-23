namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The ServerType enumeration defines the type of the server type such as
    /// Web, App, Radium, Nag or SQL.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// The server type is either unknown or not configured.
        /// </summary>
        Unknown,
        /// <summary>
        /// The server type contains all tiers. This is appropriate for development environments.
        /// </summary>
        All,
        /// <summary>
        /// The server type is on the web tier.
        /// </summary>
        Web,
        /// <summary>
        /// The server type is on the app tier.
        /// </summary>
        App,
        /// <summary>
        /// The server type is SQL Server.
        /// </summary>
        Sql,
        /// <summary>
        /// The server type only contains Radium.
        /// </summary>
        Radium,
        /// <summary>
        /// The server type only contains Nag.
        /// </summary>
        Nag,
        /// <summary>
        /// The server type is some other type.
        /// </summary>
        Other
    }
}