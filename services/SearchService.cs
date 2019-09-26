using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using netPDQContainer.collections;
using netPDQContainer.hubs;

namespace netPDQContainer.services
{
    /// <summary>
    /// Search/lookup service. Background worker for SearchHub PDQ search requests.
    /// </summary>
    public class SearchService:BackgroundService
    {
        private readonly IBackgroundTaskQueue<Tuple<string, int, string>> _labelFindQueue;
        private readonly netMIH.Index _index;
        private readonly IHubContext<SearchHub> _hubContext;
        private readonly IOptions<SearchServiceOptions> _options;
        
        /// <summary>
        /// Default constructor. All parameters populated via dependency injection
        /// </summary>
        /// <param name="labelFindQueue">Search request queue. See <see cref="SearchHub"/> for further information.</param>
        /// <param name="index">netMIH index for lookups</param>
        /// <param name="hubContext">SearchHub context for sending responses/results.</param>
        /// <param name="options">Configurable options</param>
        public SearchService(IBackgroundTaskQueue<Tuple<string, int, string>> labelFindQueue, netMIH.Index index, IHubContext<SearchHub> hubContext, IOptions<SearchServiceOptions> options)
        {
            _labelFindQueue = labelFindQueue;
            _index = index;
            _hubContext = hubContext;
            _options = options;
        }

        /// <summary>
        /// Service thread. Managed by stack, initiated at creation.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns>async task.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cts = new CancellationTokenSource();
           
            var tasks = new List<Task>();
            while (!stoppingToken.IsCancellationRequested)
            {
                while (tasks.Count < _options.Value.Workers)
                {
                    tasks.Add(RunWorker(cts.Token));
                }
                
                tasks.RemoveAll(x => x.Status != TaskStatus.Running);
                await Task.Delay(100, stoppingToken);
            }
            cts.Cancel();
            
            
        }
        
        /// <summary>
        /// Run worker for processing requests. Conduct searches and return results to clients.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>async task</returns>
        private async Task RunWorker(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var tasking = await _labelFindQueue.DequeueAsync(ct);
                if(tasking !=null)
                {
                    try
                    {
                        await _hubContext.Clients.Client(tasking.Item3).SendAsync("Results", new SearchResult(){Hash = tasking.Item1, Results = _index.Query(tasking.Item1, tasking.Item2).ToArray()}, ct);
                    }
                    catch (Exception)
                    {
                        //connection probably lost - need to add some proper catching here
                    }
                    
                }
                else
                {
                    await Task.Delay(100, ct);
                }    
            }
            
        }
    }
}