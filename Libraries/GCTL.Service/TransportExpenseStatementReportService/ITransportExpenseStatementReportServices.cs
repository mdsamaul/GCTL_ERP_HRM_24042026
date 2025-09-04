using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.TransportExpenseStatementReport;

namespace GCTL.Service.TransportExpenseStatementReportService
{
    public interface ITransportExpenseStatementReportServices
    {
        Task<TransportExpenseStatementReportFilterResultListDto>
          GetAllTransportExpenseStatementDropdownSelectReportAsync(TransportExpenseStatementReportFilterDataDto filter);
        
        Task<List<TransportExpenseStatementReportSetupViewModel>>
          GetAllTransportExpenseStatementResultReportAsync(TransportExpenseStatementReportFilterDataDto filter); Task<List<TransportExpenseStatementReportSetupViewModel>>
          GetAllTransportExpenseStatementResultReportExcelAsync(TransportExpenseStatementReportFilterDataDto filter);
    }
}
