using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;
using GCTL.Data.Models;

namespace GCTL.UI.Core.ViewModels.HrmPayOthersAdjustments
{
    public class HrmPayOthersAdjustmentPageViewModel:BaseViewModel
    {
        public HrmPayOthersAdjustmentEntryViewModel Setup { get; set; } = new HrmPayOthersAdjustmentEntryViewModel();
        public List<HrmPayOthersAdjustmentEntryViewModel> List { get; set; } = new List<HrmPayOthersAdjustmentEntryViewModel>();

    }
}
