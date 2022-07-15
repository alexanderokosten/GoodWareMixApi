
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.IO;
using GoodWareMixApi.Scheduler;

namespace GoodWareMixApi.Scheduler.SchedulTask
{
    public class TaskSchedul : ScheduledProcessor
    {

        public TaskSchedul(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
                
        }

        protected override string Schedule => "*/1 * * * *"; // every 1 min 

        public override async System.Threading.Tasks.Task ProcessInScope(IServiceProvider scopeServiceProvider)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"http://172.16.41.203:45456/api/Product?PageSize=1");
            string json = await response.Content.ReadAsStringAsync();

            // serialize JSON to a string and then write string to a file
         
            //client.BaseAddress = new Uri("http://172.16.41.203:45455/");
            //var result = client.GetAsync("https://172.16.41.203:45455/api/Products");
            //var products = result.Result;

            Console.WriteLine("SampleTask1 : " + DateTime.Now.ToString() + $"{json}");

            // return Task.CompletedTask;


            await System.Threading.Tasks.Task.Run(() => {
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }
    }
}
