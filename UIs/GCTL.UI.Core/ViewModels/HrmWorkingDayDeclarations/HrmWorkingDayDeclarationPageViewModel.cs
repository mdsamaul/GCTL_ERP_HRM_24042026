using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;

namespace GCTL.UI.Core.ViewModels.HrmWorkingDayDeclarations
{
    public class HrmWorkingDayDeclarationPageViewModel:BaseViewModel
    {
        public HrmWorkingDayDeclarationViewModel Setup { get; set; } = new HrmWorkingDayDeclarationViewModel();
        public List<HrmWorkingDayDeclarationViewModel> TableListData { get; set; } = new List<HrmWorkingDayDeclarationViewModel>();   
    }
}
