using GCTL.UI.Core.ViewModels.Brand;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class BrandController : BaseController
    {
        public IActionResult Index()
        {
            BrandViewModel model = new BrandViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
    }
}
