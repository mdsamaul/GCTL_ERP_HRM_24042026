using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmServiceNotConfirmEntries
{
    public class HrmServiceNotConfirmViewModel:BaseViewModel
    {
        public decimal Tc { get; set; }
        public List<decimal>? Tcs { get; set; }
        public string Sncid { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? DuePaymentDate { get; set; }
        public string RefLetterNo { get; set; }
        public DateTime? RefLetterDate { get; set; }
        public string Remarks { get; set; }
        public string CompanyCode { get; set; }
        public string UserEmployeeId { get; set; }
    }

    public class LookupItemDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class EmployeeFilterResultDto
    {
        public Dictionary<string, List<LookupItemDto>> LookupData { get; set; } = new();
        //public List<EmployeeListItemViewModel> Employees { get; set; } = new();
    }

    public class EmployeeFilterViewModel
    {
        public List<string>? EmployeeIDs { get; set; }
    }
}
