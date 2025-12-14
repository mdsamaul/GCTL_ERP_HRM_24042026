using AutoMapper;
using Dapper;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.RMG_CostingInfo;
using GCTL.Data.Models;
using GCTL.Service.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;

namespace GCTL.Service.RMG_CostingInfo
{
    public class RMG_CostingInfoService : AppService<RmgCostingInfo>, IRMG_CostingInfoService
    {
        private readonly IRepository<RmgCostingInfo> costingInfoRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<RmgProdOrderDetails> prodOrderDetailsRepo;
        private readonly IRepository<RmgProdTempColorSizeBreakup> tempColorSizeBreakupRepo;
        private readonly IRepository<RmgProdTempListColorSizeBreakup> tempListColorSizeBreakupRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> offiRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmDefDesignation> desRepo;
        private readonly IRepository<InvDefItem> itemRepo;
        private readonly IRepository<RmgProdDefUnitType> unitTypeRepo;
        private readonly IRepository<InvDefPortInfo> portRepo;
        private readonly IRepository<RmgProdDefBuyer> buyerRepo;
        private readonly IRepository<RmgProdDefBrand> buyerBrandRepo;
        private readonly IRepository<ProdDefStyle> styleRepo;
        private readonly IRepository<RmgProdDefSeason> seasonRepo;
        private readonly IRepository<CaDefCurrency> currencyRepo;
        private readonly IRepository<RmgProdDefColor> colorRepo;
        private readonly IRepository<RmgProdDefSize> sizeRepo;
        private readonly IRepository<RmgCostingDetailsTemp> rmgCostingDetailsTempRepo;
        private readonly IRepository<RmgCostingDetails> rmgCostingDetailsRepo;
        private readonly IRepository<SalesSupplier> supplieRepo;
        private readonly IMapper mapper;
        private readonly ICommonService commonService;
        private readonly string _connectionString;

        public RMG_CostingInfoService(
            IRepository<RmgCostingInfo> costingInfoRepo,
            IRepository<CoreAccessCode> accessCodeRepository,
            IRepository<RmgProdOrderDetails> ProdOrderDetailsRepo,
            IRepository<RmgProdTempColorSizeBreakup> TempColorSizeBreakupRepo,
            IRepository<RmgProdTempListColorSizeBreakup> TempListColorSizeBreakupRepo,
            IRepository<HrmEmployeeOfficialInfo> offiRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmDefDesignation> desRepo,
            IRepository<InvDefItem> itemRepo,
            IRepository<RmgProdDefUnitType> unitTypeRepo,
             IRepository<InvDefPortInfo> portRepo,
             IRepository<RmgProdDefBuyer> buyerRepo,
            IRepository<RmgProdDefBrand> buyerBrandRepo,
            IRepository<ProdDefStyle> styleRepo,
            IRepository<RmgProdDefSeason> seasonRepo,
             IRepository<CaDefCurrency> currencyRepo,
             IRepository<RmgProdDefColor> colorRepo,
             IRepository<RmgProdDefSize> sizeRepo,
             IRepository<RmgCostingDetailsTemp> RmgCostingDetailsTempRepo,
             IRepository<RmgCostingDetails> RmgCostingDetailsRepo,
            IConfiguration configuration,
              IRepository<SalesSupplier> supplieRepo,
            IMapper mapper,
            ICommonService commonService
            ) : base(costingInfoRepo)
        {
            this.costingInfoRepo = costingInfoRepo;
            this.accessCodeRepository = accessCodeRepository;
            prodOrderDetailsRepo = ProdOrderDetailsRepo;
            tempColorSizeBreakupRepo = TempColorSizeBreakupRepo;
            tempListColorSizeBreakupRepo = TempListColorSizeBreakupRepo;
            this.offiRepo = offiRepo;
            this.empRepo = empRepo;
            this.desRepo = desRepo;
            this.itemRepo = itemRepo;
            this.unitTypeRepo = unitTypeRepo;
            this.portRepo = portRepo;
            this.buyerRepo = buyerRepo;
            this.buyerBrandRepo = buyerBrandRepo;
            this.styleRepo = styleRepo;
            this.seasonRepo = seasonRepo;
            this.currencyRepo = currencyRepo;
            this.colorRepo = colorRepo;
            this.sizeRepo = sizeRepo;
            rmgCostingDetailsTempRepo = RmgCostingDetailsTempRepo;
            rmgCostingDetailsRepo = RmgCostingDetailsRepo;
            this.supplieRepo = supplieRepo;
            this.mapper = mapper;
            this.commonService = commonService;
            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }

        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";

