using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmPayDefDeductionTypes
{
    public class DeductionTypeSetupViewModel:BaseViewModel
    {
        public decimal Tc { get; set; }
        public string DeductionTypeId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Deduction Type")]
        public string DeductionType { get; set; }

        [Display(Name = "Deduction Type Short Name")]
        public string? ShortName { get; set; }
        public string CompanyCode { get; set; }

    }
}
