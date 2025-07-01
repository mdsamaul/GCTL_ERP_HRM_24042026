using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.EmployeeLoanInformationReport
{
    public class EmployeeLoanReportResponseVM
    {
        public List<EmployeeLoanInformationReportVM> LoanReports { get; set; }
        public List<CompanyBasicInfoVM> Companies { get; set; }
        public List<EmployeeBasicInfoVM> Employees { get; set; }
        public List<string> LoanIDs { get; set; }
    }

    public class CompanyBasicInfoVM
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
    }

}
