using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;
using GCTL.Core.ViewModels.HrmPayMonths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmPayMonthlyOtEntries
{
    public interface IHrmPayMonthlyOtEntryService
    {
        Task<bool> BulkDeleteAsync(List<decimal> tcs);
        Task<bool> EditAsync(HrmPayMonthlyOtEntryViewModel model);
        Task<byte[]> GenerateExcelSampleAsync();
        Task<string> GenerateMonthlyOtIdAsync();
        Task<HrmPayMonthlyOtEntryViewModel> GetByIdAsync(decimal id);
        Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model);
        Task<(List<HrmPayMonthlyOtEntryViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
        Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync();
        Task<bool> ProcessExcelFileAsync(Stream stream, HrmPayMonthlyOtEntryViewModel model);
        Task<bool> SaveAsync(HrmPayMonthlyOtEntryViewModel model);
    }
}
