using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GoodWareMixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttributeValuesController : ControllerBase
    {
        // GET: api/<AttributeValuesController>
        [HttpGet]
        public async Task<List<List<string>>> Get([FromQuery] string[] attributeName)
        {
            var currentAttribute = Connection.context.GetAttributeName("Product", attributeName);
            return currentAttribute;
        }
        [HttpGet("{attributeName}")]
        public async Task<List<string>> Get(string attributeName)
        {
            var currentAttribute = Connection.context.GetOneAttributeName("Product", attributeName);
            return currentAttribute;
        }

      
    }
}
