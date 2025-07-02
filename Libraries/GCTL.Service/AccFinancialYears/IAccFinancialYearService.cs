using GCTL.Core.ViewModels.AccFinancialYears;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AccFinancialYears
{
    public interface IAccFinancialYearService
    {
        Task<List<AccFinancialYearSetupViewModel>> GetAllAsync();
        Task<AccFinancialYearSetupViewModel> GetByIdAsync(string code);
        Task<bool> IsExistAcync(DateTime? startDate, DateTime? endDate, string typeCode);

        Task<bool> SaveAsync(AccFinancialYearSetupViewModel entityVM);
        Task<bool> UpdateAsync(AccFinancialYearSetupViewModel entityVM);
        Task<bool> DeleteTab(List<string> ids);

        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
    }
}
