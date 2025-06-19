using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations
{
    public class HRMEmployeeHolidayDeclarationSetupViewModel : BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string Ehdid { get; set; } = null!;
        public string EmployeeId { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Remark { get; set; } = null!;
        public string CompanyCode { get; set; } = null!;
    }
}
