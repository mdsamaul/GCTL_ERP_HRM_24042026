using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.PFAssignEntry;

namespace GCTL.Service.PFAssignEntryReport
{
    public interface IPFAssignEntryReportServices
    {
        Task<PFAssignEntryFilterListDto> GetRosterDataAsync(PFAssignEntryFilterDto FilterData);
        Task<PFAssignEntryFilterListDto> GetRosterDataPdfAsync(PFAssignEntryFilterDto FilterData);
    }
}
