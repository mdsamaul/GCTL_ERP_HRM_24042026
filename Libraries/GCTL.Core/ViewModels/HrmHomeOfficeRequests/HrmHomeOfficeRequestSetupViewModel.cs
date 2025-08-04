using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmHomeOfficeRequests
{
    public class HrmHomeOfficeRequestSetupViewModel:BaseViewModel
    {
        public decimal Tc { get; set; }
        public List<decimal>? Tcs { get; set; }
        public string Horid { get; set; }
        public string EmployeeId { get; set; }
        public string HremployeeId { get; set; }
        public string HremployeeName { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDatetime { get; set; }
        public string Remarks { get; set; }
        public string CompanyCode { get; set; }
        public string EntryBy { get; set; }
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
        public string CompanyCode { get; set; }
        public string EmployeeID { get; set; }
    }

    public class EmployeeListItemViewModel
    {
        public string? EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? JoiningDate { get; set; }
    }


}
