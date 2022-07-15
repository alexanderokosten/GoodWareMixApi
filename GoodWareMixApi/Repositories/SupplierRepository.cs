using GoodWareMixApi.Filter;
using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace GoodWareMixApi.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly IUriService uriService;
        private readonly ISupplierRepository supplierRepository;
        //ParserDocuments documentService = new ParserDocuments();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        private readonly ILogger<ProductRepository> _logger;
        public SupplierRepository(IUriService uriService, ILogger<ProductRepository> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            this.uriService = uriService;
            _logger = logger;
            this.environment = environment;
        }
        public async Task<ActionResult<ProFileSupplier>> Delete(string supplierName)
        {
            if (string.IsNullOrEmpty(supplierName))
            {
                return null;
            }
            else
            {
                var currentSupplier = await Connection.context.GetProfileSupplier("Supplier", supplierName);

                if (currentSupplier != null)
                {
                    //supplier.Id = currentSupplier.Id;
                    await Connection.context.DeleteSupplier("Supplier", supplierName);
                    return currentSupplier;
                }
                else
                {
                    return null;
                }

            }
        }


        public async Task<ActionResult<ProFileSupplier>> DeleteProduct(string supplierId)
        {
            if (string.IsNullOrEmpty(supplierId))
            {
                return null;
            }
            else
            {

                //supplier.Id = currentSupplier.Id;
                await Connection.context.DeleteSupplierProduct("Supplier", supplierId);
                return null;


            }
        }

        public async Task<PagedResponse<List<ProFileSupplier>>> Get([FromQuery] PaginationFilter filter, string route)
        {
            //var route = Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.Search, filter.Supplier, filter.OrderBy, filter.Attributes, Convert.ToBoolean(filter.InternalCodeCheckBool));
            var pagedData = await Connection.context.GetDataSupplier<ProFileSupplier>("Supplier", validFilter);
            var totalRecords = await Connection.context.GetDataSupplierCount<ProFileSupplier>("Supplier", filter);
            var pagedResponse = PaginationHelper.CreatePagedReponse<ProFileSupplier>(pagedData, validFilter, Convert.ToInt32(totalRecords), uriService, route);
            return pagedResponse;
        }

        public ProFileSupplier Get(string id)
        {
            ProFileSupplier supplier = Connection.context.GetSupplierEntity("Supplier", id);
            return supplier;
        }

        public async Task<ProFileSupplier> Post(ProFileSupplier supplier)
        {
            if (supplier == null)
            {
                return null;
            }
            else
            {
                var currentSupplier = Connection.context.GetProfileSupplier("Supplier", supplier.SupplierName).Result;

                if (currentSupplier == null)
                {
                    supplier.Id = ObjectId.GenerateNewId().ToString();
                    Connection.context.InsertRecord("Supplier", supplier);
                    return supplier;
                }
                else
                {
                    return currentSupplier;
                }

            }

        }

        public async Task<ActionResult<ProFileSupplier>> Put(string supplierName, [FromBody] ProFileSupplier supplier)
        {
            if (supplier == null)
            {
                return supplier;
            }
            else
            {
                var currentSupplier = await Connection.context.GetProfileSupplier("Supplier", supplierName);

                if (currentSupplier != null)
                {
                    supplier.Id = currentSupplier.Id;
                    await Connection.context.UpdateSupplier("Supplier", supplier);
                    return supplier;
                }
                else
                {
                    return currentSupplier;
                }

            }
        }

    }
}
