using GCTL.Core.ViewModels.HrmHomeOfficeRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmHomeOfficeRequests
{
    public interface IHrmHomeOfficeRequestService
    {
        Task<bool> SaveAsync(HrmHomeOfficeRequestSetupViewModel model);
        Task<bool> EditAsync(HrmHomeOfficeRequestSetupViewModel model);
        Task<bool> BulkDeleteAsync(List<decimal> id);
        Task<HrmHomeOfficeRequestSetupViewModel> GetByIdAsync(decimal id);
        Task<(List<HrmHomeOfficeRequestSetupViewModel> Data, int TotalRecords, int filteredRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);        
        Task<EmployeeListItemViewModel> GetDataByEmpId(string selectedEmpId);
        Task<List<LookupItemDto>> GetHodAsync();
        Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model);
        Task<bool> HasDuplicate(string empId, DateTime? startDate, DateTime? endDate, string? hodId = null);
    }
}
