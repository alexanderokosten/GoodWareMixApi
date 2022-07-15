using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Model;
using Quartz;
using static Quartz.MisfireInstruction;

namespace GoodWareMixApi.Quartz
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class JobSupplier : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var schedulerContext = context.Scheduler.Context;
            var productRepository = (IProductRepository)schedulerContext.Get("productRepository");
            var supplierRepository = (ISupplierRepository)schedulerContext.Get("supplierRepository");
            var schedulerTaskRepository = (ISchedulerTaskRepository)schedulerContext.Get("schedulerTaskRepository");


            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string supplierName = dataMap.GetString("supplier");
            string nameTask = dataMap.GetString("nameTask");


            var supplier = supplierRepository.Get(supplierName);
            var currentTask = await schedulerTaskRepository.GetTask(nameTask);


            if (supplier != null)
            {

                Console.WriteLine($"{supplier.SupplierName} - CURRENT SUPPLIER ");
                productRepository.RunTaskQUARTZAsync(supplier);
                //productRepository.RunTaskQUARTZAsyncInternalCode(supplier);
                var expression = new CronExpression($"{currentTask.CronExpression}");
                DateTimeOffset? time = expression.GetTimeAfter(DateTimeOffset.UtcNow);
                Console.WriteLine($"Следующая дата запуска: " + time);
                currentTask.NextDateTask = time;
                currentTask.IsEnable = true;
                await schedulerTaskRepository.Put(currentTask.NameTask, currentTask);
                Console.WriteLine($"Дата следующего запуска для {supplier.SupplierName} обновлена");
                //TODO: Рассчитать дату следующей итерации
                //currentTask.


            }


        }
    }
}
