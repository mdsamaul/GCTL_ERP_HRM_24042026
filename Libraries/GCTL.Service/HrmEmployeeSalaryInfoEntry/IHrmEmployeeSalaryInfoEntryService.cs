using GCTL.Core.ViewModels.HrmEmployeeSalaryInfoEntry;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmEmployeeSalaryInfoEntry
{
    public interface IHrmEmployeeSalaryInfoEntryService
    {
        Task<bool> BulkDeleteAsync(List<decimal> autoIds);
        Task<bool> BulkEditAsync(HrmEmployeeSalaryInfoEntryViewModel model);
        Task<byte[]> GenerateExcelSampleAsync();
        Task<bool> ProcessExcelFileAsync(Stream stream, EmployeeListItemViewModel model);
        Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model);
        Task<List<HrmEisDefDisbursementMethod>> GetPaymentMode();
    }
}
