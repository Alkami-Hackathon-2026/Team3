namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The HostingType enumeration defines the type of the hosting environment such as
    /// Firehost, AWS and OnPremise.
    /// </summary>
    public enum HostingType
    {
        /// <summary>
        /// The hosting type is either unknown or not configured.
        /// </summary>
        Unknown,
        /// <summary>
        /// There is no hosting environment. This is appropriate for development, QA
        /// and internal servers.
        /// </summary>
        None,
        /// <summary>
        /// The hosting environment is Firehost.
        /// </summary>
        Firehost,
        /// <summary>
        /// The hosting environment is an on premise environment.
        /// </summary>
        OnPremise,
        /// <summary>
        /// The hosting envrionment is within Amazon AWS.
        /// </summary>
        Aws,
        /// <summary>
        /// The hosting environment is within Microsoft Azure.
        /// </summary>
        Azure,
        /// <summary>
        /// The hosting environment is some other environment.
        /// </summary>
        Other
    }
}