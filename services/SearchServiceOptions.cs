namespace netPDQContainer.services
{
    /// <summary>
    /// Options for SearchService. <see cref="SearchService"/> 
    /// </summary>
    public class SearchServiceOptions
    {
        /// <summary>
        /// Number workers to run lookups/searches. Defaults to 3
        /// </summary>
        /// <example>3</example>
        public int Workers { get; set; } = 3;
    }
}