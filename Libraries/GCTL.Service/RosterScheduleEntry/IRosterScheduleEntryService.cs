using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.PFAssignEntry;
using GCTL.Core.ViewModels.RosterScheduleEntry;

namespace GCTL.Service.RosterScheduleEntry
{
    public interface IRosterScheduleEntryService
    {
        Task<RosterScheduleEntryFilterListDto> GetFilterDataAsync(RosterScheduleEntryFilterDto filter);
        Task<List<PAYMonthResultDto>> getAllMonthService();
        Task<List<RosterShiftDto>> getAlllShiftService();
        Task<(bool isSuccess, string isMessage , object data )> CreateAndUpdateService(RosterScheduleEntrySetupViewModel FromModel);
        Task<List<RosterScheduleEntrySetupViewModel>> GetRosterScheduleGridService();
        Task<RosterScheduleEntrySetupViewModel> EditGetServices(string id);
        //Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<byte[]> GenerateEmpRosterExcelDownload();
        Task<(bool isSuccess, string message, object data)> SaveRosterExcel(RosterScheduleEntrySetupViewModel model);
    }
}
