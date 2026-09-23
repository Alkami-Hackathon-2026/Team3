using Alkami.Services.Subscriptions.Data;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Common.Logging;


namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// Internal generic WCF client base class that remains open so as to reduce the overhead of the TLS handshake
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlkamiCachedClient<T> : ClientBase<T> where T : class
    {
        private static ILog _logger = LogManager.GetLogger("Alkami.Services.Subscriptions.ParticipatingClient.AlkamiCachedClient");

        [Obsolete]
        internal ServiceEndpoint<T> ReferenceServiceEndpoint { get; set; }

        internal bool UsingAlkamiClientFactory { get; set; }

#if NET6_0_OR_GREATER
        /// <summary>
        /// The <see cref="System.Runtime.Serialization.ISerializationSurrogateProvider"/> that is used on operations for customizing serialization of objects
        /// </summary>
        public static Utilities.Rpc.IAlkamiSerializationSurrogateProvider SerializationSurrogateProvider { get; }

        static AlkamiCachedClient()
        {
            SerializationSurrogateProvider = Utilities.Rpc.SerializationSurrogateProvider.GetDefault();
        }
#endif

        /// <summary>
        /// Construct client with specified bindings and service endpoint address from an http binding
        /// </summary>
        /// <param name="binding">Client bindings.</param>
        /// <param name="remoteAddress">Service endpoint address.</param>
        public AlkamiCachedClient(AlkamiHttpBinding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
#if NET6_0_OR_GREATER
            foreach (var operation in Endpoint.Contract.Operations)
            {
                if (operation.Behaviors.Find<DataContractSerializerOperationBehavior>() is DataContractSerializerOperationBehavior dataContractBehavior)
                {
                    dataContractBehavior.SerializationSurrogateProvider = SerializationSurrogateProvider;
                }
                else
                {
                    dataContractBehavior = new DataContractSerializerOperationBehavior(operation);
                    dataContractBehavior.SerializationSurrogateProvider = SerializationSurrogateProvider;
                    operation.OperationBehaviors.Add(dataContractBehavior);
                }

                operation.OperationBehaviors.Add(new SecurityBehavior());
            }
#else
            foreach (var operation in Endpoint.Contract.Operations)
            {
                operation.OperationBehaviors.Add(new SecurityBehavior());
            }
#endif

#if NETFRAMEWORK || NET8_0_OR_GREATER
            AddTelemetryEndpointBehaviorToEndpointBehaviors(Endpoint);
            AddMetricEndpointBehaviorToEndpointBehaviors(Endpoint);
#endif
        }

        /// <summary>
        /// Construct client with specified bindings and service endpoint address from a net tcp binding
        /// </summary>
        /// <param name="binding">Client bindings.</param>
        /// <param name="remoteAddress">Service endpoint address.</param>
        /// <param name="clientCredentials">Client Credentials</param>
        public AlkamiCachedClient(NetTcpBinding binding, EndpointAddress remoteAddress, ClientCredentials clientCredentials) : base(binding, remoteAddress)
        {
            if (ClientCredentials != null && clientCredentials != null)
            {
                ClientCredentials.ServiceCertificate.DefaultCertificate = clientCredentials.ServiceCertificate.DefaultCertificate;
                ClientCredentials.ClientCertificate.Certificate = clientCredentials.ClientCertificate.Certificate;
                ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = clientCredentials.ServiceCertificate.Authentication.CertificateValidationMode;
            }

#if NET6_0_OR_GREATER
            foreach (var operation in Endpoint.Contract.Operations)
            {
                if (operation.Behaviors.Find<DataContractSerializerOperationBehavior>() is DataContractSerializerOperationBehavior dataContractBehavior)
                {
                    dataContractBehavior.SerializationSurrogateProvider = SerializationSurrogateProvider;
                }
                else
                {
                    dataContractBehavior = new DataContractSerializerOperationBehavior(operation);
                    dataContractBehavior.SerializationSurrogateProvider = SerializationSurrogateProvider;
                    operation.OperationBehaviors.Add(dataContractBehavior);
                }

                operation.OperationBehaviors.Add(new SecurityBehavior());
            }
#else
            foreach (var operation in Endpoint.Contract.Operations)
            {
                operation.OperationBehaviors.Add(new SecurityBehavior());
            }
#endif

#if NETFRAMEWORK || NET8_0_OR_GREATER
            AddTelemetryEndpointBehaviorToEndpointBehaviors(Endpoint);
            AddMetricEndpointBehaviorToEndpointBehaviors(Endpoint);
#endif
        }

        /// <summary>
        /// Checks state and will return either the current instance or try to return a new instance if using older system.
        /// Will create new instance if<see cref="ClientBase{TChannel}.State"/> is <see cref="CommunicationState.Faulted"/>, <see cref="CommunicationState.Closed"/>, or <see cref="CommunicationState.Closing"/>
        /// </summary>
        /// <returns></returns>
        [Obsolete("Do not use. This will only return a new instance if used with older ServiceEndpoint obsoleted functionality")]
        public AlkamiCachedClient<T> Refresh()
        {
            if (UsingAlkamiClientFactory)
            {
                return this;
            }
            if (!this.IsInBadState())
            {
                return this;
            }
#pragma warning disable CS0612 // Type or member is obsolete
            if (ReferenceServiceEndpoint != null)
            {
                var client = ReferenceServiceEndpoint.CreateNewClient();
                return client;
            }
#pragma warning restore CS0612 // Type or member is obsolete
            return this;
        }

        /// <summary>
        /// Returns the Channel of <typeparamref name="T"/>
        /// </summary>
        /// <remarks>
        /// By default the Channel property is protected in the base class, so must override it to expose it.
        /// </remarks>
        public new T Channel { get { return (InnerChannel) as T; } }

        /// <summary>
        /// Calls the ServiceEndpoint to requeue the service if the <see cref="ClientBase{TChannel}.State"/> is <see cref="CommunicationState.Opened"/>
        /// </summary>
        [Obsolete("Do not use. This will only return a new instance if used with older ServiceEndpoint obsoleted functionality")]
        public void Release()
        {
            if (UsingAlkamiClientFactory)
            {
                //do nothing
            }
            else if (State == CommunicationState.Opened)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (ReferenceServiceEndpoint != null)
                {
                    ReferenceServiceEndpoint.ReplenishClient(this);
                }
#pragma warning restore CS0612 // Type or member is obsolete
                else
                {
                    CloseOrAbort();
                }
            }
        }

        /// <summary>
        /// Will try to close, and if it fails will instead abort. Based on https://www.codeproject.com/Articles/622989/WCF-and-the-Try-Catch-Abort-Pattern
        /// </summary>
        public void CloseOrAbort()
        {
            try
            {
                Close();
            }
            catch
            {
                Abort();
            }
        }

