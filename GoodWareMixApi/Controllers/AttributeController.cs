using GoodWareMixApi.Filter;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GoodWareMixApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AttributeController : ControllerBase
    {
        private readonly IUriService uriService;
        private readonly IAttributeRepository _attributeRepository;
        public AttributeController(IUriService uriSmoervice , IAttributeRepository attribute)
        {
            this.uriService = uriSmoervice;
            this._attributeRepository = attribute;
        }
        // GET: api/<AttributeController>
        [HttpGet]
        public async Task<PagedResponse<List<AttributeEntity>>> Get([FromQuery] PaginationFilter filter)
        {
            var route = Request.Path.Value;
            var products = await _attributeRepository.Get(filter, route);
            return products;

        }
        [HttpGet("{idAttribute}")]
        public async Task<AttributeEntity> Get(string idAttribute)
        {
            var products = await _attributeRepository.GetOneAttribute(idAttribute);
            return products;          
        }
        [HttpPost]
        public async Task<ActionResult> Post(AttributeHelper attributeHelper)
        {
            await _attributeRepository.PostUpdateAttribute(attributeHelper);
            return Ok();
        }




        //[HttpGet]
        //public async Task<PagedResponse<List<AttributeEntity>>> Get([FromQuery] PaginationFilter filter, string route)
        //{
        //    //var route = Request.Path.Value;
        //    //var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.Search, filter.Attributes);
        //    //var pagedData = await Connection.context.GetDataAttribute("Attribute");
        //    //var sorted = pagedData.Sort();


        //    var totalRecords = await Connection.context.GetDataAttribute("Attribute", filter);
        //    var pagedResponse = PaginationHelper.CreatePagedReponse<AttributeEntity>(totalRecords.Data, filter, Convert.ToInt32(totalRecords.Count), uriService, route);
        //    return pagedResponse;

        //}

        // GET api/<AttributeController>/5
        //[HttpGet]
        //public async Task<List<string[]>> Get([FromQuery] string[] attributeName)
        //{
        //    var currentAttribute = Connection.context.GetAttributeName("Product", attributeName);
        //    return currentAttribute;
        //}
        //[HttpGet]
        //public async Task<List<AttributeEntity>> Get()
        //{
        //    var currentAttribute = Connection.context.GetTables("Attribute", new AttributeEntity());
        //    return currentAttribute;
        //}


        // POST api/<AttributeController>
        //[HttpPost]
        //public async Task<ActionResult> Post([FromBody] AttributeEntity value)
        //{


        //    return Ok();

        //}

        //// PUT api/<AttributeController>/5
        [HttpPut]
        public async Task<ActionResult> Put(AttributeEntity attribute)
        {
            Thread myThread = new Thread(() => _attributeRepository.AddAttribute(attribute));
            myThread.Start();
            return Ok();
        }

        //// DELETE api/<AttributeController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string  id)
        {
            Thread myThread = new Thread(() => _attributeRepository.DeleteAttribute(id));
            myThread.Start();       
            return Ok();
        }
    }
}
