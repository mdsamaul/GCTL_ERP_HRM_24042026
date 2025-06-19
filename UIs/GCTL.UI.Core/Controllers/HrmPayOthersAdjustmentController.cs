using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;
using GCTL.Service.HrmPayOthersAdjustmentEntries;
using GCTL.UI.Core.ViewModels.HrmPayOthersAdjustments;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPayOthersAdjustmentController : BaseController
    {
        private readonly IHrmPayOthersAdjustmentEntryService entryService;

        public HrmPayOthersAdjustmentController(IHrmPayOthersAdjustmentEntryService entryService)
        {
            this.entryService = entryService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HrmPayOthersAdjustmentPageViewModel
            {
                PageUrl = Url.Action(nameof(Index)),
            };

            //HrmPayOthersAdjustmentPageViewModel model = new HrmPayOthersAdjustmentPageViewModel()
            //{
            //    PageUrl=Url.Action(nameof(Index)),
            //    AddUrl = Url.Action(nameof(Setup))
            //};

            try
            {
                //var list = await entryService.GetPaginatedDataAsync();
                model.Setup = new HrmPayOthersAdjustmentEntryViewModel();
            }
            catch (Exception)
            {

                model.Setup = new HrmPayOthersAdjustmentEntryViewModel();
            }

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
        public async Task<IActionResult> getBenefitTypeDD()
        {
            var data = await entryService.GetBenefitTypeAsync();
            return Json(data);
        }

        public async Task<IActionResult> GenerateNewId()
        {
            var newId = await entryService.GetOthersAdjustmentIdAsync();
            return Json(newId);
        }

        public async Task<IActionResult> GetById(decimal id)
        {
            var result = await entryService.GetByIdAsync(id);

            return Json(new { data = result });
        }
        public async Task<IActionResult> GetPaginatedOtherBenefit()
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


        public async Task<IActionResult> SaveOthersBenefit([FromBody] HrmPayOthersAdjustmentEntryViewModel model)
        {
            if (model.Tc == 0)
            {
                model.ToAudit(LoginInfo);
                var result = await entryService.SaveAsync(model);
                return Json(new
                {
                    success = result,
                    message = "Saved Successfully"
                });
            }
            else
            {
                model.ToAudit(LoginInfo, true);
                var result = await entryService.EditAsync(model);
                return Json(new
                {
                    success = result,
                    message = "Updated Successfully"
                });
            }

        }

        public async Task<IActionResult> BulkDelete([FromBody] HrmPayOthersAdjustmentEntryViewModel model)
        {
            try
            {
                if (model.Tcs == null || !model.Tcs.Any() || model.Tcs.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No benefit entry is selected to delete" });
                }

                var result = await entryService.BulkDeleteAsync(model.Tcs);

                if (!result)
                {
                    return Json(new { isSuccess = false, message = "No benefit entry found to delete" });
                }
                return Json(new { isSuccess = true, message = $"Deleted {model.Tcs.Count} BenefitEnrty Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await entryService.GenerateExcelSampleAsync();
            string excelName = "OthersBenefitEntry.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [Route("/HrmPayOthersAdjustment/UploadExcelAsync")]
        public async Task<IActionResult> UploadExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "File must be an Excel (.xlsx) file" });
            }

            HrmPayOthersAdjustmentEntryViewModel model = new HrmPayOthersAdjustmentEntryViewModel();
            model.ToAudit(LoginInfo);

            try
            {
                //string userName = model.Luser;
                //string userIp = model.Lip;
                //string userMac = model.Lmac;
                model.CompanyCode = Request.Form["CompanyCode"];
                using (var stream = file.OpenReadStream())
                {
                    var result = await entryService.ProcessExcleFileAsync(stream, model);

                    if (result)
                    {
                        return Ok(new { success = true, message = "Data inserted Successfully" });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Data insert Failed" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error processing Excel file", error = ex.Message });
            }
        }

    }
}
