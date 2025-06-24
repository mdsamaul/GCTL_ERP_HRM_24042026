using GCTL.Core.ViewModels.HrmEmployeeSalaryInfoEntry;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmEmployeeSalaryInfoEntry
{
    public interface IHrmEmployeeSalaryInfoEntry
    {
        Task<bool> BulkDeleteAsync(List<decimal> autoIds);
        Task<bool> BulkEditAsync(HrmEmployeeSalaryInfoEntryViewModel model);
        Task<byte[]> GenerateExcelSampleAsync();
        Task<bool> ProcessExcelFileAsync(Stream stream, EmployeeListItemViewModel model);
        Task<(List<EmployeeFilterResultDto> data, int TotalRecords)> GetFilterEmployeeAsync(EmployeeFilterViewModel model, string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
        Task<List<HrmEisDefDisbursementMethod>> GetPaymentMode();
    }
}
