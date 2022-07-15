using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using GoodWareMixApi.Model.Entity;
using Quartz;
using Quartz.Impl;

namespace GoodWareMixApi.Quartz
{
    public class DataScheduler
    {
        public async void Start(SchedulerTask task, 
            IProductRepository productRepository, ISupplierRepository supplierRepository,
            ISchedulerTaskRepository schedulerTaskRepository)
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            scheduler.Context.Put("productRepository", productRepository);
            scheduler.Context.Put("supplierRepository", supplierRepository);
            scheduler.Context.Put("schedulerTaskRepository", schedulerTaskRepository);
            //scheduler.Context.Put("Supplier", supplier);
            await scheduler.Start();
            IJobDetail jobDetail = JobBuilder.Create<JobSupplier>()
                .WithIdentity($"{task.Id}",$"{task.SupplierId}")
                .UsingJobData("supplier", task.SupplierId)
                .UsingJobData("nameTask", task.NameTask).Build();


            if (task.StartAt != null)
            {
                if(task.StartAt.Equals(Convert.ToDateTime("01.01.0001 0:00:00")))
                {
                    ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity($"trigger {task.Id}", $"{task.SupplierId}")

                  .StartNow()
                  .WithCronSchedule($"{task.CronExpression}")
                   .WithPriority(1)
                  .Build();
                    await scheduler.ScheduleJob(jobDetail, trigger);
                    task.IsEnable = true;
                    schedulerTaskRepository.Put(task.NameTask, task);
                }
                else
                {
                  
                    ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"trigger {task.Id}", $"{task.SupplierId}")

                    .StartAt((DateTimeOffset)task.StartAt)
                    .WithCronSchedule($"{task.CronExpression}")
                    
                     .WithPriority(1)
                    .Build();
                     await scheduler.ScheduleJob(jobDetail, trigger);
                  
                


                    task.IsEnable = true;

                    schedulerTaskRepository.Put(task.NameTask, task);

                }
             
            }
          


            //scheduler1.JobFactory.NewJob((TriggerFiredBundle)trigger, scheduler);
          
            //scheduler.AddJob(jobDetail , true);
            //a.Add(scheduler);


        }
        public async void Pause(SchedulerTask task,
            IProductRepository productRepository, ISupplierRepository supplierRepository,
            ISchedulerTaskRepository schedulerTaskRepository)
        {
            //StdSchedulerFactory factory = new StdSchedulerFactory();
            //IScheduler scheduler = await factory.GetScheduler();

            //scheduler.Context.Put("productRepository", productRepository);
            //scheduler.Context.Put("supplierRepository", supplierRepository);
            //scheduler.Context.Put("schedulerTaskRepository", schedulerTaskRepository);
            //scheduler.Context.Put("Supplier", supplier);
            JobKey jobKey = new JobKey($"{task.Id}", $"{task.SupplierId}");
           
            StdSchedulerFactory factory = new StdSchedulerFactory();

            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.PauseJob(jobKey);
            Task.Delay(1000);
            var result = scheduler.DeleteJob(jobKey);
            task.NextDateTask = null;
            task.IsEnable = false;
            schedulerTaskRepository.Put(task.NameTask, task);
            Console.WriteLine($"{task.SupplierId}-ЗАДАЧА ОСТАНОВЛЕНА");

            //scheduler1.JobFactory.NewJob((TriggerFiredBundle)trigger, scheduler);

            //scheduler.AddJob(jobDetail , true);
            //a.Add(scheduler);


        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
