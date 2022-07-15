namespace GoodWareMixApi.Filter
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; }
        public bool InternalCodeCheckBool { get; set; }  
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string Supplier { get; set; }
        public string[] Attributes { get; set; }
        public string OrderBy { get; set; }
        public PaginationFilter()
        {
            this.PageNumber = 1;
            this.PageSize = 10;
            this.Search = "";
            this.Supplier = "";
            this.OrderBy = "";
            this.Attributes = new string[] { };
            this.InternalCodeCheckBool = false;
        }
        public PaginationFilter(int pageNumber, int pageSize, string Search, string Supplier, string OrderBy, string[] Attributes , bool InternalCodeCheckBool)
        {
            this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.PageSize = pageSize <= 0 ? 10 : pageSize;
            this.Search = Search == null ? "":Search;
            this.Supplier = Supplier == null ? "":Supplier;
            this.OrderBy = OrderBy == null ? "": OrderBy;
            this.Attributes = Attributes == null ? new string[] { } : Attributes;
            this.InternalCodeCheckBool = InternalCodeCheckBool;
        }
    }
}