        public async Task<bool> PagePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "RMGProdOrderInformationEntry" && x.TitleCheck);
        }

        public async Task<bool> SavePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "RMGProdOrderInformationEntry" && x.CheckAdd);
        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "RMGProdOrderInformationEntry" && x.CheckEdit);
        }

        public async Task<bool> DeletePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "RMGProdOrderInformationEntry" && x.CheckDelete);
        }

        public async Task<List<ProdOrderReportDto>> GetProdOrderReport(ProdOrderFilterDto filter)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@BuyerId", string.IsNullOrWhiteSpace(filter.BuyerId) ? null : filter.BuyerId);
                parameters.Add("@JobNo", string.IsNullOrWhiteSpace(filter.JobNo) ? null : filter.JobNo);
                parameters.Add("@StyleId", string.IsNullOrWhiteSpace(filter.StyleId) ? null : filter.StyleId);
                parameters.Add("@MPO", string.IsNullOrWhiteSpace(filter.MPO) ? null : filter.MPO);
                parameters.Add("@PurchaseOrder", string.IsNullOrWhiteSpace(filter.PurchaseOrder) ? null : filter.PurchaseOrder);

                var rawData = await connection.QueryAsync<ProdOrderReportRawDto>(
                    "GetProdOrderReport",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var groupedData = rawData
                    .GroupBy(x => new
                    {
                        x.BuyerId,
                        x.IntegraJOBNo,
                        x.StylePOWise,
                        x.StyleId,
                        x.MasterPurchaseOrder,
                        x.PurchaseOrder,
                        x.ProductId,
                        x.PDescription,
                        x.SupplierId,
                        x.DeliveryDate,
                        x.LUser,
                        x.ProductName,
                        x.StyleName,
                        x.BuyerName
                    })
                    .Select(g => new ProdOrderReportDto
                    {
                        BuyerId = g.Key.BuyerId,
                        BuyerName = g.Key.BuyerName,
                        IntegraJOBNo = g.Key.IntegraJOBNo,
                        StylePOWise = g.Key.StylePOWise,
                        StyleId = g.Key.StyleId,
                        StyleName = g.Key.StyleName,
                        MasterPurchaseOrder = g.Key.MasterPurchaseOrder,
                        PurchaseOrder = g.Key.PurchaseOrder,
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        PDescription = g.Key.PDescription,
                        SupplierId = g.Key.SupplierId,
                        DeliveryDate = g.Key.DeliveryDate,
                        LUser = g.Key.LUser,
                        ColorSizeBreakups = g
                            .Where(x => !string.IsNullOrWhiteSpace(x.ColorId) || !string.IsNullOrWhiteSpace(x.SizeId))
                            .Select(x => new ColorSizeBreakupDto
                            {
                                StyleId = g.Key.StyleId,
                                StyleName = styleRepo.All().Where(st => st.StyleId == g.Key.StyleId).Select(s => s.Style).FirstOrDefault(),
                                ColorId = x.ColorId,
                                ColorName = colorRepo.All().Where(c => c.ColorId == x.ColorId).Select(s => s.Color).FirstOrDefault(),
                                SizeId = x.SizeId,
                                SizeName = sizeRepo.All().Where(si => si.SizeId == x.SizeId).Select(s => s.Size).FirstOrDefault(),
                                Quantity = x.Quantity
                            })
                            .ToList()
                    })
                    .ToList();

                return groupedData;
            }
        }

        public async Task<FilterOptionsDto> GetFilterOptions(ProdOrderFilterDto filter = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@BuyerId", string.IsNullOrWhiteSpace(filter?.BuyerId) ? null : filter.BuyerId);
                parameters.Add("@JobNo", string.IsNullOrWhiteSpace(filter?.JobNo) ? null : filter.JobNo);
                parameters.Add("@StyleId", string.IsNullOrWhiteSpace(filter?.StyleId) ? null : filter.StyleId);
                parameters.Add("@MPO", string.IsNullOrWhiteSpace(filter?.MPO) ? null : filter.MPO);
                parameters.Add("@PurchaseOrder", string.IsNullOrWhiteSpace(filter?.PurchaseOrder) ? null : filter.PurchaseOrder);

                var result = await connection.QueryAsync<ProdOrderReportRawDto>(
                    "GetProdOrderReport",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var data = result.ToList();

                return new FilterOptionsDto
                {


                    Buyers = data.Where(x => !string.IsNullOrWhiteSpace(x.BuyerId))
                    .Select(x => x.BuyerId.Trim())
                    .Distinct()
                    .Select(id =>
                    {
                        var buyer = buyerRepo.All().FirstOrDefault(b => b.BuyerId == id);
                        return new BuyerDto
                        {
                            Id = id,
                            Name = buyer != null ? buyer.BuyerName : id
                        };
                    })
                    .OrderBy(x => x.Name)
                    .ToList(),


                    JobNos = data
                        .Where(x => !string.IsNullOrWhiteSpace(x.IntegraJOBNo))
                        .Select(x => new JobNoDto { Id = x.IntegraJOBNo.Trim(), Name = x.IntegraJOBNo.Trim() })
                        .GroupBy(x => x.Id)
                        .Select(g => g.First())
                        .OrderBy(x => x.Name)
                        .ToList(),

                    Styles = data
                        .Where(x => !string.IsNullOrWhiteSpace(x.StyleId))
                        .Select(x => x.StyleId.Trim())
                        .Distinct()
                        .Select(id =>
                        {
                            var style = styleRepo.All().FirstOrDefault(s => s.StyleId == id);
                            return new StyleDto
                            {
                                Id = id,
                                Name = style != null ? style.Style : id
                            };
                        })
                        .OrderBy(x => x.Name)
                        .ToList(),


                    MasterPOs = data
                        .Where(x => !string.IsNullOrWhiteSpace(x.MasterPurchaseOrder))
                        .Select(x => new MasterPODto { Id = x.MasterPurchaseOrder.Trim(), Name = x.MasterPurchaseOrder.Trim() })
                        .GroupBy(x => x.Id)
                        .Select(g => g.First())
                        .OrderBy(x => x.Name)
                        .ToList(),

                    PurchaseOrders = data
                        .Where(x => !string.IsNullOrWhiteSpace(x.PurchaseOrder))
                        .Select(x => new PurchaseOrderDto { Id = x.PurchaseOrder.Trim(), Name = x.PurchaseOrder.Trim() })
                        .GroupBy(x => x.Id)
                        .Select(g => g.First())
                        .OrderBy(x => x.Name)
                        .ToList()
                };
            }
        }

        public async Task<List<RmgCostingDetailsTempDto>> GetAllByCostingIdAsync(
    string costingId,
    bool clearTemp = true,
    string username = null)
        {
            if (clearTemp)
            {
                // after temporary rows delete
                //var existing = await rmgCostingDetailsTempRepo.All()
                //    .Where(x => x.CostingId == costingId)
                //    .ToListAsync();
                var existing = await rmgCostingDetailsTempRepo.All()
                    .ToListAsync();

                await rmgCostingDetailsTempRepo.DeleteRangeAsync(existing);

                // after auto-incremented ID generate
                var firstGeneratedCode = commonService.GenerateNextCode(
                                            "CostingDetailsId",
                                            "RMG_CostingDetails",
                                            8,
                                            "CO_DL_");

                int currentNumber = int.Parse(firstGeneratedCode.Replace("CO_DL_", ""));

                // 10 rows auto-increment IDs with insert
                for (int i = 1; i <= 10; i++)
                {
                    string nextId = "CO_DL_" + currentNumber.ToString("D8");
                    currentNumber++;

                    var entity = new RmgCostingDetailsTemp
                    {
                        CostingDetailsId = nextId,
                        CostingId = costingId,
                        Slno = i.ToString(),
                        ItemId = "",
                        Description = "",
                        Width = "",
                        ColorId = "",
                        SupplierId = "",
                        PoNo = "",
                        Quantity = 0,
                        Consumption = 0,
                        Extra = 0,
                        TotalQuantity = 0,
                        TotalQuantityUnit = "",
                        UnitPrice = 0,
                        TotalPriceCurrencyId = "",
                        TotalAmountShhkg = 0,
                        TotalAmountBdt = 0,
                        TotalAmountThb = 0,
                        ResponsibleBy = "",
                        Luser = username ?? "",
                        TotalPrice = 0,
                        BookingItemTypeId = ""
                    };


                    await rmgCostingDetailsTempRepo.AddAsync(entity);
                }
            }

            var tempDetails = await rmgCostingDetailsTempRepo.All()
                .Where(x => x.CostingId == costingId)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var dtoList = tempDetails.Select(detail => new RmgCostingDetailsTempDto
            {
                Id = detail.Id,
                CostingDetailsId = detail.CostingDetailsId ?? "",
                CostingId = detail.CostingId ?? "",
                Slno = detail.Slno ?? "",
                ItemId = detail.ItemId ?? "",
                Description = detail.Description ?? "",
                Width = detail.Width ?? "",
                ColorId = detail.ColorId ?? "",
                SupplierId = detail.SupplierId ?? "",
                PoNo = detail.PoNo ?? "",
                Quantity = detail.Quantity ?? 0,
                Consumption = detail.Consumption ?? 0,
                Extra = detail.Extra ?? 0,
                TotalQuantity = detail.TotalQuantity ?? 0,
                TotalQuantityUnit = detail.TotalQuantityUnit ?? "",
                UnitPrice = detail.UnitPrice ?? 0,
                TotalPriceCurrencyId = detail.TotalPriceCurrencyId ?? "",
                TotalAmountShhkg = detail.TotalAmountShhkg ?? 0,
                TotalAmountBdt = detail.TotalAmountBdt ?? 0,
                TotalAmountThb = detail.TotalAmountThb ?? 0,
                ResponsibleBy = detail.ResponsibleBy ?? ""
            }).ToList();


            return dtoList;
        }




        public async Task<RmgCostingDetailsTempDto> GetByIdAsync(string id)
        {
            var detail = await rmgCostingDetailsTempRepo.All().Where(x => x.CostingDetailsId == id).FirstOrDefaultAsync();
            if (detail == null) return null;

            return new RmgCostingDetailsTempDto
            {
                Id = detail.Id,
                CostingId = detail.CostingId,
                ItemId = detail.ItemId,
                Description = detail.Description,
                Width = detail.Width,
                ColorId = detail.ColorId,
                SupplierId = detail.SupplierId,
                PoNo = detail.PoNo,
                Quantity = detail.Quantity,
                Consumption = detail.Consumption,
                Extra = detail.Extra,
                TotalQuantity = detail.TotalQuantity,
                TotalQuantityUnit = detail.TotalQuantityUnit,
                UnitPrice = detail.UnitPrice,
                TotalPriceCurrencyId = detail.TotalPriceCurrencyId,
                TotalAmountShhkg = detail.TotalAmountShhkg,
                TotalAmountBdt = detail.TotalAmountBdt,
                TotalAmountThb = detail.TotalAmountThb,
                ResponsibleBy = detail.ResponsibleBy
            };
        }

        public async Task<RmgCostingDetailsTempDto> AddAsync(RmgCostingDetailsTempDto dto)
        {
            try
            {
                var entity = new RmgCostingDetailsTemp
                {
                    CostingDetailsId = dto.CostingDetailsId,
                    CostingId = dto.CostingId,
                    Slno = dto.Slno ?? "1",
                    BookingItemTypeId = dto.BookingItemTypeId ?? "",
                    ItemId = dto.ItemId ?? "",
                    Description = dto.Description ?? "",
                    Width = dto.Width ?? "",
                    ColorId = dto.ColorId ?? "",
                    SupplierId = dto.SupplierId ?? "",
                    PoNo = dto.PoNo ?? "",
                    Quantity = dto.Quantity ?? 0,
                    Consumption = dto.Consumption ?? 0,
                    Extra = dto.Extra ?? 0,
                    TotalQuantityUnit = dto.TotalQuantityUnit ?? "",
                    UnitPrice = dto.UnitPrice ?? 0,
                    TotalPriceCurrencyId = dto.TotalPriceCurrencyId ?? "",
                    ResponsibleBy = dto.ResponsibleBy ?? "",
                    Luser = "System"
                };

                CalculateRowTotals(entity);
                await rmgCostingDetailsTempRepo.AddAsync(entity);

                dto.Id = entity.Id;
                dto.TotalQuantity = entity.TotalQuantity;
                dto.TotalPrice = entity.TotalPrice;
                dto.TotalAmountShhkg = entity.TotalAmountShhkg;
                dto.TotalAmountBdt = entity.TotalAmountBdt;
                dto.TotalAmountThb = entity.TotalAmountThb;

                return dto;
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Save error: {innerMsg}", ex);
            }
        }

        public async Task<RmgCostingDetailsTempDto> UpdateAsync(RmgCostingDetailsTempDto dto)
        {
            var entity = await rmgCostingDetailsTempRepo.All().Where(x => x.Id.ToString() == dto.Id.ToString()).FirstOrDefaultAsync();
            if (entity == null) throw new Exception("Record not found");

            entity.ItemId = dto.ItemId;
            entity.Description = dto.Description;
            entity.Width = dto.Width;
            entity.ColorId = dto.ColorId;
            entity.SupplierId = dto.SupplierId;
            entity.PoNo = dto.PoNo;
            entity.Quantity = dto.Quantity;
            entity.Consumption = dto.Consumption;
            entity.Extra = dto.Extra;
            entity.TotalQuantityUnit = dto.TotalQuantityUnit;
            entity.UnitPrice = dto.UnitPrice;
            entity.TotalPriceCurrencyId = dto.TotalPriceCurrencyId;
            entity.ResponsibleBy = dto.ResponsibleBy;
            entity.TotalAmountShhkg = dto.TotalAmountShhkg;
            entity.TotalAmountBdt = dto.TotalAmountBdt;
            entity.TotalAmountThb = dto.TotalAmountThb;
            //CalculateRowTotals(entity);
            await rmgCostingDetailsTempRepo.UpdateAsync(entity);

            dto.TotalQuantity = entity.TotalQuantity;
            dto.TotalPrice = entity.TotalPrice;
            dto.TotalAmountShhkg = entity.TotalAmountShhkg;
            dto.TotalAmountBdt = entity.TotalAmountBdt;
            dto.TotalAmountThb = entity.TotalAmountThb;

            return dto;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await rmgCostingDetailsTempRepo.All().Where(x => x.Id.ToString() == id).FirstOrDefaultAsync();
            if (entity == null) return false;

            await rmgCostingDetailsTempRepo.DeleteAsync(entity);
            return true;
        }

        public async Task<bool> DeleteByCostingIdAsync(string costingId)
        {
            var entities = await rmgCostingDetailsTempRepo.All()
                .Where(x => x.CostingId == costingId)
                .ToListAsync();
            await rmgCostingDetailsTempRepo.DeleteRangeAsync(entities);

            return true;
        }
        public async Task<RmgCostingSummaryDto> CalculateSummaryAsync(string costingId, decimal damagePercent, decimal interestPercent, decimal cmAndProfit, decimal handlingCharge, decimal productionUpchargePercent)
        {
            var details = await rmgCostingDetailsTempRepo.All()
                .Where(x => x.CostingId == costingId)
                .ToListAsync();

            var summary = new RmgCostingSummaryDto();

            //  amount calculate
            var totalShhkg = details.Sum(x => x.TotalAmountShhkg ?? 0);
            var totalBdt = details.Sum(x => x.TotalAmountBdt ?? 0);
            var totalThb = details.Sum(x => x.TotalAmountThb ?? 0);

            summary.SubTotalShhkg = totalShhkg;
            summary.SubTotalBdt = totalBdt;
            summary.SubTotalThb = totalThb;
            summary.SubTotal = totalShhkg + totalBdt + totalThb;

            // Total Gar Qty ( last row  quantity)
            var totalGarQty = details
                .OrderByDescending(x => x.CostingDetailsId)
                .Select(s => s.Quantity)
                .FirstOrDefault() ?? 0;

            // Sub Total (per Gar. Qty) - 
            if (totalGarQty > 0)
            {
                summary.SubTotalPerGarQtyShhkg = totalShhkg / totalGarQty;
                summary.SubTotalPerGarQtyBdt = totalBdt / totalGarQty;
                summary.SubTotalPerGarQtyThb = totalThb / totalGarQty;
            }

            // Damage % - 
            summary.DamagePercent = damagePercent;
            summary.DamageAmountShhkg = summary.SubTotalPerGarQtyShhkg * (damagePercent / 100);
            summary.DamageAmountBdt = summary.SubTotalPerGarQtyBdt * (damagePercent / 100);
            summary.DamageAmountThb = summary.SubTotalPerGarQtyThb * (damagePercent / 100);

            // Interest/Overhead % - 
            summary.InterestOverheadPercent = interestPercent;
            summary.InterestOverheadAmountShhkg = summary.SubTotalPerGarQtyShhkg * (interestPercent / 100);
            summary.InterestOverheadAmountBdt = summary.SubTotalPerGarQtyBdt * (interestPercent / 100);
            summary.InterestOverheadAmountThb = summary.SubTotalPerGarQtyThb * (interestPercent / 100);

            // Total (Sub Total + Damage + Interest)
            summary.TotalShhkg = summary.SubTotalPerGarQtyShhkg + summary.DamageAmountShhkg + summary.InterestOverheadAmountShhkg;
            summary.TotalBdt = summary.SubTotalPerGarQtyBdt + summary.DamageAmountBdt + summary.InterestOverheadAmountBdt;
            summary.TotalThb = summary.SubTotalPerGarQtyThb + summary.DamageAmountThb + summary.InterestOverheadAmountThb;

            // Material Cost
            summary.TotalMaterialCostOverseas = summary.TotalShhkg;
            summary.TotalMaterialCostBangladesh = summary.TotalBdt;
            //summary.TotalMaterialCostBkk = summary.TotalThb * 1.2m; // +20%
            summary.TotalMaterialCostBkk = summary.TotalThb; // +20%

            // User inputs
            summary.CmAndProfit = cmAndProfit;
            summary.HandlingCharge = handlingCharge;
            summary.ProductionUpchargePercent = productionUpchargePercent;
            //summary.ProductionUpcharge = (summary.TotalMaterialCostOverseas + summary.TotalMaterialCostBangladesh + summary.TotalMaterialCostBkk) * (productionUpchargePercent / 100);
            summary.ProductionUpcharge = productionUpchargePercent;

            // FF Price
            summary.FfPrice = summary.TotalMaterialCostOverseas +
                              summary.TotalMaterialCostBangladesh +
                              summary.TotalMaterialCostBkk +
                              summary.CmAndProfit +
                              summary.HandlingCharge +
                              summary.ProductionUpcharge;

            // Grand Total
            summary.GrandTotal = summary.FfPrice * totalGarQty;

            return summary;
        }

        private void CalculateRowTotals(RmgCostingDetailsTemp entity)
        {
            if (entity.Quantity.HasValue && entity.Consumption.HasValue)
            {
                var baseTotal = entity.Quantity.Value * entity.Consumption.Value;
                var extraPercent = entity.Extra ?? 0;
                entity.TotalQuantity = baseTotal * (1 + extraPercent / 100);
            }

            if (entity.TotalQuantity.HasValue && entity.UnitPrice.HasValue)
            {
                entity.TotalPrice = entity.TotalQuantity.Value * entity.UnitPrice.Value;
            }

            if (entity.TotalPrice.HasValue && !string.IsNullOrEmpty(entity.TotalPriceCurrencyId))
            {
                entity.TotalAmountShhkg = 0;
                entity.TotalAmountBdt = 0;
                entity.TotalAmountThb = 0;

                switch (entity.TotalPriceCurrencyId.ToUpper())
                {
                    case "USD":
                    case "HKD":
                        entity.TotalAmountShhkg = entity.TotalPrice;
                        break;
                    case "BDT":
                        entity.TotalAmountBdt = entity.TotalPrice;
                        break;
                    case "THB":
                        entity.TotalAmountThb = entity.TotalPrice;
                        break;
                }
            }
        }

        // ========== EXCEL PREVIEW WITH EPPLUS ==========
        public async Task<List<RmgCostingDetailsTempDto>> PreviewExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                throw new Exception("Only Excel files (.xlsx, .xls) are supported");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var dtoList = new List<RmgCostingDetailsTempDto>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        throw new Exception("Worksheet not found in Excel file");

                    int rowCount = worksheet.Dimension?.End.Row ?? 0;
                    if (rowCount < 2)
                        throw new Exception("Excel file has no data rows");

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var Slno = worksheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(Slno)) continue;

                        var dto = new RmgCostingDetailsTempDto
                        {
                            Slno = Slno,
                            ItemName = worksheet.Cells[row, 2].Text?.Trim(),
                            Description = worksheet.Cells[row, 3].Text?.Trim(),
                            Width = worksheet.Cells[row, 4].Text?.Trim(),
                            ColorName = worksheet.Cells[row, 5].Text?.Trim(),
                            SupplierName = worksheet.Cells[row, 6].Text?.Trim(),
                            PoNo = worksheet.Cells[row, 7].Text?.Trim(),
                            Quantity = ParseDecimal(worksheet.Cells[row, 8].Value),
                            Consumption = ParseDecimal(worksheet.Cells[row, 9].Value),
                            TotalQuantity = ParseDecimal(worksheet.Cells[row, 10].Value),
                            UnitName = worksheet.Cells[row, 11].Text?.Trim(),
                            UnitPrice = ParseDecimal(worksheet.Cells[row, 12].Value),
                            ResponsibleByName = worksheet.Cells[row, 13].Text?.Trim()
                        };

                        dtoList.Add(dto);
                    }
                }
            }

            return dtoList;
        }

        public async Task<bool> ImportExcelAsync(IFormFile file, string costingId, string username)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx")
                throw new Exception("Only .xlsx files are supported");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var importList = new List<RmgCostingDetailsTemp>();

            var existingTempData = await rmgCostingDetailsTempRepo.All().ToListAsync();

            if (existingTempData.Any())
            {
                await rmgCostingDetailsTempRepo.DeleteRangeAsync(existingTempData);
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                try
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new Exception("Worksheet not found");

                        int rowCount = worksheet.Dimension?.End.Row ?? 0;
                        if (rowCount < 2)
                            throw new Exception("Excel file has no data rows");

                        // Load lookup data to memory first, then create dictionary
                        var itemsList = await itemRepo.All().ToListAsync();
                        var items = itemsList
                            .Where(x => !string.IsNullOrWhiteSpace(x.ItemName))
                            .GroupBy(x => x.ItemName.Trim())
                            .ToDictionary(g => g.Key, g => g.First().ItemId);

                        var colorsList = await colorRepo.All().ToListAsync();
                        var colors = colorsList
                            .Where(x => !string.IsNullOrWhiteSpace(x.Color))
                            .GroupBy(x => x.Color.Trim())
                            .ToDictionary(g => g.Key, g => g.First().ColorId);

                        var suppliersList = await supplieRepo.All().ToListAsync();
                        var suppliers = suppliersList
                            .Where(x => !string.IsNullOrWhiteSpace(x.SupplierName))
                            .GroupBy(x => x.SupplierName.Trim())
                            .ToDictionary(g => g.Key, g => g.First().SupplierId);

                        var unitsList = await unitTypeRepo.All().ToListAsync();
                        var units = unitsList
                            .Where(x => !string.IsNullOrWhiteSpace(x.UnitTypeName))
                            .GroupBy(x => x.UnitTypeName.Trim())
                            .ToDictionary(g => g.Key, g => g.First().UnitTypId);

                        // ✅ auto-incremented ID generate
                        var firstGeneratedCode = commonService.GenerateNextCode(
                            "CostingDetailsId",
                            "RMG_CostingDetails",
                            8,
                            "CO_DL_");
                        int currentNumber = int.Parse(firstGeneratedCode.Replace("CO_DL_", ""));

                        int rowNum = 1;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var material = worksheet.Cells[row, 2].Text?.Trim();
                            if (string.IsNullOrWhiteSpace(material)) continue;

                            items.TryGetValue(material, out var itemId);

                            var colorName = worksheet.Cells[row, 5].Text?.Trim();
                            string colorId = null;
                            if (!string.IsNullOrWhiteSpace(colorName))
                                colors.TryGetValue(colorName, out colorId);

                            var supplierName = worksheet.Cells[row, 6].Text?.Trim();
                            string supplierId = null;
                            if (!string.IsNullOrWhiteSpace(supplierName))
                                suppliers.TryGetValue(supplierName, out supplierId);

                            var unitName = worksheet.Cells[row, 11].Text?.Trim();
                            string unitId = null;
                            if (!string.IsNullOrWhiteSpace(unitName))
                                units.TryGetValue(unitName, out unitId);

                            var responsibleName = worksheet.Cells[row, 13].Text?.Trim();

                            // ✅ Auto-incremented ID generate
                            string nextId = "CO_DL_" + currentNumber.ToString("D8");

                            var entity = new RmgCostingDetailsTemp
                            {
                                CostingDetailsId = nextId,
                                CostingId = costingId,
                                Slno = rowNum.ToString(),
                                ItemId = itemId ?? "",
                                Description = worksheet.Cells[row, 3].Text?.Trim() ?? "",
                                Width = worksheet.Cells[row, 4].Text?.Trim() ?? "",
                                ColorId = colorId ?? "",
                                SupplierId = supplierId ?? "",
                                PoNo = worksheet.Cells[row, 7].Text?.Trim() ?? "",
                                Quantity = ParseDecimal(worksheet.Cells[row, 8].Value),
                                Consumption = ParseDecimal(worksheet.Cells[row, 9].Value),
                                Extra = 0,
                                TotalQuantityUnit = unitId ?? "",
                                UnitPrice = ParseDecimal(worksheet.Cells[row, 12].Value),
                                TotalPriceCurrencyId = "USD",
                                ResponsibleBy = responsibleName ?? "",
                                Luser = username
                            };

                            CalculateExcelRowTotals(entity);
                            importList.Add(entity);

                            rowNum++;
                            currentNumber++;
                        }
                    }
                }
                catch (InvalidDataException)
                {
                    throw new Exception("Invalid Excel file. Please upload a valid .xlsx file.");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error processing Excel: {ex.Message}");
                }
            }

            if (importList.Count == 0)
                throw new Exception("No valid data found in Excel file");

            // Delete old records first using EF Core
            var existingRecords = await rmgCostingDetailsTempRepo.All()
                .Where(x => x.CostingId == costingId)
                .ToListAsync();

            if (existingRecords.Any())
            {
                await rmgCostingDetailsTempRepo.DeleteRangeAsync(existingRecords);
            }

            // Bulk Insert using SqlBulkCopy
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var dataTable = new DataTable();
                        dataTable.Columns.Add("CostingDetailsID", typeof(string));
                        dataTable.Columns.Add("CostingID", typeof(string));
                        dataTable.Columns.Add("SLNO", typeof(string));
                        dataTable.Columns.Add("BookingItemTypeID", typeof(string));
                        dataTable.Columns.Add("ItemID", typeof(string));
                        dataTable.Columns.Add("Description", typeof(string));
                        dataTable.Columns.Add("Width", typeof(string));
                        dataTable.Columns.Add("ColorID", typeof(string));
                        dataTable.Columns.Add("SupplierID", typeof(string));
                        dataTable.Columns.Add("PoNo", typeof(string));
                        dataTable.Columns.Add("Quantity", typeof(decimal));
                        dataTable.Columns.Add("Consumption", typeof(decimal));
                        dataTable.Columns.Add("Extra", typeof(decimal));
                        dataTable.Columns.Add("TotalQuantity", typeof(decimal));
                        dataTable.Columns.Add("TotalQuantityUnit", typeof(string));
                        dataTable.Columns.Add("UnitPrice", typeof(decimal));
                        dataTable.Columns.Add("TotalPrice", typeof(decimal));
                        dataTable.Columns.Add("TotalPriceCurrencyId", typeof(string));
                        dataTable.Columns.Add("TotalAmountSHHKG", typeof(decimal));
                        dataTable.Columns.Add("TotalAmountBDT", typeof(decimal));
                        dataTable.Columns.Add("TotalAmountTHB", typeof(decimal));
                        dataTable.Columns.Add("ResponsibleBy", typeof(string));
                        dataTable.Columns.Add("LUser", typeof(string));

                        foreach (var item in importList)
                        {
                            var row = dataTable.NewRow();
                            row["CostingDetailsID"] = item.CostingDetailsId;
                            row["CostingID"] = item.CostingId;
                            row["SLNO"] = item.Slno ?? "";
                            row["BookingItemTypeID"] = item.BookingItemTypeId ?? "";
                            row["ItemID"] = item.ItemId ?? "";
                            row["Description"] = item.Description ?? "";
                            row["Width"] = item.Width ?? "";
                            row["ColorID"] = item.ColorId ?? "";
                            row["SupplierID"] = item.SupplierId ?? "";
                            row["PoNo"] = item.PoNo ?? "";
                            row["Quantity"] = item.Quantity ?? 0;
                            row["Consumption"] = item.Consumption ?? 0;
                            row["Extra"] = item.Extra ?? 0;
                            row["TotalQuantity"] = item.TotalQuantity ?? 0;
                            row["TotalQuantityUnit"] = item.TotalQuantityUnit ?? "";
                            row["UnitPrice"] = item.UnitPrice ?? 0;
                            row["TotalPrice"] = item.TotalPrice ?? 0;
                            row["TotalPriceCurrencyId"] = item.TotalPriceCurrencyId ?? "";
                            row["TotalAmountSHHKG"] = item.TotalAmountShhkg ?? 0;
                            row["TotalAmountBDT"] = item.TotalAmountBdt ?? 0;
                            row["TotalAmountTHB"] = item.TotalAmountThb ?? 0;
                            row["ResponsibleBy"] = item.ResponsibleBy ?? "";
                            row["LUser"] = username;
                            dataTable.Rows.Add(row);
                        }

                        using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                        {
                            bulkCopy.DestinationTableName = "RMG_CostingDetailsTemp";
                            bulkCopy.BatchSize = 1000;
                            bulkCopy.BulkCopyTimeout = 300;

                            bulkCopy.ColumnMappings.Add("CostingDetailsID", "CostingDetailsID");
                            bulkCopy.ColumnMappings.Add("CostingID", "CostingID");
                            bulkCopy.ColumnMappings.Add("SLNO", "SLNO");
                            bulkCopy.ColumnMappings.Add("BookingItemTypeID", "BookingItemTypeID");
                            bulkCopy.ColumnMappings.Add("ItemID", "ItemID");
                            bulkCopy.ColumnMappings.Add("Description", "Description");
                            bulkCopy.ColumnMappings.Add("Width", "Width");
                            bulkCopy.ColumnMappings.Add("ColorID", "ColorID");
                            bulkCopy.ColumnMappings.Add("SupplierID", "SupplierID");
                            bulkCopy.ColumnMappings.Add("PoNo", "PoNo");
                            bulkCopy.ColumnMappings.Add("Quantity", "Quantity");
                            bulkCopy.ColumnMappings.Add("Consumption", "Consumption");
                            bulkCopy.ColumnMappings.Add("Extra", "Extra");
                            bulkCopy.ColumnMappings.Add("TotalQuantity", "TotalQuantity");
                            bulkCopy.ColumnMappings.Add("TotalQuantityUnit", "TotalQuantityUnit");
                            bulkCopy.ColumnMappings.Add("UnitPrice", "UnitPrice");
                            bulkCopy.ColumnMappings.Add("TotalPrice", "TotalPrice");
                            bulkCopy.ColumnMappings.Add("TotalPriceCurrencyId", "TotalPriceCurrencyId");
                            bulkCopy.ColumnMappings.Add("TotalAmountSHHKG", "TotalAmountSHHKG");
                            bulkCopy.ColumnMappings.Add("TotalAmountBDT", "TotalAmountBDT");
                            bulkCopy.ColumnMappings.Add("TotalAmountTHB", "TotalAmountTHB");
                            bulkCopy.ColumnMappings.Add("ResponsibleBy", "ResponsibleBy");
                            bulkCopy.ColumnMappings.Add("LUser", "LUser");

                            await bulkCopy.WriteToServerAsync(dataTable);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Bulk insert failed: {ex.Message}", ex);
                    }
                }
            }
        }

        // ========== HELPER METHODS ==========
        private decimal? ParseDecimal(object value)
        {
            if (value == null) return null;
            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;
            return null;
        }

        private void CalculateExcelRowTotals(RmgCostingDetailsTemp entity)
        {
            // Total Quantity calculation
            if (entity.Quantity.HasValue && entity.Consumption.HasValue)
            {
                var baseTotal = entity.Quantity.Value * entity.Consumption.Value;
                var extraPercent = entity.Extra ?? 0;
                entity.TotalQuantity = baseTotal * (1 + extraPercent / 100);
            }

            // Total Price calculation
            if (entity.TotalQuantity.HasValue && entity.UnitPrice.HasValue)
            {
                entity.TotalPrice = entity.TotalQuantity.Value * entity.UnitPrice.Value;
            }

            // Initialize all amounts to 0
            entity.TotalAmountShhkg = 0;
            entity.TotalAmountBdt = 0;
            entity.TotalAmountThb = 0;

            // Set Currency and Amount based on ResponsibleBy
            if (entity.TotalPrice.HasValue && !string.IsNullOrWhiteSpace(entity.ResponsibleBy))
            {
                switch (entity.ResponsibleBy.ToUpper().Trim())
                {
                    case "BKK":
                        entity.TotalAmountShhkg = entity.TotalPrice;
                        entity.TotalPriceCurrencyId = "002"; // USD
                        break;
                    case "FF":
                        entity.TotalAmountBdt = entity.TotalPrice;
                        entity.TotalPriceCurrencyId = "001"; // BDT
                        break;
                    case "THB":
                        entity.TotalAmountThb = entity.TotalPrice;
                        entity.TotalPriceCurrencyId = "003"; // EUR
                        break;
                    default:
                        //  ResponsibleBy match  default USD
                        entity.TotalAmountShhkg = entity.TotalPrice;
                        entity.TotalPriceCurrencyId = "002";
                        break;
                }
            }
        }


        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(RmgCostingInfoDto model, string companyCode)
        {
            try
            {
                RmgCostingInfo entity;

                var mess = "";
                // Default values
                model.EntryDate = model.EntryDate == default ? DateTime.Now : model.EntryDate;
                model.ShipmentDate = model.ShipmentDate == default ? DateTime.Now : model.ShipmentDate;

                // ===========================
                // INSERT (NEW COSTING)
                // ===========================
                if (model.AutoId == 0)
                {
                    entity = new RmgCostingInfo
                    {
                        Luser = model.Luser ?? "",
                        Lip = model.Lip ?? "",
                        Lmac = model.Lmac ?? "",
                        Ldate = DateTime.Now,
                        CostingId = model.CostingId ?? Guid.NewGuid().ToString(),
                        EntryDate = model.EntryDate,
                        BuyerId = model.BuyerId ?? "",
                        StyleId = model.StyleId ?? "",
                        MasterPurchaseOrder = model.MasterPurchaseOrder ?? "",
                        PoNo = model.PoNo ?? "",
                        IntegraJobNo = model.IntegraJobNo ?? "",
                        ExportLcnoSc = model.ExportLcnoSc ?? "",
                        ShipmentDate = model.ShipmentDate,
                        FactorySuplier = model.FactorySuplier ?? "",
                        IssuedBy = model.IssuedBy ?? "",
                        CheckedBy = model.CheckedBy ?? "",
                        SubTotalAmountShhkg = model.SubTotalAmountShhkg,
                        SubTotalAmountBdt = model.SubTotalAmountBdt,
                        SubTotalAmountThb = model.SubTotalAmountThb,
                        DamagePercentage = model.DamagePercentage,
                        DamageAmountShhkg = model.DamageAmountShhkg,
                        DamageAmountBdt = model.DamageAmountBdt,
                        DamageAmountThb = model.DamageAmountThb,
                        InterestOverheadPercentage = model.InterestOverheadPercentage,
                        InterestOverheadShhkg = model.InterestOverheadShhkg,
                        InterestOverheadBdt = model.InterestOverheadBdt,
                        InterestOverheadThb = model.InterestOverheadThb,
                        TotalAmountShhkg = model.TotalAmountShhkg,
                        TotalAmountBdt = model.TotalAmountBdt,
                        TotalAmountThb = model.TotalAmountThb,
                        TotalMaterialCostOverseas = model.TotalMaterialCostOverseas,
                        TotalMaterialCostBdt = model.TotalMaterialCostBdt,
                        TotalMaterialCostBkk = model.TotalMaterialCostBkk,
                        CmandProfit = model.CmandProfit,
                        HandlingCharge = model.HandlingCharge,
                        ProductionUpCharge = model.ProductionUpCharge,
                        GrandTotal = model.GrandTotal,
                        Ffprice = model.Ffprice,
                        SubTotalByPerPcsShhkg = model.SubTotalByPerPcsShhkg ?? 0,
                        SubTotalByPerPcsBdt = model.SubTotalByPerPcsBdt ?? 0,
                        SubTotalByPerPcsThb = model.SubTotalByPerPcsThb ?? 0,
                        HandlingChargePerUnit = model.HandlingChargePerUnit ?? 0,
                        CmprofitUperUnit = model.CmprofitUperUnit ?? 0,
                        CompanyCode = companyCode,
                        EmployeId = model.UserInfoEmployeeId ?? ""
                    };

                    mess = CreateSuccess;
                    await costingInfoRepo.AddAsync(entity);
                }
                else
                {
                    // ===========================
                    // UPDATE (EXISTING COSTING)
                    // ===========================
                    entity = await costingInfoRepo.GetByIdAsync(model.AutoId);
                    if (entity == null)
                        return (false, "Update failed: record not found", null);

                    entity.ModifyDate = DateTime.Now;


                    // update all fields
                    entity.BuyerId = model.BuyerId ?? "";
                    entity.StyleId = model.StyleId ?? "";
                    entity.MasterPurchaseOrder = model.MasterPurchaseOrder ?? "";
                    entity.PoNo = model.PoNo ?? "";
                    entity.IntegraJobNo = model.IntegraJobNo ?? "";
                    entity.ExportLcnoSc = model.ExportLcnoSc ?? "";
                    entity.ShipmentDate = model.ShipmentDate;
                    entity.FactorySuplier = model.FactorySuplier ?? "";
                    entity.IssuedBy = model.IssuedBy ?? "";
                    entity.CheckedBy = model.CheckedBy ?? "";
                    entity.SubTotalAmountShhkg = model.SubTotalAmountShhkg;
                    entity.SubTotalAmountBdt = model.SubTotalAmountBdt;
                    entity.SubTotalAmountThb = model.SubTotalAmountThb;
                    entity.DamagePercentage = model.DamagePercentage;
                    entity.DamageAmountShhkg = model.DamageAmountShhkg;
                    entity.DamageAmountBdt = model.DamageAmountBdt;
                    entity.DamageAmountThb = model.DamageAmountThb;
                    entity.InterestOverheadPercentage = model.InterestOverheadPercentage;
                    entity.InterestOverheadShhkg = model.InterestOverheadShhkg;
                    entity.InterestOverheadBdt = model.InterestOverheadBdt;
                    entity.InterestOverheadThb = model.InterestOverheadThb;
                    entity.TotalAmountShhkg = model.TotalAmountShhkg;
                    entity.TotalAmountBdt = model.TotalAmountBdt;
                    entity.TotalAmountThb = model.TotalAmountThb;
                    entity.TotalMaterialCostOverseas = model.TotalMaterialCostOverseas;
                    entity.TotalMaterialCostBdt = model.TotalMaterialCostBdt;
                    entity.TotalMaterialCostBkk = model.TotalMaterialCostBkk;
                    entity.CmandProfit = model.CmandProfit;
                    entity.HandlingCharge = model.HandlingCharge;
                    entity.ProductionUpCharge = model.ProductionUpCharge;
                    entity.GrandTotal = model.GrandTotal;
                    entity.Ffprice = model.Ffprice;

                    mess = UpdateSuccess;
                    await costingInfoRepo.UpdateAsync(entity);
                }

                // ===========================
                // TEMP → MAIN SYNC START
                // ===========================

                try
                {
                    // 1️⃣ Main  data delete
                    var mainData = await rmgCostingDetailsRepo.All()
                        .Where(x => x.CostingId == entity.CostingId)
                        .ToListAsync();

                    if (mainData.Any())
                        await rmgCostingDetailsRepo.DeleteRangeAsync(mainData);

                    // 2️⃣ Temp থেকে main-এ data insert
                    var tempData = await rmgCostingDetailsTempRepo.All()
                        .Where(x => x.CostingId == entity.CostingId)
                        .ToListAsync();

                    var rowsToInsert = tempData.Select(t => new RmgCostingDetails
                    {
                        CostingDetailsId = t.CostingDetailsId ?? "",
                        CostingId = t.CostingId ?? "",
                        Slno = t.Slno ?? "",
                        BookingItemTypeId = t.BookingItemTypeId ?? "",
                        ItemId = t.ItemId ?? "",
                        Description = t.Description ?? "",
                        Width = t.Width ?? "",
                        ColorId = t.ColorId ?? "",
                        SupplierId = t.SupplierId ?? "",
                        PoNo = t.PoNo ?? "",
                        Quantity = t.Quantity ?? 0,
                        Consumption = t.Consumption ?? 0,
                        Extra = t.Extra ?? 0,
                        TotalQuantity = t.TotalQuantity ?? 0,
                        TotalQuantityUnit = t.TotalQuantityUnit ?? "",
                        UnitPrice = t.UnitPrice ?? 0,
                        TotalPrice = t.TotalPrice ?? 0,
                        TotalPriceCurrencyId = t.TotalPriceCurrencyId ?? "",
                        TotalAmountBdt = t.TotalAmountBdt ?? 0,
                        TotalAmountThb = t.TotalAmountThb ?? 0,
                        TotalAmountShhkg = t.TotalAmountShhkg ?? 0,
                        ResponsibleBy = t.ResponsibleBy ?? "",
                        BookinOrderNo = t.BookingItemTypeId ?? "",
                        Luser = model.Luser
                    }).ToList();

                    if (rowsToInsert.Any())
                        await rmgCostingDetailsRepo.AddRangeAsync(rowsToInsert);

                    //await _dbContext.SaveChangesAsync();
                    //await transaction.CommitAsync();

                    return (true, mess, entity);
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    return (false, ex.Message, entity);
                }

            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(int total, List<RmgCostingInfoListDto> data)> GetAllForDataTableAsync(
      int start,
      int length,
      string? search)
        {
            var query = costingInfoRepo.All();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.CostingId.Contains(search) ||
                    x.PoNo.Contains(search) ||
                    x.IntegraJobNo.Contains(search) ||
                    x.StyleId.Contains(search) ||
                    x.MasterPurchaseOrder.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.AutoId)
                .Skip(start)
                .Take(length)
                .Select(x => new RmgCostingInfoListDto
                {
                    AutoId = x.AutoId,
                    CostingId = x.CostingId,
                    EntryDate = x.EntryDate ?? DateTime.Now,
                    IntegraJobNo = x.IntegraJobNo,
                    StyleId = x.StyleId,
                    StyleName = styleRepo.All().Where(c => c.StyleId == x.StyleId).Select(s => s.Style).FirstOrDefault() ?? "",
                    MasterPurchaseOrder = x.MasterPurchaseOrder,
                    PoNo = x.PoNo,
                    ExportLcnoSc = x.ExportLcnoSc,
                    IssuedBy = x.IssuedBy,
                    CheckedBy = x.CheckedBy,
                    CheckedName = empRepo.All().Where(c => c.EmployeeId == x.CheckedBy).Select(w => w.FirstName + " " + w.LastName).FirstOrDefault(),
                    CreateDate = x.Ldate.HasValue ? x.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    ModifyDate = x.ModifyDate.HasValue ? x.Ldate.Value.ToString("dd/MM/yyyy") : ""
                })
                .ToListAsync();

            return (total, data);
        }

        public async Task<(bool isSuccess, string message)> DeleteAsync(int autoId)
        {
            try
            {
                var entity = await costingInfoRepo.GetByIdAsync(autoId);
                if (entity == null)
                    return (false, "Record not found");

                // Delete related details from temp table
                var tempDetails = await rmgCostingDetailsTempRepo.All()
                    .Where(x => x.CostingId == entity.CostingId)
                    .ToListAsync();

                if (tempDetails.Any())
                {
                    await rmgCostingDetailsTempRepo.DeleteRangeAsync(tempDetails);
                }

                // Delete related details from main table
                var mainDetails = await rmgCostingDetailsRepo.All()
                    .Where(x => x.CostingId == entity.CostingId)
                    .ToListAsync();

                if (mainDetails.Any())
                {
                    await rmgCostingDetailsRepo.DeleteRangeAsync(mainDetails);
                }

                // Delete main record
                await costingInfoRepo.DeleteAsync(entity);

                return (true, "Deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message, RmgCostingInfoDto data)> EditCostingAsync(int autoId)
        {
            try
            {
                var entity = await costingInfoRepo.GetByIdAsync(autoId);
                if (entity == null)
                    return (false, "Record not found", null);

                // Get details from main table and copy to temp table
                var mainDetails = await rmgCostingDetailsRepo.All()
                    .Where(x => x.CostingId == entity.CostingId)
                    .ToListAsync();

                // Clear existing temp data
                var existingTemp = await rmgCostingDetailsTempRepo.All()
                    .ToListAsync();

                if (existingTemp.Any())
                {
                    await rmgCostingDetailsTempRepo.DeleteRangeAsync(existingTemp);
                }

                // Copy to temp table
                if (mainDetails.Any())
                {
                    var tempList = mainDetails.Select(d => new RmgCostingDetailsTemp
                    {
                        CostingDetailsId = d.CostingDetailsId,
                        CostingId = d.CostingId,
                        Slno = d.Slno,
                        ItemId = d.ItemId ?? "",
                        Description = d.Description ?? "",
                        Width = d.Width ?? "",
                        ColorId = d.ColorId ?? "",
                        SupplierId = d.SupplierId ?? "",
                        PoNo = d.PoNo ?? "",
                        Quantity = d.Quantity ?? 0,
                        Consumption = d.Consumption ?? 0,
                        Extra = d.Extra ?? 0,
                        TotalQuantityUnit = d.TotalQuantityUnit ?? "",
                        UnitPrice = d.UnitPrice ?? 0,
                        TotalPriceCurrencyId = d.TotalPriceCurrencyId ?? "",
                        TotalAmountShhkg = d.TotalAmountShhkg ?? 0,
                        TotalAmountBdt = d.TotalAmountBdt ?? 0,
                        TotalAmountThb = d.TotalAmountThb ?? 0,
                        ResponsibleBy = d.ResponsibleBy ?? "",
                        TotalPrice = d.TotalPrice ?? 0,
                        TotalQuantity = d.TotalQuantity ?? 0,
                        BookingItemTypeId = d.BookingItemTypeId ?? "",
                        Luser = d.Luser,
                    }).ToList();

                    await rmgCostingDetailsTempRepo.AddRangeAsync(tempList);
                }

                var model = new RmgCostingInfoDto
                {
                    AutoId = entity.AutoId,
                    CostingId = entity.CostingId,
                    EntryDate = entity.EntryDate ?? DateTime.Now,
                    BuyerId = entity.BuyerId,
                    StyleId = entity.StyleId,
                    MasterPurchaseOrder = entity.MasterPurchaseOrder,
                    PoNo = entity.PoNo,
                    IntegraJobNo = entity.IntegraJobNo,
                    ExportLcnoSc = entity.ExportLcnoSc,
                    ShipmentDate = entity.ShipmentDate ?? DateTime.Now,
                    FactorySuplier = entity.FactorySuplier,
                    IssuedBy = entity.IssuedBy,
                    CheckedBy = entity.CheckedBy,
                    SubTotalAmountShhkg = entity.SubTotalAmountShhkg ?? 0,
                    SubTotalAmountBdt = entity.SubTotalAmountBdt ?? 0,
                    SubTotalAmountThb = entity.SubTotalAmountThb ?? 0,
                    DamagePercentage = entity.DamagePercentage ?? 0,
                    DamageAmountShhkg = entity.DamageAmountShhkg ?? 0,
                    DamageAmountBdt = entity.DamageAmountBdt ?? 0,
                    DamageAmountThb = entity.DamageAmountThb ?? 0,
                    InterestOverheadPercentage = entity.InterestOverheadPercentage ?? 0,
                    InterestOverheadShhkg = entity.InterestOverheadShhkg ?? 0,
                    InterestOverheadBdt = entity.InterestOverheadBdt ?? 0,
                    InterestOverheadThb = entity.InterestOverheadThb ?? 0,
                    TotalAmountShhkg = entity.TotalAmountShhkg ?? 0,
                    TotalAmountBdt = entity.TotalAmountBdt ?? 0,
                    TotalAmountThb = entity.TotalAmountThb ?? 0,
                    TotalMaterialCostOverseas = entity.TotalMaterialCostOverseas ?? 0,
                    TotalMaterialCostBdt = entity.TotalMaterialCostBdt ?? 0,
                    TotalMaterialCostBkk = entity.TotalMaterialCostBkk ?? 0,
                    CmandProfit = entity.CmandProfit ?? 0,
                    HandlingCharge = entity.HandlingCharge ?? 0,
                    ProductionUpCharge = entity.ProductionUpCharge ?? 0,
                    GrandTotal = entity.GrandTotal ?? 0,
                    Ffprice = entity.Ffprice ?? 0,
                    ShowCreateDate = entity.Ldate.HasValue ? entity.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    ShowModifyDate = entity.ModifyDate.HasValue ? entity.Ldate.Value.ToString("dd/MM/yyyy") : ""
                };

                return (true, "Data loaded successfully", model);
            }
            catch (Exception ex)
            {
                return (false, $"Failed: {ex.Message}", null);
            }
        }

        public async Task<CostingReportDto> GetCostingReportByIdAsync(
    string costingId,
    string integraJobNo,
    string purchaseOrder,
    string productId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var multi = await conn.QueryMultipleAsync(
                    "dbo.GetCostingReportByCostingId",
                    new
                    {
                        CostingId = costingId,
                        IntegraJOBNo = integraJobNo,
                        PurchaseOrder = purchaseOrder,
                        ProductId = productId
                    },
                    commandType: CommandType.StoredProcedure))
                {
                    var master = await multi.ReadFirstOrDefaultAsync<CostingReportDto>();
                    if (master == null)
                        return null;
                    var breakup = (await multi.ReadAsync<ColorSizeBreakupReportDto>()).ToList();
                    var details = (await multi.ReadAsync<CostingDetailReportDto>()).ToList();
                    master.ColorSizeBreakups = breakup;
                    master.Details = details;
                    return master;
                }
            }
        }
    }
}