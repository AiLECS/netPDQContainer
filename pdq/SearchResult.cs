using netMIH;

namespace netPDQContainer
{
    /// <summary>
    /// Search result for queued/batch search (used by websocket (SignalR core) implementation).
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Candidate hash for search
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// Result array <see cref="netMIH.Result"/> for search. 
        /// </summary>
        public Result[] Results { get; set; }
    }
}