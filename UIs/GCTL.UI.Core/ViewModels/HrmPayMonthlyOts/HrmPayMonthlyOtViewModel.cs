using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;


namespace GCTL.UI.Core.ViewModels.HrmPayMonthlyOts
{
    public class HrmPayMonthlyOtViewModel:BaseViewModel
    {
        public HrmPayMonthlyOtEntryViewModel Setup {  get; set; } = new HrmPayMonthlyOtEntryViewModel();
        //public List<HrmPayMonthlyOtViewModel>
    }
}
