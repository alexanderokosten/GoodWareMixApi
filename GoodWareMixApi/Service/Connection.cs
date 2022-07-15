namespace GoodWareMixApi.Service
{
    public static class Connection
    {
        public static MongoDBService context { get; set; }
        public static string InternalCodeAPI { get; set; }
        public static string URLPagination { get; set; }
    }
}
