namespace netPDQContainer.services
{
    /// <summary>
    /// Configuration options for HashService
    /// </summary>
    public class HashServiceOptions
    {
        /// <summary>
        /// Number worker threads to support. Defaults to 3.
        /// </summary>
        /// <example>3</example>
        public int Workers { get; set; } = 3;
    }
}