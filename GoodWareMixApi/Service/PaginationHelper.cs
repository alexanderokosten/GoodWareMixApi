using GoodWareMixApi.Filter;
using GoodWareMixApi.Model;
namespace GoodWareMixApi.Service
{
    public class PaginationHelper
    {
        public static PagedResponse<List<T>> CreatePagedReponse<T>(List<T> pagedData, PaginationFilter validFilter, int totalRecords, IUriService uriService, string route)
        {
            var respose = new PagedResponse<List<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize);
            var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));
            respose.NextPage =
                validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
                ? uriService.GetPageUri(new PaginationFilter(validFilter.PageNumber + 1, validFilter.PageSize,validFilter.Search, validFilter.Supplier, validFilter.OrderBy, validFilter.Attributes , validFilter.InternalCodeCheckBool), route)
                : null;
            respose.PreviousPage =
                validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
                ? uriService.GetPageUri(new PaginationFilter(validFilter.PageNumber - 1, validFilter.PageSize, validFilter.Search, validFilter.Supplier, validFilter.OrderBy, validFilter.Attributes , validFilter.InternalCodeCheckBool), route)
                : null;
            respose.FirstPage = uriService.GetPageUri(new PaginationFilter(1, validFilter.PageSize, validFilter.Search, validFilter.Supplier, validFilter.OrderBy, validFilter.Attributes ,validFilter.InternalCodeCheckBool), route);
            respose.LastPage = uriService.GetPageUri(new PaginationFilter(roundedTotalPages, validFilter.PageSize, validFilter.Search, validFilter.Supplier, validFilter.OrderBy, validFilter.Attributes , validFilter.InternalCodeCheckBool), route);
            respose.TotalPages = roundedTotalPages;
            respose.TotalRecords = totalRecords;
            return respose;
        }
    }
}
