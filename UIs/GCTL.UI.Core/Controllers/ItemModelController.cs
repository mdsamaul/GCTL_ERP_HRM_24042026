using GCTL.UI.Core.ViewModels.ItemModel;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class ItemModelController : BaseController
    {
        public IActionResult Index(bool isPartial)
        {
            ItemModelViewModel model = new ItemModelViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            if (isPartial)
            {
                return PartialView(model);
            }
            return View(model);
        }
    }
}
