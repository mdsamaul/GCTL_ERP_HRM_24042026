using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.ManualEarnLeaveEntry;
using GCTL.Core.ViewModels.PFAssignEntry;

namespace GCTL.Service.PFAssignEntry
{
    public interface IPFAssignEntryService
    {
        Task<PFAssignEntryFilterListDto> GetFilterDataAsync(PFAssignEntryFilterDto filter);
        Task<(bool isSuccess, string message, object data)> CreateUpdatePFAssignService(PFAssignEntrySetupViewModel fromData);
        Task<byte[]> GeneratePfAssignExcelDownload();
        Task<(bool isSuccess, string message, object data)> SavePFAssignExcel(PFAssignEntrySetupViewModel fromData);
        Task<List<PFAssignEntrySetupViewModel>> GetPfAssignDataService();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<PFAssignEntrySetupViewModel> getAssignValueById(string id);
    }
}

