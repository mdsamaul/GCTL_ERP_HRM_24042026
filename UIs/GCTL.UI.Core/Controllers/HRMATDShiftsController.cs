
using ClosedXML.Excel;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmAtdShifts;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using GCTL.Service.Common;
using GCTL.Service.Designations;
using GCTL.Service.HrmAtdShifts;

using GCTL.UI.Core.ViewModels.HrmAtdShifts;


using Microsoft.AspNetCore.Mvc.Rendering;
using GCTL.Service.Shifts;
using GCTL.Core.Data;
using GCTL.Data.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace GCTL.UI.Core.Controllers
{
    public class HRMATDShiftsController : BaseController
    {


        private readonly IHrmAtdShiftService hrmAtdShiftService;
        private readonly ICommonService commonService;
        private readonly IRepository<HmsShift> shiftService;
        public HRMATDShiftsController(IHrmAtdShiftService  hrmAtdShiftService, IRepository<HmsShift> shiftService, ICommonService commonService)
        {
            this.hrmAtdShiftService = hrmAtdShiftService;
            this.commonService = commonService;
            this.shiftService = shiftService;

        }



        #region GetByallId
        public async Task<IActionResult> Index(string? id)
        {
            var hasPermission = await hrmAtdShiftService.PagePermissionAsync(LoginInfo.AccessCode);
            if (!hasPermission)
            {
                return RedirectToAction("Login", "Accounts");
            }
            //Get all
            HrmAtdShiftPageViewModel model = new HrmAtdShiftPageViewModel();
            var list = await hrmAtdShiftService.GetAllAsync();
            model.HrmAttendenceShiftList = list ?? new List<HrmAtdShiftSetupViewModel>();
            //Get By Id
            if (!string.IsNullOrEmpty(id))
            {
                
                model.Setup = await hrmAtdShiftService.GetByIdAsync(id);
            }

            //
            var shiftTypes = new List<SelectListItem>
            {
                 new SelectListItem { Value = "Day shift", Text = "Day shift" },
                 new SelectListItem { Value = "Night shift", Text = "Night shift" }
            };

            // Assign the shift types to ViewBag
            ViewBag.ShiftTypeDD = new SelectList(shiftTypes, "Value", "Text");
            //
            //ViewBag.ShiftDDD = new SelectList(shiftService.All(), "ShiftCode", "HMS_Shift");
          

            model.PageUrl = Url.Action(nameof(Index));
            return View(model);
        }
        #endregion

        #region get all pdf download
        //get all shift
        [HttpGet]
        public async Task<IActionResult> GetAllShifts()
        {
            var list = await hrmAtdShiftService.GetAllAsync();

            if (list == null || !list.Any())
            {
                return Json(new { success = false, message = "No shifts found." });
            }

            return Json(new { success = true, data = list });
        }

        #endregion
        #region excel download
        [HttpPost]
        public IActionResult DownloadShiftExcel([FromBody] List<HrmAtdShiftSetupViewModel> shifts)
        {
            if (shifts == null || !shifts.Any())
            {
                return BadRequest(new { message = "No data found to generate Excel." });
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Shift Report");

                var headers = new string[]
                {
            "SN", "Shift Name", "Description", "In Time", "Out Time", "Late Time",
            "Lunch Out Time", "Lunch In Time", "Lunch Break(Hr)", "Absent Time", "W.E.F", "Shift Type", "Remarks"
                };

                int totalColumns = headers.Length;

                // === HEADER 1 ===
                worksheet.Cells[1, 1, 1, totalColumns].Merge = true;
                worksheet.Cells[1, 1].Value = "Data Path";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;

                // === HEADER 2 ===
                worksheet.Cells[2, 1, 2, totalColumns].Merge = true;
                worksheet.Cells[2, 1].Value = "HRM Shift Setup Report";
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Bold = true;

                // === HEADER ROW 3 ===
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[3, i + 1].Value = headers[i];
                    worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[3, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    worksheet.Cells[3, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // === DATA ROWS FROM ROW 4 ===
                int rowIndex = 4;
                int sn = 1;
                foreach (var shift in shifts)
                {
                    worksheet.Cells[rowIndex, 1].Value = sn++;
                    worksheet.Cells[rowIndex, 2].Value = shift.ShiftName ?? "";
                    worksheet.Cells[rowIndex, 3].Value = shift.Description ?? "";
                    worksheet.Cells[rowIndex, 4].Value = FormatTime(shift.ShiftStartTime);
                    worksheet.Cells[rowIndex, 5].Value = FormatTime(shift.ShiftEndTime);
                    worksheet.Cells[rowIndex, 6].Value = FormatTime(shift.LateTime);
                    worksheet.Cells[rowIndex, 7].Value = FormatTime(shift.LunchOutTime);
                    worksheet.Cells[rowIndex, 8].Value = FormatTime(shift.LunchInTime);
                    worksheet.Cells[rowIndex, 9].Value = shift.LunchBreakHour;
                    worksheet.Cells[rowIndex, 10].Value = FormatTime(shift.AbsentTime);
                    worksheet.Cells[rowIndex, 11].Value = FormatDate(shift.Wef);
                    worksheet.Cells[rowIndex, 12].Value = shift.ShiftTypeId ?? "";
                    worksheet.Cells[rowIndex, 13].Value = shift.Remarks ?? "";

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        var cell = worksheet.Cells[rowIndex, col];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        // Center align by default
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Left-align only for Shift Name (col 2) and Description (col 3)
                        if (col == 2 || col == 3)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }

                    rowIndex++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = "HRM_Shift_Report.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }



        // Helpers
        private string FormatTime(DateTime? time)
        {
            return time.HasValue ? time.Value.ToString("hh:mm:ss tt") : "";
        }

        private string FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd/MM/yyyy") : "";
        }

        #endregion

        #region Post Update 

        [HttpPost]
        [ValidateAntiForgeryToken]


        public async Task<IActionResult> Setup(HrmAtdShiftSetupViewModel modelVM)
        {
            try
            {

                if (await hrmAtdShiftService.IsExistAsync(modelVM.ShiftName, modelVM.ShiftCode))
                {
                    return Json(new { isSuccess = false, message = $"Already Exists!", isDuplicate = true });
                }


                if (string.IsNullOrEmpty(modelVM.ShiftCode))
                {
                    modelVM.ShiftCode = await hrmAtdShiftService.GenerateNextCode();
                }


                modelVM.ToAudit(LoginInfo, modelVM.AutoId > 0);
                if (modelVM.AutoId == 0)
                {
                    var hasSavePermission = await hrmAtdShiftService.SavePermissionAsync(LoginInfo.AccessCode);
                    if (hasSavePermission)
                    {
                        await hrmAtdShiftService.SaveAsync(modelVM);
                        return Json(new { isSuccess = true, message = "Saved Successfully", lastCode = modelVM.ShiftCode });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access to save", noSavePermission = true });
                    }
                }
                else
                {

                    var hasUpdatePermission = await hrmAtdShiftService.UpdatePermissionAsync(LoginInfo.AccessCode);
                    if (hasUpdatePermission)
                    {
                        await hrmAtdShiftService.UpdateAsync(modelVM);
                        return Json(new { isSuccess = true, message = "Updated Successfully", lastCode = modelVM.ShiftCode });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access to update", noUpdatePermission = true });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }




        #endregion


        #region Delete

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<string> ids)
        {
            try
            {

                var hasPermission = await hrmAtdShiftService.DeletePermissionAsync(LoginInfo.AccessCode);
                if (hasPermission)
                {

                    foreach (var id in ids)
                    {
                        var result = hrmAtdShiftService.DeleteLeaveType(id);

                    }

                    return Json(new { isSuccess = true, message = "Data Deleted Successfully" });
                }
                else
                {

                    return Json(new { isSuccess = false, message = "You have no access" });
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error deleting  type: {ex.Message}");

                return StatusCode(500, new { isSuccess = false, message = ex.Message });
            }
        }


        #endregion

        #region NeaxtCode
        [HttpGet]
        public async Task<IActionResult> GenerateNextCode()
        {
            var nextCode = await hrmAtdShiftService.GenerateNextCode();
            return Json(nextCode);
        }
        #endregion

        #region CheckAvailability
        [HttpPost]
        public async Task<JsonResult> CheckAvailability(string name, string code)
        {
            if (await hrmAtdShiftService.IsExistAsync(name, code))
            {

                return Json(new { isSuccess = true, message = $"Already  Exists!" });

            }

            return Json(new { isSuccess = false });
        }
        #endregion

        #region TabeleLodaing
       
        [HttpGet]
        public async Task<IActionResult> GetTableData()
        {
            try
            {
                var list = await hrmAtdShiftService.GetAllAsync();               
                return PartialView("_Grid", list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //
        #endregion


        #region Pdf
        public async Task<IActionResult> ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ShiftInformations");

                // Add title
                
                worksheet.Cell(1, 1).Value = "Shift Information";
                worksheet.Range(1, 1, 1, 9).Merge(); // Merge cells across the header columns
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Font.FontSize = 14;
                worksheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Row(1).Height = 20; // Adjust row height for visibility
                worksheet.Row(1).Style.Font.FontColor = XLColor.Black; // Ensure font is visible


                //
                // Leave an empty row
                worksheet.Range(2, 1, 2, 9).Merge();

                int dataStartRow = 3;

                // Add headers
                worksheet.Cell(dataStartRow, 1).Value = "Shift Id";
                worksheet.Cell(dataStartRow, 2).Value = "Shift Name";
                worksheet.Cell(dataStartRow, 3).Value = "Description";
                worksheet.Cell(dataStartRow, 4).Value = "In Time";
                worksheet.Cell(dataStartRow, 5).Value = "Out Time";
                worksheet.Cell(dataStartRow, 6).Value = "Late Time";
                worksheet.Cell(dataStartRow, 7).Value = "Absent Time";
                worksheet.Cell(dataStartRow, 8).Value = "Effective Date";
                worksheet.Cell(dataStartRow, 9).Value = "Shift Type";
                worksheet.Row(dataStartRow).Style.Font.Bold = true;
                worksheet.Row(dataStartRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(dataStartRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Add headers
                //worksheet.Cell(dataStartRow, 1).Value = "Shift Id";
                //worksheet.Cell(dataStartRow, 2).Value = "Shift Name";
                //worksheet.Cell(dataStartRow, 3).Value = "Description";
                //worksheet.Cell(dataStartRow, 4).Value = "In Time";
                //worksheet.Cell(dataStartRow, 5).Value = "Out Time";
                //worksheet.Cell(dataStartRow, 6).Value = "Leave Time";
                //worksheet.Cell(dataStartRow, 7).Value = "Absent Time";
                //worksheet.Cell(dataStartRow, 8).Value = "W.E.F";
                //worksheet.Cell(dataStartRow, 9).Value = "Shift Type";

                //// Make header row bold
                //worksheet.Row(dataStartRow).Style.Font.Bold = true;

                //// Set borders for the header row
                //var headerRange = worksheet.Range(dataStartRow, 1, dataStartRow, 9); // Define the range for headers
                //headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                //headerRange.Style.Border.OutsideBorderColor = XLColor.Black;
                //headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                //headerRange.Style.Border.InsideBorderColor = XLColor.Black;

                //// Center alignment for the header row
                //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                // Add data
                var designations =await hrmAtdShiftService.GetAllAsync();
                int row = dataStartRow + 1;
                foreach (var designation in designations)
                {
                    // worksheet.Cell(row, 1).Value = designation.DepartmentCode;
                    //worksheet.Cell(row, 1).Value = designation.DepartmentCode.PadLeft(2, '0');
                    worksheet.Cell(row, 1).Value = "'" + designation.ShiftCode.PadLeft(2, '0');


                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 2).Value = designation.ShiftName;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    worksheet.Cell(row, 3).Value = designation.Description;
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    worksheet.Cell(row, 4).Value = designation.ShiftStartTime.ToString("hh:mm ss tt");
                    worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 5).Value = designation.ShiftEndTime.ToString("hh:mm ss tt");
                    worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                    worksheet.Cell(row, 6).Value = designation.LateTime.ToString("hh:mm ss tt");
                    worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 7).Value = designation.AbsentTime.ToString("hh:mm ss tt");
                    worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 8).Value = designation.Wef.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 9).Value = designation.ShiftTypeId;
                    worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //center

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                // Save to a stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ShiftInformations.xlsx");
                }
            }
        }
        #endregion


       

    }
}


