using System;
using System.Collections.Generic;
using System.IO;
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
    /// Background service for processing PDQ hash requests.
    /// </summary>
    public class HashService:BackgroundService
    {
        private readonly IBackgroundTaskQueue<Tuple<byte[], string, string>> _hashRequestQueue;
        private readonly IHubContext<SearchHub> _hubContext;
        private readonly PDQWrapper _wrapper;
        private readonly IOptions<HashServiceOptions> _options;
        
        /// <summary>
        /// Default constructor. All parameters passed via dependency injection.
        /// </summary>
        /// <param name="hashRequestQueue">Queue for incoming requests. <see cref="SearchHub"/> for further details. </param>
        /// <param name="index">netMIH index for lookups (populated at startup)</param>
        /// <param name="hubContext">Context for SearchHub (<see cref="SearchHub"/></param>
        /// <param name="wrapper">PDQ wrapper</param>
        /// <param name="options">Configurable options</param>
        public HashService(IBackgroundTaskQueue<Tuple<byte[], string, string>> hashRequestQueue, netMIH.Index index, IHubContext<SearchHub> hubContext,  PDQWrapper wrapper, IOptions<HashServiceOptions> options)
        {
            _hashRequestQueue = hashRequestQueue;
            _hubContext = hubContext;
            _wrapper = wrapper;
            _options = options;
        }

        /// <summary>
        /// Called at startup
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        /// <returns>async task (managed by stack)</returns>
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
        /// Run worker.
        /// </summary>
        /// <param name="ct">CancellationToken. </param>
        /// <returns>Task</returns>
        private async Task RunWorker(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var tasking = await _hashRequestQueue.DequeueAsync(ct);

                if(tasking !=null)
                {
                    var tempFile = Path.GetTempFileName();
                    try
                    {
                        File.WriteAllBytes(tempFile, tasking.Item1);
                        await _hubContext.Clients.Client(tasking.Item3).SendAsync("HashResult", new HashResult(){Id = tasking.Item2, Result = _wrapper.GetHash(tempFile)}, ct);
                       // Console.WriteLine($"Response sent to client {tasking.Item3}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        //connection probably lost - need to add some proper catching here
                    }
                    finally
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
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