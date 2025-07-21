using GCTL.Core.ViewModels.PrintingStationeryPurchaseReport;
using GCTL.Service.PrintingStationeryPurchaseReportService;
using GCTL.UI.Core.ViewModels.PrintingStationeryPurchaseReport;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class PrintingStationeryPurchaseReportController : BaseController
    {
        private readonly IPrintingStationeryPurchaseReportService printingStationeryPurchaseReportService;

        public PrintingStationeryPurchaseReportController(
            IPrintingStationeryPurchaseReportService printingStationeryPurchaseReportService
            )
        {
            this.printingStationeryPurchaseReportService = printingStationeryPurchaseReportService;
        }
        public IActionResult Index()
        {
            PrintingStationeryPurchaseReportViewModel model = new PrintingStationeryPurchaseReportViewModel
            {
               PageUrl=Url.Action(nameof(Index))
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CategoryLoadList([FromBody] PrintingStationeryPurchaseReportFilterDto model)
        {
            var result = await printingStationeryPurchaseReportService.GetAllPROCPrintingAndStationeryReport(model);
            return Json(new { data = result});
        }
        [HttpPost]
        public async Task<IActionResult> GetFilteredDropdowns([FromBody] PrintingStationeryPurchaseReportFilterDto filter)
        {
            var result = await printingStationeryPurchaseReportService.GetFilteredDropdownsAsync(filter);
            return Ok(result);
        }

    }
}
