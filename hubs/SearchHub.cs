using System;
using ImageMagick;
using Microsoft.AspNetCore.SignalR;
using netPDQContainer.collections;
    
namespace netPDQContainer.hubs
{
    public class SearchHub:Hub
    {
        private readonly IBackgroundTaskQueue<Tuple<string, int, string>> _labelFindQueue;
        private readonly IBackgroundTaskQueue<Tuple<byte[], string, string>> _hashRequestQueue;
        
        public SearchHub(IBackgroundTaskQueue<Tuple<string, int, string>> labelFindQueue,  IBackgroundTaskQueue<Tuple<byte[], string, string>> hashRequestQueue)
        {
            this._labelFindQueue = labelFindQueue;
            _hashRequestQueue = hashRequestQueue;
        }


        public void Search(string hash, int maxHD = 32)
        {
            _labelFindQueue.QueueBackgroundWorkItem(new Tuple<string, int, string>(hash, maxHD, Context.ConnectionId));
        }

        public bool Hash(byte[] data, string id)
        {
            try
            {
                using (var image = new MagickImage(data))
                {
                    var size = new MagickGeometry(512,512){IgnoreAspectRatio = true};
                    image.Resize(size);
                    _hashRequestQueue.QueueBackgroundWorkItem(new Tuple<byte[], string, string>(image.ToByteArray(),id, Context.ConnectionId));
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }
    }
}