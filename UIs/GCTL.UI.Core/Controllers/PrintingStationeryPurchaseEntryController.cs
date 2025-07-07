using GCTL.UI.Core.ViewModels.PrintingStationeryPurchaseEntry;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class PrintingStationeryPurchaseEntryController : BaseController
    {
        public IActionResult Index()
        {
            PrintingStationeryPurchaseEntryViewModel model = new PrintingStationeryPurchaseEntryViewModel() { 
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
    }
}
