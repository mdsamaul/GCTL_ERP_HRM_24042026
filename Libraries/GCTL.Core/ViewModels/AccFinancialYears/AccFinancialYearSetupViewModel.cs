using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AccFinancialYears
{
    public class AccFinancialYearSetupViewModel : BaseViewModel
    {
        public decimal Tc { get; set; }
        public string FinancialCodeNo { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
