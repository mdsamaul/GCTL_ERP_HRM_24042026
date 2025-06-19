using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;

namespace GCTL.UI.Core.ViewModels.HrmPayDeductionTypes
{
    public class HrmPayDeductionTypePageViewModel:BaseViewModel
    {
        public DeductionTypeSetupViewModel Setup { get; set; } = new DeductionTypeSetupViewModel();
    }
}
