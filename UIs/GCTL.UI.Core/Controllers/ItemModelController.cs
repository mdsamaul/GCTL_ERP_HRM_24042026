using GCTL.UI.Core.ViewModels.ItemModel;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class ItemModelController : BaseController
    {
        public IActionResult Index()
        {
            ItemModelViewModel model = new ItemModelViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
    }
}
