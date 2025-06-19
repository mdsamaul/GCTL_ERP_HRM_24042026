//using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using AutoMapper;
using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;
using GCTL.Service.Common;
using GCTL.Service.HRM_PAY_SalaryDeductionEntry;
using GCTL.UI.Core.ViewModels.HrmPaySalaryDeductions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPaySalaryDeductionController : BaseController
    {
        private readonly IHrmPaySalaryDeductionEntryService entryService;
        private readonly ICommonService commonService;
        private readonly ICompositeViewEngine viewEngine;
        private readonly IMapper mapper;

        public HrmPaySalaryDeductionController(IHrmPaySalaryDeductionEntryService entryService,
                                               ICommonService commonService,
                                               IMapper mapper,
                                               ICompositeViewEngine viewEngine)
        {
            this.entryService = entryService;
            this.commonService = commonService;
            this.mapper = mapper;
            this.viewEngine = viewEngine;
        }

        public async Task<IActionResult> Index()
        {
            HrmPaySalaryDeductionViewModel model = new HrmPaySalaryDeductionViewModel()
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

        public async Task<IActionResult> getDeductionTypeDD()
        {
            var data = await entryService.GetDeductionTypeAsync();
            return Json(data);
        }

        public async Task<IActionResult> GenerateNewId()
        {
            var newId = await entryService.GenerateDeductionIdAsync();
            return Json(newId);
        }

        public async Task<IActionResult> GetById(decimal id)
        {
            var result = await entryService.GetByIdAsync(id);

            return Json(new { data = result });
        }

        public async Task<IActionResult> GetSalaryDeduction()
        {
            var result = await entryService.GetAllAsync();

            return Json(new { data = result });
        }

        public async Task<IActionResult> GetPaginatedSalaryDeduction()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            var page = string.IsNullOrEmpty(start) ? 1 : (Convert.ToInt32(start)/pageSize)+1;

            var result = await entryService.GetPaginatedDataAsync(searchValue,page,pageSize,sortColumn,sortDirection);

            var response = new
            {
                draw = draw,
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.TotalRecords,
                data = result.Data
            };

            return Ok(response);
        }

        public async Task<IActionResult> SaveSalaryDeduction([FromBody] HrmPaySalaryDeductionEntryViewModel model)
        {
             if (model.AutoId == 0)
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

        public async Task<IActionResult> BulkDelete([FromBody] HrmPaySalaryDeductionEntryViewModel model)
        {
            try
            {
                if (model.AutoIds == null || !model.AutoIds.Any() || model.AutoIds.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No salary deduction is selected to delete" });
                }

                var result = await entryService.BulkDeleteAsync(model.AutoIds);

                if (!result)
                {
                    return Json(new { isSuccess = false, message = "No salary deduction found to delete" });
                }
                return Json(new { isSuccess = true, message = $"Deleted {model.AutoIds.Count} salary deduction Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await entryService.GenerateExcelSampleAsync();
            string excelName = "SalaryDeductionEntry.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [Route("/HrmPaySalaryDeduction/UploadExcelAsync")]
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

            HrmPaySalaryDeductionEntryViewModel model = new HrmPaySalaryDeductionEntryViewModel();
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

                    if (result =true)
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


