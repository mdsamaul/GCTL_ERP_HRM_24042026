using GCTL.UI.Core.ViewModels.HRM_Size;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HRM_SizeController : BaseController
    {
        public IActionResult Index()
        {
            HRM_SizeViewModel model = new HRM_SizeViewModel() {
             PageUrl= Url.Action(nameof(Index)),
            };
            return View(model);
        }
    }
}
