using Dapper;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.MonthWiseOrderBookingReport;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace GCTL.Service.MonthWiseOrderBookingReport
{
    public class OrderReportService : AppService<OrderReportData>, IOrderReportService
    {
        private readonly string _connectionString;
        private readonly IRepository<OrderReportData> orderReportDataRepo;
        private readonly IRepository<RmgProdDefBuyer> buyerRepo;
        private readonly IRepository<ProdDefStyle> styleRepo;
        private readonly IRepository<InvDefItem> itemRepo;

        public OrderReportService(
            IRepository<OrderReportData> orderReportDataRepo,
            IRepository<RmgProdDefBuyer> buyerRepo,
            IRepository<ProdDefStyle> styleRepo,
            IRepository<InvDefItem> itemRepo,
            IConfiguration configuration
            ) : base(orderReportDataRepo)
        {

            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
            this.orderReportDataRepo = orderReportDataRepo;
            this.buyerRepo = buyerRepo;
            this.styleRepo = styleRepo;
            this.itemRepo = itemRepo;
        }





        //public async Task<OrderReportResponse> GetOrderReportAsync(OrderReportRequest request)
        //{
        //    try
        //    {
        //        using (var connection = new SqlConnection(_connectionString))
        //        {
        //            var parameters = new DynamicParameters();
        //            parameters.Add("@FromDate", request.FromDate);
        //            parameters.Add("@ToDate", request.ToDate);
        //            parameters.Add("@FromYear", request.FromYear);
        //            parameters.Add("@ToYear", request.ToYear);
        //            parameters.Add("@BuyerIds", request.BuyerIds != null && request.BuyerIds.Any()
        //                ? string.Join(",", request.BuyerIds) : null);
        //            parameters.Add("@StyleId", request.StyleIds != null && request.StyleIds.Any()
        //                ? string.Join(",", request.StyleIds) : null);
        //            parameters.Add("@PurchaseOrder", request.PurchaseOrders != null && request.PurchaseOrders.Any()
        //                ? string.Join(",", request.PurchaseOrders) : null);
        //            parameters.Add("@ColorId", request.ColorIds != null && request.ColorIds.Any()
        //                ? string.Join(",", request.ColorIds) : null);
        //            parameters.Add("@SizeId", request.SizeIds != null && request.SizeIds.Any()
        //                ? string.Join(",", request.SizeIds) : null);
        //            parameters.Add("@PageNumber", request.PageNumber);
        //            parameters.Add("@PageSize", request.PageSize);

        //            var result = await connection.QueryAsync<dynamic>(
        //                "sp_GetOrderReport",
        //                parameters,
        //                commandType: CommandType.StoredProcedure
        //            );

        //            var dataList = new List<OrderReportData>();
        //            int totalRecords = 0;

        //            foreach (var row in result)
        //            {
        //                var rowDict = (IDictionary<string, object>)row;
        //                var data = new OrderReportData
        //                {
        //                    OrderId = (int)rowDict["OrderId"],
        //                    BuyerId = rowDict["BuyerId"]?.ToString(),
        //                    BuyerOrderNo = rowDict["BuyerOrderNo"]?.ToString(),
        //                    MasterPurchaseOrder = rowDict["MasterPurchaseOrder"]?.ToString(),
        //                    StyleId = rowDict["StyleId"]?.ToString(),
        //                    DetailOrderId = (int)rowDict["DetailOrderId"],
        //                    ProductId = rowDict["ProductId"]?.ToString(),
        //                    PurchaseOrder = rowDict["PurchaseOrder"]?.ToString(),
        //                    Style = rowDict["Style"]?.ToString(),
        //                    RefNo = rowDict["RefNo"]?.ToString(),
        //                    ColorId = rowDict["ColorId"]?.ToString(),
        //                    SizeId = rowDict["SizeId"]?.ToString(),
        //                    TotalQuantity = (int)rowDict["TotalQuantity"],
        //                    TotalRecords = (int)rowDict["TotalRecords"],
        //                    MonthlyQuantities = new Dictionary<string, int>()
        //                };

        //                totalRecords = data.TotalRecords;

        //                // Extract dynamic month columns
        //                foreach (var key in rowDict.Keys)
        //                {
        //                    if (key.Contains("-") && int.TryParse(rowDict[key]?.ToString(), out int qty))
        //                    {
        //                        data.MonthlyQuantities[key] = qty;
        //                    }
        //                }

        //                dataList.Add(data);
        //            }

        //            return new OrderReportResponse
        //            {
        //                Success = true,
        //                Message = "Data retrieved successfully",
        //                Data = dataList,
        //                TotalRecords = totalRecords,
        //                TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize),
        //                CurrentPage = request.PageNumber
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new OrderReportResponse
        //        {
        //            Success = false,
        //            Message = $"Error: {ex.Message}",
        //            Data = new List<OrderReportData>(),
        //            TotalRecords = 0
        //        };
        //    }
        //}

        //public async Task<List<OrderReportData>> GetOrderReportAsync(OrderReportRequest request)
        //{
        //    using var connection = new SqlConnection(_connectionString);

        //    var parameters = new DynamicParameters();
        //    parameters.Add("@FromDate", request.FromDate, DbType.Date);
        //    parameters.Add("@ToDate", request.ToDate, DbType.Date);
        //    parameters.Add("@FromYear", request.FromYear, DbType.Int32);
        //    parameters.Add("@ToYear", request.ToYear, DbType.Int32);
        //    parameters.Add("@BuyerIds", request.BuyerIds?.Any() == true ? string.Join(",", request.BuyerIds) : null, DbType.String);
        //    parameters.Add("@StyleId", request.StyleIds?.Any() == true ? string.Join(",", request.StyleIds) : null, DbType.String);
        //    parameters.Add("@PurchaseOrder", request.PurchaseOrder, DbType.String);
        //    parameters.Add("@ColorId", request.ColorIds?.Any() == true ? string.Join(",", request.ColorIds) : null, DbType.String);
        //    parameters.Add("@SizeId", request.SizeIds?.Any() == true ? string.Join(",", request.SizeIds) : null, DbType.String);

        //    var result = await connection.QueryAsync<dynamic>("sp_GetOrderReport", parameters, commandType: CommandType.StoredProcedure);

        //    var data = new List<OrderReportData>();

        //    foreach (var row in result)
        //    {
        //        var dto = new OrderReportData
        //        {
        //            OrderId = row.OrderId?.ToString(),
        //            BuyerId = row.BuyerId?.ToString(),
        //            BuyerOrderNo = row.BuyerOrderNo?.ToString(),
        //            MasterPurchaseOrder = row.MasterPurchaseOrder?.ToString(),
        //            StyleId = row.StyleId?.ToString(),
        //            PODate = row.PODate != null ? ((DateTime)row.PODate).ToString("yyyy-MM-dd") : null,
        //            DetailOrderId = row.DetailOrderId?.ToString(),
        //            ProductId = row.ProductId?.ToString(),
        //            PurchaseOrder = row.PurchaseOrder?.ToString(),
        //            Style = row.Style?.ToString(),
        //            RefNo = row.RefNo?.ToString(),
        //            ColorId = row.ColorId?.ToString(),
        //            SizeId = row.SizeId?.ToString(),
        //            TotalQuantity = row.TotalQuantity?.ToString()
        //        };

        //        var rowDict = (IDictionary<string, object>)row;
        //        foreach (var kvp in rowDict)
        //        {
        //            if (kvp.Key.Contains("-"))
        //            {
        //                dto.MonthlyQuantities[kvp.Key] = kvp.Value?.ToString() ?? "0";
        //            }
        //        }

        //        data.Add(dto);
        //    }

        //    return data;
        //}



        // okay 

        //public async Task<List<OrderReportData>> GetOrderReportAsync(OrderReportRequest request)
        //{
        //    try
        //    {
        //        using var connection = new SqlConnection(_connectionString);

        //        var parameters = new DynamicParameters();
        //        parameters.Add("@FromDate", request.FromDate, DbType.Date);
        //        parameters.Add("@ToDate", request.ToDate, DbType.Date);
        //        parameters.Add("@FromYear", request.FromYear, DbType.Int32);
        //        parameters.Add("@ToYear", request.ToYear, DbType.Int32);
        //        parameters.Add("@BuyerIds", request.BuyerIds?.Any() == true ? string.Join(",", request.BuyerIds) : null, DbType.String);
        //        parameters.Add("@StyleId", request.StyleIds?.Any() == true ? string.Join(",", request.StyleIds) : null, DbType.String);
        //        parameters.Add("@PurchaseOrder", request.PurchaseOrders?.Any() == true ? string.Join(",", request.PurchaseOrders) : null, DbType.String);
        //        parameters.Add("@ColorId", request.ColorIds?.Any() == true ? string.Join(",", request.ColorIds) : null, DbType.String);
        //        parameters.Add("@SizeId", request.SizeIds?.Any() == true ? string.Join(",", request.SizeIds) : null, DbType.String);

        //        var result = await connection.QueryAsync<dynamic>("sp_GetOrderReport", parameters, commandType: CommandType.StoredProcedure);

        //        var data = new List<OrderReportData>();

        //        foreach (var row in result)
        //        {
        //            var dto = new OrderReportData
        //            {
        //                OrderId = row.OrderId?.ToString(),
        //                BuyerId = row.BuyerId?.ToString(),
        //                BuyerOrderNo = row.BuyerOrderNo?.ToString(),
        //                MasterPurchaseOrder = row.MasterPurchaseOrder?.ToString(),
        //                StyleId = row.StyleId?.ToString(),
        //                PODate = row.PODate != null ? ((DateTime)row.PODate).ToString("yyyy-MM-dd") : null,
        //                DetailOrderId = row.DetailOrderId?.ToString(),
        //                ProductId = row.ProductId?.ToString(),
        //                PurchaseOrder = row.PurchaseOrder?.ToString(),
        //                Style = row.Style?.ToString(),
        //                RefNo = row.RefNo?.ToString(),
        //                ColorId = row.ColorId?.ToString(),
        //                SizeId = row.SizeId?.ToString(),
        //                TotalQuantity = row.TotalQuantity?.ToString()
        //            };

        //            var rowDict = (IDictionary<string, object>)row;
        //            foreach (var kvp in rowDict)
        //            {
        //                if (kvp.Key.Contains("-"))
        //                {
        //                    dto.MonthlyQuantities[kvp.Key] = kvp.Value?.ToString() ?? "0";
        //                }
        //            }

        //            data.Add(dto);
        //        }

        //        return data;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}


        public async Task<OrderReportAllStyleResponse> GetOrderReportAllStyleAsync(OrderReportRequest request)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", request.FromDate, DbType.Date);
            parameters.Add("@ToDate", request.ToDate, DbType.Date);
            parameters.Add("@FromYear", request.FromYear, DbType.Int32);
            parameters.Add("@ToYear", request.ToYear, DbType.Int32);
            parameters.Add("@BuyerIds", request.BuyerIds?.Any() == true ? string.Join(",", request.BuyerIds) : null, DbType.String);
            parameters.Add("@StyleId", request.StyleIds?.Any() == true ? string.Join(",", request.StyleIds) : null, DbType.String);
            parameters.Add("@PurchaseOrder", request.PurchaseOrders?.Any() == true ? string.Join(",", request.PurchaseOrders) : null, DbType.String);
            parameters.Add("@ColorId", request.ColorIds?.Any() == true ? string.Join(",", request.ColorIds) : null, DbType.String);
            parameters.Add("@SizeId", request.SizeIds?.Any() == true ? string.Join(",", request.SizeIds) : null, DbType.String);

            var result = await connection.QueryAsync<dynamic>("sp_GetOrderReport", parameters, commandType: CommandType.StoredProcedure);

            // Group by Buyer only
            var groupedData = new Dictionary<string, OrderReportDataAllStyle>();
            var allMonthKeys = new HashSet<string>();
            int slNo = 1;

            foreach (var row in result)
            {
                string buyerId = row.BuyerId?.ToString() ?? "";
                string buyerName = buyerRepo.All().Where(x => x.BuyerId == buyerId).Select(x => x.BuyerName).FirstOrDefault() ?? "";

                if (!groupedData.ContainsKey(buyerName))
                {
                    groupedData[buyerName] = new OrderReportDataAllStyle
                    {
                        SlNo = slNo++.ToString(),
                        BuyerName = buyerName,
                        Style = "",
                        Item = "",
                        TotalOrderQuantity = "0"
                    };
                }

                var dto = groupedData[buyerName];

                // Collect unique styles
                string currentId = row.Style?.ToString() ?? "";
                string currentStyle = styleRepo.All().Where(x => x.StyleId == currentId).Select(e => e.Style).FirstOrDefault() ?? "";
                if (!string.IsNullOrEmpty(currentStyle) && !dto.Style.Contains(currentStyle))
                {
                    dto.Style = string.IsNullOrEmpty(dto.Style) ? currentStyle : dto.Style + ", " + currentStyle;
                }

                // Collect unique items
                string currentItemId = row.ProductId?.ToString() ?? "";
                string currentItem = itemRepo.All().Where(x => x.ItemId == currentItemId).Select(s => s.ItemName).FirstOrDefault() ?? "";
                if (!string.IsNullOrEmpty(currentItem) && !dto.Item.Contains(currentItem))
                {
                    dto.Item = string.IsNullOrEmpty(dto.Item) ? currentItem : dto.Item + ", " + currentItem;
                }

                // Extract monthly quantities
                var rowDict = (IDictionary<string, object>)row;
                foreach (var kvp in rowDict)
                {
                    if (kvp.Key.Contains("-"))
                    {
                        allMonthKeys.Add(kvp.Key);

                        decimal currentValue = 0;
                        if (dto.MonthlyQuantities.ContainsKey(kvp.Key))
                        {
                            decimal.TryParse(dto.MonthlyQuantities[kvp.Key], out currentValue);
                        }

                        decimal newValue = 0;
                        if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out newValue))
                        {
                            currentValue += newValue;
                        }

                        dto.MonthlyQuantities[kvp.Key] = currentValue.ToString();
                    }
                }
            }

            // Calculate total order quantity for each buyer
            foreach (var dto in groupedData.Values)
            {
                decimal total = 0;
                foreach (var qty in dto.MonthlyQuantities.Values)
                {
                    if (decimal.TryParse(qty, out decimal val))
                    {
                        total += val;
                    }
                }
                dto.TotalOrderQuantity = total.ToString();
            }

            // Determine report year
            string reportYear = "";
            if (request.FromYear.HasValue && request.ToYear.HasValue)
            {
                reportYear = request.FromYear == request.ToYear
                    ? $"({request.FromYear})"
                    : $"({request.FromYear} - {request.ToYear})";
            }
            else if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                int startYear = request.FromDate.Value.Year;
                int endYear = request.ToDate.Value.Year;
                reportYear = startYear == endYear
                    ? $"({startYear})"
                    : $"({startYear} - {endYear})";
            }

            return new OrderReportAllStyleResponse
            {
                CompanyName = "Integra Apparels (Bangladesh) Limited",
                ReportTitle = "Month Wise Order Booking Status",
                ReportYear = reportYear,
                Data = groupedData.Values.OrderBy(x => int.Parse(x.SlNo)).ToList()
            };
        }

        //public async Task<OrderReportAllStyleResponse> GetOrderReportAllStyleAsync(OrderReportRequest request)
        //{
        //    using var connection = new SqlConnection(_connectionString);

        //    var parameters = new DynamicParameters();
        //    parameters.Add("@FromDate", request.FromDate, DbType.Date);
        //    parameters.Add("@ToDate", request.ToDate, DbType.Date);
        //    parameters.Add("@FromYear", request.FromYear, DbType.Int32);
        //    parameters.Add("@ToYear", request.ToYear, DbType.Int32);
        //    parameters.Add("@BuyerIds", request.BuyerIds?.Any() == true ? string.Join(",", request.BuyerIds) : null, DbType.String);
        //    parameters.Add("@StyleId", request.StyleIds?.Any() == true ? string.Join(",", request.StyleIds) : null, DbType.String);
        //    parameters.Add("@PurchaseOrder", request.PurchaseOrders?.Any() == true ? string.Join(",", request.PurchaseOrders) : null, DbType.String);
        //    parameters.Add("@ColorId", request.ColorIds?.Any() == true ? string.Join(",", request.ColorIds) : null, DbType.String);
        //    parameters.Add("@SizeId", request.SizeIds?.Any() == true ? string.Join(",", request.SizeIds) : null, DbType.String);

        //    var result = await connection.QueryAsync<dynamic>("sp_GetOrderReport", parameters, commandType: CommandType.StoredProcedure);

        //    var groupedData = new Dictionary<string, OrderReportDataAllStyle>();
        //    var allMonthKeys = new HashSet<string>();
        //    int slNo = 1;

        //    foreach (var row in result)
        //    {
        //        string buyerId = row.BuyerId?.ToString() ?? "";
        //        string styleId = row.Style?.ToString() ?? "";
        //        string itemId = row.ProductId?.ToString() ?? "";

        //        string buyerName = buyerRepo.All().Where(x => x.BuyerId == buyerId).Select(x => x.BuyerName).FirstOrDefault() ?? "";
        //        string styleName = styleRepo.All().Where(x => x.StyleId == styleId).Select(x => x.Style).FirstOrDefault() ?? "";
        //        string itemName = itemRepo.All().Where(x => x.ItemId == itemId).Select(x => x.ItemName).FirstOrDefault() ?? "";

        //        // Key by Buyer + Style + Item
        //        string key = $"{buyerName}|{styleName}|{itemName}";

        //        if (!groupedData.ContainsKey(key))
        //        {
        //            groupedData[key] = new OrderReportDataAllStyle
        //            {
        //                SlNo = slNo++.ToString(),
        //                BuyerName = buyerName,
        //                Style = styleName,
        //                Item = itemName,
        //                TotalOrderQuantity = "0",
        //                MonthlyQuantities = new Dictionary<string, string>()
        //            };
        //        }

        //        var dto = groupedData[key];

        //        // Extract monthly quantities
        //        var rowDict = (IDictionary<string, object>)row;
        //        foreach (var kvp in rowDict)
        //        {
        //            if (kvp.Key.Contains("-"))
        //            {
        //                allMonthKeys.Add(kvp.Key);

        //                decimal currentValue = 0;
        //                if (dto.MonthlyQuantities.ContainsKey(kvp.Key))
        //                {
        //                    decimal.TryParse(dto.MonthlyQuantities[kvp.Key], out currentValue);
        //                }

        //                decimal newValue = 0;
        //                if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out newValue))
        //                {
        //                    currentValue += newValue;
        //                }

        //                dto.MonthlyQuantities[kvp.Key] = currentValue.ToString();
        //            }
        //        }
        //    }

        //    // Calculate total order quantity for each row
        //    foreach (var dto in groupedData.Values)
        //    {
        //        decimal total = 0;
        //        foreach (var qty in dto.MonthlyQuantities.Values)
        //        {
        //            if (decimal.TryParse(qty, out decimal val))
        //            {
        //                total += val;
        //            }
        //        }
        //        dto.TotalOrderQuantity = total.ToString();
        //    }

        //    // Determine report year
        //    string reportYear = "";
        //    if (request.FromYear.HasValue && request.ToYear.HasValue)
        //    {
        //        reportYear = request.FromYear == request.ToYear
        //            ? $"({request.FromYear})"
        //            : $"({request.FromYear} - {request.ToYear})";
        //    }
        //    else if (request.FromDate.HasValue && request.ToDate.HasValue)
        //    {
        //        int startYear = request.FromDate.Value.Year;
        //        int endYear = request.ToDate.Value.Year;
        //        reportYear = startYear == endYear
        //            ? $"({startYear})"
        //            : $"({startYear} - {endYear})";
        //    }

        //    return new OrderReportAllStyleResponse
        //    {
        //        CompanyName = "Integra Apparels (Bangladesh) Limited",
        //        ReportTitle = "Month Wise Order Booking Status",
        //        ReportYear = reportYear,
        //        Data = groupedData.Values.OrderBy(x => int.Parse(x.SlNo)).ToList()
        //    };
        //}

        //public async Task<OrderReportAllStyleResponse> GetOrderReportAllStyleAsync(OrderReportRequest request)
        //{
        //    using var connection = new SqlConnection(_connectionString);

        //    var parameters = new DynamicParameters();
        //    parameters.Add("@FromDate", request.FromDate, DbType.Date);
        //    parameters.Add("@ToDate", request.ToDate, DbType.Date);
        //    parameters.Add("@FromYear", request.FromYear, DbType.Int32);
        //    parameters.Add("@ToYear", request.ToYear, DbType.Int32);
        //    parameters.Add("@BuyerIds", request.BuyerIds?.Any() == true ? string.Join(",", request.BuyerIds) : null, DbType.String);
        //    parameters.Add("@StyleId", request.StyleIds?.Any() == true ? string.Join(",", request.StyleIds) : null, DbType.String);
        //    parameters.Add("@PurchaseOrder", request.PurchaseOrders?.Any() == true ? string.Join(",", request.PurchaseOrders) : null, DbType.String);
        //    parameters.Add("@ColorId", request.ColorIds?.Any() == true ? string.Join(",", request.ColorIds) : null, DbType.String);
        //    parameters.Add("@SizeId", request.SizeIds?.Any() == true ? string.Join(",", request.SizeIds) : null, DbType.String);

        //    var result = await connection.QueryAsync<dynamic>("sp_GetOrderReport", parameters, commandType: CommandType.StoredProcedure);

        //    // Group by Buyer only
        //    var groupedData = new Dictionary<string, OrderReportDataAllStyle>();
        //    var allMonthKeys = new HashSet<string>();
        //    int slNo = 1;

        //    foreach (var row in result)
        //    {
        //        string buyerName = row.BuyerId?.ToString() ?? "";

        //        if (!groupedData.ContainsKey(buyerName))
        //        {
        //            groupedData[buyerName] = new OrderReportDataAllStyle
        //            {
        //                SlNo = slNo++.ToString(),
        //                BuyerName = buyerName,
        //                Style = "",
        //                Item = "",
        //                TotalOrderQuantity = "0"
        //            };
        //        }

        //        var dto = groupedData[buyerName];

        //        // Collect unique styles
        //        string currentStyle = row.Style?.ToString() ?? "";
        //        if (!string.IsNullOrEmpty(currentStyle) && !dto.Style.Contains(currentStyle))
        //        {
        //            dto.Style = string.IsNullOrEmpty(dto.Style) ? currentStyle : dto.Style + ", " + currentStyle;
        //        }

        //        // Collect unique items
        //        string currentItem = row.ProductId?.ToString() ?? "";
        //        if (!string.IsNullOrEmpty(currentItem) && !dto.Item.Contains(currentItem))
        //        {
        //            dto.Item = string.IsNullOrEmpty(dto.Item) ? currentItem : dto.Item + ", " + currentItem;
        //        }

        //        // Extract monthly quantities
        //        var rowDict = (IDictionary<string, object>)row;
        //        foreach (var kvp in rowDict)
        //        {
        //            if (kvp.Key.Contains("-"))
        //            {
        //                allMonthKeys.Add(kvp.Key);

        //                decimal currentValue = 0;
        //                if (dto.MonthlyQuantities.ContainsKey(kvp.Key))
        //                {
        //                    decimal.TryParse(dto.MonthlyQuantities[kvp.Key], out currentValue);
        //                }

        //                decimal newValue = 0;
        //                if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out newValue))
        //                {
        //                    currentValue += newValue;
        //                }

        //                dto.MonthlyQuantities[kvp.Key] = currentValue.ToString();
        //            }
        //        }
        //    }

        //    // Calculate total order quantity for each buyer
        //    foreach (var dto in groupedData.Values)
        //    {
        //        decimal total = 0;
        //        foreach (var qty in dto.MonthlyQuantities.Values)
        //        {
        //            if (decimal.TryParse(qty, out decimal val))
        //            {
        //                total += val;
        //            }
        //        }
        //        dto.TotalOrderQuantity = total.ToString();
        //    }

        //    // Determine report year
        //    string reportYear = "";
        //    if (request.FromYear.HasValue && request.ToYear.HasValue)
        //    {
        //        reportYear = request.FromYear == request.ToYear
        //            ? $"({request.FromYear})"
        //            : $"({request.FromYear} - {request.ToYear})";
        //    }
        //    else if (request.FromDate.HasValue && request.ToDate.HasValue)
        //    {
        //        int startYear = request.FromDate.Value.Year;
        //        int endYear = request.ToDate.Value.Year;
        //        reportYear = startYear == endYear
        //            ? $"({startYear})"
        //            : $"({startYear} - {endYear})";
        //    }

        //    return new OrderReportAllStyleResponse
        //    {
        //        CompanyName = "Integra Apparels (Bangladesh) Limited",
        //        ReportTitle = "Month Wise Order Booking Status",
        //        ReportYear = reportYear,
        //        Data = groupedData.Values.OrderBy(x => int.Parse(x.SlNo)).ToList()
        //    };
        //}

        //public async Task<OrderReportAllStyleResponse> GetOrderReportAllStyleAsync(OrderReportRequest request)
        //{
        //    using var connection = new SqlConnection(_connectionString);

        //    var parameters = new DynamicParameters();
        //    parameters.Add("@FromDate", request.FromDate, DbType.Date);
        //    parameters.Add("@ToDate", request.ToDate, DbType.Date);
        //    parameters.Add("@FromYear", request.FromYear, DbType.Int32);
        //    parameters.Add("@ToYear", request.ToYear, DbType.Int32);
        //    parameters.Add("@BuyerIds", request.BuyerIds?.Any() == true ? string.Join(",", request.BuyerIds) : null, DbType.String);
        //    parameters.Add("@StyleId", request.StyleIds?.Any() == true ? string.Join(",", request.StyleIds) : null, DbType.String);
        //    parameters.Add("@PurchaseOrder", request.PurchaseOrders?.Any() == true ? string.Join(",", request.PurchaseOrders) : null, DbType.String);
        //    parameters.Add("@ColorId", request.ColorIds?.Any() == true ? string.Join(",", request.ColorIds) : null, DbType.String);
        //    parameters.Add("@SizeId", request.SizeIds?.Any() == true ? string.Join(",", request.SizeIds) : null, DbType.String);

        //    var result = await connection.QueryAsync<dynamic>("sp_GetOrderReport", parameters, commandType: CommandType.StoredProcedure);

        //    // Group by Buyer, Style, Item to remove duplicates
        //    var groupedData = new Dictionary<string, OrderReportDataAllStyle>();
        //    var allMonthKeys = new HashSet<string>();
        //    int slNo = 1;

        //    foreach (var row in result)
        //    {
        //        string buyerName = row.BuyerId?.ToString() ?? "";
        //        string style = row.Style?.ToString() ?? "";
        //        string item = row.ProductId?.ToString() ?? "";

        //        string key = $"{buyerName}|{style}|{item}";

        //        if (!groupedData.ContainsKey(key))
        //        {
        //            groupedData[key] = new OrderReportDataAllStyle
        //            {
        //                SlNo = slNo++.ToString(),
        //                BuyerName = buyerName,
        //                Style = style,
        //                Item = item,
        //                TotalOrderQuantity = "0"
        //            };
        //        }

        //        var dto = groupedData[key];

        //        // Extract monthly quantities
        //        var rowDict = (IDictionary<string, object>)row;
        //        foreach (var kvp in rowDict)
        //        {
        //            if (kvp.Key.Contains("-"))
        //            {
        //                allMonthKeys.Add(kvp.Key);

        //                decimal currentValue = 0;
        //                if (dto.MonthlyQuantities.ContainsKey(kvp.Key))
        //                {
        //                    decimal.TryParse(dto.MonthlyQuantities[kvp.Key], out currentValue);
        //                }

        //                decimal newValue = 0;
        //                if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out newValue))
        //                {
        //                    currentValue += newValue;
        //                }

        //                dto.MonthlyQuantities[kvp.Key] = currentValue.ToString();
        //            }
        //        }
        //    }

        //    // Calculate total order quantity for each item
        //    foreach (var dto in groupedData.Values)
        //    {
        //        decimal total = 0;
        //        foreach (var qty in dto.MonthlyQuantities.Values)
        //        {
        //            if (decimal.TryParse(qty, out decimal val))
        //            {
        //                total += val;
        //            }
        //        }
        //        dto.TotalOrderQuantity = total.ToString();
        //    }

        //    // Determine report year
        //    string reportYear = "";
        //    if (request.FromYear.HasValue && request.ToYear.HasValue)
        //    {
        //        reportYear = request.FromYear == request.ToYear
        //            ? $"({request.FromYear})"
        //            : $"({request.FromYear} - {request.ToYear})";
        //    }
        //    else if (request.FromDate.HasValue && request.ToDate.HasValue)
        //    {
        //        int startYear = request.FromDate.Value.Year;
        //        int endYear = request.ToDate.Value.Year;
        //        reportYear = startYear == endYear
        //            ? $"({startYear})"
        //            : $"({startYear} - {endYear})";
        //    }

        //    return new OrderReportAllStyleResponse
        //    {
        //        CompanyName = "Integra Apparels (Bangladesh) Limited",
        //        ReportTitle = "Month Wise Order Booking Status",
        //        ReportYear = reportYear,
        //        Data = groupedData.Values.OrderBy(x => int.Parse(x.SlNo)).ToList()
        //    };
        //}



        //public async Task<List<BuyerMaster>> GetBuyersAsync()
        //{
        //    try
        //    {
        //        using (var connection = new SqlConnection(_connectionString))
        //        {
        //            var query = "SELECT BuyerId, BuyerName FROM RMG_Master_Buyer WHERE IsActive = 1 ORDER BY BuyerName";
        //            var result = await connection.QueryAsync<BuyerMaster>(query);
        //            return result.ToList();
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

        public async Task<List<StyleMaster>> GetStylesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT StyleId, StyleName FROM RMG_Master_Style WHERE IsActive = 1 ORDER BY StyleName";
                var result = await connection.QueryAsync<StyleMaster>(query);
                return result.ToList();
            }
        }

        public async Task<List<ColorMaster>> GetColorsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT ColorId, ColorName FROM RMG_Master_Color WHERE IsActive = 1 ORDER BY ColorName";
                var result = await connection.QueryAsync<ColorMaster>(query);
                return result.ToList();
            }
        }

        public async Task<List<SizeMaster>> GetSizesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT SizeId, SizeName FROM RMG_Master_Size WHERE IsActive = 1 ORDER BY SizeName";
                var result = await connection.QueryAsync<SizeMaster>(query);
                return result.ToList();
            }
        }

        //Task<OrderReportResponse> IOrderReportService.GetOrderReportAsync(OrderReportRequest request)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
