using GCTL.Core.ViewModels.ManageNoticeEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ManageNoticeEntries
{
    public interface IManageNoticeService
    {
        Task<bool> BulkDeleteAsync(List<int> tcs);
        Task<bool> EditAsync(ManageNoticeSetupViewModel model);
        Task<string> GenerateIdAsync();
        Task<ManageNoticeSetupViewModel> GetByIdAsync(int id);
        Task<EmployeeFilterResultDto> GetFilterEmpAsync(EmployeeFilterViewModel filter);
        Task<(List<ManageNoticeSetupViewModel> Data, int TotalRecords, int FilterRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
        Task<bool> SaveAsync(ManageNoticeSetupViewModel model);
        Task<bool> SentNoticeToEmployeeAsync(List<string> empIds, List<int> tcs);
    }
}
