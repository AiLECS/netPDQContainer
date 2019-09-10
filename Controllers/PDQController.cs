using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using netMIH;

namespace netPDQContainer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PDQController : ControllerBase
    {
        private readonly netMIH.Index _index;
        private string _regex;
        public PDQController(netMIH.Index index)
        {
            this._index = index;
        }
        // GET api/values
        [HttpGet]
        [Route("categories")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return _index.ListCategories().ToArray();
        }

        [HttpGet]
        public ActionResult<IEnumerable<Result>> Get([FromQuery, Required, RegularExpression("^[a-f0-9]{64}$")] string hash, [FromQuery] int maxhd = 32)
        {
            return new OkObjectResult(_index.Query(hash, maxhd));
        }

    }
}