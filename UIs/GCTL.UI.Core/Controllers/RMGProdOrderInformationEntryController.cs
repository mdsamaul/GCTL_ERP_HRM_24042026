using GCTL.UI.Core.ViewModels.SalesDefInvMainItem;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class RMGProdOrderInformationEntryController : Controller
    {
        public IActionResult Index()
        {
            SalesDefInvMainItemDto model = new SalesDefInvMainItemDto()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }
    }
}
