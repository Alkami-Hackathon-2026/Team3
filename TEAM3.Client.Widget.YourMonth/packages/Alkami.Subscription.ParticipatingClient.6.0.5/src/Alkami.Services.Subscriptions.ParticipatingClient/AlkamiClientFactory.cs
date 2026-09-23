using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Alkami.Services.Subscriptions.Data;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    internal class AlkamiClientFactory<TChannel> : IAlkamiClientFactory<TChannel> where TChannel : class
    {
        private readonly Uri _clientEndpoint;
        private readonly int _timeoutSeconds;

        private readonly ConcurrentQueue<AlkamiCachedClient<TChannel>> _clients;

        private const int MAX_CLIENTS = 50;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientEndpoint"></param>
        /// <param name="timeoutSeconds"></param>
        public AlkamiClientFactory(Uri clientEndpoint, int timeoutSeconds)
        {
            _clientEndpoint = clientEndpoint;
            _timeoutSeconds = timeoutSeconds;
            _clients = new ConcurrentQueue<AlkamiCachedClient<TChannel>>();
        }

        public AlkamiCachedClient<TChannel> GetClient()
        {
            return _clients.TryDequeue(out var client) ? CheckAndReturnClient(client) : CreateNewClient();
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        /// <returns></returns>
        internal AlkamiCachedClient<TChannel> CreateNewClient()
        {
            var clientEndpoint = _clientEndpoint;
            if (typeof(TChannel).FullName == "Alkami.Subscriptions.ProviderBased.Contracts.IPluginContract")
            {
                clientEndpoint = new Uri(_clientEndpoint.AbsoluteUri + "/" + "Alkami.Subscriptions.ProviderBased.Contracts.IPluginContract");
            }

            if (clientEndpoint.Scheme == Uri.UriSchemeHttp || clientEndpoint.Scheme == Uri.UriSchemeHttps)
            {
                var httpBinding = SubscriptionBindings.HttpBinding(_timeoutSeconds, https: clientEndpoint.Scheme == Uri.UriSchemeHttps);

                return new AlkamiCachedClient<TChannel>(httpBinding, new EndpointAddress(clientEndpoint)) { UsingAlkamiClientFactory = true };
            }

            var netTcpBinding = SubscriptionBindings.NetTcpBinding(_timeoutSeconds);

            var behavior = new ClientCredentials();
            behavior.ClientCertificate.Certificate = SubscriptionBindings.GetClientCertificate();
            behavior.ServiceCertificate.DefaultCertificate = SubscriptionBindings.GetServiceCertificate();
            behavior.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

            return new AlkamiCachedClient<TChannel>(netTcpBinding, new EndpointAddress(clientEndpoint, SubscriptionBindings.Identity), behavior) { UsingAlkamiClientFactory = true };
        }

        public void Release(AlkamiCachedClient<TChannel> client)
        {
            if (client.State != CommunicationState.Opened)
            {
                client.CloseOrAbort();
                return;
            }

            if (_clients.Count < MAX_CLIENTS)
            {
                _clients.Enqueue(client);
            }
            else
            {
                client.CloseOrAbort();
            }
        }

        public void Dispose()
        {
            while (_clients.TryDequeue(out var client))
            {
                client.CloseOrAbort();
            }
        }

        private AlkamiCachedClient<TChannel> CheckAndReturnClient(AlkamiCachedClient<TChannel> client)
        {
            if (client.IsInBadState())
            {
                client.CloseOrAbort();
                return CreateNewClient();
            }
            return client;
        }

    }
}
