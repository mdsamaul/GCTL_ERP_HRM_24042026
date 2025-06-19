using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;

namespace GCTL.UI.Core.ViewModels.HrmPayDefBenefitTypes
{
    public class HrmPayDefBenefitTypePagesViewModel:BaseViewModel
    {
        public HrmPayDefBenefitTypeViewModel Setup { get; set; }=new HrmPayDefBenefitTypeViewModel();
        public List<HrmPayDefBenefitTypeViewModel> BenefitTypeList { get; set; } = new List<HrmPayDefBenefitTypeViewModel>();
    }
}
