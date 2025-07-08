using GCTL.UI.Core.ViewModels.Category;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class CategoryController : BaseController
    {
        public IActionResult Index()
        {
            CategoryViewModel model =  new CategoryViewModel()
            {
                PageUrl=Url.Action(nameof(Index))
            };
            return View(model);
        }
    }
}
