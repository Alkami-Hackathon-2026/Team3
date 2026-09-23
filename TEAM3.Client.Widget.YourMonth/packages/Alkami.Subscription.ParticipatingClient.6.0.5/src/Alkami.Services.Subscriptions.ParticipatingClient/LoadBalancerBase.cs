using System.Collections.Generic;
using Alkami.Contracts;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LoadBalancerBase<T> where T : class
    {
        protected readonly List<T> _storageBag;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageBag"></param>
        protected LoadBalancerBase(List<T> storageBag)
        {
            _storageBag = storageBag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract T GetNext(BaseRequest request);
    }
}