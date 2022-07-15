using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace GoodWareMixApi.Interfaces
{
    public interface IProductRepository
    {
        Task<PagedResponse<List<Product>>> Get(PaginationFilter filter,string route);
        Task<Product> Get([FromQuery] string internalCode);
        Task<Product> GetProductById(string id);
        Task<List<string>> Post(string supplierName, IFormFile file);
        Task<List<string>> BindingInternalCode(string supplierName, IFormFile file);
        Task<List<string>> RunTaskAsync(ProFileSupplier name, IFormFile file);
        Task<ProFileSupplier> RunTaskQUARTZAsync(ProFileSupplier supplier);
        //void RunTaskQUARTZAsyncInternalCode(ProFileSupplier supplier);
    }
}
