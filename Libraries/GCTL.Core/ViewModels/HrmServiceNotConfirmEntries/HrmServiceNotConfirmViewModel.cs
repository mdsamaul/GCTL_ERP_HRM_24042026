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
        public string? EmployeeName { get; set; }
        public string? Name { get; set; }
        public string? Id { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? DuePaymentDate { get; set; }
        public string? RefLetterNo { get; set; }
        public DateTime? RefLetterDate { get; set; }
        public string? Remarks { get; set; }
        public string? CompanyCode { get; set; }
        public string? UserEmployeeId { get; set; }
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
        public string? GrossSalary { get; set; }
        public string? JoiningDate { get; set; }
        public string? ProbationPeriod { get; set; }
        public string? EndOn { get; set; }
        public string? ServiceLength { get; set; }
    }
    public class ReportExportRequest
    {
        public ReportFilterViewModel FilterData { get; set; }
        public string ExportFormat { get; set; }
    }
    public class ReportFilterViewModel
    {
        public List<string>? CompanyCode { get; set; }
        public List<string> BranchCodes { get; set; }
        public List<string> EmployeeId { get; set; }
        public string? EffectiveDateFrom { get; set; }
        public string? EffectiveDateTo { get; set; }
        public string? DuePaymentDateFrom { get; set; }
        public string? DuePaymentDateTo { get; set; }
        public string? JoiningDateFrom { get; set; }
        public string? JoiningDateTo { get; set; }
    }
    public class ReportFilterListViewModel
    {
        public List<ReportFilterResultViewModel> Companies { get; set; }
        public List<ReportFilterResultViewModel> Branches { get; set; }
        public List<ReportFilterResultViewModel> EmployeeIds { get; set; }
        public List<ReportFilterResultViewModel> ServiceNotConfirms { get; set; }
    }

    public class ReportFilterResultViewModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? DepartmentName { get; set; }
        public string? SncId { get; set; }
        public string? EmployeeId { get; set; }
        public string? JoiningDate { get; set; }
        public string? EffectiveDate { get; set; }
        public string? DuePaymentDate { get; set; }
        public string? RefLetterNo { get; set; }
        public string? RefLetterDate { get; set; }
        public string? Remarks { get; set; }
    }
}
