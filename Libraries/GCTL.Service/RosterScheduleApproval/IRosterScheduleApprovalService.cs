using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.RosterScheduleApproval;
using GCTL.Core.ViewModels.RosterScheduleEntry;
using GCTL.UI.Core.Views.RosterScheduleApproval;

namespace GCTL.Service.RosterScheduleApproval
{
    public interface IRosterScheduleApprovalService
    {

        Task<RosterFilterListDto> GetRosterDataAsync(RosterFilterDto filter);
        Task<(bool isSuccess, string isMessage)> ApprovalRosterServices(ApprovalRequest modelData);
        Task<List<RosterScheduleEntrySetupViewModel>> GetRosterScheduleGridService();

    }
}
