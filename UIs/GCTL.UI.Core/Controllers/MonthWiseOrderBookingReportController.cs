using GCTL.Core.Data;
using GCTL.Core.ViewModels.MonthWiseOrderBookingReport;
using GCTL.Data.Models;
using GCTL.Service.MonthWiseOrderBookingReport;
using GCTL.UI.Core.ViewModels.MonthWiseOrderBookingReportViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace GCTL.UI.Core.Controllers
{
    public class MonthWiseOrderBookingReportController : BaseController
    {
        private readonly IOrderReportService _orderReportService;
        private readonly IRepository<RmgProdDefBuyer> buyerRepo;

        public MonthWiseOrderBookingReportController(
            IOrderReportService orderReportService,
            IRepository<RmgProdDefBuyer> buyerRepo
            )
        {
            _orderReportService = orderReportService;
            this.buyerRepo = buyerRepo;
        }

        // View Page Load
        public IActionResult Index()
        {
            ViewBag.buyersList = new SelectList(buyerRepo.All().Select(x => new { id = x.BuyerId, name = x.BuyerName }), "id", "name");
            //ViewBag.ProductList = new SelectList(productRepo.All().Select(x => new { x.ProductCode, x.ProductName }), "ProductCode", "ProductName");
            OrderReportDataViewModel model = new OrderReportDataViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }

        // POST: Get Order Report Data
        //[HttpPost]
        //public async Task<IActionResult> GetOrderReport([FromBody] OrderReportRequest request)
        //{
        //    var result = await _orderReportService.GetOrderReportAsync(request);
        //    return Json(result);
        //}

        //public async Task<IActionResult> GetOrderReport([FromBody] OrderReportRequest request)
        //{
        //    try
        //    {
        //        var result = await _orderReportService.GetOrderReportAsync(request);
        //        return Ok(new { success = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}


        //public async Task<IActionResult> GetOrderAllStyleReport([FromBody] OrderReportRequest request)
        //{
        //    try
        //    {
        //        var result = await _orderReportService.GetOrderReportAllStyleAsync(request);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}

        public async Task<IActionResult> DownloadOrderAllStyleReport([FromBody] OrderReportRequest request)
        {
            try
            {
                var reportData = await _orderReportService.GetOrderReportAllStyleAsync(request);
                var excelFile = GenerateExcel(reportData);

                return File(excelFile,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"MonthWiseOrderReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        //private byte[] GenerateExcel(OrderReportAllStyleResponse reportData)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //    using var package = new ExcelPackage();
        //    var worksheet = package.Workbook.Worksheets.Add("Order Report");

        //    // Header Section
        //    worksheet.Cells[1, 1].Value = reportData.CompanyName;
        //    worksheet.Cells[1, 1].Style.Font.Bold = true;
        //    worksheet.Cells[1, 1].Style.Font.Size = 14;
        //    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //    worksheet.Cells[2, 1].Value = reportData.ReportTitle + " " + reportData.ReportYear;
        //    worksheet.Cells[2, 1].Style.Font.Bold = true;
        //    worksheet.Cells[2, 1].Style.Font.Size = 12;
        //    worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //    // Get dynamic month columns
        //    var monthColumns = reportData.Data.FirstOrDefault()?.MonthlyQuantities.Keys.ToList() ?? new List<string>();
        //    int totalColumns = 5 + monthColumns.Count;

        //    // Merge header cells
        //    worksheet.Cells[1, 1, 1, totalColumns].Merge = true;
        //    worksheet.Cells[2, 1, 2, totalColumns].Merge = true;

        //    // Column Headers (Row 4)
        //    int row = 4;
        //    int col = 1;

        //    worksheet.Cells[row, col++].Value = "Sl No.";
        //    worksheet.Cells[row, col++].Value = "Buyer Name";
        //    worksheet.Cells[row, col++].Value = "Style";
        //    worksheet.Cells[row, col++].Value = "Item";
        //    worksheet.Cells[row, col++].Value = "Total Order Quantity";

        //    foreach (var month in monthColumns)
        //    {
        //        worksheet.Cells[row, col++].Value = month;
        //    }

        //    // Style header row
        //    using (var range = worksheet.Cells[row, 1, row, totalColumns])
        //    {
        //        range.Style.Font.Bold = true;
        //        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        //        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        //    }

        //    // Data Rows
        //    row = 5;
        //    foreach (var item in reportData.Data)
        //    {
        //        col = 1;
        //        worksheet.Cells[row, col++].Value = item.SlNo;
        //        worksheet.Cells[row, col++].Value = item.BuyerName;
        //        worksheet.Cells[row, col++].Value = item.Style;
        //        worksheet.Cells[row, col++].Value = item.Item;
        //        worksheet.Cells[row, col++].Value = item.TotalOrderQuantity;

        //        foreach (var month in monthColumns)
        //        {
        //            if (item.MonthlyQuantities.TryGetValue(month, out string value))
        //            {
        //                worksheet.Cells[row, col++].Value = value;
        //            }
        //            else
        //            {
        //                worksheet.Cells[row, col++].Value = "0";
        //            }
        //        }

        //        row++;
        //    }

        //    // Auto-fit columns
        //    worksheet.Cells.AutoFitColumns();

        //    // Add borders to all data
        //    using (var range = worksheet.Cells[4, 1, row - 1, totalColumns])
        //    {
        //        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        //    }

        //    return package.GetAsByteArray();
        //}


        private byte[] GenerateExcel(OrderReportAllStyleResponse reportData)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Order Report");

            // Header Section
            worksheet.Cells[1, 1].Value = reportData.CompanyName;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[2, 1].Value = reportData.ReportTitle + " " + reportData.ReportYear;
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.Font.Size = 12;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Get dynamic month columns
            var monthColumns = reportData.Data.FirstOrDefault()?.MonthlyQuantities.Keys.ToList() ?? new List<string>();
            int totalColumns = 5 + monthColumns.Count;

            // Merge header cells
            worksheet.Cells[1, 1, 1, totalColumns].Merge = true;
            worksheet.Cells[2, 1, 2, totalColumns].Merge = true;

            // Column Headers (Row 4)
            int row = 4;
            int col = 1;

            worksheet.Cells[row, col++].Value = "Sl No.";
            worksheet.Cells[row, col++].Value = "Buyer Name";
            worksheet.Cells[row, col++].Value = "Style";
            worksheet.Cells[row, col++].Value = "Item";
            worksheet.Cells[row, col++].Value = "Total Order Quantity";

            foreach (var month in monthColumns)
            {
                worksheet.Cells[row, col++].Value = month;
            }

            // Style header row
            using (var range = worksheet.Cells[row, 1, row, totalColumns])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data Rows
            row = 5;
            foreach (var item in reportData.Data)
            {
                col = 1;
                if (decimal.TryParse(item.SlNo, out decimal sln))
                {
                    worksheet.Cells[row, col].Value = sln;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                col++;
                //worksheet.Cells[row, col++].Value = item.SlNo;
                worksheet.Cells[row, col++].Value = item.BuyerName;
                worksheet.Cells[row, col++].Value = item.Style;
                worksheet.Cells[row, col++].Value = item.Item;

                // Total Order Quantity - Numeric & Right Aligned
                if (decimal.TryParse(item.TotalOrderQuantity, out decimal totalQty))
                {
                    worksheet.Cells[row, col].Value = totalQty;
                    worksheet.Cells[row, col].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                col++;

                // Monthly Quantities - Numeric & Right Aligned
                foreach (var month in monthColumns)
                {
                    if (item.MonthlyQuantities.TryGetValue(month, out string value))
                    {
                        if (decimal.TryParse(value, out decimal monthQty))
                        {
                            worksheet.Cells[row, col].Value = monthQty;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "#,##0";
                            worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    }
                    else
                    {
                        worksheet.Cells[row, col].Value = 0;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "#,##0";
                        worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                    col++;
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Add borders to all data
            using (var range = worksheet.Cells[4, 1, row - 1, totalColumns])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            return package.GetAsByteArray();
        }

        // GET: Buyers Dropdown
        //[HttpGet]
        //public async Task<IActionResult> GetBuyers()
        //{
        //    try
        //    {
        //        ViewBag.buyersList = new SelectList(buyerRepo.All().Select(x => new { 'id' = x.BuyerId, 'name' = x.BuyerName }), 'id', 'name');
        //        //ViewBag.ProductList = new SelectList(productRepo.All().Select(x => new { x.ProductCode, x.ProductName }), "ProductCode", "ProductName");
        //        return Ok();
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

        // GET: Styles Dropdown
        [HttpGet]
        public async Task<IActionResult> GetStyles()
        {
            var styles = await _orderReportService.GetStylesAsync();
            return Json(styles);
        }

        // GET: Colors Dropdown
        [HttpGet]
        public async Task<IActionResult> GetColors()
        {
            var colors = await _orderReportService.GetColorsAsync();
            return Json(colors);
        }

        // GET: Sizes Dropdown
        [HttpGet]
        public async Task<IActionResult> GetSizes()
        {
            var sizes = await _orderReportService.GetSizesAsync();
            return Json(sizes);
        }
    }
}
