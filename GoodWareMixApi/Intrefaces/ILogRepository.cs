using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;

namespace GoodWareMixApi.Intrefaces
{
    public interface ILogRepository
    {
        Task<PagedResponse<List<Log>>> GetLog(PaginationFilter filter, string route);

        void Delete();
        //      Task<PagedResponse<List<Product>>> Get(PaginationFilter filter, string route);
        //Task<Product> Get([FromQuery] string internalCode);
        //Task<Product> GetProductById(string id);
        //Task<List<string>> Post(string supplierName, IFormFile file);
        //Task<List<string>> RunTaskAsync(string name, IFormFile file);
    }
}
