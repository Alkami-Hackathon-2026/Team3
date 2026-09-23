using Alkami.Contracts;
using Common.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    public class RoundRobinLoadBalancer<T> : LoadBalancerBase<ServiceEndpoint<T>> where T : class
    {
        private static int indexTracker = 0;
        private const int maxIndexTrackerAllowed = 10000;
        private static readonly ILog _logger = LogManager.GetLogger("RoundRobinLoadBalancer");

        internal const string Name = "roundrobin";

        public override ServiceEndpoint<T> GetNext(BaseRequest request)
        {
            var matches = _storageBag.ToList();

            _logger.Trace(m => m($"Found{matches.Count} definitions that match the criteria.[BankIdentifier: { request.BankIdentifier.GetValueOrDefault().ToString()}].  Definitions that match are [{string.Join(",", matches.Select(x => x.ToString()))}]"));

            ServiceEndpoint<T> chosenServiceEndpoint = null;
       
            switch (matches.Count)
            {
                case int n when n == 1:
                    chosenServiceEndpoint = matches.Single();
                    break;
                case int n when n > 1:
                    var match = matches[GetIndex(matches.Count)];
                    chosenServiceEndpoint = match;
                    break;
                default: chosenServiceEndpoint = null;//should already be null but this displays my intent
                    break;
            }

            _logger.Trace(m => m($"Service URL chosen by load balancer:  {chosenServiceEndpoint?.Endpoint?.AbsoluteUri ?? ""}"));

            return chosenServiceEndpoint;
        }

        private static int GetIndex(int itemCount)
        {
            var index = Interlocked.Increment(ref indexTracker);
            if (indexTracker > maxIndexTrackerAllowed)
            {
                indexTracker = 0;
            }

            return index % itemCount;
        }

        public RoundRobinLoadBalancer(List<ServiceEndpoint<T>> storageBag) : base(storageBag)
        {
            _logger.Debug(x => x($"{nameof(RoundRobinLoadBalancer<T>) } initialized"));
        }
    }
}
