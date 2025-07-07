using GCTL.Core.ViewModels;

namespace GCTL.UI.Core.ViewModels.ItemMasterInformation
{
    public class ItemMasterInformationViewModel : BaseViewModel
    {
      public  ItemMasterInformationSetupViewModel Setup { get; set; } = new ItemMasterInformationSetupViewModel();
    }
}
