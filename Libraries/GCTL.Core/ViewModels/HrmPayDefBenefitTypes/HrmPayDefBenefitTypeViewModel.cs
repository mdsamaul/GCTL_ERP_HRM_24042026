using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmPayDefBenefitTypes
{
    public class HrmPayDefBenefitTypeViewModel:BaseViewModel
    {
            public decimal Tc { get; set; }
            public string BenefitTypeId { get; set; }
            public string BenefitType { get; set; }
            public string ShortName { get; set; }
            public string CompanyCode { get; set; }
        
    }
}
