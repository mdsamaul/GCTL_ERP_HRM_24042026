using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;

namespace GCTL.UI.Core.ViewModels.HrmPaySalaryDeductions
{
    public class HrmPaySalaryDeductionViewModel : BaseViewModel
    {
        public HrmPaySalaryDeductionSetupViewModel Setup {  get; set; } = new HrmPaySalaryDeductionSetupViewModel();

        public List<HrmPaySalaryDeductionSetupViewModel> salaryDeductionList { get; set; } = new List<HrmPaySalaryDeductionSetupViewModel>();
    }
}
