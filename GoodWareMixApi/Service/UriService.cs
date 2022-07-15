using GoodWareMixApi.Filter;
using Microsoft.AspNetCore.WebUtilities;

namespace GoodWareMixApi.Service
{
    public class UriService : IUriService
    {
        public UriService(string baseUri)
        {
            //_baseUri = baseUri;
        }
        public Uri GetPageUri(PaginationFilter filter, string route)
        {
            if (!string.IsNullOrEmpty(filter.Search) && filter.Attributes.Length != 0)
            {
                var _enpointUri = new Uri(string.Concat(Connection.URLPagination, route));
                var modifiedUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "Search", filter.Search.ToString());

                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "InternalCodeCheckBool", filter.InternalCodeCheckBool.ToString());
                foreach (var item in filter.Attributes)
                {
                    modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "Attributes", item.ToString());
                }
                return new Uri(modifiedUri);



               
            }
            else if(!string.IsNullOrEmpty(filter.Search))
            {
                var _enpointUri = new Uri(string.Concat(Connection.URLPagination, route));
                var modifiedUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "Search", filter.Search.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "InternalCodeCheckBool", filter.InternalCodeCheckBool.ToString());
                return new Uri(modifiedUri);
            }
            else if(filter.Attributes.Length != 0)
            {
                var _enpointUri = new Uri(string.Concat(Connection.URLPagination, route));
                var modifiedUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
                foreach (var item in filter.Attributes)
                {
                    modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "Attributes", item.ToString());
                }
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "InternalCodeCheckBool", filter.InternalCodeCheckBool.ToString());
                return new Uri(modifiedUri);
            }
            else
            {

                var _enpointUri = new Uri(string.Concat(Connection.URLPagination, route));
                var modifiedUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "InternalCodeCheckBool", filter.InternalCodeCheckBool.ToString());
                return new Uri(modifiedUri);
            }
        
        }
    }
}
