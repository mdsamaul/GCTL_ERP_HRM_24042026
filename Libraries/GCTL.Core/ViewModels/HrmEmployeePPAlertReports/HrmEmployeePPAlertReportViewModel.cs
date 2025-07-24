using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmEmployeePPAlertReports
{
    public class HrmEmployeePPAlertReportViewModel : BaseViewModel
    {
        public string? Name { get; set; }
        public string? DesignationCode { get; set; }
        public string? CompanyName { get; set; }
        public string? DesingationName { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? BranchName { get; set; }

        public string? GrossSalary { get; set; }
        public string? JoiningDate { get; set; }
        public string? ProbationPeriod { get; set; }
        public string? ProbationPeriodEndOn { get; set; }
        public string? ServiceLength { get; set; }
        public string? CompanyCode { get; set; }
    }
    public class ReportExportRequest
    {
        public EmployeeFilterViewModel FilterData { get; set; }
        public string ExportFormat { get; set; }
    }
    public class EmployeeFilterViewModel 
    {
        public List<string>? CompanyCodes { get; set; }
        //public List<string>? BranchCodes { get; set; }
        public List<string>? DepartmentCodes { get; set; }
        public List<string>? DesignationCodes { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public string? ProbationEndDays { get; set; }
    }

    public class LookupItemDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class  EmployeeFilterResultViewModel
    {
        public Dictionary<string, List<LookupItemDto>> LookupData { get; set; } = new();
        public List<HrmEmployeePPAlertReportViewModel> Employees { get; set; } = new();
    }
}
