using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace GoodWareMixApi.Intrefaces
{
    public interface ISupplierRepository
    {
        public Task<PagedResponse<List<ProFileSupplier>>> Get([FromQuery] PaginationFilter filter, string route);
        public ProFileSupplier Get(string id);
        public Task<ProFileSupplier> Post(ProFileSupplier supplier);
        public Task<ActionResult<ProFileSupplier>> Put(string supplierName, [FromBody] ProFileSupplier supplier);
        public Task<ActionResult<ProFileSupplier>> Delete(string supplierName);

        public Task<ActionResult<ProFileSupplier>> DeleteProduct(string supplierId);
    }
}
