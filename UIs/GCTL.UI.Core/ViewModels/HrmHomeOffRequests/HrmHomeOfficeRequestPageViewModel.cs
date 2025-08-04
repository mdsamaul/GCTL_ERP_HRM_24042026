using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmHomeOfficeRequests;

namespace GCTL.UI.Core.ViewModels.HrmHomeOffRequests
{
    public class HrmHomeOfficeRequestPageViewModel : BaseViewModel
    {
        public HrmHomeOfficeRequestSetupViewModel Setup { get; set; } = new HrmHomeOfficeRequestSetupViewModel();
    }
}
