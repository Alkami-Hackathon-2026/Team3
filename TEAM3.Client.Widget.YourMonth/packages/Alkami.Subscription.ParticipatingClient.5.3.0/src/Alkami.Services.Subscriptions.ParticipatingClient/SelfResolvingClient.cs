using Alkami.Broker.App;
using Alkami.Broker.MessageTemplates;
using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Monitoring;
using Alkami.Services.Subscriptions.Data;
using Alkami.Services.Subscriptions.Resolver;
using Alkami.Utilities.Kubernetes;
using Alkami.Utilities.Rpc.Extensions;
using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    ///
    /// </summary>
    public class SelfResolvingClient
    {
        /// <summary>
        /// Gets or sets the legacy shim factory.
        /// </summary>
        /// <value>
        /// The legacy shim factory.
        /// </value>
        public static Func<ClaimsIdentity> LegacyShimFactory
        {
            get { return SecurityInjectorInspector.LegacyShimFactory; }
            set { SecurityInjectorInspector.LegacyShimFactory = value; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelfResolvingClient<T> : SelfResolvingClient, IDisposable where T : class
    {
        private static readonly ILog _logger;
        private static readonly TimeSpan DefaultCoolDownPeriodInSeconds = TimeSpan.FromSeconds(60);
        internal static IServiceEndpointResolver<T> _endpointResolver;
        internal static IServiceEndpointRepository<T> _serviceEndpointRepository;
        private static Subject<bool> coolDownSubject;
        private static bool _found;
        public static IEnumerable<ServiceEndpoint<T>> K8sServiceEndpoints => ServiceEndpoints.Where(s => s.ServiceDefinition.K8sConfiguration != null);

        public const string EnvironmentVariable_FavoredClaimsIdentitySerializationMethod = "ALKAMI_FAVORED_SUT_SERIALIZATION";
        private int? timeoutSeconds = null;
        protected Version MinVersion;
        protected string Name;
        private static ISettingsUtility _settingsUtility = new SettingsUtility();

        static SelfResolvingClient()
        {
            _logger = LogManager.GetLogger(typeof(SelfResolvingClient<T>));

            if (!ServiceUrlSettings.ShouldGetServicesFromSubscriptionMS())
                return;

            _endpointResolver = new ServiceEndpointResolver<T>(_logger);
            coolDownSubject = new Subject<bool>();
            _found = true;

            var seconds = DefaultCoolDownPeriodInSeconds;
            if (ConfigurationManager.AppSettings["CoolDownPeriodInSeconds"] != null)
            {
                TimeSpan.TryParse(ConfigurationManager.AppSettings["CoolDownPeriodInSeconds"].ToString(), out seconds);
            }

            _serviceEndpointRepository = new ServiceEndpointRepository<T>(_logger);
            _serviceEndpointRepository.InitializeServiceResolverListener();

            coolDownSubject.Buffer(seconds).Subscribe(list =>
            {
                if (!_found)
                {
                    _found = true;
                    _logger.Trace($"Circuit breaker flipped on for {typeof(T).FullName}.");
                }
            });

            Subscription.Add(Events.StateChangeArgs, dictionary =>
            {
                var args = dictionary.ConvertArgsTo<StatusChangeArgs>();

                if (args.IsValid() && args.IsShuttingDown)
                {
                    string extraData;
                    if (args.TryGetValue("ServiceDefinition", out extraData))
                    {
                        try
                        {
                            var definition = JsonConvert.DeserializeObject<ServiceDefinition>(extraData);
                            _serviceEndpointRepository.ServiceEndpointHostHasIssues(definition.EndpointUri);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn(
                                $"A problem occurred trying to handle a status changed event. The message received was {extraData}. \t\t The error was as follows: {ex}");
                        }
                    }
                }

                if (!_found && args.IsValid() && args.IsStartingUp)
                {
                    string extraData;
                    if (args.TryGetValue("ServiceDefinition", out extraData))
                    {
                        try
                        {
                            var definition = JsonConvert.DeserializeObject<ServiceDefinition>(extraData);
                            _found = definition.Name ==
                                     typeof(T)
                                         .FullName; // This will short circuit if a new service, one that we marked as not there, comes up
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn(
                                $"A problem occurred trying to handle a status changed event. The message received was {extraData}. \t\t The error was as follows: {ex}");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// The base class for all the clients
        /// </summary>
        /// 
        /// <param name="timeoutSeconds"></param>
        /// <param name="minCompatibleVersion"></param>
        public SelfResolvingClient(int timeoutSeconds, Version minCompatibleVersion = null) : this(minCompatibleVersion)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        /// <summary>
        /// The base class for all the clients
        /// </summary>
        /// <param name="timeoutSeconds"></param>
        /// <param name="minCompatibleVersion"></param>
        public SelfResolvingClient(int? timeoutSeconds, Version minCompatibleVersion) : this(minCompatibleVersion)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        /// <summary>
        /// The base class for all the clients
        /// </summary>
        /// <param name="minCompatibleVersion"></param>
        public SelfResolvingClient(Version minCompatibleVersion = null)
        {
            if (minCompatibleVersion == null)
            {
                var clientVersion = typeof(T).Assembly.GetName().Version;
                minCompatibleVersion = new Version(clientVersion.Major, clientVersion.Minor);
            }

            MinVersion = minCompatibleVersion;
            Name = typeof(T).FullName;

            _logger.Trace(i => i("Instantiating a new {0} client with minimum version of {1}", Name, MinVersion));
        }

        /// <summary>
        /// The base class for all the clients
        /// </summary>
        /// <param name="containerRequestService"></param>
        [Obsolete]
        public SelfResolvingClient(IContainerRequestService<T> containerRequestService) : this(containerRequestService, null)
        {

        }

        /// <summary>
        /// The base class for all the clients
        /// </summary>
        /// <param name="containerRequestService"></param>
        /// <param name="minCompatibleVersion"></param>
        [Obsolete]
        public SelfResolvingClient(IContainerRequestService<T> containerRequestService, Version minCompatibleVersion) : this(minCompatibleVersion)
        {

        }

        /// <summary>
        /// The base class for all the clients
        /// </summary>
        /// <param name="containerRequestService"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="minCompatibleVersion"></param>
        [Obsolete]
        public SelfResolvingClient(IContainerRequestService<T> containerRequestService, int? timeoutSeconds, Version minCompatibleVersion) : this(minCompatibleVersion)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        internal static Func<string> GetFavoredClaimsIdentitySerializationMethod_EnvironmentVariable = _GetFavoredClaimsIdentitySerializationMethod_EnvironmentVariable;

        internal static string _GetFavoredClaimsIdentitySerializationMethod_EnvironmentVariable()
        {
            //Linux only supports EnvironmentVariableTarget.Process
            var target = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? EnvironmentVariableTarget.Process : EnvironmentVariableTarget.Machine;
            return Environment.GetEnvironmentVariable(EnvironmentVariable_FavoredClaimsIdentitySerializationMethod, target);
        }

        /// <summary>
        /// Gets the preferred serialization method.
        /// </summary>
        /// <returns>The preferred <see cref="ClaimsIdentitySerializationMethod"/>.</returns>
        protected static ClaimsIdentitySerializationMethod GetFavoredClaimsIdentitySerializationMethod()
        {
            return GetFavoredClaimsIdentitySerializationMethod(false);
        }

        /// <summary>
        /// Gets the preferred serialization method.
        /// </summary>
        /// <param name="isForKubernetes">If true then FavoredClaimsIdentitySerializationMethod must be either <see cref="ClaimsIdentitySerializationMethod.Json"/> or <see cref="ClaimsIdentitySerializationMethod.CompressedJson"/></param>
        /// <returns></returns>
        protected static ClaimsIdentitySerializationMethod GetFavoredClaimsIdentitySerializationMethod(bool isForKubernetes)
        {
            var value = GetFavoredClaimsIdentitySerializationMethod_EnvironmentVariable();

            if (!Enum.TryParse<ClaimsIdentitySerializationMethod>(value, out var favoredSerializationMethod) || !Enum.IsDefined(typeof(ClaimsIdentitySerializationMethod), favoredSerializationMethod))
            {
                return ClaimsIdentitySerializationMethod.CompressedJson;
            }
            else if (!isForKubernetes || (favoredSerializationMethod == ClaimsIdentitySerializationMethod.Json || favoredSerializationMethod == ClaimsIdentitySerializationMethod.CompressedJson))
            {
                return favoredSerializationMethod;
            }
            return ClaimsIdentitySerializationMethod.CompressedJson;
        }

        /// <summary>
        /// Executes the provided function safely to ensure the local client is properly released.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="operation">The operation to call on the client.</param>
        /// <param name="request">The instance of the request object to pass to the operation.</param>
        /// <param name="callingMethod">The name of the method calling this method.</param>
        /// <returns>The response of the operation.</returns>
        protected async Task<TResponse> ProxyCall<TRequest, TResponse>(Func<T, TRequest, Task<TResponse>> operation, TRequest request, [CallerMemberName] string callingMethod = null)
            where TResponse : BaseResponse, new()
            where TRequest : BaseRequest, new()
        {
            return await ProxyCallInternal(operation, await this.Client(request), request, callingMethod);
        }

        /// <summary>
        /// Executes the provided function safely to ensure the local client is properly released.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="operation">The operation to call on the client.</param>
        /// <param name="client">The <see cref="AlkamiCachedClient{T}"/> to execute against.</param>
        /// <param name="request">The instance of the request object to pass to the operation.</param>
        /// <param name="callingMethod">The name of the method calling this method.</param>
        /// <returns>The response of the operation.</returns>
        protected async Task<TResponse> ProxyCallInternal<TRequest, TResponse>(Func<T, TRequest, Task<TResponse>> operation, AlkamiCachedClient<T> client, TRequest request, string callingMethod)
            where TResponse : BaseResponse, new()
        {
            // If we end up putting any extra logic between the start of the method and the first await, add "await Task.Yield();" to force the thread to start as soon as possible.
            TResponse result;
            var sw = Stopwatch.StartNew();

            try
            {
                result = await operation(client.Channel, request);
            }
            catch (Exception ex)
            {
                //in the case of a timeout this is a bit of an assumption, but otherwise this is currently the safer course of action
                if (ServiceUrlSettings.ShouldGetServicesFromSubscriptionMS() && client.ReferenceServiceEndpoint?.ServiceDefinition.K8sConfiguration == null)
                {
                    _serviceEndpointRepository.ShutdownAndRemoveServiceEndPoint(client.ReferenceServiceEndpoint);
                }

                client.Abort();

                if (_logger.IsErrorEnabled)
                    _logger.ErrorFormat("Unhandled exception [Client machine: {0}]", ex, Environment.MachineName);

                Metric.NoticeError(ex);

                return new TResponse()
                {
                    HasError = true,
                    SystemMessage = "An unhandled exception has occurred.",
                    ValidationResults = new List<ValidationResult>()
                    {
                        new ValidationResult()
                        {
                            ErrorCode = ErrorCode.SystemNonFatalError,
                            Severity = Severity.Error,
                            Field = string.Empty,
                            Message = ex.Message,
                        }
                    }
                };
            }
            finally
            {
                if (client != null)
                    client.Release();
            }

            sw.Stop();

            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("ProxyCall from {0} with request {1} took {2} [Client machine: {3}, Server Time: {4}].",
                    callingMethod,
                    typeof(TRequest).Name,
                    sw.Elapsed,
                    Environment.MachineName,
                    result.Elapsed);
            }

            Metric.RecordResponseTimeMetric(result.Elapsed, "Client", callingMethod + "ResponseTime");

            return result;
        }


        /// <summary>
        /// Gets the current <see cref="ServiceEndpoint{T}"/>s for the client.
        /// </summary>
        public static List<ServiceEndpoint<T>> ServiceEndpoints => _serviceEndpointRepository.GetServiceEndpoints().ToList();

        /// <summary>
        /// Determines which serialization method to use for the given <see cref="ServiceDefinition"/>.
        /// </summary>
        /// <param name="definition">The <see cref="ServiceDefinition"/> to get the serialization method to use.</param>
        /// <param name="favoredSerializationMethod">The preferred <see cref="ClaimsIdentitySerializationMethod"/> to use.</param>
        /// <returns>The <see cref="ClaimsIdentitySerializationMethod"/> to use.</returns>
        protected static ClaimsIdentitySerializationMethod DetermineClaimsIdentitySerializationMethod(ServiceDefinition definition, ClaimsIdentitySerializationMethod favoredSerializationMethod)
        {
            var supportedSerializationMethods = definition.SupportedClaimsIdentitySerializationMethods.GetValueOrDefault(ClaimsIdentitySerializationMethod.DataContractSerializer);

            if (supportedSerializationMethods.HasFlag(favoredSerializationMethod))
                return favoredSerializationMethod;

            if (supportedSerializationMethods.HasFlag(ClaimsIdentitySerializationMethod.CompressedJson))
                return ClaimsIdentitySerializationMethod.CompressedJson;
            else if (supportedSerializationMethods.HasFlag(ClaimsIdentitySerializationMethod.Json))
                return ClaimsIdentitySerializationMethod.Json;
            else
                return ClaimsIdentitySerializationMethod.DataContractSerializer;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        /// <exception cref="ServiceInstanceNotFoundException"></exception>
        internal async Task<AlkamiCachedClient<T>> Client(BaseRequest request)
        {
            if (!ServiceUrlSettings.ShouldGetServicesFromSubscriptionMS())
            {
                return await this.BuildInternalK8Client(request);
            }
            else
            {
                if (!_found)
                {
                    _logger.Warn($"Could not find a compatible service for {typeof(T).FullName} due to circuit breaker.");
                    throw new ServiceInstanceNotFoundException($"Could not find a compatible service for {typeof(T).FullName} with minimum version of [{MinVersion}]. Please try again later.");
                }

                var endpoint = SelectServiceEndpoint(request);

                if (endpoint == null)
                {
                    await ServiceResolver.Refresh();

                    endpoint = SelectServiceEndpoint(request);

                    if (endpoint == null)
                    {
                        coolDownSubject.OnNext(false);
                        _found = false;
                        _logger.Warn($"Circuit breaker flipped off in Client(...) due no compatible service for {typeof(T).FullName}.");
                        throw new ServiceInstanceNotFoundException($"Could not find a compatible service for {typeof(T).FullName} with minimum version of [{MinVersion}]. Please try again later");
                    }
                }

                if (endpoint.ServiceDefinition.K8sConfiguration != null && ServiceUrlSettings.IsRunningInKubernetes())
                {
                    //The service definitions provided  by subs is running in Kubernetes and this service is in Kubernetes.
                    //Build out the endpoint with the internal DNS address to avoid egress out of the cluster for an internal service.
                    return await this.BuildInternalK8Client(request);
                }

                var favoredSerializationMethod = GetFavoredClaimsIdentitySerializationMethod();
                var selectedSerializationMethod = DetermineClaimsIdentitySerializationMethod(endpoint.ServiceDefinition, favoredSerializationMethod);

                // Set the serialization method, which will ensure to clear the SUT if needed.
                request.ClaimsIdentitySerializationMethod = selectedSerializationMethod;

                var client = timeoutSeconds.HasValue ? endpoint.GetClient(timeoutSeconds.Value) : endpoint.GetClient();

                return client;
            }
        }

        /// <summary>
        /// Builds an internal Uri for accessing this service from within a Kubernetes cluster.
        /// </summary>)
        protected Func<BaseRequest, Task<Uri>> InternalKubernetesUri = rqt => Task.FromResult(new Uri($"{ServiceUrlSettings.GenerateServiceUrl(typeof(T).BuildRoutedNameFromContractType())}/{typeof(T).Name}"));

        private async Task<AlkamiCachedClient<T>> BuildInternalK8Client(BaseRequest request)
        {
            var url = await this.InternalKubernetesUri(request);
            var endpoint = new ServiceEndpoint<T>()
            {
                ServiceDefinition = new ServiceDefinition
                {
                    EndpointUri = url
                },
                Endpoint = url,
            };

            request.ClaimsIdentitySerializationMethod = ClaimsIdentitySerializationMethod.CompressedJson;
            var client = timeoutSeconds.HasValue ? endpoint.GetClient(timeoutSeconds.Value) : endpoint.GetClient();
            return client;
        }

        private ServiceEndpoint<T> SelectServiceEndpoint(BaseRequest request)
        {
            ServiceEndpoint<T> endpoint = null;

            if (K8sServiceEndpoints.Any())
            {
                var k8sEndpoint = _endpointResolver.SelectEndPointByMinimumSupportedVersion(request, K8sServiceEndpoints, MinVersion);
                var k8sServiceDefinition = k8sEndpoint?.ServiceDefinition;

                if (k8sServiceDefinition != null && k8sServiceDefinition.ShouldUseForRequest(request))
                {
                    endpoint = k8sEndpoint;
                }
            }

            return endpoint ?? _endpointResolver.SelectEndPointByMinimumSupportedVersion(request, ServiceEndpoints.Where(e => e.ServiceDefinition.K8sConfiguration == null), MinVersion);
        }

        public void Dispose()
        {
            // Intentionally left blank.
        }
    }
}
