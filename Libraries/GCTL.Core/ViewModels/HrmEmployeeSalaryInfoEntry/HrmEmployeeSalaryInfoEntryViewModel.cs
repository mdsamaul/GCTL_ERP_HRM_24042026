using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmEmployeeSalaryInfoEntry
{
    public class HrmEmployeeSalaryInfoEntryViewModel
    {
        public List<EmployeeListItemViewModel> SalaryInfoUpdate { get; set; }
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
        public List<string>? DepartmentCodes { get; set; }
        public List<string>? DesignationCodes { get; set; }
        public List<string>? EmployeeIDs { get; set; }
        public DateTime? JoiningDateFrom { get; set; }
        public DateTime? JoiningDateTO { get; set; }
        public List<string>? EmployeeStatuses { get; set; }
        public List<string>? EmployeeNatureCodes { get; set; }
        public List<string>? EmployeeTypes { get; set; }
    }

    public class EmployeeListItemViewModel:BaseViewModel
    {
        public string? AutoId { get; set; }
        public string? EmployeeId { get; set; }
        public string? PayId { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? EmployeeTypeName { get; set; }
        public string? EmploymentNature { get; set; }
        public string? JoiningDate { get; set; }
        public string? SeparationDate { get; set; }
        public string? EmployeeStatus { get; set; }
        public string? LastIncDate { get; set; }
        public decimal? GrossSalary { get; set; }
        public string? DisbursementMethodId { get; set; }
        public string? DisbursementMethodName { get; set; }
        //public string? BranchName { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
    }

}
