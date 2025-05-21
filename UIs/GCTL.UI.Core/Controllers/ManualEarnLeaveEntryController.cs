using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HRM_EmployeeWeekendDeclaration;
using GCTL.Core.ViewModels.ManualEarnLeaveEntry;
using GCTL.Service.Common;
using GCTL.Service.Companies;
using GCTL.Service.EmployeeWeekendDeclaration;
using GCTL.Service.ManualEarnLeaveEntry;
using GCTL.UI.Core.ViewModels.ManualEarnLeaveEntry;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace GCTL.UI.Core.Controllers
{
    public class ManualEarnLeaveEntryController : BaseController
    {
        private readonly IManualEarnLeaveEntryService manualEarnLeaveEntryService;

        public ManualEarnLeaveEntryController(
            IManualEarnLeaveEntryService manualEarnLeaveEntryService
            )
        {
            this.manualEarnLeaveEntryService = manualEarnLeaveEntryService;
        }
        public IActionResult Index()
        {
            ManualEarnLeaveEntryViewModel model = new ManualEarnLeaveEntryViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> getAllFilterEmp([FromBody] ManualEarnLeaveEntryEmployeeFilterDto filterDto)
        {
            //filterDto.EmployeeStatuses = new List<string> { "01" };
            var result = await manualEarnLeaveEntryService.GetFilterDataAsync(filterDto);
            if (result != null)
            {
                return Json(new { isSuccess = true, message = "successed data load", data = result });
            }
            return Json(new { isSuccess = false, message = "Data load Failed" });

        }
        [HttpPost]
        public async Task<IActionResult> CreateManualEarnLeave([FromBody] ManualEarnLeaveEntryEmployeeCreateDto FromData)
        {
            //FromData.ToAudit(LoginInfo);
            FromData.ToAudit(LoginInfo, FromData.isUpdate);
            var result = await manualEarnLeaveEntryService.SaveUpdateEarnLeaveServices(FromData);
            //return Ok(new { message = "Manual Earn Leave Saved Successfully" });
            return Json(new
            {
                success = result.isSuccess,
                message = result.message,
                data = result.data,
            });
        }

        public IActionResult GetEarmLeaveEmployee()
        {
            var result = manualEarnLeaveEntryService.GetEarnLeaveEmployeeService();
            return Json(new { data = result });
        }

        //download 
        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await manualEarnLeaveEntryService.GenerateEmpEarnLeaveExcelDownload();
            string excelName = $"EarnLeaveData.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        //uploaded data from excel
        //[HttpPost]
        //[Route("ManualEarnLeaveEntry/UploadExcel")]
        public async Task<IActionResult> UploadExcel(IFormFile excelFile, ManualEarnLeaveEntryEmployeeCreateDto modelVm)
        {
            if (excelFile == null || excelFile.Length ==0)
            {
                return Json(new { isSuccess = false, message="Please select a valid Excel file." });
            }    
            modelVm.ToAudit(LoginInfo);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    modelVm.EmployeeID = new List<string>();
                    modelVm.YearList = new List<string>();
                    modelVm.GrantedLeaveDaysList = new List<decimal>();
                    modelVm.AvailedLeaveDaysList = new List<decimal>();
                    modelVm.BalancedLeaveDaysList = new List<decimal>();
                    modelVm.RemarksList = new List<string>();
                    for(int row = 2; row<= rowCount; row++)
                    {
                        var empId = worksheet.Cells[row, 1].Text.Trim();
                        var year = worksheet.Cells[row, 2].Text.Trim();
                        var grantedLeaveDay = worksheet.Cells[row, 3].Text.Trim();
                        var availedLeaveDay = worksheet.Cells[row, 4].Text.Trim();
                        var balancedLeaveDay = worksheet.Cells[row, 5].Text.Trim();
                        var remark = worksheet.Cells[row, 6].Text.Trim();

                        modelVm.EmployeeID.Add(empId);
                        modelVm.YearList.Add(year);
                        modelVm.GrantedLeaveDaysList.Add(decimal.Parse( grantedLeaveDay));
                        modelVm.AvailedLeaveDaysList.Add(decimal.Parse( availedLeaveDay));
                        modelVm.BalancedLeaveDaysList.Add(decimal.Parse( balancedLeaveDay));
                        modelVm.RemarksList.Add(remark);
                    }
                }
            }
            var result = await manualEarnLeaveEntryService.SaveEarnLeaveExcel(modelVm);
            return Json(new { isSuccss = result.isSuccess, message = result.message });
        }

        public async Task<ActionResult> BulkDeleteEmpWeelend(List<decimal> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "Employee not selected" });
                }

                var result = await manualEarnLeaveEntryService.BulkDeleteAsync(ids);
                if (!result)
                {
                    return Json(new { isSuccess = false, message = "Employee not found" });
                }
                return Json(new { isSuccess = true, message = $"Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditEarnLeaveEmployee(string id)
        {
            var emp = await manualEarnLeaveEntryService.getEarnLeaveEmployeeById(id);
            if (emp == null)
            {
                return Json(new { isSuccess = false, message = "Employee Not Found" });
            }
            return Json(new { isSuccess = true, message = "Employee Found", data = emp });
        }
        //[HttpPost]
        //public async Task<IActionResult> UpdateEarnLeaveEmployee([FromBody] ManualEarnLeaveEntryEmployeeCreateDto modelVM)
        //{
        //    var employee = await manualEarnLeaveEntryService.Update
        //    return Json(new {modelVM });
        //}

    }
}
