using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries
{
    public class HrmPayOthersAdjustmentEntryViewModel:BaseViewModel
    {
        public decimal Tc { get; set; }
        public string OtherBenefitId { get; set; }
        public string? EmployeeId { get; set; }
        public decimal? BenefitAmount { get; set; }
        public string SalaryMonth { get; set; }
        public string SalaryYear { get; set; }
        public string Remarks { get; set; }
        public string CompanyCode { get; set; }
        public string BenefitTypeId { get; set; }
        public decimal? PaidDays { get; set; }


        public List<decimal>? Tcs { get; set; }
        public List<string>? EmployeeIds { get; set; }
        public string? EmployeeName { get; set; }
        public string? Designation {  get; set; }
        public string? Department {  get; set; }
        public string? BenefitType { get; set; }
    }


    public class LookupItemDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class EmployeeFilterResultDto
    {
        public Dictionary<string, List<LookupItemDto>> LookupData { get; set; } = new();
        public List<EmployeeListItemViewModel> Employees { get; set; } = new();
    }

    public class EmployeeFilterViewModel
    {
        public List<string>? CompanyCodes { get; set; }
        public List<string>? BranchCodes { get; set; }
        public List<string>? DivisionCodes { get; set; }
        public List<string>? DepartmentCodes { get; set; }
        public List<string>? DesignationCodes { get; set; }
        public List<string>? EmployeeIDs { get; set; }
        public List<string>? EmployeeStatuses { get; set; }

        public List<string>? MonthIDs { get; set; }
        public List<string>? BenefitTypeIDs{ get; set; }
        public string? SalaryYear { get; set; }
    }

    public class EmployeeListItemViewModel
    {
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? JoiningDate { get; set; }
        public string? DesignationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? BranchName { get; set; }
        public string? CompanyName { get; set; }
        public string? EmployeeTypeName { get; set; }
        public string? EmployeeStatus { get; set; }
        public string? EmploymentNature { get; set; }
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
        public List<string> BenefitTypeIDs { get; set; }
        public List<string> MonthIDs { get; set; }
        public string SalaryYear { get; set; } = DateTime.Now.Year.ToString();
    }

    public class ReportFilterListViewModel
    {
        public List<ReportFilterResultDto> Companies { get; set; }
        public List<ReportFilterResultDto> Branches { get; set; }
        public List<ReportFilterResultDto> Departments { get; set; }
        public List<ReportFilterResultDto> Designations { get; set; }
        public List<ReportFilterResultDto> Employees { get; set; }
        public List<ReportFilterResultDto> BenefitTypes { get; set; }
        public List<ReportFilterResultDto> Months { get; set; }
        public List<ReportFilterResultDto> OtherBenefits { get; set; }
    }
    public class ReportFilterResultDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string CompanyName { get; set; }
        public string EmpId { get; set; }
        public string DesignationName { get; set; }
        public string BranchName { get; set; }
        //public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        //public string EmployeeStatus { get; set; }
        public string OtherBenefitId { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string BenefitType { get; set; }
        public decimal? BenefitAmount { get; set; }
        public string EmployeeID { get; set; }
        public decimal? PaidDays { get; set; }
        public string Remarks { get; set; }
    }
}
