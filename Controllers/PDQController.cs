using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using netMIH;
using Swashbuckle.AspNetCore.Annotations;


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
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="index">netMIH index (provided via dependency injection</param>
        /// <param name="wrapper">PDQWrapper (provided via dependency injection</param>
        public PDQController(netMIH.Index index, PDQWrapper wrapper)
        {
            this._index = index;
            this._wrapper = wrapper;
        }
        
        //ignore lack of XML comments here - swagger annotations will populate all relevant documentation.
        #pragma warning disable 1591
        [HttpGet]
        [SwaggerOperation(
            Summary = "List categories",
            Description = "Return a list of categories within index")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request OK", typeof(string[]))]
        [Route("categories")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return _index.ListCategories().ToArray();
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Query PDQ hash",
            Description = "Query index for PDQ hashes within match window")]
        [SwaggerResponse(StatusCodes.Status200OK, "PDQ generated", typeof(IEnumerable<Result>))]
        public ActionResult<IEnumerable<Result>> Get([FromQuery, Required, 
                                                      RegularExpression("^[A-Fa-f0-9]{64}$" ), 
                                                      SwaggerParameter("Candidate PDQ hash - anticipates 64 character hex string. e.g. dfa38c60505ed2bacb06b60b8fe7aed0015B5dea5aef9105aca354Cfda5ffe36")] string hash, 
            [FromQuery, SwaggerParameter("Maximum Hamming (i.e. edit) Distance. e.g. 32", Required = false)] int maxhd = 32)
        {
            return new OkObjectResult(_index.Query(hash.ToLower(), maxhd));
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Hash an image",
            Description = "Generate the PDQ hash for an image (i.e. picture). Reducing image size to 512px (long end, maintain aspect ratio) will reduce network overhead without reducing accuracy ")]
        [SwaggerResponse(StatusCodes.Status200OK, "PDQ hash and quality metric for image", typeof(PDQHashCalculation))]
        public async Task<ActionResult<PDQHashCalculation>> GetHash([FromBody, SwaggerParameter("File for upload (named in form as \"file\"", Required = true)] IFormFile file)
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
            
        #pragma warning restore 1591   
        }

    }
}