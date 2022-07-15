using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodWareMixApi.Scheduler.SchedulServices
{
    public interface IReportGenerator
    {
        string GenerateDailyReport();

        Task GenerateDailyReportAsync();
    }
}
