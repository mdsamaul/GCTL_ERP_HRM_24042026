using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.PrintingStationeryPurchaseReport;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GCTL.Service.PrintingStationeryPurchaseReportService
{
    public class PrintingStationeryPurchaseReportService : AppService<RmgPurchaseOrderReceive>, IPrintingStationeryPurchaseReportService
    {
        private readonly IRepository<RmgPurchaseOrderReceive> purchaseOrderReceiveRepo;
        private readonly string _connectionString;

        public PrintingStationeryPurchaseReportService(
            IRepository<RmgPurchaseOrderReceive> purchaseOrderReceiveRepo,
            IConfiguration configuration
            ) : base(purchaseOrderReceiveRepo)
        {
            this.purchaseOrderReceiveRepo = purchaseOrderReceiveRepo;
            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }

        public async Task<List<PrintingStationeryPurchaseReportResultDto>> GetAllPROCPrintingAndStationeryReport(PrintingStationeryPurchaseReportFilterDto model)
        {
            try
            {
                var results = new List<PrintingStationeryPurchaseReportResultDto>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("PROCPrintingAndStationeryReport", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CatagoryID", model.CategoryIds != null ? string.Join(",", model.CategoryIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProductID", model.ProductIds != null ? string.Join(",", model.ProductIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@BrandID", model.BrandIds != null ? string.Join(",", model.BrandIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModelID", model.ModelIds != null ? string.Join(",", model.ModelIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", model.FromDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", model.ToDate ?? (object)DBNull.Value);

                        await conn.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(new PrintingStationeryPurchaseReportResultDto
                                {
                                    PurchaseOrderReceiveDetailsID = reader["PurchaseOrderReceiveDetailsID"] != DBNull.Value ? Convert.ToDecimal(reader["PurchaseOrderReceiveDetailsID"]) : 0,
                                    ProductCode = reader["ProductCode"]?.ToString() ?? "",
                                    ProductName = reader["ProductName"]?.ToString() ?? "",
                                    Description = reader["Description"]?.ToString() ?? "",

                                    CatagoryID = reader["CatagoryID"]?.ToString() ?? "",
                                    CatagoryName = reader["CatagoryName"]?.ToString() ?? "",

                                    BrandID = reader["BrandID"]?.ToString() ?? "",
                                    BrandName = reader["BrandName"]?.ToString() ?? "",

                                    ModelID = reader["ModelID"]?.ToString() ?? "",
                                    ModelName = reader["ModelName"]?.ToString() ?? "",

                                    SizeID = reader["SizeID"]?.ToString() ?? "",
                                    SizeName = reader["SizeName"]?.ToString() ?? "",

                                    WarrantyPeriod = reader["WarrantyPeriod"]?.ToString() ?? "0",
                                    PeriodName = reader["PeriodName"]?.ToString() ?? "",

                                    ReceiveDate = reader["ReceiveDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReceiveDate"]) : DateTime.MinValue,
                                    ReqQty = reader["ReqQty"]?.ToString() ?? "0",

                                    UnitTypeName = reader["UnitTypeName"]?.ToString() ?? "",
                                    PurchaseCost = reader["PurchaseCost"] != DBNull.Value ? Convert.ToDecimal(reader["PurchaseCost"]) : 0,
                                    TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0
                                });
                            }
                        }
                    }
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PrintingStationeryDropdownDto> GetFilteredDropdownsAsync(PrintingStationeryPurchaseReportFilterDto model)
        {
            try
            {
                var dropdownDto = new PrintingStationeryDropdownDto
                {
                    CategoryIds = new List<DropdownItemDto>(),
                    ProductIds = new List<DropdownItemDto>(),
                    BrandIds = new List<DropdownItemDto>(),
                    ModelIds = new List<DropdownItemDto>()
                };

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("PROCPrintingAndStationeryReport", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CatagoryID", model.CategoryIds != null ? string.Join(",", model.CategoryIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProductID", model.ProductIds != null ? string.Join(",", model.ProductIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@BrandID", model.BrandIds != null ? string.Join(",", model.BrandIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModelID", model.ModelIds != null ? string.Join(",", model.ModelIds) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate",  (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", (object)DBNull.Value);

                        await conn.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var categoryDict = new Dictionary<string, string>();
                            var productDict = new Dictionary<string, string>();
                            var brandDict = new Dictionary<string, string>();
                            var modelDict = new Dictionary<string, string>();

                            while (await reader.ReadAsync())
                            {
                                var catId = reader["CatagoryID"]?.ToString();
                                var catName = reader["CatagoryName"]?.ToString();

                                var prodId = reader["ProductCode"]?.ToString();
                                var prodName = reader["ProductName"]?.ToString();

                                var brandId = reader["BrandID"]?.ToString();
                                var brandName = reader["BrandName"]?.ToString();

                                var modelId = reader["ModelID"]?.ToString();
                                var modelName = reader["ModelName"]?.ToString();

                                if (!string.IsNullOrWhiteSpace(catId) && !categoryDict.ContainsKey(catId))
                                    categoryDict[catId] = catName;

                                if (!string.IsNullOrWhiteSpace(prodId) && !productDict.ContainsKey(prodId))
                                    productDict[prodId] = prodName;

                                if (!string.IsNullOrWhiteSpace(brandId) && !brandDict.ContainsKey(brandId))
                                    brandDict[brandId] = brandName;

                                if (!string.IsNullOrWhiteSpace(modelId) && !modelDict.ContainsKey(modelId))
                                    modelDict[modelId] = modelName;
                            }

                            dropdownDto.CategoryIds = categoryDict.Select(x => new DropdownItemDto { Id = x.Key, Name = x.Value }).ToList();
                            dropdownDto.ProductIds = productDict.Select(x => new DropdownItemDto { Id = x.Key, Name = x.Value }).ToList();
                            dropdownDto.BrandIds = brandDict.Select(x => new DropdownItemDto { Id = x.Key, Name = x.Value }).ToList();
                            dropdownDto.ModelIds = modelDict.Select(x => new DropdownItemDto { Id = x.Key, Name = x.Value }).ToList();
                        }
                    }
                }

                return dropdownDto;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                throw;
            }
        }


    }
}
