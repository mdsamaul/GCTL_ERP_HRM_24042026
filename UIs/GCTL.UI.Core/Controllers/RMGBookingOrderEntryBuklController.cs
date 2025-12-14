using GCTL.Core.Data;
using GCTL.Core.ViewModels.RMGBookingOrderEntryBukl;
using GCTL.Data.Models;
using GCTL.Service.Common;
using GCTL.Service.RMGBookingOrderEntryBukl;
using GCTL.UI.Core.ViewModels.RMGBookingOrderEntryBukl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GCTL.UI.Core.Controllers
{
    public class RMGBookingOrderEntryBuklController : BaseController
    {
        private readonly IRepository<InvDefItem> itemRepo;
        private readonly IRepository<ProdDefStyle> styleRepo;
        private readonly IRepository<RmgProdDefColor> colorRepo;
        private readonly IRepository<RmgProdDefUnitType> unitRepo;
        private readonly IRepository<RmgProdDefSize> sizeRepo;
        private readonly IRepository<RmgProdDefThreadCount> threadCountRepo;
        private readonly IRepository<CaDefCurrency> currenciesRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsCarton> cartonRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsButton> buttonRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsButtonTemp> buttonTempRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsExtra> extraRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsExtraTemp> extraTempRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsFebric> febricRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsFebricTemp> febricTempRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsCartonTemp> cartonTempRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsPoly> polyRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsPolyTemp> polyTempRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsThread> threadRepo;
        private readonly IRepository<RmgInvBookingReceivedDetailsThreadTemp> threadTempRepo;
        private readonly IRepository<InvDefBookingItemType> bTypeRepo;
        private readonly IRepository<RmgProdDefBuyer> buyerRepo;
        private readonly IRepository<RmgDefSupplier> supplierRepo;
        private readonly IRepository<CaDefCountry> countryRepo;
        private readonly IRepository<HrmEmployee2> empRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffiRepo;
        private readonly IRepository<RmgProdDefDeliveryMethod> deliveryRepo;
        private readonly IRepository<SalesDefPaymentTerms> paymentTermRepo;
        private readonly IRepository<HrmDefDesignation> degRepo;
        private readonly ICommonService commonService;
        private readonly IRMGBookingOrderEntryBuklService rmgBookingOrderEntryBuklService;
        private readonly string _connectionString;




        public RMGBookingOrderEntryBuklController(
            IRepository<InvDefItem> itemRepo,
            IRepository<ProdDefStyle> styleRepo,
            IRepository<RmgProdDefColor> colorRepo,
            IRepository<RmgProdDefUnitType> unitRepo,
            IRepository<RmgProdDefSize> sizeRepo,
            IRepository<RmgProdDefThreadCount> threadCountRepo,
            IRepository<CaDefCurrency> currenciesRepo,
            IRepository<RmgInvBookingReceivedDetailsCarton> cartonRepo,
            IRepository<RmgInvBookingReceivedDetailsButton> buttonRepo,
            IRepository<RmgInvBookingReceivedDetailsButtonTemp> buttonTempRepo,
            IRepository<RmgInvBookingReceivedDetailsExtra> extraRepo,
            IRepository<RmgInvBookingReceivedDetailsExtraTemp> extraTempRepo,
            IRepository<RmgInvBookingReceivedDetailsFebric> febricRepo,
            IRepository<RmgInvBookingReceivedDetailsFebricTemp> febricTempRepo,
            IRepository<RmgInvBookingReceivedDetailsCartonTemp> cartonTempRepo,
            IRepository<RmgInvBookingReceivedDetailsPoly> polyRepo,
            IRepository<RmgInvBookingReceivedDetailsPolyTemp> polyTempRepo,
            IRepository<RmgInvBookingReceivedDetailsThread> threadRepo,
            IRepository<RmgInvBookingReceivedDetailsThreadTemp> threadTempRepo,
            IRepository<InvDefBookingItemType> bTypeRepo,
            IRepository<RmgProdDefBuyer> buyerRepo,
            IRepository<RmgDefSupplier> supplierRepo,
            IRepository<CaDefCountry> countryRepo,
            IRepository<HrmEmployee2> empRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffiRepo,
            IRepository<RmgProdDefDeliveryMethod> deliveryRepo,
            IRepository<SalesDefPaymentTerms> paymentTermRepo,
            IRepository<HrmDefDesignation> degRepo,
            IConfiguration configuration,
            ICommonService commonService,
            IRMGBookingOrderEntryBuklService rmgBookingOrderEntryBuklService
            )
        {
            this.itemRepo = itemRepo;
            this.styleRepo = styleRepo;
            this.colorRepo = colorRepo;
            this.unitRepo = unitRepo;
            this.sizeRepo = sizeRepo;
            this.threadCountRepo = threadCountRepo;
            this.currenciesRepo = currenciesRepo;
            this.cartonRepo = cartonRepo;
            this.buttonRepo = buttonRepo;
            this.buttonTempRepo = buttonTempRepo;
            this.extraRepo = extraRepo;
            this.extraTempRepo = extraTempRepo;
            this.febricRepo = febricRepo;
            this.febricTempRepo = febricTempRepo;
            this.cartonTempRepo = cartonTempRepo;
            this.polyRepo = polyRepo;
            this.polyTempRepo = polyTempRepo;
            this.threadRepo = threadRepo;
            this.threadTempRepo = threadTempRepo;
            this.bTypeRepo = bTypeRepo;
            this.buyerRepo = buyerRepo;
            this.supplierRepo = supplierRepo;
            this.countryRepo = countryRepo;
            this.empRepo = empRepo;
            this.empOffiRepo = empOffiRepo;
            this.deliveryRepo = deliveryRepo;
            this.paymentTermRepo = paymentTermRepo;
            this.degRepo = degRepo;
            this.commonService = commonService;
            this.rmgBookingOrderEntryBuklService = rmgBookingOrderEntryBuklService;
            //this.configuration = configuration.GetConnectionString("ApplicationDbConnection");
            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }
        public async Task<IActionResult> Index()
        {

            ViewBag.BookingTypeList = new SelectList(bTypeRepo.All().Select(x => new { id = x.BookingItemTypeId, name = x.BookingItemType }), "id", "name");
            ViewBag.SupplierList = new SelectList(supplierRepo.All().Select(x => new { id = x.SupplierId, name = x.SupplierName }), "id", "name");
            ViewBag.CountryList = new SelectList(countryRepo.All().Select(x => new { id = x.CountryId, name = x.CountryName }), "id", "name");
            ViewBag.CurrencyList = new SelectList(currenciesRepo.All().Select(x => new { id = x.CurrencyId, name = x.CurrencyName }), "id", "name");
            ViewBag.pTermList = new SelectList(paymentTermRepo.All().Select(x => new { id = x.PaymentTermsId, name = x.PaymentTermsName }), "id", "name");
            ViewBag.deliveryList = new SelectList(deliveryRepo.All().Select(x => new { id = x.DeliveryMethodId, name = x.DeliveryMethod }), "id", "name");
            var empList = new List<EmployeeDto>();

            string query = @"
                SELECT 
                    emp.EmployeeID,
                    emp.FirstName + ' ' + emp.LastName AS FullName
                FROM HRM_EmployeeOfficialInfo empOff
                LEFT JOIN HRM_Employee emp 
                       ON emp.EmployeeID = empOff.EmployeeID
                LEFT JOIN HRM_Def_Designation des 
                       ON des.DesignationCode = empOff.DesignationCode
                WHERE des.DesignationCode = @DesignationCode";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@DesignationCode", "028"); // dynamic হলে dto.DesignationCode ব্যবহার করুন

                await con.OpenAsync();
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        empList.Add(new EmployeeDto
                        {
                            EmployeeID = rdr["EmployeeID"].ToString(),
                            FullName = rdr["FullName"].ToString()
                        });
                    }
                }
            }

            // Dropdown এর জন্য ViewBag সেট করা
            ViewBag.EmpList = new SelectList(empList, "EmployeeID", "FullName");




            RMGBookingOrderEntryBuklViewModel model = new RMGBookingOrderEntryBuklViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
                bookingReceivedDetailsThreadSetup = new BookingReceivedDetailsThreadDto()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BookingOrderAutoId()
        {
            try
            {
                var y = DateTime.Now.Year;
                var bookingOrderId = commonService.GenerateNextCode("BookinOrderNO", "RMG_BookingOrder", 3, "FAWI-" + y + "-");
                return Json(new { data = bookingOrderId });
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetSupplierDetails(string supplierId)
        {
            if (string.IsNullOrEmpty(supplierId))
            {
                return BadRequest(new { success = false, message = "SupplierId is required" });
            }

            // Example: fetch supplier details from repository or database
            var supplier = await supplierRepo.GetByIdAsync(supplierId);

            if (supplier == null)
            {
                return NotFound(new { success = false, message = "Supplier not found" });
            }

            // Return JSON data
            return Json(new
            {
                success = true,
                id = supplier.SupplierId,
                name = supplier.SupplierName,
                address = supplier.Address,
            });
        }

        //[HttpPost]
        //public async Task<IActionResult> LoadBookingTable([FromBody] ItemTypeFilterDto dto)
        //{
        //    try
        //    {
        //        var dropdownData = await GetDropdownData();
        //        List<object> data = new List<object>();
        //        var bookingType = dto.BookingType.ToLower();

        //        switch (bookingType)
        //        {
        //            case "04"://carton
        //                data = await GetCartonBookingData(dto);
        //                break;
        //            case "07"://thread
        //                data = await GetThreadBookingData(dto);
        //                break;
        //            case "03"://poly
        //                data = await GetPolyBookingData(dto);
        //                break;
        //            case "02"://button
        //                data = await GetButtonBookingData(dto);
        //                break;

        //            case "01": // fabric
        //                data = (await GetFebricBookingData(dto)).Cast<object>().ToList();
        //                break;

        //            default:

        //                data = (await GetExtraBookingData(dto)).Cast<object>().ToList();
        //                break;
        //        }

        //        return Json(new { success = true, data = data, dropdownData = dropdownData });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> LoadBookingTable([FromBody] ItemTypeFilterDto dto)
        {
            try
            {
                if (dto.CostingId == null || !dto.CostingId.Any())
                    return Json(new { success = false, message = "No costing selected" });

                var dropdownData = await GetDropdownData();
                List<object> data;

                switch (dto.BookingType)
                {
                    case "04":
                        data = await GetCartonBookingData(dto);
                        break;
                    case "07":
                        data = await GetThreadBookingData(dto);
                        break;
                    case "03":
                        data = await GetPolyBookingData(dto);
                        break;
                    case "02":
                        data = await GetButtonBookingData(dto);
                        break;
                    case "01":
                        data = (await GetFebricBookingData(dto)).Cast<object>().ToList();
                        break;
                    default:
                        data = (await GetExtraBookingData(dto)).Cast<object>().ToList();
                        break;
                }

                return Json(new { success = true, data, dropdownData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        private async Task<object> GetDropdownData()
        {
            // Fetch dropdown data from your database
            var items = await itemRepo.All()
                .Select(x => new { id = x.ItemId, name = x.ItemName })
                .ToListAsync();

            var colors = await colorRepo.All()
                .Select(x => new { id = x.ColorId, name = x.Color })
                .ToListAsync();

            var units = await unitRepo.All()
                .Select(x => new { id = x.UnitTypId, name = x.UnitTypeName })
                .ToListAsync();

            var sizes = await sizeRepo.All()
                .Select(x => new { id = x.SizeId, name = x.Size })
                .ToListAsync();

            var threadCounts = await threadCountRepo.All()
                .Select(x => new { id = x.ThreadCountId, name = x.ThreadCountName })
                .ToListAsync();

            var currencies = await currenciesRepo.All()
                .Select(x => new { id = x.CurrencyId, name = x.ShortName })
                .ToListAsync();

            return new
            {
                items = items,
                colors = colors,
                units = units,
                sizes = sizes,
                threadCounts = threadCounts,
                currencies = currencies
            };
        }

        // ==================== Carton Methods ====================
        //private async Task<List<object>> GetCartonBookingData()
        //{
        //    var data = await cartonRepo.All()
        //        .Select(x => new
        //        {
        //            id = x.Id,
        //            poNo = x.PoNo,
        //            itemID = x.ItemId,
        //            description = x.ItemDescription,
        //            colorID = x.ColorId,
        //            sizeID = x.SizeId,
        //            cartonLength = x.CartonLeangth,
        //            leangthUnitID = x.LeangthUnitId,
        //            cartonWidth = x.CartonWidth,
        //            widthUnitID = x.WidthUnitId,
        //            catonHeight = x.CatonHeight,
        //            heightUnitID = x.HeightUnitId,
        //            garmentQty = x.OrderQty,
        //            garmentQtyUnitID = x.OrderUnitId,
        //            consumption = x.Consumption,
        //            consumptionUnitID = x.ConsumptionUnitId,
        //            totalQty = x.RequiredQty,
        //            totalQtyUnitID = x.RequiredQtyUnitId,
        //            orderQty = x.OrderQty,
        //            orderQtyUnitID = x.OrderUnitId,
        //            percentage = x.CartonPercent,
        //            unitPrice = x.UnitPrice,
        //            totalPrice = x.TotalPrice,
        //            currencyID = x.CurrencyId,
        //            remarks = x.Remarks
        //        })
        //        .ToListAsync();

        //    return data.Cast<object>().ToList();
        //}

        //    private async Task<List<object>> GetCartonBookingData(ItemTypeFilterDto dto)
        //    {
        //        // ============================
        //        // CLEAR OLD TEMP DATA
        //        // ============================
        //        var exTempCarton = cartonTempRepo.All().ToList();
        //        if (exTempCarton != null && exTempCarton.Count > 0)
        //        {
        //            await cartonTempRepo.DeleteRangeAsync(exTempCarton);
        //        }

        //        // ============================
        //        // SQL QUERY (Dynamic Based on DTO)
        //        // ============================

        //        string query = @"

        //    SELECT 
        //    cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
        //    cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
        //    cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
        //    cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
        //    cd.TotalPriceCurrencyId,

        //    ci.IntegraJobNO AS CiIntegraJob,
        //    ci.StyleID AS CiStyleID,
        //    ci.PoNo AS CiPoNo,
        //    ci.MasterPurchaseOrder AS CiMasterPo
        //FROM RMG_CostingInfo ci
        //LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        //LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        //LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        //WHERE cd.CostingID =@CostingId and di.ItemTypeID =@BookingType";

        //        using var con = new SqlConnection(_connectionString);
        //        using var cmd = new SqlCommand(query, con);

        //        // ============================
        //        // ADD PARAMETERS
        //        // ============================
        //        cmd.Parameters.AddWithValue("@BookingType", dto.BookingType ?? "");
        //        cmd.Parameters.AddWithValue("@CostingId", dto.CostingId ?? "");

        //        await con.OpenAsync();
        //        using var rdr = await cmd.ExecuteReaderAsync();

        //        // ============================
        //        // INSERT INTO TEMP CARTON TABLE
        //        // ============================
        //        while (await rdr.ReadAsync())
        //        {
        //            var temp = new RmgInvBookingReceivedDetailsCartonTemp
        //            {
        //                PurchaseReceiveNo = await GenerateAutoCartonBooking(),
        //                ItemId = rdr["ItemID"].ToString(),
        //                ItemDescription = rdr["Description"].ToString(),
        //                ColorId = rdr["ColorID"].ToString(),
        //                SizeId = rdr["Width"].ToString(), // adjust if SizeId comes differently
        //                OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                RequiredQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
        //                RequiredQtyUnitId = rdr["TotalQuantityUnit"].ToString(),
        //                UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
        //                TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
        //                CurrencyId = rdr["TotalPriceCurrencyId"].ToString(),
        //                IntegraJobNo = rdr["CiIntegraJob"].ToString(),
        //                PoNo = rdr["CiPoNo"].ToString(),
        //                Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"])
        //            };

        //            await cartonTempRepo.AddAsync(temp);
        //        }

        //        // ============================
        //        // RETURN DATA FROM TEMP TABLE
        //        // ============================
        //        var data = await cartonTempRepo.All()
        //            .Select(x => new
        //            {
        //                id = x.Id,
        //                poNo = x.PoNo,
        //                itemID = x.ItemId,
        //                description = x.ItemDescription,
        //                colorID = x.ColorId,
        //                sizeID = x.SizeId,
        //                cartonLength = x.CartonLeangth,
        //                leangthUnitID = x.LeangthUnitId,
        //                cartonWidth = x.CartonWidth,
        //                widthUnitID = x.WidthUnitId,
        //                catonHeight = x.CatonHeight,
        //                heightUnitID = x.HeightUnitId,
        //                garmentQty = x.OrderQty,
        //                orderQty = x.OrderQty,
        //                garmentQtyUnitID = x.OrderUnitId,
        //                consumption = x.Consumption,
        //                consumptionUnitID = x.ConsumptionUnitId,
        //                totalQty = x.RequiredQty,
        //                totalQtyUnitID = x.RequiredQtyUnitId,
        //                percentage = x.CartonPercent,
        //                unitPrice = x.UnitPrice,
        //                totalPrice = x.TotalPrice,
        //                currencyID = x.CurrencyId,
        //                remarks = x.Remarks
        //            })
        //            .ToListAsync();

        //        return data.Cast<object>().ToList();
        //    }



        private async Task<List<object>> GetCartonBookingData(ItemTypeFilterDto dto)
        {
            await cartonTempRepo.DeleteRangeAsync(cartonTempRepo.All().ToList());

            var costingIds = dto.CostingId;

            string inClause = string.Join(",", costingIds.Select((x, i) => $"@cid{i}"));

            //        string query = $@"
            //SELECT
            //    cd.*, 
            //    ci.IntegraJobNO,
            //    ci.StyleID,
            //    ci.PoNo,
            //    ci.MasterPurchaseOrder
            //FROM RMG_CostingInfo ci
            //JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
            //JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
            //WHERE ci.CostingID IN ({inClause})
            //  AND di.ItemTypeID = @BookingType";
            string query = $@"
    SELECT
           cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
            cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
            cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
            cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
            cd.TotalPriceCurrencyId,

            ci.IntegraJobNO ,
            ci.StyleID ,
            ci.PoNo ,
            ci.MasterPurchaseOrder 
    FROM RMG_CostingInfo ci
    JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
    JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
    WHERE ci.CostingID IN ({inClause})
      AND di.ItemTypeID = @BookingType";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);

            for (int i = 0; i < costingIds.Count; i++)
                cmd.Parameters.AddWithValue($"@cid{i}", costingIds[i]);

            cmd.Parameters.AddWithValue("@BookingType", dto.BookingType);

            await con.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                await cartonTempRepo.AddAsync(new RmgInvBookingReceivedDetailsCartonTemp
                {
                    PurchaseReceiveNo = await GenerateAutoCartonBooking(),
                    ItemId = rdr["ItemID"].ToString(),
                    ItemDescription = rdr["Description"].ToString(),
                    OrderQty = Convert.ToDecimal(rdr["Quantity"]),
                    RequiredQty = Convert.ToDecimal(rdr["TotalQuantity"]),
                    UnitPrice = Convert.ToDecimal(rdr["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(rdr["TotalPrice"]),
                    PoNo = rdr["PoNo"].ToString(),
                    IntegraJobNo = rdr["IntegraJobNO"].ToString()
                });
            }

            return cartonTempRepo.All()
                .Select(x => new
                {
                    id = x.Id,
                    poNo = x.PoNo,
                    itemID = x.ItemId,
                    description = x.ItemDescription,
                    colorID = x.ColorId,
                    sizeID = x.SizeId,
                    cartonLength = x.CartonLeangth,
                    leangthUnitID = x.LeangthUnitId,
                    cartonWidth = x.CartonWidth,
                    widthUnitID = x.WidthUnitId,
                    catonHeight = x.CatonHeight,
                    heightUnitID = x.HeightUnitId,
                    garmentQty = x.OrderQty,
                    orderQty = x.OrderQty,
                    garmentQtyUnitID = x.OrderUnitId,
                    consumption = x.Consumption,
                    consumptionUnitID = x.ConsumptionUnitId,
                    totalQty = x.RequiredQty,
                    totalQtyUnitID = x.RequiredQtyUnitId,
                    percentage = x.CartonPercent,
                    unitPrice = x.UnitPrice,
                    totalPrice = x.TotalPrice,
                    currencyID = x.CurrencyId,
                    remarks = x.Remarks
                })
                .Cast<object>()
                .ToList();
        }






        public async Task<string> GenerateAutoCartonBooking()
        {
            var getYear = DateTime.Now.Year.ToString();
            var prefix = "POR_" + getYear + "_";

            string lastCode = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // First check Temp table
                string queryTemp = @"SELECT MAX(PurchaseReceiveNo) 
                             FROM RMG_Inv_BookingReceivedDetails_CartonTemp 
                             WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                using (SqlCommand cmd = new SqlCommand(queryTemp, con))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    var result = await cmd.ExecuteScalarAsync();
                    lastCode = result?.ToString();
                }

                // If nothing found in Temp, check main table
                if (string.IsNullOrEmpty(lastCode))
                {
                    string queryMain = @"SELECT MAX(PurchaseReceiveNo) 
                                 FROM RMG_Inv_BookingReceivedDetails_Carton 
                                 WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                    using (SqlCommand cmd = new SqlCommand(queryMain, con))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix);
                        var result = await cmd.ExecuteScalarAsync();
                        lastCode = result?.ToString();
                    }
                }
            }

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract numeric part after the prefix
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Pad with zeros to length 6
            string nextCode = prefix + nextNumber.ToString().PadLeft(6, '0');
            return nextCode;
        }





        // ==================== Thread Methods ====================
        //private async Task<List<object>> GetThreadBookingData()
        //{
        //    var data = await threadRepo.All()
        //        .Select(x => new
        //        {
        //            id = x.Id,
        //            poNo = x.PoNo,
        //            itemID = x.ItemId,
        //            description = x.FebricDetail,
        //            colorID = x.ThreadColorId,
        //            threadCountID = x.ThreadCountId,
        //            garmentQty = x.OrderQty,
        //            garmentQtyUnitID = x.QtyUnitId,
        //            consumption = x.Consumption,
        //            consumptionUnitID = x.ConsumtionUnitId,
        //            totalQty = x.TotalQty,
        //            totalQtyUnitID = x.TotalQtyUnitId,
        //            orderQty = x.OrderQty,
        //            orderQtyUnitID = x.ThreadReqUnit,
        //            percentage = x.Threadpercent,
        //            unitPrice = x.UnitPrice,
        //            totalPrice = x.TotalPrice,
        //            currencyID = x.CurrencyId,
        //            remarks = x.Remarks
        //        })
        //        .ToListAsync();

        //    return data.Cast<object>().ToList();
        //}

        //    private async Task<List<object>> GetThreadBookingData(ItemTypeFilterDto dto)
        //    {
        //        // ============================
        //        // CLEAR OLD TEMP DATA
        //        // ============================
        //        var exTempThread = threadTempRepo.All().ToList();
        //        if (exTempThread != null && exTempThread.Count > 0)
        //        {
        //            await threadTempRepo.DeleteRangeAsync(exTempThread);
        //        }

        //        // ============================
        //        // SQL QUERY (Thread Based)
        //        // ============================
        //        string query = @"
        //        SELECT 
        //            cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
        //            cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
        //            cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
        //            cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
        //            cd.TotalPriceCurrencyId,

        //            ci.IntegraJobNO AS CiIntegraJob,
        //            ci.StyleID AS CiStyleID,
        //            ci.PoNo AS CiPoNo,
        //            ci.MasterPurchaseOrder AS CiMasterPo
        //         FROM RMG_CostingInfo ci
        //LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        //LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        //LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        //WHERE cd.CostingID =@CostingId and di.ItemTypeID =@BookingType";

        //        using var con = new SqlConnection(_connectionString);
        //        using var cmd = new SqlCommand(query, con);

        //        // ============================
        //        // ADD PARAMETERS
        //        // ============================
        //        cmd.Parameters.AddWithValue("@BookingType", dto.BookingType ?? "");
        //        cmd.Parameters.AddWithValue("@CostingId", dto.CostingId ?? "");
        //        await con.OpenAsync();
        //        using var rdr = await cmd.ExecuteReaderAsync();

        //        // ============================
        //        // INSERT INTO TEMP TABLE
        //        // ============================
        //        while (await rdr.ReadAsync())
        //        {
        //            var temp = new RmgInvBookingReceivedDetailsThreadTemp
        //            {
        //                PurchaseReceiveNo = await GenerateAutoThreadBooking(),

        //                ItemId = rdr["ItemID"].ToString(),
        //                ColorId = rdr["ColorID"].ToString(),
        //                Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"]),

        //                // Thread Fields Mapping
        //                Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
        //                TotalQtyUnitId = rdr["TotalQuantityUnit"].ToString(),
        //                UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
        //                TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
        //                CurrencyId = rdr["TotalPriceCurrencyId"].ToString(),
        //                OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),

        //                IntegraJobNo = rdr["CiIntegraJob"].ToString(),
        //                PoNo = rdr["CiPoNo"].ToString()
        //            };

        //            await threadTempRepo.AddAsync(temp);
        //        }

        //        // ============================
        //        // RETURN DATA FROM TEMP TABLE
        //        // ============================
        //        var data = await threadTempRepo.All()
        //            .Select(x => new
        //            {
        //                id = x.Id,
        //                purchaseReceiveNo = x.PurchaseReceiveNo,

        //                itemID = x.ItemId,
        //                colorID = x.ColorId,
        //                slno = x.Slno,

        //                orderQty = x.OrderQty,
        //                qtyUnitId = x.QtyUnitId,

        //                consumption = x.Consumption,
        //                consumtionUnitId = x.ConsumtionUnitId,

        //                totalQty = x.TotalQty,
        //                totalQtyUnitId = x.TotalQtyUnitId,

        //                totalReceivedQty = x.TotalReceivedQty,
        //                currentReceiveQty = x.CurrentReceiveQty,
        //                pendingReceiveQty = x.PendingReceiveQty,
        //                garmentQty = x.OrderQty,
        //                unitPrice = x.UnitPrice,
        //                receivedUnitPrice = x.ReceivedUnitPrice,
        //                totalPrice = x.TotalPrice,
        //                currencyId = x.CurrencyId,

        //                remarks = x.Remarks,
        //                employeeId = x.EmployeeId,

        //                integraJobNo = x.IntegraJobNo,
        //                poNo = x.PoNo
        //            })
        //            .ToListAsync();

        //        return data.Cast<object>().ToList();
        //    }


        private async Task<List<object>> GetThreadBookingData(ItemTypeFilterDto dto)
        {
            // ============================
            // CLEAR OLD TEMP DATA
            // ============================
            var exTempThread = threadTempRepo.All().ToList();
            if (exTempThread.Any())
            {
                await threadTempRepo.DeleteRangeAsync(exTempThread);
            }

            // ============================
            // BUILD IN CLAUSE FOR CostingId LIST
            // ============================
            var costingIds = dto.CostingId;
            if (costingIds == null || !costingIds.Any())
                return new List<object>();

            string inClause = string.Join(",", costingIds.Select((x, i) => $"@cid{i}"));

            // ============================
            // SQL QUERY (Thread)
            // ============================
            string query = $@"
    SELECT 
        cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
        cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
        cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
        cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
        cd.TotalPriceCurrencyId,

        ci.IntegraJobNO AS CiIntegraJob,
        ci.StyleID AS CiStyleID,
        ci.PoNo AS CiPoNo,
        ci.MasterPurchaseOrder AS CiMasterPo
    FROM RMG_CostingInfo ci
    LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
    LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
    LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
    WHERE ci.CostingID IN ({inClause})
      AND di.ItemTypeID = @BookingType";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);

            // ============================
            // ADD PARAMETERS
            // ============================
            for (int i = 0; i < costingIds.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@cid{i}", costingIds[i]);
            }

            cmd.Parameters.AddWithValue("@BookingType", dto.BookingType);

            await con.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            // ============================
            // INSERT INTO THREAD TEMP TABLE
            // ============================
            while (await rdr.ReadAsync())
            {
                var temp = new RmgInvBookingReceivedDetailsThreadTemp
                {
                    PurchaseReceiveNo = await GenerateAutoThreadBooking(),

                    ItemId = rdr["ItemID"].ToString(),
                    ColorId = rdr["ColorID"].ToString(),
                    Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"]),

                    OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
                    Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                    TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
                    TotalQtyUnitId = rdr["TotalQuantityUnit"].ToString(),

                    UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
                    TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
                    CurrencyId = rdr["TotalPriceCurrencyId"].ToString(),

                    IntegraJobNo = rdr["CiIntegraJob"].ToString(),
                    PoNo = rdr["CiPoNo"].ToString()
                };

                await threadTempRepo.AddAsync(temp);
            }

            // ============================
            // RETURN DATA FROM TEMP TABLE
            // ============================
            var data = await threadTempRepo.All()
                .Select(x => new
                {
                    id = x.Id,
                    purchaseReceiveNo = x.PurchaseReceiveNo,

                    itemID = x.ItemId,
                    colorID = x.ColorId,
                    slno = x.Slno,

                    orderQty = x.OrderQty,
                    qtyUnitId = x.QtyUnitId,

                    consumption = x.Consumption,
                    consumtionUnitId = x.ConsumtionUnitId,

                    totalQty = x.TotalQty,
                    totalQtyUnitId = x.TotalQtyUnitId,

                    totalReceivedQty = x.TotalReceivedQty,
                    currentReceiveQty = x.CurrentReceiveQty,
                    pendingReceiveQty = x.PendingReceiveQty,

                    garmentQty = x.OrderQty,
                    unitPrice = x.UnitPrice,
                    receivedUnitPrice = x.ReceivedUnitPrice,
                    totalPrice = x.TotalPrice,
                    currencyId = x.CurrencyId,

                    remarks = x.Remarks,
                    employeeId = x.EmployeeId,

                    integraJobNo = x.IntegraJobNo,
                    poNo = x.PoNo
                })
                .ToListAsync();

            return data.Cast<object>().ToList();
        }




        public async Task<string> GenerateAutoThreadBooking()
        {
            var getYear = DateTime.Now.Year.ToString();
            var prefix = "POR_" + getYear + "_";

            string lastCode = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // First check Temp table
                string queryTemp = @"SELECT MAX(PurchaseReceiveNo) 
                             FROM RMG_Inv_BookingReceivedDetails_ThreadTemp 
                             WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                using (SqlCommand cmd = new SqlCommand(queryTemp, con))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    var result = await cmd.ExecuteScalarAsync();
                    lastCode = result?.ToString();
                }

                // If nothing found in Temp, check main table
                if (string.IsNullOrEmpty(lastCode))
                {
                    string queryMain = @"SELECT MAX(PurchaseReceiveNo) 
                                 FROM RMG_Inv_BookingReceivedDetails_Thread 
                                 WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                    using (SqlCommand cmd = new SqlCommand(queryMain, con))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix);
                        var result = await cmd.ExecuteScalarAsync();
                        lastCode = result?.ToString();
                    }
                }
            }

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract numeric part after the prefix
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Pad with zeros to length 6
            string nextCode = prefix + nextNumber.ToString().PadLeft(6, '0');
            return nextCode;
        }

        // ==================== Poly Methods ====================

        //       private async Task<List<object>> GetPolyBookingData(ItemTypeFilterDto dto)
        //       {
        //           // ============================
        //           // CLEAR OLD TEMP DATA
        //           // ============================
        //           var exTempPoly = polyTempRepo.All().ToList();
        //           if (exTempPoly != null && exTempPoly.Count > 0)
        //           {
        //               await polyTempRepo.DeleteRangeAsync(exTempPoly);
        //           }

        //           // ============================
        //           // SQL QUERY (Dynamic Based on DTO)
        //           // ============================
        //           string query = @"
        //   SELECT 
        //       cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
        //       cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
        //       cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
        //       cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
        //       cd.TotalPriceCurrencyId,

        //       ci.IntegraJobNO AS CiIntegraJob,
        //       ci.StyleID AS CiStyleID,
        //       ci.PoNo AS CiPoNo,
        //       ci.MasterPurchaseOrder AS CiMasterPo
        //   FROM RMG_CostingInfo ci
        //LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        //LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        //LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        //WHERE cd.CostingID =@CostingId and di.ItemTypeID =@BookingType";

        //           using var con = new SqlConnection(_connectionString);
        //           using var cmd = new SqlCommand(query, con);

        //           // ============================
        //           // ADD PARAMETERS
        //           // ============================
        //           cmd.Parameters.AddWithValue("@BookingType", dto.BookingType ?? "");
        //           cmd.Parameters.AddWithValue("@CostingId", dto.CostingId ?? "");

        //           await con.OpenAsync();
        //           using var rdr = await cmd.ExecuteReaderAsync();

        //           // ============================
        //           // INSERT INTO TEMP POLY TABLE
        //           // ============================
        //           while (await rdr.ReadAsync())
        //           {
        //               var temp = new RmgInvBookingReceivedDetailsPolyTemp
        //               {
        //                   PurchaseReceiveNo = await GenerateAutoPolyBooking(),
        //                   ItemId = rdr["ItemID"].ToString(),
        //                   ItemDescription = rdr["Description"].ToString(),
        //                   ColorId = rdr["ColorID"].ToString(),
        //                   Width = rdr["Width"].ToString(),
        //                   GarmentQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                   Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                   TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
        //                   TotalQtyUnitId = rdr["TotalQuantityUnit"].ToString(),
        //                   UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
        //                   TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
        //                   CurrencyId = rdr["TotalPriceCurrencyId"].ToString(),
        //                   IntegraJobNo = rdr["CiIntegraJob"].ToString(),
        //                   PoNo = rdr["CiPoNo"].ToString(),
        //                   SerialNo = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"])
        //               };

        //               await polyTempRepo.AddAsync(temp);
        //           }

        //           // ============================
        //           // RETURN DATA FROM TEMP TABLE
        //           // ============================
        //           var data = await polyTempRepo.All()
        //   .Select(x => new
        //   {
        //       id = x.Id,
        //       purchaseReceiveNo = x.PurchaseReceiveNo,
        //       brdId = x.Brdid,
        //       serialNo = x.SerialNo,
        //       itemID = x.ItemId,
        //       description = x.ItemDescription,
        //       colorID = x.ColorId,
        //       referenceCode = x.RefernceCode,
        //       length = x.Length,
        //       lengthUnitID = x.LengthUnitId,
        //       width = x.Width,
        //       widthUnitID = x.WidthUnitId,
        //       flap = x.Flap,
        //       flapUnitID = x.FlapUnitId,
        //       guest = x.Guest,
        //       guestUnitID = x.GuestUnitId,
        //       garmentQty = x.GarmentQty,
        //       garmentQtyUnitID = x.GarmentQtyUnitId,
        //       consumption = x.Consumption,
        //       consumptionUnitID = x.ConsumptionUnitId,
        //       totalQty = x.TotalQty,
        //       OrderQty = x.GarmentQty,
        //       totalQtyUnitID = x.TotalQtyUnitId,
        //       percentage = x.Percentage,
        //       totalReceivedQty = x.TotalReceivedQty,
        //       currentReceiveQty = x.CurrentReceiveQty,
        //       receivedUnitType = x.ReceivedUnitType,
        //       unitPrice = x.UnitPrice,
        //       receivedUnitPrice = x.ReceivedUnitPrice,
        //       totalPrice = x.TotalPrice,
        //       currencyID = x.CurrencyId,
        //       remarks = x.Remarks,
        //       employeeId = x.EmployeeId,
        //       totalReceivedQtyPre = x.TotalReceivedQtyPre,
        //       pendingReceiveQty = x.PendingReceiveQty,
        //       pendingReceiveQtyPre = x.PendingReceiveQtyPre,
        //       integraJobNo = x.IntegraJobNo,
        //       poNo = x.PoNo
        //   })
        //   .ToListAsync();

        //           return data.Cast<object>().ToList();
        //       }


        private async Task<List<object>> GetPolyBookingData(ItemTypeFilterDto dto)
        {
            // ============================
            // CLEAR OLD TEMP DATA
            // ============================
            var exTempPoly = polyTempRepo.All().ToList();
            if (exTempPoly.Any())
            {
                await polyTempRepo.DeleteRangeAsync(exTempPoly);
            }

            // ============================
            // BUILD IN CLAUSE FOR CostingId LIST
            // ============================
            var costingIds = dto.CostingId;
            if (costingIds == null || !costingIds.Any())
                return new List<object>();

            string inClause = string.Join(",", costingIds.Select((x, i) => $"@cid{i}"));

            // ============================
            // SQL QUERY (POLY)
            // ============================
            string query = $@"
    SELECT 
        cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
        cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
        cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
        cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
        cd.TotalPriceCurrencyId,

        ci.IntegraJobNO AS CiIntegraJob,
        ci.StyleID AS CiStyleID,
        ci.PoNo AS CiPoNo,
        ci.MasterPurchaseOrder AS CiMasterPo
    FROM RMG_CostingInfo ci
    LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
    LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
    LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
    WHERE ci.CostingID IN ({inClause})
      AND di.ItemTypeID = @BookingType";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);

            // ============================
            // ADD PARAMETERS
            // ============================
            for (int i = 0; i < costingIds.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@cid{i}", costingIds[i]);
            }

            cmd.Parameters.AddWithValue("@BookingType", dto.BookingType);

            await con.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            // ============================
            // INSERT INTO TEMP POLY TABLE
            // ============================
            while (await rdr.ReadAsync())
            {
                var temp = new RmgInvBookingReceivedDetailsPolyTemp
                {
                    PurchaseReceiveNo = await GenerateAutoPolyBooking(),

                    ItemId = rdr["ItemID"].ToString(),
                    ItemDescription = rdr["Description"].ToString(),
                    ColorId = rdr["ColorID"].ToString(),
                    Width = rdr["Width"].ToString(),

                    GarmentQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
                    Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                    TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
                    TotalQtyUnitId = rdr["TotalQuantityUnit"].ToString(),

                    UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
                    TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
                    CurrencyId = rdr["TotalPriceCurrencyId"].ToString(),

                    IntegraJobNo = rdr["CiIntegraJob"].ToString(),
                    PoNo = rdr["CiPoNo"].ToString(),

                    SerialNo = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"])
                };

                await polyTempRepo.AddAsync(temp);
            }

            // ============================
            // RETURN DATA FROM TEMP TABLE
            // ============================
            var data = await polyTempRepo.All()
                .Select(x => new
                {
                    id = x.Id,
                    purchaseReceiveNo = x.PurchaseReceiveNo,
                    brdId = x.Brdid,
                    serialNo = x.SerialNo,

                    itemID = x.ItemId,
                    description = x.ItemDescription,
                    colorID = x.ColorId,

                    referenceCode = x.RefernceCode,

                    length = x.Length,
                    lengthUnitID = x.LengthUnitId,

                    width = x.Width,
                    widthUnitID = x.WidthUnitId,

                    flap = x.Flap,
                    flapUnitID = x.FlapUnitId,

                    guest = x.Guest,
                    guestUnitID = x.GuestUnitId,

                    garmentQty = x.GarmentQty,
                    garmentQtyUnitID = x.GarmentQtyUnitId,
                    OrderQty = x.GarmentQty,

                    consumption = x.Consumption,
                    consumptionUnitID = x.ConsumptionUnitId,

                    totalQty = x.TotalQty,
                    totalQtyUnitID = x.TotalQtyUnitId,

                    percentage = x.Percentage,

                    totalReceivedQty = x.TotalReceivedQty,
                    currentReceiveQty = x.CurrentReceiveQty,
                    receivedUnitType = x.ReceivedUnitType,

                    unitPrice = x.UnitPrice,
                    receivedUnitPrice = x.ReceivedUnitPrice,
                    totalPrice = x.TotalPrice,
                    currencyID = x.CurrencyId,

                    remarks = x.Remarks,
                    employeeId = x.EmployeeId,

                    totalReceivedQtyPre = x.TotalReceivedQtyPre,
                    pendingReceiveQty = x.PendingReceiveQty,
                    pendingReceiveQtyPre = x.PendingReceiveQtyPre,

                    integraJobNo = x.IntegraJobNo,
                    poNo = x.PoNo
                })
                .ToListAsync();

            return data.Cast<object>().ToList();
        }



        public async Task<string> GenerateAutoPolyBooking()
        {
            var getYear = DateTime.Now.Year.ToString();
            var prefix = "POR_" + getYear + "_";

            string lastCode = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // First check Temp table
                string queryTemp = @"SELECT MAX(PurchaseReceiveNo) 
                             FROM RMG_Inv_BookingReceivedDetails_PolyTemp 
                             WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                using (SqlCommand cmd = new SqlCommand(queryTemp, con))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    var result = await cmd.ExecuteScalarAsync();
                    lastCode = result?.ToString();
                }

                // If nothing found in Temp, check main table
                if (string.IsNullOrEmpty(lastCode))
                {
                    string queryMain = @"SELECT MAX(PurchaseReceiveNo) 
                                 FROM RMG_Inv_BookingReceivedDetails_Poly 
                                 WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                    using (SqlCommand cmd = new SqlCommand(queryMain, con))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix);
                        var result = await cmd.ExecuteScalarAsync();
                        lastCode = result?.ToString();
                    }
                }
            }

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract numeric part after the prefix
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Pad with zeros to length 6
            string nextCode = prefix + nextNumber.ToString().PadLeft(6, '0');
            return nextCode;
        }

        // ==================== Button Methods ====================
        //private async Task<List<object>> GetButtonBookingData()
        //{
        //    var data = await buttonRepo.All()
        //        .Select(x => new
        //        {
        //            id = x.Id,
        //            poNo = x.PoNo,
        //            itemID = x.ItemId,
        //            description = x.Description,
        //            colorID = x.ColorId,
        //            garmentQty = x.GermentQty,
        //            garmentQtyUnitID = x.GermentsQtyUnitId,
        //            consumption = x.Consumption,
        //            consumptionUnitID = x.ConsumptionUnitId,
        //            totalQty = x.TotalQty,
        //            totalQtyUnitID = x.TotalQtyUnitId,
        //            orderQty = x.OrderQty,
        //            orderQtyUnitID = x.OrderQtyUnitId,
        //            percentage = x.Percentage,
        //            unitPrice = x.UnitPrice,
        //            totalPrice = x.TotalPrice,
        //            currencyID = x.CurrencyId,
        //            remarks = x.Remarks
        //        })
        //        .ToListAsync();

        //    return data.Cast<object>().ToList();
        //}
        //        private async Task<List<object>> GetButtonBookingData(ItemTypeFilterDto dto)
        //        {
        //            try
        //            {
        //                // ============================
        //                // CLEAR OLD TEMP DATA
        //                // ============================
        //                var exTempButton = buttonTempRepo.All().ToList();
        //                if (exTempButton != null && exTempButton.Count > 0)
        //                {
        //                    await buttonTempRepo.DeleteRangeAsync(exTempButton);
        //                }

        //                // ============================
        //                // SQL QUERY (Entity-aligned)
        //                // ============================
        //                string query = @"
        //SELECT 
        //    cd.Id,
        //    cd.SLNO,
        //    cd.ItemID,
        //    cd.Description,
        //    cd.ColorID,   
        //    cd.SLNO,
        //    cd.Quantity AS GermentQty,
        //    cd.TotalQuantity AS TotalQty,
        //    cd.TotalQuantityUnit AS TotalQtyUnitId,
        //    cd.Consumption,
        //    cd.Consumption AS ConsumptionUnitId,
        //    cd.Extra AS Percentage,
        //    cd.Quantity AS CurrentReceiveQty,
        //    cd.UnitPrice,
        //    cd.UnitPrice AS ReceivedUnitPrice,
        //    cd.TotalPrice,
        //    cd.TotalPriceCurrencyId AS CurrencyId,
        //    cd.SupplierID AS EmployeeId,
        //    cd.Quantity AS TotalReceivedQtyPre,
        //    0 AS PendingReceiveQty,
        //    0 AS PendingReceiveQtyPre,
        //    ci.IntegraJobNO AS IntegraJobNo,
        //    ci.PoNo AS PoNo
        // FROM RMG_CostingInfo ci
        // LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        // LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        // LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        // WHERE cd.CostingID =@CostingId and di.ItemTypeID =@BookingType";

        //                using var con = new SqlConnection(_connectionString);
        //                using var cmd = new SqlCommand(query, con);

        //                // ============================
        //                // ADD PARAMETERS
        //                // ============================
        //                cmd.Parameters.AddWithValue("@BookingType", dto.BookingType ?? "");
        //                cmd.Parameters.AddWithValue("@CostingId", dto.CostingId ?? "");

        //                await con.OpenAsync();
        //                using var rdr = await cmd.ExecuteReaderAsync();

        //                // ============================
        //                // INSERT INTO TEMP TABLE
        //                // ============================
        //                while (await rdr.ReadAsync())
        //                {
        //                    var temp = new RmgInvBookingReceivedDetailsButtonTemp
        //                    {
        //                        PurchaseReceiveNo = await GenerateAutoButtonId(),
        //                        SerialNo = rdr["SLNO"] == DBNull.Value ? null : Convert.ToInt32(rdr["SLNO"]),
        //                        ItemId = rdr["ItemID"].ToString(),
        //                        Description = rdr["Description"].ToString(),
        //                        ColorId = rdr["ColorID"].ToString(),


        //                        GermentQty = rdr["GermentQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["GermentQty"]),
        //                        TotalQty = rdr["TotalQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQty"]),
        //                        TotalQtyUnitId = rdr["TotalQtyUnitId"].ToString(),
        //                        Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                        ConsumptionUnitId = rdr["ConsumptionUnitId"].ToString(),
        //                        //OrderQty = rdr["OrderQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["OrderQty"]),
        //                        //OrderQtyUnitId = rdr["OrderQtyUnitId"].ToString(),

        //                        Percentage = rdr["Percentage"].ToString(),
        //                        CurrentReceiveQty = rdr["CurrentReceiveQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["CurrentReceiveQty"]),
        //                        UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
        //                        ReceivedUnitPrice = rdr["ReceivedUnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["ReceivedUnitPrice"]),
        //                        TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
        //                        CurrencyId = rdr["CurrencyId"].ToString(),

        //                        TotalReceivedQtyPre = rdr["TotalReceivedQtyPre"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalReceivedQtyPre"]),
        //                        PendingReceiveQty = rdr["PendingReceiveQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["PendingReceiveQty"]),
        //                        PendingReceiveQtyPre = rdr["PendingReceiveQtyPre"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["PendingReceiveQtyPre"]),

        //                        IntegraJobNo = rdr["IntegraJobNo"].ToString(),
        //                        PoNo = rdr["PoNo"].ToString()
        //                    };

        //                    await buttonTempRepo.AddAsync(temp);
        //                }

        //                // ============================
        //                // RETURN LIST FROM TEMP TABLE
        //                // ============================
        //                var items = buttonTempRepo.All().Select(x => new
        //                {
        //                    Id = x.Id,
        //                    ItemID = x.ItemId,
        //                    ColorID = x.ColorId,
        //                    Description = x.Description,
        //                    Quantity = x.CurrentReceiveQty,
        //                    garmentQty = x.GermentQty,
        //                    totalQty = x.TotalQty,
        //                    TotalQuantityUnit = x.TotalQtyUnitId,
        //                    UnitPrice = x.ReceivedUnitPrice,
        //                    SLNO = x.SerialNo ?? 0,
        //                    IntegraJobNO = x.IntegraJobNo,
        //                    PoNo = x.PoNo,
        //                    Consumption = x.Consumption,
        //                    OrderQty = x.TotalQty,
        //                    Percentage = x.Percentage,
        //                    TotalPrice = x.TotalPrice
        //                }).ToList();


        //                return items.Cast<object>().ToList();
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }


        //        }


        private async Task<List<object>> GetButtonBookingData(ItemTypeFilterDto dto)
        {
            try
            {
                // ============================
                // CLEAR OLD TEMP DATA
                // ============================
                var exTempButton = buttonTempRepo.All().ToList();
                if (exTempButton.Any())
                {
                    await buttonTempRepo.DeleteRangeAsync(exTempButton);
                }

                // ============================
                // BUILD IN CLAUSE FOR CostingId LIST
                // ============================
                var costingIds = dto.CostingId;
                if (costingIds == null || !costingIds.Any())
                    return new List<object>();

                string inClause = string.Join(",", costingIds.Select((x, i) => $"@cid{i}"));

                // ============================
                // SQL QUERY (BUTTON)
                // ============================
                string query = $@"
        SELECT 
            cd.Id,
            cd.SLNO,
            cd.ItemID,
            cd.Description,
            cd.ColorID,   
            cd.Quantity AS GermentQty,
            cd.TotalQuantity AS TotalQty,
            cd.TotalQuantityUnit AS TotalQtyUnitId,
            cd.Consumption,
            cd.Consumption AS ConsumptionUnitId,
            cd.Extra AS Percentage,
            cd.Quantity AS CurrentReceiveQty,
            cd.UnitPrice,
            cd.UnitPrice AS ReceivedUnitPrice,
            cd.TotalPrice,
            cd.TotalPriceCurrencyId AS CurrencyId,
            cd.SupplierID AS EmployeeId,
            cd.Quantity AS TotalReceivedQtyPre,
            0 AS PendingReceiveQty,
            0 AS PendingReceiveQtyPre,
            ci.IntegraJobNO AS IntegraJobNo,
            ci.PoNo AS PoNo
        FROM RMG_CostingInfo ci
        LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        WHERE ci.CostingID IN ({inClause})
          AND di.ItemTypeID = @BookingType";

                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, con);

                // ============================
                // ADD PARAMETERS
                // ============================
                for (int i = 0; i < costingIds.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@cid{i}", costingIds[i]);
                }

                cmd.Parameters.AddWithValue("@BookingType", dto.BookingType);

                await con.OpenAsync();
                using var rdr = await cmd.ExecuteReaderAsync();

                // ============================
                // INSERT INTO TEMP BUTTON TABLE
                // ============================
                while (await rdr.ReadAsync())
                {
                    var temp = new RmgInvBookingReceivedDetailsButtonTemp
                    {
                        PurchaseReceiveNo = await GenerateAutoButtonId(),

                        SerialNo = rdr["SLNO"] == DBNull.Value ? null : Convert.ToInt32(rdr["SLNO"]),
                        ItemId = rdr["ItemID"].ToString(),
                        Description = rdr["Description"].ToString(),
                        ColorId = rdr["ColorID"].ToString(),

                        GermentQty = rdr["GermentQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["GermentQty"]),
                        TotalQty = rdr["TotalQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQty"]),
                        TotalQtyUnitId = rdr["TotalQtyUnitId"].ToString(),

                        Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                        ConsumptionUnitId = rdr["ConsumptionUnitId"].ToString(),

                        Percentage = rdr["Percentage"].ToString(),
                        CurrentReceiveQty = rdr["CurrentReceiveQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["CurrentReceiveQty"]),

                        UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
                        ReceivedUnitPrice = rdr["ReceivedUnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["ReceivedUnitPrice"]),
                        TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),
                        CurrencyId = rdr["CurrencyId"].ToString(),

                        TotalReceivedQtyPre = rdr["TotalReceivedQtyPre"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalReceivedQtyPre"]),
                        PendingReceiveQty = rdr["PendingReceiveQty"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["PendingReceiveQty"]),
                        PendingReceiveQtyPre = rdr["PendingReceiveQtyPre"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["PendingReceiveQtyPre"]),

                        IntegraJobNo = rdr["IntegraJobNo"].ToString(),
                        PoNo = rdr["PoNo"].ToString()
                    };

                    await buttonTempRepo.AddAsync(temp);
                }

                // ============================
                // RETURN LIST FROM TEMP TABLE
                // ============================
                var items = buttonTempRepo.All()
                    .Select(x => new
                    {
                        Id = x.Id,
                        ItemID = x.ItemId,
                        ColorID = x.ColorId,
                        Description = x.Description,
                        Quantity = x.CurrentReceiveQty,
                        garmentQty = x.GermentQty,
                        totalQty = x.TotalQty,
                        TotalQuantityUnit = x.TotalQtyUnitId,
                        UnitPrice = x.ReceivedUnitPrice,
                        SLNO = x.SerialNo ?? 0,
                        IntegraJobNO = x.IntegraJobNo,
                        PoNo = x.PoNo,
                        Consumption = x.Consumption,
                        OrderQty = x.TotalQty,
                        Percentage = x.Percentage,
                        TotalPrice = x.TotalPrice
                    })
                    .ToList();

                return items.Cast<object>().ToList();
            }
            catch
            {
                throw;
            }
        }



        public async Task<string> GenerateAutoButtonId()
        {
            var getYear = DateTime.Now.Year.ToString();
            var prefix = "POR_" + getYear + "_";

            string lastCode = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // First check Temp table
                string queryTemp = @"SELECT MAX(PurchaseReceiveNo) 
                             FROM RMG_Inv_BookingReceivedDetails_ButtonTemp 
                             WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                using (SqlCommand cmd = new SqlCommand(queryTemp, con))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    var result = await cmd.ExecuteScalarAsync();
                    lastCode = result?.ToString();
                }

                // If nothing found in Temp, check main table
                if (string.IsNullOrEmpty(lastCode))
                {
                    string queryMain = @"SELECT MAX(PurchaseReceiveNo) 
                                 FROM RMG_Inv_BookingReceivedDetails_Button
                                 WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                    using (SqlCommand cmd = new SqlCommand(queryMain, con))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix);
                        var result = await cmd.ExecuteScalarAsync();
                        lastCode = result?.ToString();
                    }
                }
            }

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract numeric part after the prefix
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Pad with zeros to length 6
            string nextCode = prefix + nextNumber.ToString().PadLeft(6, '0');
            return nextCode;
        }
        // ==================== Extra Methods ====================

        //        private async Task<List<object>> GetExtraBookingData(ItemTypeFilterDto dto)
        //        {
        //            // ============================
        //            // CLEAR OLD TEMP DATA
        //            // ============================
        //            var exTempExtra = extraTempRepo.All().ToList();
        //            if (exTempExtra != null && exTempExtra.Count > 0)
        //            {
        //                await extraTempRepo.DeleteRangeAsync(exTempExtra);
        //            }

        //            // ============================
        //            // SQL QUERY (Only valid columns)
        //            // ============================
        //            string query = @"
        //SELECT 
        //    cd.Id,
        //    cd.CostingDetailsID,
        //    cd.CostingID AS DetailCostingID,
        //    cd.SLNO,
        //    cd.BookingItemTypeID,
        //    cd.ItemID,
        //    cd.Description,
        //    cd.Width,
        //    cd.ColorID,
        //    cd.SupplierID,
        //    cd.PoNo AS DetailPoNo,
        //    cd.Quantity,
        //    cd.Consumption,
        //    cd.TotalQuantity,
        //    cd.UnitPrice,
        //    cd.TotalPrice,
        //    cd.Extra,

        //    ci.IntegraJobNO AS IntegraJobNo,
        //    ci.PoNo AS PoNo
        // FROM RMG_CostingInfo ci
        // LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        // LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        // LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        // WHERE cd.CostingID =@CostingId and di.ItemTypeID =@BookingType";

        //            using var con = new SqlConnection(_connectionString);
        //            using var cmd = new SqlCommand(query, con);

        //            // ============================
        //            // ADD PARAMETERS
        //            // ============================
        //            cmd.Parameters.AddWithValue("@BookingType", dto.BookingType ?? "");
        //            cmd.Parameters.AddWithValue("@CostingId", dto.CostingId ?? "");

        //            await con.OpenAsync();
        //            using var rdr = await cmd.ExecuteReaderAsync();

        //            // ============================
        //            // INSERT INTO TEMP TABLE
        //            // ============================
        //            while (await rdr.ReadAsync())
        //            {
        //                var temp = new RmgInvBookingReceivedDetailsExtraTemp
        //                {
        //                    //PurchaseReceiveNo = await GenerateAutoExtraId(),
        //                    //ItemId = rdr["ItemID"]?.ToString(),
        //                    //Description = rdr["Description"]?.ToString(),
        //                    //ColorId = rdr["ColorID"]?.ToString(),

        //                    //OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                    //Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                    //TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
        //                    //UnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
        //                    //TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),

        //                    //Percentage = rdr["Extra"] == DBNull.Value ? "0" : rdr["Extra"].ToString(),

        //                    //IntegraJobNo = rdr["IntegraJobNo"]?.ToString(),
        //                    //PoNo = rdr["PoNo"]?.ToString(),
        //                    //Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"])

        //                    PurchaseReceiveNo = await GenerateAutoExtraId(),
        //                    ItemId = rdr["ItemID"].ToString(),
        //                    FabricColorId = dto.BookingType, // ItemTypeID passed from UI
        //                    ColorId = rdr["ColorID"].ToString(),
        //                    Description = rdr["Description"].ToString(),

        //                    //ConsumptionUnitId = rdr["TotalQuantityUnit"].ToString(),
        //                    //ConsumtionUnit = rdr["Consumption"].ToString(),
        //                    Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),

        //                    TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
        //                    ReqQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                    OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                    ReceivedUnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),

        //                    TotalReceivedQty = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                    PendingReceiveQty = 0,
        //                    Percentage = rdr["Extra"].ToString(),
        //                    IntegraJobNo = rdr["IntegraJobNo"].ToString(),
        //                    PoNo = rdr["PoNo"].ToString(),
        //                    Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"]),
        //                    TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TotalPrice"]),
        //                };

        //                await extraTempRepo.AddAsync(temp);
        //            }

        //            // ============================
        //            // RETURN LIST FROM TEMP TABLE
        //            // ============================
        //            var items = extraTempRepo.All().Select(x => new
        //            {
        //                //id = x.Id,
        //                //poNo = x.PoNo,
        //                //ItemID = x.ItemId,
        //                //ColorID = x.ColorId,
        //                //description = x.Description,
        //                //GarmentQty = x.OrderQty,
        //                //OrderQty = x.OrderQty,
        //                //TotalQuantity = x.TotalQty,
        //                //UnitPrice = x.UnitPrice,
        //                //SLNO = x.Slno ?? 0,
        //                //IntegraJobNO = x.IntegraJobNo,
        //                //Consumption = x.Consumption,
        //                //Percentage = x.Percentage,
        //                //TotalPrice = x.TotalPrice

        //                Id = x.Id,
        //                ItemID = x.ItemId,
        //                ColorID = x.ColorId,
        //                Description = x.Description,
        //                Quantity = x.CurrentReceiveQty,
        //                TotalQuantity = x.TotalQty,
        //                TotalQuantityUnit = x.ReqQtyUnitId,
        //                UnitPrice = x.ReceivedUnitPrice,
        //                SLNO = x.Slno ?? 0,
        //                IntegraJobNO = x.IntegraJobNo,
        //                PoNo = x.PoNo,
        //                Consumption = x.Consumption,
        //                TotalQty = x.TotalQty,
        //                GarmentQty = x.OrderQty,
        //                OrderQty = x.OrderQty,
        //                Percentage = x.Percentage,
        //                TotalPrice = x.TotalPrice,

        //            }).ToList();

        //            return items.Cast<object>().ToList();
        //        }



        private async Task<List<object>> GetExtraBookingData(ItemTypeFilterDto dto)
        {
            // ============================
            // CLEAR OLD TEMP DATA
            // ============================
            var exTempExtra = extraTempRepo.All().ToList();
            if (exTempExtra.Any())
            {
                await extraTempRepo.DeleteRangeAsync(exTempExtra);
            }

            // ============================
            // VALIDATE CostingId LIST
            // ============================
            var costingIds = dto.CostingId;
            if (costingIds == null || !costingIds.Any())
                return new List<object>();

            // ============================
            // BUILD IN CLAUSE
            // ============================
            string inClause = string.Join(",", costingIds.Select((x, i) => $"@cid{i}"));

            // ============================
            // SQL QUERY (EXTRA)
            // ============================
            string query = $@"
    SELECT 
        cd.Id,
        cd.SLNO,
        cd.ItemID,
        cd.Description,
        cd.ColorID,
        cd.Quantity,
        cd.Consumption,
        cd.TotalQuantity,
        cd.UnitPrice,
        cd.TotalPrice,
        cd.Extra,

        ci.IntegraJobNO AS IntegraJobNo,
        ci.PoNo AS PoNo
    FROM RMG_CostingInfo ci
    LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
    LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
    LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
    WHERE ci.CostingID IN ({inClause})
      AND di.ItemTypeID = @BookingType";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);

            // ============================
            // ADD PARAMETERS
            // ============================
            for (int i = 0; i < costingIds.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@cid{i}", costingIds[i]);
            }
            cmd.Parameters.AddWithValue("@BookingType", dto.BookingType);

            await con.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            // ============================
            // INSERT INTO EXTRA TEMP TABLE
            // ============================
            while (await rdr.ReadAsync())
            {
                var temp = new RmgInvBookingReceivedDetailsExtraTemp
                {
                    PurchaseReceiveNo = await GenerateAutoExtraId(),

                    ItemId = rdr["ItemID"].ToString(),
                    ColorId = rdr["ColorID"].ToString(),
                    Description = rdr["Description"].ToString(),

                    OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
                    ReqQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
                    Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                    TotalQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),

                    ReceivedUnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
                    TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),

                    Percentage = rdr["Extra"] == DBNull.Value ? "0" : rdr["Extra"].ToString(),

                    TotalReceivedQty = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                    PendingReceiveQty = 0,

                    IntegraJobNo = rdr["IntegraJobNo"].ToString(),
                    PoNo = rdr["PoNo"].ToString(),
                    Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"])
                };

                await extraTempRepo.AddAsync(temp);
            }

            // ============================
            // RETURN LIST FROM TEMP TABLE
            // ============================
            var items = extraTempRepo.All()
                .Select(x => new
                {
                    Id = x.Id,
                    ItemID = x.ItemId,
                    ColorID = x.ColorId,
                    Description = x.Description,

                    Quantity = x.OrderQty,
                    GarmentQty = x.OrderQty,
                    OrderQty = x.OrderQty,

                    Consumption = x.Consumption,
                    TotalQty = x.TotalQty,

                    UnitPrice = x.ReceivedUnitPrice,
                    TotalPrice = x.TotalPrice,

                    Percentage = x.Percentage,
                    SLNO = x.Slno ?? 0,

                    IntegraJobNO = x.IntegraJobNo,
                    PoNo = x.PoNo
                })
                .ToList();

            return items.Cast<object>().ToList();
        }



        public async Task<string> GenerateAutoExtraId()
        {
            var getYear = DateTime.Now.Year.ToString();
            var prefix = "POR_" + getYear + "_";

            string lastCode = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // First check Temp table
                string queryTemp = @"SELECT MAX(PurchaseReceiveNo) 
                             FROM RMG_Inv_BookingReceivedDetails_ExtraTemp 
                             WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                using (SqlCommand cmd = new SqlCommand(queryTemp, con))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    var result = await cmd.ExecuteScalarAsync();
                    lastCode = result?.ToString();
                }

                // If nothing found in Temp, check main table
                if (string.IsNullOrEmpty(lastCode))
                {
                    string queryMain = @"SELECT MAX(PurchaseReceiveNo) 
                                 FROM RMG_Inv_BookingReceivedDetails_Extra 
                                 WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                    using (SqlCommand cmd = new SqlCommand(queryMain, con))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix);
                        var result = await cmd.ExecuteScalarAsync();
                        lastCode = result?.ToString();
                    }
                }
            }

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract numeric part after the prefix
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Pad with zeros to length 6
            string nextCode = prefix + nextNumber.ToString().PadLeft(6, '0');
            return nextCode;
        }

        // ==================== Febric Methods ====================

        //       private async Task<List<object>> GetFebricBookingData(ItemTypeFilterDto dto)
        //       {
        //           // ============================
        //           // CLEAR OLD TEMP DATA
        //           // ============================
        //           var exTempFebric = febricTempRepo.All().ToList();
        //           if (exTempFebric != null && exTempFebric.Count > 0)
        //           {
        //               await febricTempRepo.DeleteRangeAsync(exTempFebric);
        //           }

        //           // ============================
        //           // SQL QUERY (Dynamic Based on DTO)
        //           // ============================
        //           string query = @"
        //   SELECT 
        //       cd.Id, cd.CostingDetailsID, cd.CostingID AS DetailCostingID, cd.SLNO,
        //       cd.BookingItemTypeID, cd.ItemID, cd.Description, cd.Width, cd.ColorID,
        //       cd.SupplierID, cd.PoNo AS DetailPoNo, cd.Quantity, cd.Consumption, cd.Extra,
        //       cd.TotalQuantity, cd.TotalQuantityUnit, cd.UnitPrice, cd.TotalPrice,
        //       cd.TotalPriceCurrencyId,

        //       ci.IntegraJobNO AS CiIntegraJob,
        //       ci.PoNo AS CiPoNo
        //    FROM RMG_CostingInfo ci
        //LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        //LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        //LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        //WHERE cd.CostingID =@CostingId and di.ItemTypeID =@BookingType";

        //           using var con = new SqlConnection(_connectionString);
        //           using var cmd = new SqlCommand(query, con);

        //           // ============================
        //           // ADD PARAMETERS
        //           // ============================
        //           cmd.Parameters.AddWithValue("@BookingType", dto.BookingType ?? "");
        //           cmd.Parameters.AddWithValue("@CostingId", dto.CostingId ?? "");


        //           await con.OpenAsync();
        //           using var rdr = await cmd.ExecuteReaderAsync();

        //           // ============================
        //           // INSERT INTO TEMP FABRIC TABLE
        //           // ============================
        //           while (await rdr.ReadAsync())
        //           {
        //               var temp = new RmgInvBookingReceivedDetailsFebricTemp
        //               {
        //                   PurchaseReceiveNo = await GenerateAutoFebrickId(),
        //                   ItemId = rdr["ItemID"].ToString(),
        //                   FabricItemId = dto.BookingType, // ItemTypeID passed from UI
        //                   ColorId = rdr["ColorID"].ToString(),
        //                   FebricDetails = rdr["Description"].ToString(),

        //                   QtyUnit = rdr["TotalQuantityUnit"].ToString(),
        //                   ConsumtionUnit = rdr["Consumption"].ToString(),
        //                   Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),

        //                   TotalFebricQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
        //                   CurrentReceiveQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                   OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
        //                   ReceivedUnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),

        //                   TotalReceivedQty = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
        //                   PendingReceiveQty = 0,
        //                   Percentage = rdr["Extra"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Extra"]),
        //                   IntegraJobNo = rdr["CiIntegraJob"].ToString(),
        //                   PoNo = rdr["CiPoNo"].ToString(),
        //                   Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"]),
        //                   TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TotalPrice"]),
        //               };

        //               await febricTempRepo.AddAsync(temp);
        //           }


        //           // ============================
        //           // RETURN LIST FROM TEMP TABLE
        //           // ============================
        //           var items = febricTempRepo.All().Select(x => new
        //           {
        //               Id = x.Id,
        //               ItemID = x.ItemId,
        //               ColorID = x.ColorId,
        //               Description = x.FebricDetails,
        //               Quantity = x.CurrentReceiveQty,
        //               TotalQuantity = x.TotalFebricQty,
        //               TotalQuantityUnit = x.QtyUnit,
        //               UnitPrice = x.ReceivedUnitPrice,
        //               SLNO = x.Slno ?? 0,
        //               IntegraJobNO = x.IntegraJobNo,
        //               PoNo = x.PoNo,
        //               Consumption = x.Consumption,
        //               TotalQty = x.TotalFebricQty,
        //               GarmentQty = x.OrderQty,
        //               OrderQty = x.OrderQty,
        //               Percentage = x.Percentage,
        //               TotalPrice = x.TotalPrice,


        //           }).ToList();

        //           return items.Cast<object>().ToList();
        //       }



        private async Task<List<object>> GetFebricBookingData(ItemTypeFilterDto dto)
        {
            // ============================
            // CLEAR OLD TEMP DATA
            // ============================
            var exTempFebric = febricTempRepo.All().ToList();
            if (exTempFebric.Any())
            {
                await febricTempRepo.DeleteRangeAsync(exTempFebric);
            }

            // ============================
            // VALIDATE CostingId LIST
            // ============================
            var costingIds = dto.CostingId;
            if (costingIds == null || !costingIds.Any())
                return new List<object>();

            // ============================
            // BUILD IN CLAUSE
            // ============================
            string inClause = string.Join(",", costingIds.Select((x, i) => $"@cid{i}"));

            // ============================
            // SQL QUERY (FABRIC)
            // ============================
            string query = $@"
    SELECT 
        cd.Id,
        cd.SLNO,
        cd.ItemID,
        cd.Description,
        cd.ColorID,
        cd.Quantity,
        cd.Consumption,
        cd.Extra,
        cd.TotalQuantity,
        cd.TotalQuantityUnit,
        cd.UnitPrice,
        cd.TotalPrice,

        ci.IntegraJobNO AS IntegraJobNo,
        ci.PoNo AS PoNo
    FROM RMG_CostingInfo ci
    LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
    LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
    LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
    WHERE ci.CostingID IN ({inClause})
      AND di.ItemTypeID = @BookingType";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);

            // ============================
            // ADD PARAMETERS
            // ============================
            for (int i = 0; i < costingIds.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@cid{i}", costingIds[i]);
            }
            cmd.Parameters.AddWithValue("@BookingType", dto.BookingType);

            await con.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            // ============================
            // INSERT INTO TEMP FABRIC TABLE
            // ============================
            while (await rdr.ReadAsync())
            {
                var temp = new RmgInvBookingReceivedDetailsFebricTemp
                {
                    PurchaseReceiveNo = await GenerateAutoFebrickId(),

                    ItemId = rdr["ItemID"].ToString(),
                    FabricItemId = dto.BookingType,
                    ColorId = rdr["ColorID"].ToString(),
                    FebricDetails = rdr["Description"].ToString(),

                    QtyUnit = rdr["TotalQuantityUnit"].ToString(),
                    Consumption = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                    ConsumtionUnit = rdr["TotalQuantityUnit"].ToString(),

                    TotalFebricQty = rdr["TotalQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalQuantity"]),
                    CurrentReceiveQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),
                    OrderQty = rdr["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Quantity"]),

                    ReceivedUnitPrice = rdr["UnitPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["UnitPrice"]),
                    TotalPrice = rdr["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["TotalPrice"]),

                    TotalReceivedQty = rdr["Consumption"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Consumption"]),
                    PendingReceiveQty = 0,

                    Percentage = rdr["Extra"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["Extra"]),
                    IntegraJobNo = rdr["IntegraJobNo"].ToString(),
                    PoNo = rdr["PoNo"].ToString(),
                    Slno = rdr["SLNO"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SLNO"])
                };

                await febricTempRepo.AddAsync(temp);
            }

            // ============================
            // RETURN LIST FROM TEMP TABLE
            // ============================
            var items = febricTempRepo.All()
                .Select(x => new
                {
                    Id = x.Id,
                    ItemID = x.ItemId,
                    ColorID = x.ColorId,
                    Description = x.FebricDetails,

                    Quantity = x.CurrentReceiveQty,
                    GarmentQty = x.OrderQty,
                    OrderQty = x.OrderQty,

                    Consumption = x.Consumption,
                    TotalQty = x.TotalFebricQty,
                    TotalQuantity = x.TotalFebricQty,
                    TotalQuantityUnit = x.QtyUnit,

                    UnitPrice = x.ReceivedUnitPrice,
                    TotalPrice = x.TotalPrice,

                    Percentage = x.Percentage,
                    SLNO = x.Slno ?? 0,

                    IntegraJobNO = x.IntegraJobNo,
                    PoNo = x.PoNo
                })
                .ToList();

            return items.Cast<object>().ToList();
        }




        public async Task<string> GenerateAutoFebrickId()
        {
            var getYear = DateTime.Now.Year.ToString();
            var prefix = "POR_" + getYear + "_";

            string lastCode = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // First check Temp table
                string queryTemp = @"SELECT MAX(PurchaseReceiveNo) 
                             FROM RMG_Inv_BookingReceivedDetails_FebricTemp 
                             WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                using (SqlCommand cmd = new SqlCommand(queryTemp, con))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    var result = await cmd.ExecuteScalarAsync();
                    lastCode = result?.ToString();
                }

                // If nothing found in Temp, check main table
                if (string.IsNullOrEmpty(lastCode))
                {
                    string queryMain = @"SELECT MAX(PurchaseReceiveNo) 
                                 FROM RMG_Inv_BookingReceivedDetails_Febric 
                                 WHERE PurchaseReceiveNo LIKE @prefix + '%'";
                    using (SqlCommand cmd = new SqlCommand(queryMain, con))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix);
                        var result = await cmd.ExecuteScalarAsync();
                        lastCode = result?.ToString();
                    }
                }
            }

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract numeric part after the prefix
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Pad with zeros to length 6
            string nextCode = prefix + nextNumber.ToString().PadLeft(6, '0');
            return nextCode;
        }



        //[HttpPost]
        //public IActionResult GetPurchaseOrders()
        //{
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var start = Request.Form["start"].FirstOrDefault();
        //    var length = Request.Form["length"].FirstOrDefault();

        //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
        //    int skip = start != null ? Convert.ToInt32(start) : 0;

        //    var list = new List<PurchaseOrderViewModel>();

        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {
        //        string query = @"
        //    SELECT DISTINCT 
        //           po.StyleId, 
        //           po.IntegraJOBNo,
        //           po.BuyerId, 
        //           pod.PurchaseOrder, 
        //           pod.OrderQuantity, 
        //           po.MasterPurchaseOrder  
        //    FROM RMG_Prod_Order po
        //    LEFT JOIN RMG_Prod_OrderDetails pod 
        //           ON po.OrderId = pod.OrderId";

        //        SqlCommand cmd = new SqlCommand(query, con);
        //        con.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            list.Add(new PurchaseOrderViewModel
        //            {
        //                Style = rdr["StyleId"].ToString(),
        //                StyleName = styleRepo.All().Where(c=> c.StyleId== rdr["StyleId"].ToString()).Select(x=> x.Style).FirstOrDefault(),
        //                FunJobNo = rdr["IntegraJOBNo"].ToString(),
        //                Buyer = rdr["BuyerId"].ToString(),
        //                BuyerName = buyerRepo.All().Where(x=> x.BuyerId== rdr["BuyerId"].ToString()).Select(c=> c.BuyerName).FirstOrDefault(),
        //                PoNo = rdr["PurchaseOrder"].ToString(),
        //                OrderQty = rdr["OrderQuantity"] == DBNull.Value ? null : Convert.ToInt32(rdr["OrderQuantity"]),
        //                MasterPo = rdr["MasterPurchaseOrder"].ToString()
        //            });
        //        }
        //    }

        //    var recordsTotal = list.Count;
        //    var data = list.Skip(skip).Take(pageSize).ToList();

        //    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        //}

        //[HttpPost]
        //public IActionResult GetPurchaseOrders()
        //{
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var start = Request.Form["start"].FirstOrDefault();
        //    var length = Request.Form["length"].FirstOrDefault();
        //    var searchValue = Request.Form["search[value]"].FirstOrDefault();

        //    // Filter parameters
        //    var poNo = Request.Form["poNo"].FirstOrDefault();
        //    var style = Request.Form["style"].FirstOrDefault();
        //    var buyer = Request.Form["buyer"].FirstOrDefault();
        //    var masterPo = Request.Form["masterPo"].FirstOrDefault();
        //    var funJobNo = Request.Form["funJobNo"].FirstOrDefault();

        //    int pageSize = length != null ? Convert.ToInt32(length) : 10;
        //    int skip = start != null ? Convert.ToInt32(start) : 0;

        //    var list = new List<PurchaseOrderViewModel>();

        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {

        //        string query = @"
        //    select distinct ci.StyleID, st.Style, ci.IntegraJobNO, ci.BuyerID, b.BuyerName, ci.MasterPurchaseOrder, cd.Quantity, ci.PoNo  from RMG_CostingInfo ci
        //    left join RMG_CostingDetails cd on cd.CostingID = ci.CostingID
        //    left join RMG_Prod_Def_Buyer b on b.BuyerId = ci.BuyerID
        //    left join  Prod_Def_Style st on ci.StyleID = st.StyleId
        //    where 1=1";
        //        // Global search
        //        if (!string.IsNullOrEmpty(searchValue))
        //        {
        //            query += @" AND (ci.PoNo LIKE @Search 
        //                OR ci.StyleId LIKE @Search 
        //                OR st.Style LIKE @Search 
        //                OR ci.BuyerId LIKE @Search 
        //                OR b.BuyerName LIKE @Search 
        //                OR ci.MasterPurchaseOrder LIKE @Search 
        //                OR ci.IntegraJOBNo LIKE @Search
        //                OR CAST(pod.OrderQuantity AS VARCHAR) LIKE @Search)";
        //        }

        //        // Add filters dynamically
        //        if (!string.IsNullOrEmpty(poNo))
        //            query += " AND pod.PurchaseOrder LIKE @PoNo";
        //        if (!string.IsNullOrEmpty(style))
        //            query += " AND po.StyleId LIKE @Style";
        //        if (!string.IsNullOrEmpty(buyer))
        //            query += " AND po.BuyerId LIKE @Buyer";
        //        if (!string.IsNullOrEmpty(masterPo))
        //            query += " AND po.MasterPurchaseOrder LIKE @MasterPo";
        //        if (!string.IsNullOrEmpty(funJobNo))
        //            query += " AND po.IntegraJOBNo LIKE @FunJobNo";

        //        SqlCommand cmd = new SqlCommand(query, con);

        //        // Add search parameter
        //        if (!string.IsNullOrEmpty(searchValue))
        //            cmd.Parameters.AddWithValue("@Search", "%" + searchValue + "%");

        //        // Add parameters
        //        if (!string.IsNullOrEmpty(poNo))
        //            cmd.Parameters.AddWithValue("@PoNo", "%" + poNo + "%");
        //        if (!string.IsNullOrEmpty(style))
        //            cmd.Parameters.AddWithValue("@Style", "%" + style + "%");
        //        if (!string.IsNullOrEmpty(buyer))
        //            cmd.Parameters.AddWithValue("@Buyer", "%" + buyer + "%");
        //        if (!string.IsNullOrEmpty(masterPo))
        //            cmd.Parameters.AddWithValue("@MasterPo", "%" + masterPo + "%");
        //        if (!string.IsNullOrEmpty(funJobNo))
        //            cmd.Parameters.AddWithValue("@FunJobNo", "%" + funJobNo + "%");

        //        con.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            list.Add(new PurchaseOrderViewModel
        //            {
        //                Style = rdr["StyleId"].ToString(),
        //                StyleName = styleRepo.All()
        //                    .Where(c => c.StyleId == rdr["StyleId"].ToString())
        //                    .Select(x => x.Style).FirstOrDefault(),
        //                FunJobNo = rdr["IntegraJOBNo"].ToString(),
        //                Buyer = rdr["BuyerId"].ToString(),
        //                BuyerName = buyerRepo.All()
        //                    .Where(x => x.BuyerId == rdr["BuyerId"].ToString())
        //                    .Select(c => c.BuyerName).FirstOrDefault(),
        //                PoNo = rdr["PurchaseOrder"].ToString(),
        //                OrderQty = rdr["OrderQuantity"] == DBNull.Value ?
        //                    null : Convert.ToInt32(rdr["OrderQuantity"]),
        //                MasterPo = rdr["MasterPurchaseOrder"].ToString()
        //            });
        //        }
        //    }

        //    var recordsTotal = list.Count;

        //    // Handle "All" option (length = -1)
        //    var data = pageSize == -1 ? list : list.Skip(skip).Take(pageSize).ToList();

        //    return Json(new
        //    {
        //        draw = draw,
        //        recordsFiltered = recordsTotal,
        //        recordsTotal = recordsTotal,
        //        data = data
        //    });
        //}

        [HttpPost]
        public IActionResult GetPurchaseOrders()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Filters
            var poNo = Request.Form["poNo"].FirstOrDefault();
            var style = Request.Form["style"].FirstOrDefault();
            var buyer = Request.Form["buyer"].FirstOrDefault();
            var masterPo = Request.Form["masterPo"].FirstOrDefault();
            var funJobNo = Request.Form["funJobNo"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var list = new List<PurchaseOrderViewModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
       SELECT DISTINCT
            ci.CostingID,
            ci.StyleID,
            st.Style,
            ci.IntegraJobNO,
            ci.BuyerID,
            b.BuyerName,
            ci.MasterPurchaseOrder,
            ci.PoNo,
            od.OrderQuantity
        FROM RMG_CostingInfo ci        
        left join RMG_Prod_OrderDetails od on od.PurchaseOrder= ci.PoNo
        LEFT JOIN RMG_Prod_Def_Buyer b ON b.BuyerId = ci.BuyerID
        LEFT JOIN Prod_Def_Style st ON ci.StyleID = st.StyleId
        WHERE 1 = 1";


                // 🔍 Global Search (DataTable)
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query += @"
            AND (
                ci.PoNo LIKE @Search
                OR st.Style LIKE @Search
                OR b.BuyerName LIKE @Search
                OR ci.MasterPurchaseOrder LIKE @Search
                OR ci.IntegraJobNO LIKE @Search
                OR CAST(od.OrderQuantity AS VARCHAR) LIKE @Search
            )";
                }

                // 🎯 Individual Filters
                if (!string.IsNullOrEmpty(poNo))
                    query += " AND ci.PoNo LIKE @PoNo";

                if (!string.IsNullOrEmpty(style))
                    query += " AND ci.StyleID = @Style";

                if (!string.IsNullOrEmpty(buyer))
                    query += " AND ci.BuyerID = @Buyer";

                if (!string.IsNullOrEmpty(masterPo))
                    query += " AND ci.MasterPurchaseOrder LIKE @MasterPo";

                if (!string.IsNullOrEmpty(funJobNo))
                    query += " AND ci.IntegraJobNO LIKE @FunJobNo";

                SqlCommand cmd = new SqlCommand(query, con);

                // Parameters
                if (!string.IsNullOrEmpty(searchValue))
                    cmd.Parameters.AddWithValue("@Search", "%" + searchValue + "%");

                if (!string.IsNullOrEmpty(poNo))
                    cmd.Parameters.AddWithValue("@PoNo", "%" + poNo + "%");

                if (!string.IsNullOrEmpty(style))
                    cmd.Parameters.AddWithValue("@Style", style);

                if (!string.IsNullOrEmpty(buyer))
                    cmd.Parameters.AddWithValue("@Buyer", buyer);

                if (!string.IsNullOrEmpty(masterPo))
                    cmd.Parameters.AddWithValue("@MasterPo", "%" + masterPo + "%");

                if (!string.IsNullOrEmpty(funJobNo))
                    cmd.Parameters.AddWithValue("@FunJobNo", "%" + funJobNo + "%");

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new PurchaseOrderViewModel
                    {
                        Style = rdr["StyleID"].ToString(),
                        CostingId = rdr["CostingId"].ToString(),
                        StyleName = rdr["Style"].ToString(),
                        FunJobNo = rdr["IntegraJobNO"].ToString(),
                        Buyer = rdr["BuyerID"].ToString(),
                        BuyerName = rdr["BuyerName"].ToString(),
                        PoNo = rdr["PoNo"].ToString(),
                        OrderQty = rdr["OrderQuantity"] == DBNull.Value ? null : Convert.ToInt32(rdr["OrderQuantity"]),
                        MasterPo = rdr["MasterPurchaseOrder"].ToString()
                    });
                }
            }

            var recordsTotal = list.Count;

            var data = pageSize == -1
                ? list
                : list.Skip(skip).Take(pageSize).ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = data
            });
        }



        //[HttpPost]
        //public JsonResult GetItemTypes([FromBody] string CostingId)
        //{
        //    var list = new List<ItemTypeViewModel>();

        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {
        //        string query = @"

        //   SELECT DISTINCT dit.BookingItemTypeID, dit.BookingItemType
        //                        FROM RMG_CostingInfo ci
        //                        LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
        //                        LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
        //                        LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
        //                        where cd.CostingID =@CostingId and (cd.PoNo is null or cd.PoNo ='')
        //                          AND dit.BookingItemTypeID IS NOT NULL
        //                          AND dit.BookingItemType IS NOT NULL";

        //        SqlCommand cmd = new SqlCommand(query, con);
        //        cmd.Parameters.AddWithValue("@CostingId", CostingId ?? "");


        //        con.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            list.Add(new ItemTypeViewModel
        //            {
        //                BookingItemTypeID = rdr["BookingItemTypeID"].ToString(),
        //                BookingItemType = rdr["BookingItemType"].ToString()
        //            });
        //        }
        //    }

        //    return Json(list);
        //}


        [HttpPost]
        public JsonResult GetItemTypes([FromBody] List<string> CostingIds)
        {
            var list = new List<ItemTypeViewModel>();

            if (CostingIds == null || !CostingIds.Any())
                return Json(list);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Dynamic IN clause
                var parameters = CostingIds
                    .Select((id, index) => $"@id{index}")
                    .ToArray();

                string query = $@"
            SELECT DISTINCT 
                dit.BookingItemTypeID,
                dit.BookingItemType
            FROM RMG_CostingInfo ci
            LEFT JOIN RMG_CostingDetails cd ON ci.CostingID = cd.CostingID
            LEFT JOIN Inv_Def_Item di ON di.ItemID = cd.ItemID
            LEFT JOIN Inv_Def_BookingItemType dit ON dit.BookingItemTypeID = di.ItemTypeID
            WHERE cd.CostingID IN ({string.Join(",", parameters)})
              AND (cd.PoNo IS NULL OR cd.PoNo = '')
              AND dit.BookingItemTypeID IS NOT NULL
              AND dit.BookingItemType IS NOT NULL";

                SqlCommand cmd = new SqlCommand(query, con);

                for (int i = 0; i < CostingIds.Count; i++)
                {
                    cmd.Parameters.AddWithValue(parameters[i], CostingIds[i]);
                }

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new ItemTypeViewModel
                    {
                        BookingItemTypeID = rdr["BookingItemTypeID"].ToString(),
                        BookingItemType = rdr["BookingItemType"].ToString()
                    });
                }
            }

            return Json(list);
        }




        [HttpPost]
        public async Task<IActionResult> SaveBooking([FromBody] RMGBookingOrderEntryBuklDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (isSuccess, message) = await rmgBookingOrderEntryBuklService.SaveBookingAsync(dto);

            if (isSuccess)
                return Ok(new { success = true, message = message });

            return BadRequest(new { success = false, message = message });
        }

        [HttpPost]
        public async Task<IActionResult> GetBookingList()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var search = Request.Form["search[value]"].FirstOrDefault();

            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var result = await rmgBookingOrderEntryBuklService.GetBookingListAsync(start, length, search, sortColumn, sortDir);

            return Json(new
            {
                draw = draw,
                recordsTotal = result.total,
                recordsFiltered = result.filtered,
                data = result.data
            });
        }


    }

    public class SaveBookingRequest
    {
        public string BookingType { get; set; }
        public List<Dictionary<string, object>> BookingData { get; set; }
    }
}

