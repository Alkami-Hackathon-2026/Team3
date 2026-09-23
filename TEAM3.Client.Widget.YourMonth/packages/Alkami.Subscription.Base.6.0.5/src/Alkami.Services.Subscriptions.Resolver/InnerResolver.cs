using Alkami.Monitoring;
using Alkami.Services.Subscriptions.Data;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.Resolver
{
    public class InnerResolver : IResolver, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InnerResolver));
        protected readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        protected List<ServiceDefinition> ServiceDefinitions = new List<ServiceDefinition>();
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly RegisteredWaitHandle _handle;
        private readonly ActivityListener _listener;

        /// <summary>
        /// 
        /// </summary>
        public event Action<List<ServiceDefinition>> OnChange;

        public static ServiceDefinition MyDefinition;

        private ISubscriptionService Client;

        private bool _onDemandServiceDefinitionRefreshIsEnabled;

        private string _servicesHash;

        private bool _enableCaching;

        internal static InnerResolver Instance { get; set; }

        /// <inheritdoc/>
        public string ClientVersion { get; set; }


        static InnerResolver()
        {
            Instance = new InnerResolver();
        }

        public InnerResolver()
            : this(new SubscriptionServiceProxy())
        {

        }

        ~InnerResolver()
        {
            if (_handle != null)
                _handle.Unregister(_autoResetEvent);
        }

        /// <inheritdoc />
        public async Task RefreshData(bool forceRefresh = false)
        {
            await Task.Yield();

            if (forceRefresh || _onDemandServiceDefinitionRefreshIsEnabled)
            {
                _servicesHash = null;
                CallBack(null, false);
            }
        }

        /// <inheritdoc />
        public List<ServiceDefinition> AllRegisteredDefinitions
        {
            get
            {
                try
                {
                    Lock.EnterReadLock();
                    return ServiceDefinitions.ToList();
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        [Obsolete("Please use HeartbeatAsync")]
        public void Heartbeat(ServiceDefinition serviceDefinition)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            try
            {
                MyDefinition = serviceDefinition;

                Client.Register(serviceDefinition).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        [Obsolete("Please use HeartbeatAsync")]
        public void Heartbeat(List<ServiceDefinition> serviceDefinitions)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            try
            {
                HeartbeatAsync(serviceDefinitions).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        /// <inheritdoc />
        public async Task HeartbeatAsync(ServiceDefinition serviceDefinition)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            await Task.Yield();
            try
            {
                MyDefinition = serviceDefinition;

                await Client.Register(serviceDefinition);
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        /// <inheritdoc />
        public async Task HeartbeatAsync(List<ServiceDefinition> serviceDefinitions)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            await Task.Yield();
            try
            {
                MyDefinition = serviceDefinitions.FirstOrDefault();

                //If caching is enabled then we can assume we are talking to latest subs in k8s else we must assume we are using the local 4.0.4 version of subs and should register each endpoint individually
                if (_enableCaching)
                {
                    await Client.RegisterMultiple(serviceDefinitions);
                }
                else
                {
                    foreach (var serviceDefinition in serviceDefinitions)
                    {
                        await Client.Register(serviceDefinition);
                    }
                }
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        private void CallBack(object state, bool timedOut)
        {
            using var activity = SubscriptionDiagonistics.ActivitySource.StartActivity("GET Subscriptions", ActivityKind.Internal);
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            try
            {
                List<ServiceDefinition> latest;
                var serviceHashMatched = false;
                if (_enableCaching)
                {
                    var getServicesResponse = Client.GetServicesCheckingHash(null, ClientVersion, _servicesHash);

                    // Latest will be empty here if the service list hasn't changed since the last request to Subs
                    latest = getServicesResponse.ServiceDefinitions;
                    _servicesHash = getServicesResponse.ServicesHash;
                    serviceHashMatched = getServicesResponse.ServiceHashMatched;
                }
                else
                {
                    latest = Client.GetServices(null, true, ClientVersion);
                    _servicesHash = null;
                    serviceHashMatched = false;
                }

                if (!serviceHashMatched)
                {
                    try
                    {
                        Lock.EnterWriteLock();
                        ServiceDefinitions.Clear();
                        ServiceDefinitions.AddRange(latest);
                    }
                    finally
                    {
                        Lock.ExitWriteLock();
                    }
                }

                if (Environment.UserInteractive)
                    PerformTraceLogging(serviceHashMatched, ServiceDefinitions);

                if (OnChange != null)
                    OnChange(ServiceDefinitions);
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
                Logger.Error("Exception refreshing services list: ", e);
            }
        }

        private void PerformTraceLogging(bool serviceHashMatched, List<ServiceDefinition> serviceDefinitions)
        {
            if (Logger.IsTraceEnabled)
            {
                if (MyDefinition != null)
                    Logger.TraceFormat("Endpoint For This Service: {0}", MyDefinition.EndpointUri);

                if (serviceHashMatched)
                    Logger.TraceFormat("No service changes detected:  {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToString("hh:mm:ss"));
                else
                    Logger.TraceFormat("Updated services:  {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToString("hh:mm:ss"));

                LogAvailableServices(serviceDefinitions);
            }
        }

        [Obsolete("Please use DisconnectingAsync")]
        public void Disconnecting(ServiceDefinition serviceDefinition)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            try
            {
                Client.UnRegister(serviceDefinition).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        [Obsolete("Please use DisconnectingAsync")]
        public void Disconnecting(List<ServiceDefinition> serviceDefinitions)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            try
            {
                DisconnectingAsync(serviceDefinitions).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        public async Task DisconnectingAsync(ServiceDefinition serviceDefinition)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            await Task.Yield();
            try
            {
                await Client.UnRegister(serviceDefinition);
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        public async Task DisconnectingAsync(List<ServiceDefinition> serviceDefinitions)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            await Task.Yield();
            try
            {
                //If caching is enabled then we can assume we are talking to latest subs in k8s else we must assume we are using the local 4.0.4 version of subs and should register each endpoint individually
                if (_enableCaching)
                {
                    await Client.UnRegisterMultiple(serviceDefinitions);
                }
                else
                {
                    foreach (var serviceDefinition in serviceDefinitions)
                    {
                        await Client.UnRegister(serviceDefinition);
                    }
                }
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
#if NETFRAMEWORK || NET8_0_OR_GREATER
                Activity.Current?.AddException(e);
#endif
            }
        }

        private static void LogAvailableServices(List<ServiceDefinition> services)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Available Services:");

            foreach (var group in services.GroupBy(x => x.FriendlyName))
            {
                stringBuilder.AppendLine(group.Key);

                foreach (var service in group)
                {
                    stringBuilder.AppendFormat("{0} [{1}]\n", service.EndpointUri.ToString(), service.CurrentVersion);
                }
            }

            Logger.Trace(stringBuilder.ToString());
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _listener?.Dispose();
        }

        internal InnerResolver(ISubscriptionService subscriptionServiceProxy)
            : this(subscriptionServiceProxy, new SettingsUtility())
        {

        }

        internal InnerResolver(ISubscriptionService subscriptionServiceProxy, ISettingsUtility settingsUtility)
        {
            Client = subscriptionServiceProxy;
            _onDemandServiceDefinitionRefreshIsEnabled = settingsUtility.GetOnDemandServiceDefinitionRefreshIsEnabled();
            _enableCaching = settingsUtility.GetIsSubscriptionCachingEnabled();

            int refreshIntervalSeconds = settingsUtility.GetServiceDefinitionRefreshInterval(_onDemandServiceDefinitionRefreshIsEnabled);

            this.ClientVersion = typeof(InnerResolver).Assembly.GetName().Version?.ToString(3);

            _listener = new ActivityListener
            {
                ShouldListenTo = (source) => string.Equals(source.Name, SubscriptionDiagonistics.ActivitySourceName),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.PropagationData,
            };
            ActivitySource.AddActivityListener(_listener);

            CallBack(null, false);

            _handle = ThreadPool.RegisterWaitForSingleObject(_autoResetEvent, CallBack, null, TimeSpan.FromSeconds(refreshIntervalSeconds), false);
        }
    }
}
