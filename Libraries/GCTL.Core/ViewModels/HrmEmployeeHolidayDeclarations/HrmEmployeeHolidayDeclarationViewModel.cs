using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations
{
    public class HrmEmployeeHolidayDeclarationViewModel : BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string Ehdid { get; set; } = null!;
        public string? EmployeeId { get; set; }
        public List<string>? EmployeeIds { get; set; }
        public string? EmployeeName { get; set; }
        public string? Designation { get; set; }
        public string? HolidayDecType { get; set; }
        public string? EntryUser { get; set; }
        public DateTime Date { get; set; }
        public DateTime? InPlaceofDate { get; set; }
        public string? IsDayoffDuty { get; set; }
        public string? Remark { get; set; } = null!;
        public string? CompanyCode { get; set; } = null!;

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
}
