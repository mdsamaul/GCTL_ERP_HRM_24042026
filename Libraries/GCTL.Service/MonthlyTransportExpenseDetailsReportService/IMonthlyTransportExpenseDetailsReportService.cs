using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MonthlyTransportExpenseDetailsReport;
using GCTL.Core.ViewModels.TransportExpenseStatementReport;

namespace GCTL.Service.MonthlyTransportExpenseDetailsReportService
{
    public interface IMonthlyTransportExpenseDetailsReportService
    {
        Task<MonthlyTransportExpenseDetailsReportFilterResultListDto>GetAllTransportExpenseStatementDropdownSelectReportAsync(MonthlyTransportExpenseDetailsReportFilterDataDto filter);
   
        Task<List<MonthlyTransportExpenseDetailsReportSetupDto>> GetAllTransportExpenseStatementResultReportAsync(MonthlyTransportExpenseDetailsReportFilterDataDto filter);
   
    }
}
