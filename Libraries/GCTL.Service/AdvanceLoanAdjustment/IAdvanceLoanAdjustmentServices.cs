using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.AdvanceLoanAdjustment;

namespace GCTL.Service.AdvanceLoanAdjustment
{
    public interface IAdvanceLoanAdjustmentServices
    {
        Task<List<CompanyDto>> GetAllAndFilterCompanyAsync(string searchCompanyName);
        Task<List<EmployeeAdjustmentDto>> GetEmployeesByFilterAsync(string employeeStatusId, string companyCode, string employeeName);
        Task<EmployeeAdjustmentDto> GetLoadEmployeeByIdAsync(string employeeId);
        Task<List<LoanDataDto>> GetLoanByEmployeeIdAsync(string employeeId);
        Task<LoanDataDto> GetLoanByIdAsync(string loanId);
    }
}
