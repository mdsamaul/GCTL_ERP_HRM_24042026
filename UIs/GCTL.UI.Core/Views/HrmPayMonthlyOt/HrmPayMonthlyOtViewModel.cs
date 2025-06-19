using GCTL.Core.ViewModels;

namespace GCTL.UI.Core.Views.HrmPayMonthlyOt
{
    public class HrmPayMonthlyOtViewModel:BaseViewModel
    {
        public HrmPayMonthlyOtViewModel Setup {  get; set; } = new HrmPayMonthlyOtViewModel();
        public List<HrmPayMonthlyOtViewModel> otList { get; set; } = new List<HrmPayMonthlyOtViewModel>();
    }
}
