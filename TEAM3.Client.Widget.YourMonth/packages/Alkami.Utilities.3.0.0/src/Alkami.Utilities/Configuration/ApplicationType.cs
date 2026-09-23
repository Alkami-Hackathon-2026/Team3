namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The ApplicationType enumeration defines the type of the application such as
    /// WebClient, AdminClient and Microservice.
    /// </summary>
    public enum ApplicationType
    {
        /// <summary>
        /// The application type is either unknown or not configured.
        /// </summary>
        Unknown,
        /// <summary>
        /// The application type is Web client.
        /// </summary>
        WebClient,
        /// <summary>
        /// The application type is Admin client.
        /// </summary>
        AdminClient,
        /// <summary>
        /// The application type is IP-STS.
        /// </summary>
        IPSTS,
        /// <summary>
        /// The application type is RP-STS.
        /// </summary>
        RPSTS,
        /// <summary>
        /// The application type is a WCF Service.
        /// </summary>
        WcfService,
        /// <summary>
        /// The application type is a web service.
        /// </summary>
        WebService,
        /// <summary>
        /// The application type is a microservice.
        /// </summary>
        Microservice,
        /// <summary>
        /// The application type is something else.
        /// </summary>
        Other
    }
}