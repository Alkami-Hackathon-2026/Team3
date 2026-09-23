using System;
using System.Threading;

namespace Alkami.Utilities.Kubernetes
{
    /// <summary>
    /// Exposes methods for fine tuning threading settings for applications
    /// </summary>
    public static class ThreadingConfiguration
    {
        private const string EnvironmentVariable_MinWorkerThreads = "ALKAMI_K8S_MIN_WORKER_THREADS";

        private const string EnvironmentVariable_MinPortThreads = "ALKAMI_K8S_MIN_PORT_THREADS";

        /// <summary>
        /// 
        /// </summary>
        public static void UpdateMinThreadsIfOverrideExists()
        {
            ThreadPool.GetMinThreads(out var currentMinWorkerThreads, out var currentMinPortThreads);

            int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable_MinWorkerThreads), out var envMinWorkerThreads);
            int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable_MinPortThreads), out var envMinPortThreads);

            var desiredMinWorkerThreads = Math.Max(currentMinWorkerThreads, envMinWorkerThreads);
            var desiredMinPortThreads = Math.Max(currentMinPortThreads, envMinPortThreads);

            ThreadPool.SetMinThreads(desiredMinWorkerThreads, desiredMinPortThreads);
        }
    }
}
