using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using Microsoft.AspNetCore.Mvc;

namespace GoodWareMixApi.Intrefaces
{
    public interface ISchedulerTaskRepository
    {
        public Task<PagedResponse<List<SchedulerTask>>> Get(PaginationFilter filter, string route);
        public Task<SchedulerTask> GetTask(string name);
        public Task<SchedulerTask> Post(SchedulerTask task);
        public Task<ActionResult<SchedulerTask>> Put(string nameTask, [FromBody] SchedulerTask task);
        public Task<ActionResult<SchedulerTask>> Delete(string nameTask);
    }
}
