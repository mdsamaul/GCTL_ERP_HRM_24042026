using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdvanceLoanAdjustment
{
    public class AdvancePayViewModel
    {       
            public int AdvancePayId { get; set; }
            public int? EmployeeID { get; set; }
            //public string? EmployeeID { get; set; }
            public string FullName { get; set; }
            public string DepartmentName { get; set; }
            public string DesignationName { get; set; }
            public decimal AdvanceAmount { get; set; }
            public decimal? MonthlyDeduction { get; set; }
            public string SalaryMonth { get; set; }
            public string SalaryYear { get; set; }
            public int? NoOfPaymentInstallment { get; set; }
            public int? PayHeadNameId { get; set; }
            public string Remarks { get; set; }
            public int? LoanID { get; set; }        
            //public string? LoanID { get; set; }        
    }

}
