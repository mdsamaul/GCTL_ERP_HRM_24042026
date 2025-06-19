namespace GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries
{
    public class HrmPaySalaryDeductionSetupViewModel:BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string DeductionTypeId { get; set; }
        public decimal? DeductionAmount { get; set; }
        public string SalaryMonth { get; set; }
        public string SalaryYear { get; set; }
        public string Remarks { get; set; }
    }
}
