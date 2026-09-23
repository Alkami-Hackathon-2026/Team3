using Alkami.Services.Subscriptions.Data;
using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text.RegularExpressions;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceEndpoint<T> where T : class
    {
        /// <summary>
        /// The clients
        /// </summary>
        [Obsolete]
        public static ConcurrentDictionary<string, ConcurrentQueue<AlkamiCachedClient<T>>> Clients = new ConcurrentDictionary<string, ConcurrentQueue<AlkamiCachedClient<T>>>();

        private Uri _endpoint;

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public Uri Endpoint
        {
            get { return _endpoint; }
            set
            {
                _endpoint = value;

#pragma warning disable CS0612 // Type or member is obsolete
                if (!Clients.ContainsKey(GetKey))
                    Clients[GetKey] = new ConcurrentQueue<AlkamiCachedClient<T>>();
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        /// <summary>
        /// Gets or sets the resolves for.
        /// </summary>
        /// <value>
        /// The resolves for.
        /// </value>
        public string ResolvesFor { get; set; }

        /// <summary>
        /// Gets or sets the service definition.
        /// </summary>
        /// <value>
        /// The service definition.
        /// </value>
        public ServiceDefinition ServiceDefinition { get; set; }

        /// <summary>
        /// Gets or sets the provider configuration.
        /// </summary>
        /// <value>
        /// The provider configuration.
        /// </value>
        public ProviderConfiguration ProviderConfiguration { get; set; }

        [Obsolete]
        private string GetKey
        {
            get
            {
                return this.Endpoint.ToString();
            }
        }

        /// <summary>
        /// Gets the client, with the default time from <see cref="SubscriptionBindings.DefaultTimeout"/>
        /// </summary>
        /// <returns>The client</returns>
        [Obsolete]
        public AlkamiCachedClient<T> GetClient()
        {
            return GetClient(SubscriptionBindings.DefaultTimeout);
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="timeoutSeconds">The timeout for this client's binding in seconds</param>
        /// <returns>The client</returns>
        [Obsolete]
        public AlkamiCachedClient<T> GetClient(int timeoutSeconds)
        {
            return Clients[GetKey].TryDequeue(out var client) ? client.Refresh() : CreateNewClient(timeoutSeconds);
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        [Obsolete]
        public AlkamiCachedClient<T> CreateNewClient(int timeoutSeconds = SubscriptionBindings.DefaultTimeout)
        {
            var clientEndpoint = Endpoint;
            if (typeof(T).FullName == "Alkami.Subscriptions.ProviderBased.Contracts.IPluginContract")
            {
                clientEndpoint = new Uri(Endpoint.AbsoluteUri + "/" + "Alkami.Subscriptions.ProviderBased.Contracts.IPluginContract");
            }

            if (clientEndpoint.Scheme == Uri.UriSchemeHttp || clientEndpoint.Scheme == Uri.UriSchemeHttps)
            {
                var httpBinding = SubscriptionBindings.HttpBinding(timeoutSeconds, https: clientEndpoint.Scheme == Uri.UriSchemeHttps);

                return new AlkamiCachedClient<T>(httpBinding, new EndpointAddress(clientEndpoint)) { ReferenceServiceEndpoint = this };
            }

            var netTcpBinding = SubscriptionBindings.NetTcpBinding(timeoutSeconds);

            var behavior = new ClientCredentials();
            behavior.ClientCertificate.Certificate = SubscriptionBindings.GetClientCertificate();
            behavior.ServiceCertificate.DefaultCertificate = SubscriptionBindings.GetServiceCertificate();
            behavior.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

            return new AlkamiCachedClient<T>(netTcpBinding, new EndpointAddress(clientEndpoint, SubscriptionBindings.Identity), behavior) { ReferenceServiceEndpoint = this };
        }

        /// <summary>
        /// Shuts down.
        /// </summary>
        [Obsolete]
        public void ShutDown()
        {
            if (Clients.TryGetValue(GetKey, out var clients))
            {
                while (clients.TryDequeue(out var client))
                {
                    client.CloseOrAbort();
                }
            }
        }

        /// <summary>
        /// Replenishes the client.
        /// </summary>
        /// <param name="client">The client.</param>
        [Obsolete]
        public void ReplenishClient(AlkamiCachedClient<T> client)
        {
            if (Clients[GetKey].Count < 50)
            {
                Clients[GetKey].Enqueue(client.Refresh());
            }
            else
            {
                client.CloseOrAbort();
            }
        }

    }
}
