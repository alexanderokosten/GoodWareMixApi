using GoodWareMixApi.Filter;
using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace GoodWareMixApi.Repositories
{
    public class AttributeRepository : IAttributeRepository
    {
        private readonly IUriService uriService;
        private readonly IProductRepository productRepository;

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;
        ParserDocuments documentService = new ParserDocuments();
        private readonly ILogger<ProductRepository> _logger;
        private readonly ISupplierRepository supplierRepository;

        public AttributeRepository(IUriService uriService, ILogger<ProductRepository> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, ISupplierRepository supplierRepository)
        {
            this.uriService = uriService;
            _logger = logger;

            this.environment = environment;
            documentService.environment = environment;
            this.supplierRepository = supplierRepository;

        }

        public async Task<PagedResponse<List<AttributeEntity>>> Get([FromQuery] PaginationFilter filter, string route)
        {

            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.Search, filter.Supplier, filter.OrderBy, filter.Attributes , Convert.ToBoolean(filter.InternalCodeCheckBool));
            var pagedData = await Connection.context.GetDataAttribute("Attribute", validFilter);
            var pagedResponse = PaginationHelper.CreatePagedReponse<AttributeEntity>(pagedData.Data, validFilter, Convert.ToInt32(pagedData.Count), uriService, route);
            return pagedResponse;
        }

        public async Task<AttributeEntity> GetOneAttribute(string idAttribute)
        {
            var attributeEntity = Connection.context.GetDataAttributeByID("Attribute", idAttribute);
            return attributeEntity;
        }

        public async Task PostUpdateAttribute(AttributeHelper attributeHelper)
        {
             Connection.context.UpdateKeyAttribute(attributeHelper);
        }


        public async Task DeleteAttribute(string id)
        {
            Connection.context.DeleteOneAttribute(id);
        }

        public async Task AddAttribute(AttributeEntity attribute)
        {
            Connection.context.AddAttribute(attribute);
     
        }
    }
}
