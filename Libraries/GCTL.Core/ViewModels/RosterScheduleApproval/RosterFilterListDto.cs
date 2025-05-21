using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.PFAssignEntry;

namespace GCTL.Core.ViewModels.RosterScheduleApproval
{
    public class RosterFilterListDto
    {
        public List<RosterFilterResultDto> Companies { get; set; }
        public List<RosterFilterResultDto> Branches { get; set; }
        public List<RosterFilterResultDto> Divisions { get; set; }
        public List<RosterFilterResultDto> Departments { get; set; }
        public List<RosterFilterResultDto> Designations { get; set; }
        public List<RosterFilterResultDto> Employees { get; set; }
        public List<RosterFilterResultDto> ActivityStatuses { get; set; }
        public DateTime Date { get; set; }
        public string ShiftName { get; set; }
    }
}
