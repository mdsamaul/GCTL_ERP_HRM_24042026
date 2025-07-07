using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustmentReport;
using GCTL.UI.Core.ViewModels.ItemMasterInformation;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class ItemMasterInformationController : BaseController
    {
        public IActionResult Index()
        {
            ItemMasterInformationViewModel model = new ItemMasterInformationViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }


    }
}
