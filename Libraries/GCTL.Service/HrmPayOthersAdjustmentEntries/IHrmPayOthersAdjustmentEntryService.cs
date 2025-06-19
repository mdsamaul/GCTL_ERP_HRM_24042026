using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;


namespace GCTL.Service.HrmPayOthersAdjustmentEntries
{
    public interface IHrmPayOthersAdjustmentEntryService
    {
        //Task<List<HrmPayOthersAdjustmentEntryViewModel>> GetAllAsync();
        Task<List<HrmPayDefBenefitTypeViewModel>> GetBenefitTypeAsync();
        Task<string> GetOthersAdjustmentIdAsync();
        Task<HrmPayOthersAdjustmentEntryViewModel> GetByIdAsync(decimal id);

        Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel filter);


        Task<bool> SaveAsync(HrmPayOthersAdjustmentEntryViewModel model);
        Task<bool> EditAsync(HrmPayOthersAdjustmentEntryViewModel model);
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<byte[]> GenerateExcelSampleAsync();
        Task<bool> ProcessExcleFileAsync(Stream fileStream, HrmPayOthersAdjustmentEntryViewModel model);

        Task<(List<HrmPayOthersAdjustmentEntryViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
        Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync();
    }
}
