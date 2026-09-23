using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// *********************************************************
// For some reason when running Async tasks as Synchronous in ORB you need to use this RunSync Method, if you do not then ORB Admin will just get stuck
// This was copied from ORB from the Alkami.BankService:
// https://bitbucket.corp.alkami.net/projects/APPDEV/repos/orb/browse/Alkami.BankService/Alkami.App.Bank/Helpers/AsyncTaskHelper.c
// *********************************************************
namespace Alkami.Utilities
{
    internal static class AsyncTaskHelper
    {
        private static readonly TaskFactory MyTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return MyTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            MyTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

        public static TResult[] RunSync<TResult>(Func<Task<TResult>[]> func)
        {
            var results = new List<TResult>();
            RunSync(() =>
            {
                return MyTaskFactory.ContinueWhenAll(func(), tasks =>
                {
                    foreach (var task in tasks)
                    {
                        results.Add(task.Result);
                    }
                });
            });
            return results.ToArray();
        }
    }


}
