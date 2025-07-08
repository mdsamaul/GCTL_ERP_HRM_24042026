using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ItemMasterInformation;

namespace GCTL.UI.Core.ViewModels.ItemMasterInformation
{
    public class ItemMasterInformationViewModel : BaseViewModel
    {
      public  ItemMasterInformationSetupViewModel Setup { get; set; } = new ItemMasterInformationSetupViewModel();
    }
}
