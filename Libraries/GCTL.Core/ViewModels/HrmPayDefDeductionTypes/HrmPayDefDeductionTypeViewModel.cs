using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmPayDefDeductionTypes
{
    public class HrmPayDefDeductionTypeViewModel :BaseViewModel
    {
        public decimal Tc { get; set; }
        public string DeductionTypeId { get; set; }
        public string DeductionType { get; set; }
        public string ShortName { get; set; }
        public string CompanyCode { get; set; }
    }
}
