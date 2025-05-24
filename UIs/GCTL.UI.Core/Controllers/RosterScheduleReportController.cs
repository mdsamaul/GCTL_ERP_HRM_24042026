using System.Security.Policy;
using GCTL.Core.ViewModels.RosterScheduleReport;
using GCTL.Service.RosterScheduleEntry;
using GCTL.Service.RosterScheduleReport;
using GCTL.UI.Core.ViewModels.RosterScheduleEntry;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class RosterScheduleReportController : BaseController
    {
        private readonly IRosterScheduleReportServices rosterScheduleReportServices;

        public RosterScheduleReportController(IRosterScheduleReportServices rosterScheduleReportServices)
        {
            this.rosterScheduleReportServices = rosterScheduleReportServices;
        }
        public IActionResult Index()
        {
            RosterScheduleEntryViewModel model = new RosterScheduleEntryViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> getAllFilterEmp([FromBody] RosterReportFilterDto filterDto)
        {
            var result = await rosterScheduleReportServices.GetRosterDataAsync(filterDto);
            if (result != null)
            {
                return Json(new { isSuccess = true, message = "successed data load", data = result });
            }
            return Json(new { isSuccess = false, message = "Data load Failed" });
        }
    }
}
