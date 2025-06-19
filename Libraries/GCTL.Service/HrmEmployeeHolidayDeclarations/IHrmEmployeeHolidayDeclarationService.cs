using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HrmDefHolidayDeclarationTypes;
using GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations;
using GCTL.Data.Models;

namespace GCTL.Service.HrmEmployeeHolidayDeclarations
{
    public interface IHrmEmployeeHolidayDeclarationService
    {
        Task<List<HrmEmployeeHolidayDeclarationViewModel>> GetAllAsync();
        Task<List<HrmDefHolidayDeclarationTypeViewModel>> GetAllHolidayType();
        Task<(bool isSuccess, string message, object data)> SaveAsync(HrmEmployeeHolidayDeclarationViewModel model);
        Task<(bool isSuccess, string message, object data)> EditAsync(HrmEmployeeHolidayDeclarationViewModel model);
        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> DeleteHolidayDeclarationsByDateAndEmployees(DateTime date, List<string> employeeIds, string companyCode);
        Task<HrmEmployeeHolidayDeclarationViewModel> GetByIdAsync(decimal id);
        Task<byte[]> GenerateEmployeeHolidayDeclarationExcelAsync();
        Task<(bool isSuccess, string message, object data)> ProcessExcelFileAsync(Stream fileStream, string userName, string ip, string mac, string companyCode);
        Task<(List<HrmEmployeeHolidayDeclarationViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
        List<CoreCompany> GetAllCompany();
        Task<EmployeeFilterResultDto> GetFilterDataAsync(EmployeeFilterViewModel filter);
    }
}
