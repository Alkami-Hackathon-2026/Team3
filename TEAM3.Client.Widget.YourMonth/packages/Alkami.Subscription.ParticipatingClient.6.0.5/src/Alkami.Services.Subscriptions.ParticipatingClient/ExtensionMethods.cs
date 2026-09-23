using System;
using System.Linq;
using System.ServiceModel;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    internal static class ExtensionMethods
    {
        private static readonly CommunicationState[] _badStates = { CommunicationState.Faulted, CommunicationState.Closed, CommunicationState.Closing };

        public static bool IsInBadState(this ICommunicationObject channelFactory)
        {
            return _badStates.Contains(channelFactory.State);
        }

        public static Uri NormalizeEndpoint(this Uri endpoint)
        {
            if (endpoint == null)
            {
                return endpoint;
            }

            return new Uri(endpoint.GetLeftPart(UriPartial.Path).ToLowerInvariant());
        }
    }

}
