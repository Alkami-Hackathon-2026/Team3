namespace Alkami.Utilities.Rpc.Extensions
{
    /// <summary>
    /// Service type used in generation of k8s route names
    /// </summary>
    public enum RouteNameServiceType
    {
        /// <summary>
        /// Indicates WCF based endpoints
        /// </summary>
        Rpc,
        /// <summary>
        /// Indicates REST based endpoints
        /// </summary>
        Rest
    }
}
