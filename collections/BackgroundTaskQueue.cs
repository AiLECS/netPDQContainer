using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace netPDQContainer.collections
{
    /// <summary>
    /// Interface for BackgroundTaskQueue. Implemented to enable dependency injection. 
    /// 
    /// </summary>
    /// <remarks>May not be required - if concrete class adequate, no need for interface.</remarks>
    public interface IBackgroundTaskQueue<T>
    {
        /// <summary>
        /// Enqueue work item for processing. 
        /// </summary>
        /// <param name="workItem"></param>
        void QueueBackgroundWorkItem(T workItem);

        /// <summary>
        /// Dequeue and return item asyncronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dequeued item (unless cancelled)</returns>
        Task<T> DequeueAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Check if items available for work (i.e. queued)
        /// </summary>
        /// <returns>true if items enqueued, else false.</returns>
        bool HasWork();
    }


    /// <summary>
    /// Queue for concurrent use, suitable for use on servers. taken from <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.1"/> 
    /// </summary>
    public class BackgroundQueue<T> : IBackgroundTaskQueue<T>
    {
        private readonly ConcurrentQueue<T> _workItems = new ConcurrentQueue<T>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        /// <inheritdoc />
        public void QueueBackgroundWorkItem(T workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        /// <inheritdoc />
        public bool HasWork()
        {
            return !_workItems.IsEmpty;
        }

        /// <inheritdoc />
        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {            
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem;
        }
    }

}