using GoodWareMixApi.Filter;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace GoodWareMixApi.Repositories
{
    public class SchedulerTaskRepository : ISchedulerTaskRepository
    {
        private readonly IUriService uriService;
        private readonly ISupplierRepository supplierRepository;
        //ParserDocuments documentService = new ParserDocuments();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        private readonly ILogger<ProductRepository> _logger;
        public SchedulerTaskRepository(IUriService uriService, ILogger<ProductRepository> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            this.uriService = uriService;
            _logger = logger;
            this.environment = environment;
        }

        public async Task<ActionResult<SchedulerTask>> Delete(string nameTask)
        {
            if (string.IsNullOrEmpty(nameTask))
            {
                return null;
            }
            else
            {
                var currentTask = await Connection.context.GetTask("Task", nameTask);

                if (currentTask != null)
                {
                    //supplier.Id = currentSupplier.Id;
                    await Connection.context.DeleteTask("Task", nameTask);
                    return currentTask;
                }
                else
                {
                    return null;
                }

            }
        }

        public async Task<PagedResponse<List<SchedulerTask>>> Get(PaginationFilter filter, string route)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.Search, filter.Supplier, filter.OrderBy, filter.Attributes, Convert.ToBoolean(filter.InternalCodeCheckBool));
            var pagedData = await Connection.context.GetDataTask("Task", validFilter);
            var totalRecords = await Connection.context.GetDataTaskCount<SchedulerTask>("Task", filter);
            var pagedResponse = PaginationHelper.CreatePagedReponse<SchedulerTask>(pagedData, validFilter, Convert.ToInt32(totalRecords), uriService, route);
            return pagedResponse;
        }

        public async Task<SchedulerTask> Post(SchedulerTask task)
        {
            if (task == null)
            {
                return null;
            }
            else
            {
                var currentTask = Connection.context.GetTask("Task", task.NameTask).Result;

                if (currentTask == null)
                {
                    task.Id = ObjectId.GenerateNewId().ToString();
                    Connection.context.InsertRecord("Task", task);
                    return task;
                }
                else
                {
                    return currentTask;
                }

            }
        }

        public async Task<ActionResult<SchedulerTask>> Put(string nameTask, [FromBody] SchedulerTask task)
        {
            if (task == null)
            {
                return null; 
            }
            else
            {
                var currentTask = await Connection.context.GetTask("Task", nameTask);

                if (currentTask != null)
                {
                    task.Id = currentTask.Id;
                    await Connection.context.UpdateTask("Task", task);
                    return task;
                }
                else
                {
                    return currentTask;
                }

            }
        }

        public async Task<SchedulerTask> GetTask(string name)
        {

            SchedulerTask schedulerTask = await Connection.context.GetTask("Task", name);
            return schedulerTask;
        }
    }
}
