using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using GCTL.Service.HrmServiceNotConfirmationEntries;
using GCTL.UI.Core.ViewModels.HrmServiceNotConfirmationEntry;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GCTL.UI.Core.Controllers
{
    public class HrmServiceNotConfirmationEntryController : BaseController
    {
        private readonly IHrmServiceNotConfirmationEntryService entryService;
        
        public HrmServiceNotConfirmationEntryController(IHrmServiceNotConfirmationEntryService entryService)
        {
            this.entryService = entryService;
        }

        public IActionResult Index()
        {
            HrmServiceNotConfirmationPageViewModel model = new HrmServiceNotConfirmationPageViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
                AddUrl = Url.Action(nameof(Setup))
            };

            return View(model);
        }

        public IActionResult Setup()
        {
            return View();
        }

        public async Task<IActionResult> getFilterEmp([FromBody] EmployeeFilterViewModel model)
        {
            var data = await entryService.GetFilterEmployeeAsync(model);
            return Json(data);
        }

        public async Task<IActionResult> getMonthDD()
        {
            var data = await entryService.GetPayMonthsAsync();
            return Json(data);
        }

        public async Task<IActionResult> GenerateNewId()
        {
            var newId = await entryService.GenerateSNCIdAsync();
            return Json(newId);
        }

        public async Task<IActionResult> GetById(decimal id)
        {
            var result = await entryService.GetByIdAsync(id);
            return Json(new { data = result });
        }

        public async Task<IActionResult> GetPaginatedEntries()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            var page = string.IsNullOrEmpty(start) ? 1 : (Convert.ToInt32(start) / pageSize) + 1;

            var result = await entryService.GetPaginatedDataAsync(searchValue, page, pageSize, sortColumn, sortDirection);

            var response = new
            {
                draw = draw,
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.TotalRecords,
                data = result.Data
            };

            return Ok(response);
        }

        public async Task<IActionResult> SaveEntry([FromBody] HrmServiceNotConfirmViewModel model)
        {
            model.ToAudit(LoginInfo, model.Tc>0);
            var result = await entryService.SaveAsync(model);

            string message = model.Tc>0 ? "Updated Successfully" : "Saved Successfully";
            return Json(new
             {
                 success = result,
                 message = message
             });
        }

        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await entryService.GenerateExcelSampleAsync();
            string excelName = "ServiceNotConfirmationEntry.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

    }
}
