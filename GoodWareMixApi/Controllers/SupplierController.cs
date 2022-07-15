using GoodWareMixApi.Filter;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GoodWareMixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IUriService uriService;
        private readonly ILogger<SupplierConfig> _logger;
        private ISupplierRepository _supplierRepository;
        public SupplierController(IUriService uriService, ILogger<SupplierConfig> logger, ISupplierRepository supplierRepository)
        {
            this.uriService = uriService;
            _logger = logger;
            _supplierRepository = supplierRepository;
        }
        //MongoDBService db = new MongoDBService("WebApiDatabase");


        //// GET: api/<SupplierController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //ДОДЕЛАТЬ
        //GET api/<SupplierController>/Shnieder
        [HttpGet]
        public async Task<PagedResponse<List<ProFileSupplier>>> Get([FromQuery] PaginationFilter filter)
        {

            var route = Request.Path.Value;

            var result = await _supplierRepository.Get(filter, route);

            //var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize,filter.Search,filter.Attributes);
            //var pagedData = await Connection.context.GetDataSupplier<ProFileSupplier>("Supplier", validFilter);
            //var totalRecords = await Connection.context.GetDataSupplierCount<ProFileSupplier>("Supplier", filter);
            //var pagedResponse = PaginationHelper.CreatePagedReponse<ProFileSupplier>(pagedData, validFilter, Convert.ToInt32(totalRecords), uriService, route);
            return result;
        }





        [HttpGet("{id}")]
        public ProFileSupplier Get(string id)
        {
            ProFileSupplier proFileSupplier = _supplierRepository.Get(id);          
                
            if(proFileSupplier == null)
            {
                return null;

            }
            else
            {
                return proFileSupplier;
            }
            
        }
        
        //ProFileSupplier supplier
        // POST api/<SupplierController>
        [HttpPost]
        public async Task<ActionResult<ProFileSupplier>> Post(ProFileSupplier supplier)
        {
            if (supplier == null )
            {
                return BadRequest(supplier);
            }
            else
            {
                var result = _supplierRepository.Post(supplier);
                return Ok(result);

            }
           
          
        
          
        }

        

        // PUT api/<SupplierController>/5
        [HttpPut("{supplierName}")]
        public async Task<ActionResult<ProFileSupplier>> Put(string supplierName, [FromBody]ProFileSupplier supplier)
        {
            if (supplier == null)
            {
                return BadRequest(supplier);
            }
            else
            {
                var result = _supplierRepository.Put(supplierName, supplier);
                return Ok();
              

            }
        }

        // DELETE api/<SupplierController>/5
        [HttpDelete("{supplierName}")]
        public async Task<ActionResult<ProFileSupplier>> Delete(string supplierName)
        {
            if (string.IsNullOrEmpty(supplierName))
            {
                return BadRequest(supplierName);
            }
            else
            {
                var result = _supplierRepository.Delete(supplierName);
                return Ok();
            }
        }




        [HttpDelete]
        public async Task<ActionResult> DeleteProduct(string supplierId)
        {
            if (string.IsNullOrEmpty(supplierId))
            {
                return BadRequest(supplierId);
            }
            else
            {
                var result = _supplierRepository.DeleteProduct(supplierId);
                return Ok();
            }
        }





    }
}
