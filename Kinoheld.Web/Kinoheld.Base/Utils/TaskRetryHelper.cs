using System;
using System.Threading.Tasks;

namespace Kinoheld.Base.Utils
{
    public static class TaskRetryHelper
    {
        public static async Task<TResult> WithRetry<TResult>(Func<Task<TResult>> retryTaskFunc, int maxRetriesCount)
        {
            do
            {
                var retryTask = retryTaskFunc();
                try
                {
                    return await retryTask.ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    maxRetriesCount--;
                    if (maxRetriesCount <= 0)
                    {
                        throw;
                    }
                }
            } while (true);
        }
    }
}