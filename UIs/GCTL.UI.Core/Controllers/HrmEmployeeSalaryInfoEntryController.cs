using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmEmployeeSalaryInfoEntry;
using GCTL.Service.HrmEmployeeSalaryInfoEntry;
using GCTL.UI.Core.ViewModels.HrmEmpSalaryInfoEntry;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmEmployeeSalaryInfoEntryController : BaseController
    {
        private readonly IHrmEmployeeSalaryInfoEntryService entryService;

        public HrmEmployeeSalaryInfoEntryController(IHrmEmployeeSalaryInfoEntryService entryService)
        {
            this.entryService = entryService;
        }

        public IActionResult Index()
        {
            HrmEmpSalaryInfoEntryViewModel model = new HrmEmpSalaryInfoEntryViewModel()
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

        public async Task<IActionResult> getMethodDD()
        {
            var data = await entryService.GetPaymentMode();
            return Json(data);
        }

        public async Task<IActionResult> Save([FromBody] HrmEmployeeSalaryInfoEntryViewModel model)
        {
            if (model == null) return NotFound();

            model.SalaryInfoUpdate.ForEach(x => x.ToAudit(LoginInfo, true));

            var result = await entryService.BulkEditAsync(model);
            return Json(new
            {
                success = result
            });
        }

        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await entryService.GenerateExcelSampleAsync();
            string excelName = "SalaryInfoEntry.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }


        [Route("/HrmEmployeeSalaryInfoEntry/UploadExcelAsync")]
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

            EmployeeListItemViewModel model = new EmployeeListItemViewModel();
              
            model.ToAudit(LoginInfo);
            
            try
            {
                model.CompanyCode = Request.Form["CompanyCode"];
                using (var stream = file.OpenReadStream())
                {
                    var result = await entryService.ProcessExcelFileAsync(stream, model);

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
