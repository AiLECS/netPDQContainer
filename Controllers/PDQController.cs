using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using netMIH;


namespace netPDQContainer.Controllers
{
    /// <summary>
    /// Base PDQ controller 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class PDQController : ControllerBase
    {
        private readonly netMIH.Index _index;
        private readonly PDQWrapper _wrapper;

        public PDQController(netMIH.Index index, PDQWrapper wrapper)
        {
            this._index = index;
            this._wrapper = wrapper;
        }
        /// <summary>
        /// Get a list of available categories
        /// </summary>
        /// <returns><Array of category names within system/> /returns>
        [HttpGet]
        [Route("categories")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return _index.ListCategories().ToArray();
        }
        
        /// <summary>
        /// Query PDQ hash
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="maxhd">Maximum hamming distance for match. Defaults to 32.</param>
        /// <returns>Array of <see cref="netMIH.Result"/> objects within query distance</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Result>> Get([FromQuery, Required, RegularExpression("^[a-f0-9]{64}$")] string hash, [FromQuery] int maxhd = 32)
        {
            return new OkObjectResult(_index.Query(hash, maxhd));
        }

        /// <summary>
        /// Hash a file
        /// </summary>
        /// <param name="file">Image file for hashing</param>
        /// <returns><see cref= "netMIH.PDQHashCalculation"/> for provided file</returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public async Task<ActionResult<PDQHashCalculation>> GetHash(IFormFile file)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFile, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                return new ActionResult<PDQHashCalculation>(_wrapper.GetHash(tempFile));
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
            
            
        }

    }
}