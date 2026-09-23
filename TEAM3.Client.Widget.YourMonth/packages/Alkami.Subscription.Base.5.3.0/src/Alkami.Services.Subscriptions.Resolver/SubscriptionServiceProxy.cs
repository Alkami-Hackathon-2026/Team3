using Alkami.Services.Subscriptions.Data;
using Alkami.Utilities.Kubernetes;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Alkami.Utilities.Rpc.Extensions;

namespace Alkami.Services.Subscriptions.Resolver
{
    internal class SubscriptionServiceProxy : ISubscriptionService
    {
        private ChannelFactory<ISubscriptionService> _channelFactory;
        private readonly ISettingsUtility _settingsUtility;
        private static ILog _logger = LogManager.GetLogger("Alkami.Services.Subscriptions.Resolver.SubscriptionServiceProxy");
        private readonly bool _enableGc;

        public SubscriptionServiceProxy()
            : this(new SettingsUtility())
        {
        }

        public SubscriptionServiceProxy(ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
            _enableGc = _settingsUtility.EnableGarbageCollection();
        }

        public List<ServiceDefinition> GetServices(List<string> serviceNames = null, bool includeKubernetesServices = true, string participatingClientVersion = null)
        {
            return SafeCallClient((client) => Task.FromResult(client.GetServices(serviceNames, includeKubernetesServices, participatingClientVersion))).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public GetServicesResponse GetServicesCheckingHash(List<string> serviceNames = null, string participatingClientVersion = null, string servicesHash = null)
        {
            return SafeCallClient((client) => Task.FromResult(client.GetServicesCheckingHash(serviceNames, participatingClientVersion, servicesHash))).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task Register(ServiceDefinition serviceDefinition)
        {
            await SafeCallClient((client) => client.Register(serviceDefinition));
        }

        public async Task RegisterMultiple(List<ServiceDefinition> serviceDefinitions)
        {
            await SafeCallClient((client) => client.RegisterMultiple(serviceDefinitions));
        }

        public async Task UnRegister(ServiceDefinition serviceDefinition)
        {
            await SafeCallClient((client) => client.UnRegister(serviceDefinition));
        }

        public async Task UnRegisterMultiple(List<ServiceDefinition> serviceDefinitions)
        {
            await SafeCallClient((client) => client.UnRegisterMultiple(serviceDefinitions));
        }

        private async Task SafeCallClient(Func<ISubscriptionService, Task> serviceCall)
        {
            await this.SafeCallClient(async x =>
            {
                await serviceCall(x);
                return Task.FromResult<object>(null);
            });
        }

        private async Task<T> SafeCallClient<T>(Func<ISubscriptionService, Task<T>> serviceCall)
        {
            var client = CreateServiceChannel();

            try
            {
                var result = await serviceCall.Invoke(client);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred contacting the Subscription Service: {ex}");
                throw;
            }
            finally
            {
                (client as IDisposable)?.Dispose();
                if (_enableGc)
                {
                    //This is here to deal with memory growth upon completion of a WCF call. Details here: https://jira.alkami.com/browse/DEV-156303
                    GC.Collect(GC.MaxGeneration);
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        internal ISubscriptionService CreateServiceChannel()
        {
            if (_channelFactory == null)
            {
                Uri address;
                var subscriptionServer = _settingsUtility.GetSubscriptionServiceMachineName();
                if (ServiceUrlSettings.IsRunningInKubernetes())
                {
                    //http://alk-svc-subscriptions.red24.svc.cluster.local:5000/SubscriptionService
                    address = new Uri($"{subscriptionServer}/SubscriptionService");
                }
                else
                {
                    var portNumber = _settingsUtility.GetServiceBindingPortNumber("http");
                    address = new Uri($"http://{subscriptionServer}:{portNumber}/SubscriptionService");
                }

                var serviceBinding = SubscriptionBindings.HttpBinding(https: false);
                var serviceEndpoint = new EndpointAddress(address);
                _channelFactory = new ChannelFactory<ISubscriptionService>(serviceBinding, serviceEndpoint);
            }

            return _channelFactory.CreateChannel();
        }

    }
}
