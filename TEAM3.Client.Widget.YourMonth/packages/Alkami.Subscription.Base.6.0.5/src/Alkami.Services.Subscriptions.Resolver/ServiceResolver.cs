using Alkami.Services.Subscriptions.Data;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.Resolver
{
    public static class ServiceResolver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceResolver));

        /// <summary>
        /// Factory to create an instance of <see cref="IResolver"/>
        /// <remarks>No need to override this in the default behavior</remarks>
        /// </summary>
        public static Func<IResolver> ResolverFactory = () => InnerResolver.Instance;
        private static IResolver _innerResolver;

        /// <summary>
        /// A boolean indicating that a connection was made to the subscription service at least once. This will be used to allow the service to start up
        /// </summary>
        public static bool EstablishedConnectionAtLeastOnce { get; internal set; }
        /// <summary>
        /// a boolean indicating that the connection to the subscription service is currently connected
        /// </summary>
        public static bool IsConnected { get; private set; }

        public static event Action<List<ServiceDefinition>> OnChange;

        internal static IResolver Resolver
        {
            get
            {
                if (_innerResolver == null)
                {
                    _innerResolver = ResolverFactory();
                    _innerResolver.OnChange += _innerResolver_OnChange;
                }
                return _innerResolver;
            }
        }

        public static string ClientVersion { get => Resolver.ClientVersion; set => Resolver.ClientVersion = value; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<ServiceDefinition> AllRegisteredDefinitions()
        {
            return Resolver.AllRegisteredDefinitions;
        }

        public static Task Refresh()
        {
            IsConnected = false;
            return Resolver.RefreshData();
        }

        internal static Task ForceRefresh()
        {
            IsConnected = false;
            return Resolver.RefreshData(true);
        }

        [Obsolete("Please use RegisterAsync")]
        public static void Register(ServiceDefinition serviceDefinition)
        {
            Resolver.Heartbeat(serviceDefinition);
        }

        [Obsolete("Please use RegisterAsync")]
        public static void Register(List<ServiceDefinition> serviceDefinitions)
        {
            Resolver.Heartbeat(serviceDefinitions);
        }

        public static Task RegisterAsync(ServiceDefinition serviceDefinition)
        {
            return Resolver.HeartbeatAsync(serviceDefinition);
        }

        public static Task RegisterAsync(List<ServiceDefinition> serviceDefinitions)
        {
            return Resolver.HeartbeatAsync(serviceDefinitions);
        }

        [Obsolete("Please use UnRegisterAsync")]
        public static void UnRegister(ServiceDefinition serviceDefinition)
        {
            Resolver.Disconnecting(serviceDefinition);
        }

        [Obsolete("Please use UnRegisterAsync")]
        public static void UnRegister(List<ServiceDefinition> serviceDefinitions)
        {
            Resolver.Disconnecting(serviceDefinitions);
        }

        public static Task UnRegisterAsync(ServiceDefinition serviceDefinition)
        {
            return Resolver.DisconnectingAsync(serviceDefinition);
        }

        public static Task UnRegisterAsync(List<ServiceDefinition> serviceDefinitions)
        {
            return Resolver.DisconnectingAsync(serviceDefinitions);
        }


        static void _innerResolver_OnChange(List<ServiceDefinition> obj)
        {
            EstablishedConnectionAtLeastOnce = true;
            IsConnected = true;

            if (OnChange != null)
            {
                if (Logger.IsTraceEnabled)
                {
                    var serviceEndpoints = obj.Select(x => x.EndpointUri.ToString()).ToList();

                    Logger.Trace($"ServiceResolver OnChange event triggered with the following endpoints:\n{string.Join("   \n", serviceEndpoints)}");
                }

                OnChange(obj);
            }
        }


        internal static void HardReset()
        {
            if (_innerResolver != null)
                _innerResolver.OnChange -= _innerResolver_OnChange;
            _innerResolver = null;
        }
    }
}
