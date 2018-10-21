using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Kinoheld.Application.Abstractions.BackgroundTasks;
using Microsoft.Extensions.Hosting;

namespace Kinoheld.Web.BackgroundTasks
{
    // implementation found here: https://codereview.stackexchange.com/a/188779/139203
    public class WorkItemQueueService : IHostedService
    {
        private readonly IWorkItemQueue m_workItemQueue;
        private readonly CancellationTokenSource m_shutdown = new CancellationTokenSource();
        private Task m_backgroundTask;

        public WorkItemQueueService(IWorkItemQueue workItemQueue)
        {
            m_workItemQueue = workItemQueue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // ReSharper disable once MethodSupportsCancellation - this task is not supposted to be cancelled until shutdown
            m_backgroundTask = Task.Run(BackgroundProceessing);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            m_shutdown.Cancel();
            return Task.WhenAny(m_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            m_workItemQueue.Enqueue(workItem);
        }

        private async Task BackgroundProceessing()
        {
            while (!m_shutdown.IsCancellationRequested)
            {
                var workItem = await m_workItemQueue.DequeueAsync(m_shutdown.Token);

                try
                {
                    await workItem(m_shutdown.Token);
                }
                catch (Exception)
                {
                    Debug.Fail("Work item should handle its own exceptions.");
                }
            }
        }
    }
}