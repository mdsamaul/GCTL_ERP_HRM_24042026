using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;
using GCTL.Data.Models;

namespace GCTL.Service.HrmWorkingDayDeclarations
{
    public interface IHrmWorkingDayDeclarationService
    {
        Task<List<HrmWorkingDayDeclarationViewModel>> GetAllAsync();
        Task<(bool isSuccess, string message, object data)> SaveAsync(HrmWorkingDayDeclarationViewModel model);
        Task<(bool isSuccess, string message, object data)> EditAsync(HrmWorkingDayDeclarationViewModel model);
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> DeleteDeclarationByDateAndEmployee(DateTime date, List<string> ids);
        Task<HrmWorkingDayDeclarationViewModel> GetByIdAsync(decimal id);
        Task<(List<HrmWorkingDayDeclarationViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);

        Task<EmployeeFilterResultDto> GetFilterDataAsync(EmployeeFilterViewModel filter);
    }
}
