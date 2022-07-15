using GoodWareMixApi.Filter;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GoodWareMixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
      
        private readonly IUriService uriService;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        private readonly ILogger<ProductController> _logger;

        private ILogRepository _logRepository;
        private ISupplierRepository _supplierRepository;


        public LogController(IUriService uriService, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILogger<ProductController> logger, ILogRepository logRepository, ISupplierRepository supplierRepository)
        {
            this.uriService = uriService;
            this.environment = env;
            _logger = logger;
            _logRepository = logRepository;
            _supplierRepository = supplierRepository;
        }

        // GET: api/<LogController>
        [HttpGet()]
        public async Task<PagedResponse<List<Log>>> Get([FromQuery] PaginationFilter filter)
        {
            var route = Request.Path.Value;
            var logs = await _logRepository.GetLog(filter, route);
            return logs;

        }
        [HttpDelete("DeleteLogs")]
        public async Task<bool> Delete()
        {
            //var route = Request.Path.Value;
            _logRepository.Delete();
            return true;
           

        }
    }
    
}



    

  

    

