using System;
using ImageMagick;
using Microsoft.AspNetCore.SignalR;
using netPDQContainer.collections;
    
namespace netPDQContainer.hubs
{
    /// <summary>
    /// SignalR Core (websockets) hub for PDQ searches and hash requests
    /// </summary>
    public class SearchHub:Hub
    {
        /// <summary>
        /// Search request queue. Structured as tuple with hash, max hamming distance, connectionID
        /// </summary>
        private readonly IBackgroundTaskQueue<Tuple<string, int, string>> _labelFindQueue;
        /// <summary>
        /// Hash request queue. Structured as tuple with bytearray of binary value, reference (client set), connectionID.
        /// </summary>
        private readonly IBackgroundTaskQueue<Tuple<byte[], string, string>> _hashRequestQueue;
        
        /// <summary>
        /// Constructor. All parameters provided via dependency injection
        /// </summary>
        /// <param name="labelFindQueue">Queue for search requests.</param>
        /// <param name="hashRequestQueue">Queue for hash requests.</param>
        public SearchHub(IBackgroundTaskQueue<Tuple<string, int, string>> labelFindQueue,  IBackgroundTaskQueue<Tuple<byte[], string, string>> hashRequestQueue)
        {
            this._labelFindQueue = labelFindQueue;
            _hashRequestQueue = hashRequestQueue;
        }

        /// <summary>
        /// Method for requesting hash lookups
        /// </summary>
        /// <param name="hash">Candidate hash, formatted as string</param>
        /// <param name="maxHD">Maximum hamming distance. Defaults to 32.</param>
        /// <remarks>If successful, result is returned using event/method "Results"</remarks>
        public void Search(string hash, int maxHD = 32)
        {
            _labelFindQueue.QueueBackgroundWorkItem(new Tuple<string, int, string>(hash, maxHD, Context.ConnectionId));
        }

        /// <summary>
        /// Request PDQ hash of candidate image (sent as byte array)
        /// </summary>
        /// <param name="data">Binary value of image (will be resized to 512px long edge upon receipt).</param>
        /// <param name="id">Client reference/ID for request.</param>
        /// <returns>True if image readable.</returns>
        /// <remarks>If successful, result is returned using event/method "HashResult"</remarks>
        public bool Hash(byte[] data, string id)
        {
            try
            {
                using (var image = new MagickImage(data))
                {
                    var size = new MagickGeometry(512,512){IgnoreAspectRatio = false};
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