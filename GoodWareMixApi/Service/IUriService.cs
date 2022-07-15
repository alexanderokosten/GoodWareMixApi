using GoodWareMixApi.Filter;

namespace GoodWareMixApi.Service
{
    public interface IUriService
    {
        public Uri GetPageUri(PaginationFilter filter, string route);
    }
}
