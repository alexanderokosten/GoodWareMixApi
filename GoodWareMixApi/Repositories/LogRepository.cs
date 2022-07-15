using GoodWareMixApi.Filter;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using GoodWareMixApi.Service;

namespace GoodWareMixApi.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly IUriService uriService;
        //private readonly ILogRepository logRepository;
       
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        private readonly ILogger<ProductRepository> _logger;
        private readonly ISupplierRepository supplierRepository;
        public LogRepository(IUriService uriService, ILogger<ProductRepository> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, ISupplierRepository supplierRepository)
        {
            this.uriService = uriService;
            _logger = logger;
            this.environment = environment;
            this.supplierRepository = supplierRepository;
        }
        public async Task<PagedResponse<List<Log>>> GetLog(PaginationFilter filter, string route)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.Search, filter.Supplier, filter.OrderBy, filter.Attributes, Convert.ToBoolean(filter.InternalCodeCheckBool));
            var pagedData = await Connection.context.GetLogData("Log", validFilter);
            var pagedResponse = PaginationHelper.CreatePagedReponse<Log>(pagedData.Data, validFilter, Convert.ToInt32(pagedData.Count), uriService, route);
            return pagedResponse;
        }

        public void Delete()
        {
            Connection.context.DeleteLogs("Log");

            
        }
    }
}
