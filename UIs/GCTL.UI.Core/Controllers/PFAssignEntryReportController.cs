using GCTL.Core.ViewModels.HRM_EmployeeWeekendDeclaration;
using GCTL.Core.ViewModels.RosterScheduleReport;
using GCTL.Service.EmployeeWeekendDeclarationReport;
using GCTL.UI.Core.ViewModels.PFAssignEntry;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using GCTL.Service.PFAssignEntryReport;
using GCTL.Core.ViewModels.PFAssignEntry;
using GCTL.Core.Helpers;

namespace GCTL.UI.Core.Controllers
{
    public class PFAssignEntryReportController : BaseController
    {
        private readonly IPFAssignEntryReportServices pFAssignEntryReportServices;

        public PFAssignEntryReportController(IPFAssignEntryReportServices pFAssignEntryReportServices)
        {
            this.pFAssignEntryReportServices = pFAssignEntryReportServices;
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
            var result = await pFAssignEntryReportServices.GetRosterDataAsync(filterDto);
            if (result == null)
            {
                return Json(new { isSuccess = false, message = "Data Load Failed" });
            }
            return Json(new { isSuccess = true, message = "Successed data load", data = result });
        }

        [HttpPost]
        public async Task<IActionResult> getAllPdfFilterEmp([FromBody] PFAssignEntryFilterDto filterDto)
        {
            //filterDto.ToAudit(LoginInfo);
            filterDto.ToAudit(LoginInfo);

            var result = await pFAssignEntryReportServices.GetRosterDataPdfAsync(filterDto);
            if (result != null)
            {
                return Json(new { isSuccess = true, message = "successed data load", data = result });
            }
            return Json(new { isSuccess = false, message = "Data load Failed" });
        }


        [HttpPost]
        public async Task<IActionResult> DownloadExcel([FromBody] List<PFAssignEntryFilterResultDto> employees)
        {
            if (employees == null || employees.Count == 0)
                return BadRequest(new { message = "No data found to generate Excel." });

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PF Assign Report");

                var headers = new[] { "SN", "Employee ID", "Name", "Designation", "Branch", "PF Approval", "Effective Date", "Day", "Remarks" };

                string companyName = employees.FirstOrDefault()?.Company ?? "Company Name";
                string reportTitle = "PF Assign Report";
                //string fromDate = employees.FirstOrDefault()?.FromDate ?? "";
                //string toDate = employees.FirstOrDefault()?.ToDate ?? "";
                string userName = employees.FirstOrDefault()?.Luser ?? "";
                string Address = "Address: Corner Glaze, Block#L, Road#8, Plot#2490/B, Bashundhara Residential Area, Dhaka-1229";

                // Title Rows
                worksheet.Cells[1, 1].Value = companyName;
                worksheet.Cells[1, 1, 1, headers.Length].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 1].Value = reportTitle;
                worksheet.Cells[2, 1, 2, headers.Length].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1].Value = Address;
                worksheet.Cells[3, 1, 3, headers.Length].Merge = true;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                int rowIndex = 4;

                // Group by department
                var departmentGroups = employees.GroupBy(e => e.Department ?? "Unknown");

                foreach (var dept in departmentGroups)
                {
                    // Department title
                    // Department header row
                    worksheet.Cells[rowIndex, 1, rowIndex, headers.Length].Merge = true;
                    worksheet.Cells[rowIndex, 1].Value = $"Department: {dept.Key}";

                    // Styling entire merged range
                    var deptRange = worksheet.Cells[rowIndex, 1, rowIndex, headers.Length];
                    deptRange.Style.Font.Bold = true;
                    deptRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    deptRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);

                    // Apply border to all sides of merged range
                    deptRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    deptRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    deptRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    deptRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    rowIndex++;


                    // Header row
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[rowIndex, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    rowIndex++;

                    // Employee rows
                    int sn = 1;
                    foreach (var emp in dept)
                    {
                        worksheet.Cells[rowIndex, 1].Value = sn++;
                        worksheet.Cells[rowIndex, 2].Value = emp.Code ?? "";
                        worksheet.Cells[rowIndex, 3].Value = emp.Name ?? "";
                        worksheet.Cells[rowIndex, 4].Value = emp.Designation ?? "";
                        worksheet.Cells[rowIndex, 5].Value = emp.Branch ?? "";
                        worksheet.Cells[rowIndex, 6].Value = emp.PFApprove ?? "";
                        worksheet.Cells[rowIndex, 7].Value = emp.ShowDate ?? "";
                        worksheet.Cells[rowIndex, 8].Value = emp.dayName ?? "";
                        worksheet.Cells[rowIndex, 9].Value = emp.Remarks ?? "";

                        for (int i = 1; i <= headers.Length; i++)
                        {
                            worksheet.Cells[rowIndex, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    rowIndex++;
                }

                // Footer
                int totalColumns = headers.Length;
                int midColumn = totalColumns / 2;
                worksheet.Cells[rowIndex, 1, rowIndex, midColumn].Merge = true;
                worksheet.Cells[rowIndex, 1].Value = $"Downloaded At: {DateTime.Now.ToString("dd/MM/yyyy | hh:mm:ss tt")}";

                worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[rowIndex, 1].Style.Font.Italic = true;

                worksheet.Cells[rowIndex, midColumn + 1, rowIndex, totalColumns].Merge = true;
                worksheet.Cells[rowIndex, midColumn + 1].Value = $"Downloaded By: {userName}";
                worksheet.Cells[rowIndex, midColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[rowIndex, midColumn + 1].Style.Font.Italic = true;

                rowIndex++;


                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "EmployeeWeekendDeclaration.xlsx");
            }
        }
    }
}
