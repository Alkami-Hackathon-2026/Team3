#if NETFRAMEWORK
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

namespace Alkami.Utilities
{
    /// <summary>
    /// Utility methods to handle IP addresses, request headers and user IP lookups
    /// </summary>
    public static class IpAddressUtility
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IpAddressUtility));

        /// <summary>
        /// HTTP headers to look for when trying to guess the user's IP address.
        /// Headers are checked in order with a final fallback to REMOTE_ADDR if
        /// all other headers are missing. See
        ///
        /// http://stackoverflow.com/questions/1634782/what-is-the-most-accurate-way-to-retrieve-a-users-correct-ip-address-in-php
        ///
        /// for more information.
        /// </summary>
        public static readonly string[] HttpIPHeaders =
        {
            // IIS interprets any underscore (_) characters in <HeaderName> as dashes in the actual header.
            // For example, if you specify HTTP_MY_HEADER, the server searches for a request header named MY-HEADER.
            // Alkami's Load Balancers will add X-Forwarded-For, so we will look for that header first, followed by
            // X-Cluster-Client-IP, then the rest.
            "HTTP_TRUE_CLIENT_IP",
            "HTTP_X_FORWARDED_FOR",
            "HTTP_X_CLUSTER_CLIENT_IP",
            "HTTP_X_FORWARDED",
            "HTTP_FORWARDED_FOR",
            "HTTP_FORWARDED",
            "HTTP_CLIENT_IP",
            "REMOTE_ADDR"
        };

        /// <summary>
        /// Stub to use to override for testing
        /// </summary>
        public static Func<HttpContextBase> Context = () => new HttpContextWrapper(HttpContext.Current);

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="validateFunc">
        /// A callback function to determine if a given IP address should be considered
        /// valid or not. If the validation function returns false (that is, it determines
        /// that an IP address is invalid) the IP address is not returned to the caller.
        /// If the validation function is not specified, all IP addresses are valid.
        /// </param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(string[] headerNames = null, Func<IPAddress, bool> validateFunc = null, IPAddress ipDefault = null)
        {
            return Context().GetUserIpAddress(headerNames, validateFunc, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(string[] headerNames = null, IPAddress ipDefault = null)
        {
            return Context().GetUserIpAddress(headerNames, null, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(IPAddress ipDefault = null)
        {
            return Context().GetUserIpAddress(IpAddressUtility.HttpIPHeaders, null, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="context">HttpContext for the request.</param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(this HttpContextBase context, IPAddress ipDefault = null)
        {
            return context.GetUserIpAddress(IpAddressUtility.HttpIPHeaders, null, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="context">HttpContext for the request.</param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(this HttpContext context, IPAddress ipDefault = null)
        {
            return new HttpContextWrapper(context).GetUserIpAddress(IpAddressUtility.HttpIPHeaders, null, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="context">HttpContext for the request.</param>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(this HttpContext context, string[] headerNames = null, IPAddress ipDefault = null)
        {
            return new HttpContextWrapper(context).GetUserIpAddress(headerNames, null, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="context">HttpContext for the request.</param>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="validateFunc">
        /// A callback function to determine if a given IP address should be considered
        /// valid or not. If the validation function returns false (that is, it determines
        /// that an IP address is invalid) the IP address is not returned to the caller.
        /// If the validation function is not specified, all IP addresses are valid.
        /// </param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(this HttpContext context, string[] headerNames = null, Func<IPAddress, bool> validateFunc = null, IPAddress ipDefault = null)
        {
            return new HttpContextWrapper(context).GetUserIpAddress(headerNames, validateFunc, ipDefault);
        }

        /// <summary>
        /// Determines the user's IP address from the current request (if available)
        /// by checking multiple request headers until an IP address is found. If
        /// the IP address cannot be determined, a default IP address is returned
        /// (null if not otherwise specified).
        /// </summary>
        /// <param name="context">HttpContext for the request.</param>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="validateFunc">
        /// A callback function to determine if a given IP address should be considered
        /// valid or not. If the validation function returns false (that is, it determines
        /// that an IP address is invalid) the IP address is not returned to the caller.
        /// If the validation function is not specified, all IP addresses are valid.
        /// </param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(this HttpContextBase context, string[] headerNames = null, Func<IPAddress, bool> validateFunc = null, IPAddress ipDefault = null)
        {
            return context == null
                ? ipDefault
                : GetUserIpAddress(context.Request.ServerVariables, headerNames, validateFunc, ipDefault);
        }

        /// <summary>
        /// This is an overload if you don't have an HttpContext
        /// </summary>
        /// <param name="httpRequestHeaders"></param>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="validateFunc">
        /// A callback function to determine if a given IP address should be considered
        /// valid or not. If the validation function returns false (that is, it determines
        /// that an IP address is invalid) the IP address is not returned to the caller.
        /// If the validation function is not specified, all IP addresses are valid.
        /// </param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(IEnumerable<KeyValuePair<string, IEnumerable<string>>> httpRequestHeaders, string[] headerNames = null, Func<IPAddress, bool> validateFunc = null, IPAddress ipDefault = null)
        {
            NameValueCollection headerValues = new NameValueCollection();
            foreach (var h in httpRequestHeaders)
            {
                var k = "HTTP_" + h.Key.Replace('-', '_').ToUpperInvariant();
                var v = string.Join(",", h.Value);
                headerValues.Add(k, v);
            }
            return GetUserIpAddress(headerValues, headerNames, validateFunc, ipDefault);
        }

        /// <summary>
        /// This is an overload if you don't have an HttpContext
        /// </summary>
        /// <param name="serverVariables">The request server variables or headers</param>
        /// <param name="headerNames">List of HTTP header names to check.</param>
        /// <param name="validateFunc">
        /// A callback function to determine if a given IP address should be considered
        /// valid or not. If the validation function returns false (that is, it determines
        /// that an IP address is invalid) the IP address is not returned to the caller.
        /// If the validation function is not specified, all IP addresses are valid.
        /// </param>
        /// <param name="ipDefault">
        /// Default IP address to return if a valid IP address cannot be picked for
        /// the request.
        /// </param>
        /// <returns>Returns the IP address for the request in the specified context.</returns>
        public static IPAddress GetUserIpAddress(NameValueCollection serverVariables, string[] headerNames = null, Func<IPAddress, bool> validateFunc = null, IPAddress ipDefault = null)
        {
            if (serverVariables.Count == 0 || !serverVariables.HasKeys())
            {
                return ipDefault;
            }

            if (headerNames == null)
            {
                headerNames = IpAddressUtility.HttpIPHeaders;
            }

            try
            {
                foreach (var headerName in headerNames.Intersect(serverVariables.AllKeys))
                {
                    Logger.TraceFormat("Attempting to get the IP address from the header with name [{0}]", headerName);

                    var headerPossibleIPs = serverVariables.Get(headerName);

                    Logger.TraceFormat("Found this value for this header: [{0}] value: [{1}]", headerName, headerPossibleIPs);

                    if (string.IsNullOrWhiteSpace(headerPossibleIPs))
                    {
                        continue;
                    }

                    // A given header may contain multiple comma-separated IP addresses.
                    var ipStrings = headerPossibleIPs.Split(',');

                    foreach (var ipString in ipStrings)
                    {
                        IPAddress ipAddress;
                        if (IPAddress.TryParse(ipString, out ipAddress) && (validateFunc == null || validateFunc(ipAddress)))
                        {
                            Logger.TraceFormat("Found a valid response IPAddress [{0}]", ipAddress);
                            return ipAddress;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("could not parse IP address", ex);
            }

            Logger.Trace("returning the default IP address");
            return ipDefault;
        }
    }
}
#endif
