using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using netMIH;
using netPDQContainer.collections;
using netPDQContainer.hubs;

namespace netPDQContainer.services
{
    public class SearchService:BackgroundService
    {
        private readonly IBackgroundTaskQueue<Tuple<string, int, string>> _labelFindQueue;
        private readonly netMIH.Index _index;
        private readonly IHubContext<SearchHub> _hubContext;
        public SearchService(IBackgroundTaskQueue<Tuple<string, int, string>> labelFindQueue, netMIH.Index index, IHubContext<SearchHub> hubContext)
        {
            _labelFindQueue = labelFindQueue;
            _index = index;
            _hubContext = hubContext;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cts = new CancellationTokenSource();
            
            //arbitrary number. Should put in config
            var workerCount = 3;
            var tasks = new List<Task>();
            while (!stoppingToken.IsCancellationRequested)
            {
                while (tasks.Count < workerCount)
                {
                    tasks.Add(RunWorker(cts.Token));
                }
                
                tasks.RemoveAll(x => x.Status != TaskStatus.Running);
                await Task.Delay(100);
            }
            cts.Cancel();
            
            
        }

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
                    catch (Exception e)
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