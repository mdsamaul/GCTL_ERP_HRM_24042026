using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.EmployeeLoanInformationReport;

namespace GCTL.Service.EmployeeLoanInformationReport
{
    public interface IEmployeeLoanInformationReportServices
    {
        Task<List<EmployeeLoanInformationReportVM>> GetLoanDetailsByEmployeeIdAsync(string employeeId);
        Task<List<EmployeeBasicInfoVM>> GetDistinctLoanEmployeesAsync();
    }
}
