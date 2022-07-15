using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;

namespace GoodWareMixApi.Service
{
    public static class StartSheduler
    {
        public static void Start()
        {
            MongoDBService mongoDBService = new MongoDBService("WebApiDatabase");
            mongoDBService.StartSheduler();
        }

        public static void CheckLog()
        {
            MongoDBService mongoDBService = new MongoDBService("WebApiDatabase");
            mongoDBService.StartSheduler();
        }


    }
}
