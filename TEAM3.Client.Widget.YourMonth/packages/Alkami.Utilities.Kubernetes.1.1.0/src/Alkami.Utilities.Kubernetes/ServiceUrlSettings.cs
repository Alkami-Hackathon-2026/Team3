using Alkami.Utilities.Exceptions;
using System;
using System.Runtime.InteropServices;

namespace Alkami.Utilities.Kubernetes
{
    /// <summary>
    /// Standard helpers to get information related to the Service Url
    /// </summary>
    public static class ServiceUrlSettings
    {
        /// <summary>
        /// The environment variable that contains the DNS namespace of the Kubernetes docker container
        /// </summary>
        public const string EnvironmentVariable_DnsNamespace = "ALKAMI_DNS_NAMESPACE";

        /// <summary>
        /// The environment variable that is contains the host of the Kubernetes Host if running in Kubernetes
        /// </summary>
        public const string EnvironmentVariable_KubernetesServiceHost = "KUBERNETES_SERVICE_HOST";

        /// <summary>
        /// The environment variable that determines which port the application in k8s pod is running on
        /// </summary>
        public const string EnvironmentVariable_AppPort = "ALKAMI_APP_PORT";

        /// <summary>
        /// The environment variable that determines if the application is running on a dev box
        /// </summary>
        public const string EnvironmentVariable_IsDev = "ALKAMI_ENVIRONMENT_IS_DEVBOX";

        /// <summary>
        /// Environment variable that determines if egress to EC2 from Kubernetes is enabled.
        /// </summary>
        public const string EnvironmentVariable_KubernetesEgressToEc2EnvVarName = "ENABLE_K8_EGRESS_TO_EC2";

        /// <summary>
        /// Determines whether or not a service should actively participate to Subscriptions with Participating Client.
        /// Scenarios where this would not be enabled are: Kubernetes services without the EC2 callback enabled.
        /// All services running on Windows will have this active by default.
        /// </summary>
        public static bool ShouldGetServicesFromSubscriptionMS()
        {
#if NET6_0_OR_GREATER
            bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable_KubernetesEgressToEc2EnvVarName), out var shouldGetServicesFromSubscriptionMs);
            shouldGetServicesFromSubscriptionMs |= IsRunningInWindows();
            return shouldGetServicesFromSubscriptionMs;
#else
            return true;
#endif
        }

        internal static Func<bool> IsRunningInWindowsInternal = () => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Whether or not we're running in Windows.
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningInWindows()
        {
            return IsRunningInWindowsInternal();
        }


        /// <summary>
        /// Determines if the current process is running in Kubernetes
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningInKubernetes()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariable_KubernetesServiceHost));
        }

        /// <summary>
        /// Determines if the current process has been setup to run as an Developer's machine
        /// </summary>
        /// <returns></returns>
        public static bool IsDevBox()
        {
            _ = bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable_IsDev), out var devBoxEnabled);
            return devBoxEnabled;
        }

        /// <summary>
        /// Determines the default application port that is set for the running application
        /// </summary>
        /// <returns></returns>
        public static int GetAppPort()
        {
            return int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable_AppPort), out int aPort) ? aPort : 5000;
        }

        /// <summary>
        /// Builds out the service url to connect to a service in kubernetes based on if the current process is running in kubernetes or not
        /// </summary>
        /// <param name="serviceHostName">This is the host name of the service that is running in kubernetes</param>
        /// <returns>The full url of the service</returns>
        public static string GenerateServiceUrl(string serviceHostName)
        {
            if (string.IsNullOrWhiteSpace(serviceHostName))
                throw new ArgumentException("Must have a non-empty value", nameof(serviceHostName));

            var dnsNamespace = Environment.GetEnvironmentVariable(EnvironmentVariable_DnsNamespace);

            if (string.IsNullOrEmpty(dnsNamespace))
                throw new ConfigurationException($"Missing env variable: {EnvironmentVariable_DnsNamespace}.");

            var appPort = GetAppPort();

            string result;

            if (IsRunningInKubernetes())
            {
                // Kubernetes encrypts service to service communication via a sidecar policy, also specify the app port as we're behind the proxy.
                result = $"http://{serviceHostName}.{dnsNamespace}.svc.cluster.local:{appPort}";
            }
            else
            {
                // If we are in local dev we have to specify the port since we are not behind the proxy  
                var sslPort = IsDevBox() ? appPort : 443;
                result = $"https://{dnsNamespace}:{sslPort}/{serviceHostName}";
            }

            return result;
        }
    }

}
