using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.RosterScheduleApproval;
using GCTL.Core.ViewModels.RosterScheduleEntry;
using GCTL.Core.ViewModels.RosterScheduleReport;

namespace GCTL.Service.RosterScheduleReport
{
    public interface IRosterScheduleReportServices
    {
        Task<RosterReportFilterListDto> GetRosterDataAsync(RosterReportFilterDto filter);
    }
}
