using System.ComponentModel.DataAnnotations;

namespace netPDQContainer
{
    /// <summary>
    /// PDQ hash calculation. NOTE: no attempt is made to evaluate value of PDQ hash given quality metric. If the metric is low,
    /// you need to consider disregarding this output! (it indicates the picture is either too uniform or possibly pixellated
    /// to allow reliable similarity measures.
    /// </summary>
    public class PDQHashCalculation
    {
        /// <summary>
        /// PDQ hash value. Populates as a 64 character hex string
        /// </summary>
        /// <example>dfa38C60505ed2bacb06b60b8fe7aed0015B5dea5aef9105aca354dfda5ffe36</example>
        [Required]
        public string Hash { get; set; }
        /// <summary>
        /// Quality metric- max value 100, minimum 0. Lower quality values result in less reliable PDQs
        /// </summary>
        /// <example>100</example>
        [Required]
        public int Quality { get; set; }
    }
}