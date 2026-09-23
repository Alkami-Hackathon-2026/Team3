#if !NET6_0_OR_GREATER
using System;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Alkami.Utilities.Certificates.Http
{
    internal class ServicePointHttpMessageHandler : DelegatingHandler
    {

        public ServicePointHttpMessageHandler(TimeSpan connectionTimeout, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.ConnectionLeaseTimeout = connectionTimeout;
        }

        public TimeSpan ConnectionLeaseTimeout { get; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var servicePoint = ServicePointManager.FindServicePoint(request.RequestUri);
            servicePoint.ConnectionLeaseTimeout = (int)this.ConnectionLeaseTimeout.TotalMilliseconds;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
#endif