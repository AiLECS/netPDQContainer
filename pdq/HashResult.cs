using System.ComponentModel.DataAnnotations;

namespace netPDQContainer
{
    /// <summary>
    /// Lookup result against hash service. Returns PDQHashCalculation <see cref="PDQHashCalculation"/> plus Id for
    /// disambiguating results (used particularly in streaming/socket scenarios.
    /// </summary>
    public class HashResult
    {
        /// <summary>
        /// Unique identifier for request. Typically assigned by client at request time.
        /// </summary>
        /// <example>Req001</example>
        [Required]
        public string Id { get; set; }
        /// <summary>
        /// PDQHashCalculation (see <see cref="PDQHashCalculation"/> for request.
        /// </summary>
        public PDQHashCalculation Result { get; set; }
    }
}