using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;

namespace GCTL.UI.Core.ViewModels.HrmServiceNotConfirmationEntry
{
    public class HrmServiceNotConfirmationPageViewModel: BaseViewModel
    {
        public HrmServiceNotConfirmViewModel Setup { get; set; } = new HrmServiceNotConfirmViewModel();
    }
}
