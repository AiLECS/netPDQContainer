using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using netMIH;
using netPDQContainer.collections;
using netPDQContainer.hubs;

namespace netPDQContainer.services
{
    public class HashService:BackgroundService
    {
        private readonly IBackgroundTaskQueue<Tuple<byte[], string, string>> _hashRequestQueue;
        private readonly IHubContext<SearchHub> _hubContext;
        private readonly PDQWrapper _wrapper;
        public HashService(IBackgroundTaskQueue<Tuple<byte[], string, string>> hashRequestQueue, netMIH.Index index, IHubContext<SearchHub> hubContext,  PDQWrapper wrapper)
        {
            _hashRequestQueue = hashRequestQueue;
            _hubContext = hubContext;
            _wrapper = wrapper;
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