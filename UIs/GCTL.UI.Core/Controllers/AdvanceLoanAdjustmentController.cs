using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustment;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class AdvanceLoanAdjustmentController : BaseController
    {
        public IActionResult Index()
        {
            AdvanceLoanAdjustmentViewModel model = new AdvanceLoanAdjustmentViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };

            return View(model);
        }
    }
}
