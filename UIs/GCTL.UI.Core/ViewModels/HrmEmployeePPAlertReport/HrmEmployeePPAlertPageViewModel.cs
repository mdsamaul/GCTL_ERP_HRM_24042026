using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmEmployeePPAlertReports;

namespace GCTL.UI.Core.ViewModels.HrmEmployeePPAlertReport
{
    public class HrmEmployeePPAlertPageViewModel:BaseViewModel
    {
        public HrmEmployeePPAlertReportViewModel Setup { get; set; } = new HrmEmployeePPAlertReportViewModel();
    }
}
