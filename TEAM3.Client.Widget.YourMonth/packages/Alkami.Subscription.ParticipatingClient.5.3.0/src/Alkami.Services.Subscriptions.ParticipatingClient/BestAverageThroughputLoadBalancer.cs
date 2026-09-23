using Alkami.Contracts;
using System.Collections.Generic;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BestAverageThroughputLoadBalancer<T> : LoadBalancerBase<ServiceEndpoint<T>> where T : class
    {
        //pass through to a different load balancer
        private LoadBalancerBase<ServiceEndpoint<T>> _replacementLoadBalancer;

        /// <inheritdoc />
        public BestAverageThroughputLoadBalancer(List<ServiceEndpoint<T>> storageBag) : base(storageBag)
        {
            _replacementLoadBalancer = DetermineLoadBalancerFromSetting(storageBag);
        }

        private LoadBalancerBase<ServiceEndpoint<T>> DetermineLoadBalancerFromSetting(List<ServiceEndpoint<T>> list)
        {
            return new RoundRobinLoadBalancer<T>(list);
        }

        /// <inheritdoc />
        public override ServiceEndpoint<T> GetNext(BaseRequest request)
        {
            var next = _replacementLoadBalancer.GetNext(request);
            return next;
        }
    }
}
