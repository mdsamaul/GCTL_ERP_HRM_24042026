using System.Security.Policy;
using GCTL.Core.ViewModels.RosterScheduleReport;
using GCTL.Service.RosterScheduleEntry;
using GCTL.Service.RosterScheduleReport;
using GCTL.UI.Core.ViewModels.RosterScheduleEntry;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.Helpers;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using GCTL.Core.ViewModels.RosterScheduleEntry;
namespace GCTL.UI.Core.Controllers
{
    public class RosterScheduleReportController : BaseController
    {
        private readonly IRosterScheduleReportServices rosterScheduleReportServices;

        public RosterScheduleReportController(IRosterScheduleReportServices rosterScheduleReportServices)
        {
            this.rosterScheduleReportServices = rosterScheduleReportServices;
        }
        public IActionResult Index()
        {
            RosterScheduleEntryViewModel model = new RosterScheduleEntryViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> getAllFilterEmp([FromBody] RosterReportFilterDto filterDto)
        {
            var result = await rosterScheduleReportServices.GetRosterDataAsync(filterDto);
            if (result != null)
            {
                return Json(new { isSuccess = true, message = "successed data load", data = result });
            }
            return Json(new { isSuccess = false, message = "Data load Failed" });
        }
        [HttpPost]
        public async Task<IActionResult> getAllPdfFilterEmp([FromBody] RosterReportFilterDto filterDto)
        {
            //filterDto.ToAudit(LoginInfo);
            filterDto.ToAudit(LoginInfo);
            var result = await rosterScheduleReportServices.GetRosterDataPdfAsync(filterDto);
            if (result != null)
            {
                return Json(new { isSuccess = true, message = "successed data load", data = result });
            }
            return Json(new { isSuccess = false, message = "Data load Failed" });
        }

        [HttpPost]
        public async Task<IActionResult> DownloadExcel([FromBody] List<RosterReportFilterResultDto> rosterData)
        {
            if (rosterData == null || rosterData.Count == 0)
            {
                return BadRequest(new { message = "No data found to generate Excel." });
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Roster Report");

                var headers = new string[]
                {
                    "SN", "Code", "Name", "Designation", "Branch", "Date", "Day", "Shift",
                    "Remark", "Approval Status", "Approved By", "App. Datetime"
                };

               
                var company = rosterData.FirstOrDefault()?.CompanyName ?? "Company Name";
                var title = "Roster Schedule Report";
                var from = rosterData.FirstOrDefault()?.FromDate?.ToString() ?? "";
                var to = rosterData.FirstOrDefault()?.ToDate?.ToString() ?? "";
                var fromDate = string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to)
                    ? "Date"
                    : $"Date: {from}-{to}";

                worksheet.Cells[1, 1].Value = company;
                worksheet.Cells[1, 1, 1, headers.Length].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 1].Value = title;
                worksheet.Cells[2, 1, 2, headers.Length].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Style.Font.Size = 15;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1].Value = fromDate;
                worksheet.Cells[3, 1, 3, headers.Length].Merge = true;
                worksheet.Cells[3, 1].Style.Font.Size = 14;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                worksheet.Cells[3, 1, 3, headers.Length].Merge = true;

                int rowIndex = 4;

                var departmentGroups = rosterData.GroupBy(emp => emp.DepartmentName ?? "Unknown");

                foreach (var deptGroup in departmentGroups)
                {
                    worksheet.Cells[rowIndex, 1].Value = "Department: " + deptGroup.Key;
                    worksheet.Cells[rowIndex, 1, rowIndex, headers.Length].Merge = true;
                    worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
                    worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowIndex, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    rowIndex++;

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[rowIndex, i + 1].Value = headers[i];
                        worksheet.Cells[rowIndex, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[rowIndex, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowIndex, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[rowIndex, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    rowIndex++;

                    int sn = 1;
                    foreach (var emp in deptGroup)
                    {
                        worksheet.Cells[rowIndex, 1].Value = sn++;
                        worksheet.Cells[rowIndex, 2].Value = emp.Code ?? "";
                        worksheet.Cells[rowIndex, 3].Value = emp.Name ?? "";
                        worksheet.Cells[rowIndex, 4].Value = emp.DesignationName ?? "";
                        worksheet.Cells[rowIndex, 5].Value = emp.BranchName ?? "";
                        worksheet.Cells[rowIndex, 6].Value = emp.ShowDate ?? "";
                        worksheet.Cells[rowIndex, 7].Value = emp.DayName ?? "";
                        worksheet.Cells[rowIndex, 8].Value = emp.ShiftName ?? "";
                        worksheet.Cells[rowIndex, 9].Value = emp.Remark ?? "";
                        worksheet.Cells[rowIndex, 10].Value = emp.ApprovalStatus ?? "";
                        worksheet.Cells[rowIndex, 11].Value = emp.ApprovedBy ?? "";
                        worksheet.Cells[rowIndex, 12].Value = emp.ShowApprovalDatetime ?? "";

                        for (int i = 1; i <= headers.Length; i++)
                        {
                            worksheet.Cells[rowIndex, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        rowIndex++;
                    }

                    worksheet.Cells[rowIndex, 1, rowIndex, headers.Length].Merge = true;
                    rowIndex++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RosterReport.xlsx");
            }
        }

    }
}
