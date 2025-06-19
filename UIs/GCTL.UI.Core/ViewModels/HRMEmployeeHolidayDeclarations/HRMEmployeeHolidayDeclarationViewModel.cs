using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations;

namespace GCTL.UI.Core.ViewModels.HRMEmployeeHolidayDeclarations
{
    public class HRMEmployeeHolidayDeclarationViewModel : BaseViewModel
    {
        public HRMEmployeeHolidayDeclarationSetupViewModel Setup { get; set; } = new HRMEmployeeHolidayDeclarationSetupViewModel();

        public List<HRMEmployeeHolidayDeclarationSetupViewModel> HRMEmployeeHolidayList { get; set; } = new List<HRMEmployeeHolidayDeclarationSetupViewModel>();
    }
}
