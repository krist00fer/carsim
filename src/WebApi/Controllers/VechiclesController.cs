using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class VechiclesController
    {
        // GET api/vehicles
        [HttpGet]
        public IEnumerable<string> Get()
        {
            // TODO: return all vehicles
            return new string[] { "value1", "value2" };
        }

        // GET api/vehicles/5
        [HttpGet("{id}")]
        public string Get(string vechicleId)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
