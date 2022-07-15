using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
using GoodWareMixApi.Service;

namespace GoodWareMixApi.Intrefaces
{
    public interface IAttributeRepository
    {
        public Task<PagedResponse<List<AttributeEntity>>> Get(PaginationFilter filter, string route);
        public Task<AttributeEntity> GetOneAttribute(string idAttribute);
        public Task PostUpdateAttribute(AttributeHelper attributeHelper);
        public Task DeleteAttribute(string id);

        public Task AddAttribute(AttributeEntity attribute);
    }
}
