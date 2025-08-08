
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmHomeOfficeRequests;
using GCTL.Service.Common;
using GCTL.Service.HrmHomeOfficeRequests;
using GCTL.UI.Core.ViewModels.HrmHomeOffRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GCTL.UI.Core.Controllers
{
    public class HrmHomeOfficeRequestController : BaseController
    {
        private readonly IHrmHomeOfficeRequestService entryService;
        private readonly ICommonService commonService;

        public HrmHomeOfficeRequestController(IHrmHomeOfficeRequestService entryService, ICommonService commonService)
        {
            this.entryService = entryService;
            this.commonService = commonService;
        }

        string strMaxNO = string.Empty;
        public IActionResult Index()
        {
            HrmHomeOfficeRequestPageViewModel model = new HrmHomeOfficeRequestPageViewModel()
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

        public async Task<IActionResult> GetHod()
        {
            var data = await entryService.GetHodAsync();
            return Json(data);
        }

        public async Task<IActionResult> GetEmpData(string selectedEmpId)
        {
            var data = await entryService.GetDataByEmpId(selectedEmpId);
            return Json(data);
        }

        public async Task<IActionResult> GenerateNewId() 
        {
            commonService.FindMaxNo(ref strMaxNO, "HORID", "HRM_HomeOfficeRequest", 8);
            return Json(strMaxNO);
        }

        public async Task<IActionResult> GetById(decimal id)
        {
            var data = await entryService.GetByIdAsync(id);
            return Json(data);
        }

        public async Task<IActionResult> GetPaginatedEntires()
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
                recordsFiltered = result.filteredRecords,
                data = result.Data
            };

            return Ok(response);
        }

        public async Task<IActionResult> SaveEntry([FromBody] HrmHomeOfficeRequestSetupViewModel model)
        {
            if (model == null)
                return Json(new
                {
                    success = false,
                    message = "Saved Failed!"
                });

            bool duplicate = await entryService.HasDuplicate(model.EmployeeId, model.StartDate, model.EndDate, model.Horid);

            if (duplicate)
            {
                return Json(new
                {
                    success = false,
                    message = "Duplicate record found!"
                });
            }

            model.ToAudit(LoginInfo, model.Tc > 0);

            if (model.Tc == 0)
            {
                var result = await entryService.SaveAsync(model);
                return Json(new
                {
                    success = result,
                    message = "Saved Successfully"
                });
            }
            else
            {
                var result = await entryService.EditAsync(model);
                return Json(new
                {
                    success = result,
                    message = "Updated Successfully"
                });
            }

        }

        public async Task<IActionResult> BulkDelete([FromBody] HrmHomeOfficeRequestSetupViewModel model)
        {
            try
            {
                if (model.Tcs == null || !model.Tcs.Any() || model.Tcs.Count == 0)
                    return Json(new { isSuccess = false, message = "No data is selected" });
                var result = await entryService.BulkDeleteAsync(model.Tcs);

                if (!result)
                {
                    return Json(new { isSuccess = false, message = "No Data is found to delete" });
                }

                return Json(new { isSuccess = false, message = $"Deleted {model.Tcs.Count} Data Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
