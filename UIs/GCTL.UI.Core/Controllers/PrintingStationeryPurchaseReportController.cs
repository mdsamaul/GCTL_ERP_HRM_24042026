using GCTL.UI.Core.ViewModels.PrintingStationeryPurchaseReport;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class PrintingStationeryPurchaseReportController : BaseController
    {
        public IActionResult Index()
        {
            PrintingStationeryPurchaseReportViewModel model = new PrintingStationeryPurchaseReportViewModel
            {
               PageUrl=Url.Action(nameof(Index))
            };
            return View(model);
        }
    }
}
