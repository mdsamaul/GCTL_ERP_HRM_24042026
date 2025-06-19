using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmDefHolidayDeclarationTypes
{
    public class HrmDefHolidayDeclarationTypeViewModel:BaseViewModel
    {
        public decimal AutoId { get; set; }

        public string Hdtcode { get; set; } = null!;

        public string? Name { get; set; }

        public string CompanyCode { get; set; } = null!;

        public string EmployeeId { get; set; } = null!;
    }
}
