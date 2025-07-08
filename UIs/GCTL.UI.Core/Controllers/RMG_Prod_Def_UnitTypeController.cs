using GCTL.UI.Core.ViewModels.RMG_Prod_Def_UnitType;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class RMG_Prod_Def_UnitTypeController : BaseController
    {
        public IActionResult Index()
        {
            RMG_Prod_Def_UnitTypeViewModel model = new RMG_Prod_Def_UnitTypeViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
    }
}
