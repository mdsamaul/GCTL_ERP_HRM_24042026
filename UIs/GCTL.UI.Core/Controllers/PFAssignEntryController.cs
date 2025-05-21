using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.ManualEarnLeaveEntry;
using GCTL.Core.ViewModels.PFAssignEntry;
using GCTL.Service.ManualEarnLeaveEntry;
using GCTL.Service.PFAssignEntry;
using GCTL.UI.Core.ViewModels.ManualEarnLeaveEntry;
using GCTL.UI.Core.ViewModels.PFAssignEntry;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace GCTL.UI.Core.Controllers
{
    public class PFAssignEntryController : BaseController
    {
        private readonly IPFAssignEntryService pFAssignEntryService;

        public PFAssignEntryController(IPFAssignEntryService pFAssignEntryService)
        {
            this.pFAssignEntryService = pFAssignEntryService;
        }
        public IActionResult Index()
        {
            PFAssignEntryViewModel model = new PFAssignEntryViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> getAllFilterEmp([FromBody] PFAssignEntryFilterDto filterDto)
        {
            //filterDto.EmployeeStatuses = new List<string> { "01" };
            var result = await pFAssignEntryService.GetFilterDataAsync(filterDto);
            if (result != null)
            {
                return Json(new { isSuccess = true, message = "successed data load", data = result });
            }
            return Json(new { isSuccess = false, message = "Data load Failed" });

        }

        [HttpPost]
        public async Task<IActionResult> CreateEditPFAssignEntry([FromBody] PFAssignEntrySetupViewModel FromData)
        {
            FromData.ToAudit(LoginInfo, FromData.isUpdate);
            var result = await pFAssignEntryService.CreateUpdatePFAssignService(FromData);
            return Json(new { 
                isSuccess = result.isSuccess, 
                message = result.message, 
                data = FromData 
            });
        }
        //download 
        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await pFAssignEntryService.GeneratePfAssignExcelDownload();
            string excelName = $"PFAssignData.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        //upload excelfile
        public async Task<IActionResult> UploadExcel(IFormFile excelFile, PFAssignEntrySetupViewModel fromData)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { isSuccess = false, message = "Please select a valid Excel file." });
            }
            fromData.ToAudit(LoginInfo);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    fromData.EmployeeIds = new List<string>();
                    fromData.EFDateList = new List<string>();
                    fromData.ApprovalRemarkList = new List<string>();
                    fromData.PFApprovedStatusList = new List<string>();
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var empId = worksheet.Cells[row, 1].Text.Trim();
                        var efDate = worksheet.Cells[row, 2].Text.Trim();
                        var PFApprovedStatusList = worksheet.Cells[row, 3].Text.Trim();
                        var remarks = worksheet.Cells[row, 4].Text.Trim();

                        fromData.EmployeeIds.Add(empId);
                        fromData.EFDateList.Add(efDate);
                        fromData.ApprovalRemarkList.Add(remarks);
                        fromData.PFApprovedStatusList.Add(PFApprovedStatusList);
                    }
                }
            }
            var result = await pFAssignEntryService.SavePFAssignExcel(fromData);
            return Json(new { isSuccss = result.isSuccess, message = result.message });
        }

        public IActionResult GetPfAssignData()
        {
            var result = pFAssignEntryService.GetPfAssignDataService();
            return Json(new { data = result });
        }


        public async Task<ActionResult> BulkDeleteEmpPFAssign(List<decimal> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "Employee not selected" });
                }

                var result = await pFAssignEntryService.BulkDeleteAsync(ids);
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
        public async Task<IActionResult> EditGetAssignValue(string id)
        {
            var emp = await pFAssignEntryService.getAssignValueById(id);
            if (emp == null)
            {
                return Json(new { isSuccess = false, message = "Employee Not Found" });
            }
            return Json(new { isSuccess = true, message = "Employee Found", data = emp });
        }
    }
}
