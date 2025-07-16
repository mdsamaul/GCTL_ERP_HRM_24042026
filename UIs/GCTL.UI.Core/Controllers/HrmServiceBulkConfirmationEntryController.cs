using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmServiceBulkConfimationEntry;
using GCTL.Service.HrmServiceBulkConfirmationEntries;
using GCTL.UI.Core.ViewModels.HrmServiceBulkConfirm;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmServiceBulkConfirmationEntryController : BaseController
    {
        private readonly IHrmServiceBulkConfirmationEntryService entryService;

        public HrmServiceBulkConfirmationEntryController(IHrmServiceBulkConfirmationEntryService entryService)
        {
            this.entryService = entryService;
        }

        public IActionResult Index()
        {
            HrmServiceBulkConfirmPageViewModel model = new HrmServiceBulkConfirmPageViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> getFilterEmp([FromBody] EmployeeFilterViewModel model) 
        {
            var result = await entryService.GetFilterEmployeeAsync(model);
            return Json(result);
        }

        public async Task<IActionResult> Save([FromBody] HrmServiceBulkConfirmationViewModel model)
        {
            if (model == null) return NotFound();

            model.confirmInfo.ForEach(x => x.ToAudit(LoginInfo, true));
            var result = await entryService.BulkEditAsync(model);
            return Json(new
            {
                success = result
            });
        }
    }
}
