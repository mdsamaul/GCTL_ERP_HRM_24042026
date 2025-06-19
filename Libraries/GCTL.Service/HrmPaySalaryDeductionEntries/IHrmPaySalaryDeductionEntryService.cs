using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;
using GCTL.Data.Models;

namespace GCTL.Service.HRM_PAY_SalaryDeductionEntry
{
    public interface IHrmPaySalaryDeductionEntryService
    {
        Task<HrmPaySalaryDeductionEntryViewModel> GetByIdAsync(decimal id);
        Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel filter);
        Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync();
        Task<List<HrmPayDefDeductionType>> GetDeductionTypeAsync();
        Task<string> GenerateDeductionIdAsync();
        Task<List<HrmPaySalaryDeductionEntryViewModel>> GetAllAsync();
        Task<bool> SaveAsync(HrmPaySalaryDeductionEntryViewModel model);
        Task<bool> EditAsync(HrmPaySalaryDeductionEntryViewModel model);
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<byte[]> GenerateExcelSampleAsync();
        Task<bool> ProcessExcleFileAsync(Stream fileStream, HrmPaySalaryDeductionEntryViewModel model);

        Task<(List<HrmPaySalaryDeductionEntryViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
    }
}
