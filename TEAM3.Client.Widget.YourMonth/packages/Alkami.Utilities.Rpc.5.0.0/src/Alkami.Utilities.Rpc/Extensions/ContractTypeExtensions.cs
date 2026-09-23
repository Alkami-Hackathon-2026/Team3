using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Alkami.Utilities.Rpc.Extensions
{
    /// <summary>
    /// Class containing extension methods for contract types.
    /// </summary>
    public static class ContractTypeExtensions
    {
        /// <summary>
        /// Builds the route name by convention for a provider.
        /// </summary>
        /// <param name="contractType">Type representing a service contract</param>
        /// <param name="providerType">Provider type of the provider-based service</param>
        /// <param name="providerName">The provider name for this service as configured in the database.</param>
        public static string BuildRoutedNameFromContractTypeForWcfProvider(this Type contractType, string providerType, string providerName)
        {
            var (_, version) = GetIdentifierAndVersionFromContractType(contractType, RouteNameServiceType.Rpc);
            return BuildRouteNameForWcfProvider(providerType, providerName, version);
        }

        /// <summary>
        /// Builds the route name by convention from a namespaced contract type and version for a provider.
        /// </summary>
        /// <param name="contractString">The full name of the contract (eg. Alkami.MS.RustyShackleford.Contracts.IShacklefordContract)</param>
        /// <param name="providerType">Provider type of the provider-based service</param>
        /// <param name="providerName">The name of the provider as configured in the database</param>
        /// <param name="currentVersion">The version of the contract.</param>
        public static string BuildRoutedNameFromNamespacedContractStringForWcfProvider(this string contractString, string providerType, string providerName, Version currentVersion)
        {
            return BuildRouteNameForWcfProvider(providerType, providerName, currentVersion?.Major);
        }


        /// <summary>
        /// Builds the route name by convention (Defaults build for <see cref="RouteNameServiceType.Rpc"/>)
        /// </summary>
        /// <param name="contractType"></param>
        public static string BuildRoutedNameFromContractType(this Type contractType)
        {
            return BuildRoutedNameFromContractType(contractType, RouteNameServiceType.Rpc);
        }

        /// <summary>
        /// Builds the route name by convention
        /// </summary>
        /// <param name="contractType"></param>
        /// <param name="serviceType">Indicates the type of service this url needs to be generated for</param>
        public static string BuildRoutedNameFromContractType(this Type contractType, RouteNameServiceType serviceType)
        {
            var (identifier, version) = GetIdentifierAndVersionFromContractType(contractType, serviceType);
            return BuildRouteName(identifier, version, serviceType);
        }

        /// <summary>
        /// Builds the route name by convention from a namespaced contract type and version. (Defaults build for <see cref="RouteNameServiceType.Rpc"/>)
        /// </summary>
        /// <param name="contractString">The full name of the contract (eg. Alkami.MS.RustyShackleford.Contracts.IShacklefordContract)</param>
        /// <param name="currentVersion">The version of the contract.</param>
        public static string BuildRoutedNameFromNamespacedContractString(this string contractString, Version currentVersion)
        {
            return BuildRoutedNameFromNamespacedContractString(contractString, currentVersion, RouteNameServiceType.Rpc);
        }


        /// <summary>
        /// Builds the route name by convention from a namespaced contract type and version.
        /// </summary>
        /// <param name="contractString">The full name of the contract (eg. Alkami.MS.RustyShackleford.Contracts.IShacklefordContract)</param>
        /// <param name="currentVersion">The version of the contract.</param>
        /// <param name="serviceType">Indicates the type of service this url needs to be generated for</param>
        public static string BuildRoutedNameFromNamespacedContractString(this string contractString, Version currentVersion, RouteNameServiceType serviceType)
        {
            if(currentVersion == null)
            {
                throw new ArgumentNullException(nameof(currentVersion));
            }

            var contractName = contractString?.Substring(contractString.LastIndexOf('.') + 1);
            return BuildRouteName(contractName, currentVersion.Major, serviceType);
        }

        /// <summary>
        /// Builds the Route name for the passed in service path and major version
        /// </summary>
        /// <param name="serviceIdentifier">This is the service path that will appear in the route</param>
        /// <param name="majorVersion">The major version of the service.</param>
        /// <param name="serviceType"></param>
        public static string BuildRouteName(string serviceIdentifier, int majorVersion, RouteNameServiceType serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceIdentifier))
                throw new ArgumentNullException(nameof(serviceIdentifier));

            if (majorVersion < 0)
                throw new ArgumentException($"{nameof(majorVersion)} cannot be less than zero when building routed name.");

            var hostName = $"alk-svc-{serviceType}-{serviceIdentifier}-v{majorVersion}".ToLowerInvariant();

            return hostName;
        }

        /// <summary>
        /// Builds the Kubernetes route for a WCF provider-based service
        /// </summary>
        /// <param name="providerType">Provider type of the provider-based service</param>
        /// <param name="providerName">The provider name for this service as configured in the database.</param>
        /// <param name="majorVersion">The major version of the service.</param>
        private static string BuildRouteNameForWcfProvider(string providerType, string providerName, int? majorVersion)
        {
            if (string.IsNullOrWhiteSpace(providerType))
                throw new ArgumentNullException(nameof(providerType));

            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentNullException(nameof(providerName));

            if (majorVersion == null)
                throw new ArgumentNullException(nameof(majorVersion));

            if (majorVersion < 0)
                throw new ArgumentException($"{nameof(majorVersion)} cannot be less than zero when building routed name.");

            providerName = providerName.RemoveNonAlphaNumericCharacters();

            var hostName = $"{providerType}-{providerName}-v{majorVersion}".ToLowerInvariant().Replace("provider", "");
            return hostName;
        }

        private static (string serviceIdentifier, int majorVersion) GetIdentifierAndVersionFromContractType(Type contractType, RouteNameServiceType serviceType)
        {
            contractType = contractType ?? throw new ArgumentNullException(nameof(contractType));

            var customAttributes = contractType.GetCustomAttributes(false);
            var serviceAttribute = customAttributes.FirstOrDefault(a => string.Equals(a.GetType().FullName, "Alkami.Utilities.ServiceAttributes.ServiceClientAttribute", StringComparison.InvariantCultureIgnoreCase));

            if(serviceAttribute == null)
            {
                return (contractType.Name, contractType.Assembly.GetName().Version.Major);
            }

            else
            {
                var attributeType = serviceAttribute.GetType();
                var majorVersionProp = attributeType.GetProperty("MajorVersion");

                var majorVersion = (int?)majorVersionProp?.GetValue(serviceAttribute);
                if (majorVersion == null || majorVersion <= 0)
                {
                    majorVersion = contractType.Assembly.GetName().Version?.Major ?? 0;
                }

                var servicePathProp = attributeType.GetProperty("ServicePath");
                var servicePath = (string)servicePathProp?.GetValue(serviceAttribute) ?? string.Empty;

                if (serviceType == RouteNameServiceType.Rpc)
                {
                    servicePath = servicePath.Substring(servicePath.LastIndexOf('.') + 1);
                }

                return (servicePath, majorVersion.GetValueOrDefault());
            }
        }

        private static readonly Regex InvalidChars = new Regex(@"[\W_]+", RegexOptions.Compiled);
        private static string RemoveNonAlphaNumericCharacters(this string value)
        {
            return value == null ? string.Empty : InvalidChars.Replace(value, string.Empty);
        }
    }
}
