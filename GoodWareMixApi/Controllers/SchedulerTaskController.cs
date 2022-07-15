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
    public class SchedulerTaskController : ControllerBase
    {
        private readonly IUriService uriService;
        private readonly ILogger<SchedulerTaskController> _logger;
        private ISchedulerTaskRepository _schedulerTaskRepository;

        public SchedulerTaskController(IUriService uriService, 
            ILogger<SchedulerTaskController> logger, ISchedulerTaskRepository schedulerTaskRepository)
        {
            this.uriService = uriService;
            _logger = logger;
            _schedulerTaskRepository = schedulerTaskRepository;
        }

        // GET: api/<SchedulerTaskController>
        [HttpGet]
        public async Task<PagedResponse<List<SchedulerTask>>> Get([FromQuery] PaginationFilter filter)
        {
            var route = Request.Path.Value;
            var result = await _schedulerTaskRepository.Get(filter, route);
            return result;
        }

       

        // POST api/<SchedulerTaskController>
        [HttpPost]
        public async Task<ActionResult<SchedulerTask>> Post([FromBody]SchedulerTask task)
        {
            if (task == null)
            {
                return BadRequest(task);
            }
            else
            {
                task.IsEnable = false;
                var result = _schedulerTaskRepository.Post(task);
                return Ok(result);

            }




        }

        // PUT api/<SchedulerTaskController>/5
        [HttpPut("{nameTask}")]
        public async Task<ActionResult<SchedulerTask>> Put(string nameTask, [FromBody] SchedulerTask task)
        {
            if (task == null)
            {
                return BadRequest(task);
            }
            else
            {
                var result = _schedulerTaskRepository.Put(nameTask, task);
                return Ok();
               

            }
        }

        // DELETE api/<SchedulerTaskController>/5
        [HttpDelete("{nameTask}")]
        public async Task<ActionResult<SchedulerTask>> Delete(string nameTask)
        {
            if (string.IsNullOrEmpty(nameTask))
            {
                return BadRequest(nameTask);
            }
            else
            {
                var result = _schedulerTaskRepository.Delete(nameTask);
                return Ok();

            }
        }
    }
}