#if NETFRAMEWORK || NET8_0_OR_GREATER
        private void AddTelemetryEndpointBehaviorToEndpointBehaviors(ServiceEndpoint serviceEndpoint)
        {
            try
            {
                var type = Type.GetType("OpenTelemetry.Instrumentation.Wcf.TelemetryEndpointBehavior, OpenTelemetry.Instrumentation.Wcf", false);
                if (type != null)
                {
                    var endpointBehavior = Activator.CreateInstance(type) as IEndpointBehavior;
                    if (endpointBehavior != null)
                    {
                        serviceEndpoint.EndpointBehaviors.Add(endpointBehavior);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsTraceEnabled) {
                    _logger.Trace($"An error occurred when trying to add TelemetryEndpointBehavior: {ex}");
                }
            }
        }

        private void AddMetricEndpointBehaviorToEndpointBehaviors(ServiceEndpoint serviceEndpoint)
        {
            try
            {
                var type = Type.GetType("Alkami.OpenTelemetry.Instrumentation.Wcf.MetricsEndpointBehavior, Alkami.OpenTelemetry.Instrumentation.Wcf", false);
                if (type != null)
                {
                    var endpointBehavior = Activator.CreateInstance(type) as IEndpointBehavior;
                    if (endpointBehavior != null)
                    {
                        serviceEndpoint.EndpointBehaviors.Add(endpointBehavior);
                    }

                }
            }
            catch (Exception ex)
            {
                if (_logger.IsTraceEnabled)
                {
                    _logger.Trace($"An error occurred when trying to add MetricsEndpointBehavior: {ex}");
                }
            }
        }
#endif

    }
}
