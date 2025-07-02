using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceNotConfirmationEntries
{
    public interface IHrmServiceNotConfirmationEntryService
    {
        Task<bool> BulkDeleteAsync(List<decimal> tcs);
        Task<bool> EditAsync(HrmServiceNotConfirmViewModel model);
        Task<byte[]> GenerateExcelSampleAsync();
        Task<string> GenerateSNCIdAsync();
        Task<bool> ProcessExcelFileAsync(Stream stream, HrmServiceNotConfirmViewModel model);
        Task<HrmServiceNotConfirmViewModel> GetByIdAsync(decimal id);
        Task<(List<HrmServiceNotConfirmViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
        Task<bool> SaveAsync(HrmServiceNotConfirmViewModel model);
        Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model);
        Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync();

    }
}
