using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Kinoheld.Application.Abstractions.BackgroundTasks;

namespace Kinoheld.Application.BackgroundTasks
{
    // implementation found here: https://codereview.stackexchange.com/a/188779/139203
    public class WorkItemQueue : IWorkItemQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> m_workItemQueue = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private readonly SemaphoreSlim m_signal = new SemaphoreSlim(0);

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            m_workItemQueue.Enqueue(workItem);
            m_signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await m_signal.WaitAsync(cancellationToken);
            m_workItemQueue.TryDequeue(out var workItem);

            return workItem;
        }
    }
}