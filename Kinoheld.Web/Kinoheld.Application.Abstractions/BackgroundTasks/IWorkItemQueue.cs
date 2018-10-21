using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kinoheld.Application.Abstractions.BackgroundTasks
{
    // implementation found here: https://codereview.stackexchange.com/a/188779/139203
    public interface IWorkItemQueue
    {
        void Enqueue(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}