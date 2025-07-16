using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmServiceBulkConfimationEntry;

namespace GCTL.UI.Core.ViewModels.HrmServiceBulkConfirm
{
    public class HrmServiceBulkConfirmPageViewModel:BaseViewModel
    {
        public HrmServiceBulkConfirmationViewModel Setup { get; set; } = new HrmServiceBulkConfirmationViewModel();
    }
}
