using GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmServiceBulkConfimationEntry
{
    public class HrmServiceBulkConfirmationViewModel
    {
        public List<EmployeeListItemViewModel> confirmInfo { get; set; }
    }

    public class LookupItemDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class EmployeeFilterResultViewModel
    {
        public Dictionary<string, List<LookupItemDto>> LookupData { get; set; } = new();
        public List<EmployeeListItemViewModel> Employees { get; set; } = new();
    }

    public class EmployeeFilterViewModel
    {
        public List<string>? CompanyCodes { get; set; }
        public List<string>? BranchCodes { get; set; }
        public List<string>? DepartmentCodes { get; set; }
        public List<string>? DesignationCodes { get; set; }
        public List<string>? EmployeeIDs { get; set; }
        public List<string>? EmployeeStatuses { get; set; }
        public string? ProbationEndDays { get; set; }
    }

    public class EmployeeListItemViewModel : BaseViewModel
    {
        public string? AutoId { get; set; }
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? ConfirmeDate { get; set; }
        public string? DesignationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? GrossSalary { get; set; }
        public string? JoiningDate { get; set; }
        public string? ProbationPeriod { get; set; }
        public string? ProbationPeriodEndOn { get; set; }
        public string? ExtenPeriod { get; set; }
        public string? ExtenPeriodEndOn { get; set; }
        public string? ServiceLength { get; set; }
        public string? RefLetterNo { get; set; }
        public string? CompanyCode { get; set; }
    }

    public class ReportExportRequest
    {
        public ReportFilterViewModel FilterData { get; set; }
        public string ExportFormat { get; set; }
    }
    public class ReportFilterViewModel
    {
        public List<string> CompanyCodes { get; set; }
        public List<string> BranchCodes { get; set; }
        public List<string> DepartmentCodes { get; set; }
        public List<string> DesignationCodes { get; set; }
        public List<string> EmployeeIDs { get; set; }
        public List<string> EmpTypes { get; set; }
        public string? ProbationEndDays { get; set; }
    }

    public class ReportFilterListViewModel
    {
        public List<ReportFilterResultViewModel> Companies { get; set; }
        public List<ReportFilterResultViewModel> Branches { get; set; }
        public List<ReportFilterResultViewModel> Departments { get; set; }
        public List<ReportFilterResultViewModel> Designations { get; set; }
        public List<ReportFilterResultViewModel> Employees { get; set; }
        public List<ReportFilterResultViewModel> EmployeeIds { get; set; }
        public List<ReportFilterResultViewModel> EmpTypes { get; set; }
    }

    public class ReportFilterResultViewModel 
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? GrossSalary { get; set; }
        public string? JoiningDate { get; set; }
        public string? ProbationPeriod { get; set; }
        public string? ProbationPeriodEndOn { get; set; }
        public string? ExtenPeriod { get; set; }
        public string? ExtenPeriodEndOn { get; set; }
        public string? ServiceLength { get; set; }
        public string? RefLetterNo { get; set; }

        public string? CompanyName { get; set; }
    }
}
