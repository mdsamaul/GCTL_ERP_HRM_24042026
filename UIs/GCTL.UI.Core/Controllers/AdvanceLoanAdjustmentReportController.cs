using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdvanceLoanAdjustmentReport;
using GCTL.Service.AdvanceLoanAdjustmentReport;
using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustmentReport;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;

namespace GCTL.UI.Core.Controllers
{
    public class AdvanceLoanAdjustmentReportController : BaseController
    {
        private readonly IAdvanceLoanAdjustmentReportServices advanceLoanAdjustmentReportServices;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdvanceLoanAdjustmentReportController(
            IAdvanceLoanAdjustmentReportServices advanceLoanAdjustmentReportServices,
            IWebHostEnvironment webHostEnvironment
            )
        {
            this.advanceLoanAdjustmentReportServices = advanceLoanAdjustmentReportServices;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            AdvanceLoanAdjustmentReportViewModel model = new AdvanceLoanAdjustmentReportViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetAdvancePayReportGrouped([FromBody] HrmAdvancePayReportFilter filter)
        {
            try
            {
                if (filter == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid filter parameters",
                        Data = null
                    });
                }
                filter.ToAudit(LoginInfo);
                var groupedData = await advanceLoanAdjustmentReportServices.GetAdvancePayReportGroupedAsync(filter);
                var totalRecords = groupedData.Sum(x => x.Employees.Count);

                return Ok(new ApiResponse<List<DepartmentGroupedData>>
                {
                    Success = true,
                    Message = "Grouped data retrieved successfully",
                    Data = groupedData,
                    TotalRecords = totalRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}",
                    Data = null
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetAdvancePayReport([FromBody] HrmAdvancePayReportFilter filter)
        {
            try
            {
                if (filter == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid filter parameters",
                        Data = null
                    });
                }

                var reportData = await advanceLoanAdjustmentReportServices.GetAdvancePayReportAsync(filter);

                return Ok(new ApiResponse<List<AdvanceLoanAdjustmentReportSetupViewModel>>
                {
                    Success = true,
                    Message = "Data retrieved successfully",
                    Data = reportData,
                    TotalRecords = reportData.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAdvancePayFilterReport([FromBody] HrmAdvancePayReportFilter filter)
        {
            var data = await advanceLoanAdjustmentReportServices.GetAdvancePayFiltersAsync(filter);
            return Json(data);
        }
       
        
        [HttpPost]
        [Route("AdvanceReport/UploadPdf")]
        public async Task<IActionResult> UploadPdf()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = Guid.NewGuid().ToString() + ".pdf";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/temp/{fileName}";
            return Ok(new { url = fileUrl });
        }

        [HttpPost]
        public IActionResult ExportAdvancePayReportExcel([FromBody] List<DepartmentGroupedData> groupedData)
        {
            if (groupedData == null || !groupedData.Any())
            {
                return BadRequest(new { success = false, message = "No data to export." });
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Advance Pay Report");

            int currentRow = 1;

            var firstEmployee = groupedData.FirstOrDefault()?.Employees?.FirstOrDefault();
            string companyName = firstEmployee?.CompanyName ?? "Your Company Name Here";
            string reportName = "Advance Adjustment Report";
            string reportDetails = $"For the month of {firstEmployee?.MonthName} - {firstEmployee?.SalaryYear}";
            string logoPath = Path.Combine(webHostEnvironment.WebRootPath, "images", "DP_logo.png");

            worksheet.Row(currentRow).Height = 40;

            if (System.IO.File.Exists(logoPath))
            {
                var picture = worksheet.Drawings.AddPicture("Logo", new FileInfo(logoPath));
                picture.SetPosition(currentRow - 1, 5, 0, 5); 
                picture.SetSize(150, 50);
            }

            worksheet.Cells[currentRow, 1, currentRow, 8].Merge = true;
            worksheet.Cells[currentRow, 1].Value = companyName;
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 16;
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            currentRow++;

            worksheet.Cells[currentRow, 1, currentRow, 8].Merge = true;
            worksheet.Cells[currentRow, 1].Value = reportName;
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            currentRow++;

            worksheet.Cells[currentRow, 1, currentRow, 8].Merge = true;
            worksheet.Cells[currentRow, 1].Value = reportDetails;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            currentRow += 2;

            foreach (var group in groupedData)
            {
                int tableStartRow = currentRow; 
                var deptCell = worksheet.Cells[currentRow, 1, currentRow, 8];
                deptCell.Merge = true;
                deptCell.Value = $"Department: {group.DepartmentName}";
                deptCell.Style.Font.Bold = true;
                deptCell.Style.Font.Size = 14;
                deptCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                deptCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                deptCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                deptCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                deptCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                deptCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                deptCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                deptCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                currentRow += 1;

                string[] headers = { "Employee ID", "Name", "Designation", "Branch", "Advance Amount", "Month", "Monthly Deduction", "Remarks" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[currentRow, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                currentRow++;

                foreach (var emp in group.Employees)
                {
                    worksheet.Cells[currentRow, 1].Value = emp.EmployeeID;
                    worksheet.Cells[currentRow, 2].Value = emp.FullName;
                    worksheet.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[currentRow, 3].Value = emp.DesignationName;
                    worksheet.Cells[currentRow, 4].Value = emp.BranchName;
                    worksheet.Cells[currentRow, 5].Value = emp.AdvanceAmount;
                    worksheet.Cells[currentRow, 6].Value = emp.MonthName;
                    worksheet.Cells[currentRow, 7].Value = emp.MonthlyDeduction;
                    worksheet.Cells[currentRow, 8].Value = emp.Remarks;

                    for (int i = 1; i <= headers.Length; i++)
                    {
                        var dataCell = worksheet.Cells[currentRow, i];
                        if (i == 2 || i ==8)
                            dataCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        else
                            dataCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        dataCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    currentRow++;
                }

                int tableEndRow = currentRow - 1;

                var blockRange = worksheet.Cells[tableStartRow, 1, tableEndRow, 8];
                blockRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                blockRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                blockRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                blockRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                currentRow += 2;
            }


            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"AdvancePayReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                excelName);
        }

    }
}
