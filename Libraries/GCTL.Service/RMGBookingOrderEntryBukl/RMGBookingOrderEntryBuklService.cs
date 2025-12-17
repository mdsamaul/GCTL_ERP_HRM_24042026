using GCTL.Core.Data;
using GCTL.Core.ViewModels.RMGBookingOrderEntryBukl;
using GCTL.Data.Models;
using GCTL.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;

namespace GCTL.Service.RMGBookingOrderEntryBukl
{
    public class RMGBookingOrderEntryBuklService : AppService<RmgBookingOrder>, IRMGBookingOrderEntryBuklService
    {
        private readonly IRepository<RmgBookingOrder> boRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
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
        private readonly IRepository<RmgCostingInfo> costingRepo;
        private readonly ICommonService commonService;
        private readonly string _connectionString;

        public RMGBookingOrderEntryBuklService(
            IRepository<RmgBookingOrder> boRepo,
            IRepository<CoreAccessCode> accessCodeRepository,
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
            IRepository<RmgCostingInfo> costingRepo,
            IConfiguration configuration,
            ICommonService commonService

            ) : base(boRepo)
        {
            this.boRepo = boRepo;
            this.accessCodeRepository = accessCodeRepository;
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
            this.costingRepo = costingRepo;
            this.commonService = commonService;
            //this.configuration = configuration.GetConnectionString("ApplicationDbConnection");
            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }

        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";




        #region Duplicate Check 

        public async Task<bool> IsExistByCodeAsync(string code)
        {
            return await boRepo.All().AnyAsync(x => x.BookinOrderNo == code);
        }

        public async Task<bool> IsExistAsync(string name)
        {
            return await boRepo.All().AnyAsync(x => x.BookinOrderNo == name);
        }

        public async Task<bool> IsExistAsync(string employeeCode, string phone, string email)
        {
            var result = boRepo.All().FirstOrDefault(e => e.BookinOrderNo == employeeCode);

            return await boRepo.All().AnyAsync(x => x.BookinOrderNo == employeeCode && x.BookinOrderNo == phone && x.BookinOrderNo == email);
        }

        #endregion

        #region Permission all type

        public async Task<bool> PagePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Supplier Information" && x.TitleCheck);
        }

        public async Task<bool> SavePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Supplier Information" && x.CheckAdd);
        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Supplier Information" && x.CheckEdit);
        }

        public async Task<bool> DeletePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Supplier Information" && x.CheckDelete);
        }

        #endregion


        public async Task<(bool isSuccess, string message)> SaveBookingAsync(RMGBookingOrderEntryBuklDto dto, string companyCode)
        {

            try
            {

                try
                {
                    if (dto.BookingType == null || !dto.BookingType.Any())
                        return (false, CreateFailed);


                    switch (dto.BookingType)
                    {
                        case "04":
                            await CartonBookingData();
                            break;
                        case "07":
                            await ThreadBookingData();
                            break;
                        case "03":
                            await PolyBookingData();
                            break;
                        case "02":
                            await ButtonBookingData();
                            break;
                        case "01":
                            await FebricBookingData();
                            break;
                        default:
                            await ExtraBookingData();
                            break;
                    }

                }
                catch (Exception ex)
                {

                    return (false, $"Details Save Failed: {CreateFailed}. Error: {ex.Message}");
                }


                if (dto == null || string.IsNullOrEmpty(dto.BookinOrderNo))
                {
                    return (false, "Booking Order No cannot be empty!");
                }

                if (dto.Tc == 0) // *** CREATE ***
                {
                    // Check duplicate BookingOrderNo
                    bool exists = await IsExistByCodeAsync(dto.BookinOrderNo);
                    if (exists)
                        return (false, "This Booking Order No already exists!");





                    foreach (var costingId in dto.SelectedCostingIds)
                    {
                        var y = DateTime.Now.ToString("yyyy");
                        var CItem = costingRepo.All().Where(x => x.CostingId == costingId).FirstOrDefault();
                        if (CItem != null)
                        {
                            var entity = new RmgBookingOrder
                            {

                                BookinOrderNo = commonService.GenerateNextCode("BookinOrderNO", "RMG_BookingOrder", 3, "FAWI-" + y + "-") ?? "",
                                BookinDate = (DateTime)dto.BookinDate,
                                BuyerId = CItem.BuyerId ?? "",
                                StyleId = CItem.StyleId ?? "",
                                MasterPurchaseOrder = CItem.MasterPurchaseOrder ?? "",
                                PoNo = CItem.PoNo ?? "",
                                IntegraJobNo = CItem.IntegraJobNo ?? "",
                                PurchasedOfficer = dto.PurchasedOfficer ?? "",
                                Remarks = dto.Remarks ?? "",
                                EmployeId = dto.UserInfoEmployeeId ?? "",
                                CompanyId = companyCode ?? "",
                                DeliveryDate = dto.DeliveryDate,
                                DeliveryAddress = dto.DeliveryAddress ?? "",
                                DeliveryMethod = dto.DeliveryMethod ?? "",
                                PaymentTerms = dto.PaymentTerms ?? "",
                                TermsCondition = dto.TermsCondition ?? "",
                                BookingType = dto.BookingType ?? "",
                                BookingEntryType = dto.BookingEntryType ?? "",
                                WarehouseId = dto.WarehouseId ?? "",
                                Pino = dto.Pino ?? "",
                                Pidate = dto.Pidate,
                                Pivalue = dto.Pivalue,
                                PicurrencyId = dto.PicurrencyId ?? "",
                                SupplierId = dto.SupplierId ?? "",
                                Mrbpid = dto.Mrbpid ?? "",
                                EnterFromPageName = dto.EnterFromPageName ?? "",
                                PifilePath = dto.PifilePath ?? "",
                                Ldate = dto.Ldate ?? null,
                                Lmac = dto.Lmac ?? "",
                                Lip = dto.Lip ?? "",
                                Luser = dto.Luser ?? "",

                            };

                            await boRepo.AddAsync(entity);
                        }
                    }


                    //await transaction.CommitAsync();
                    return (true, CreateSuccess);
                }
                else // *** UPDATE ***
                {
                    bool existsByTc = await IsExistByCodeAsync(dto.BookinOrderNo);

                    if (!existsByTc)
                        return (false, "Booking entry not found!");

                    // Update full mapping
                    var entity = new RmgBookingOrder
                    {

                        BookinDate = (DateTime)dto.BookinDate,

                        PurchasedOfficer = dto.PurchasedOfficer ?? "",
                        Remarks = dto.Remarks ?? "",

                        DeliveryDate = dto.DeliveryDate,
                        DeliveryAddress = dto.DeliveryAddress ?? "",
                        DeliveryMethod = dto.DeliveryMethod ?? "",
                        PaymentTerms = dto.PaymentTerms ?? "",
                        TermsCondition = dto.TermsCondition ?? "",
                        //BookingType = dto.BookingType ?? "",
                        BookingEntryType = dto.BookingEntryType ?? "",
                        WarehouseId = dto.WarehouseId ?? "",
                        //Pino = dto.Pino ?? "",
                        Pidate = dto.Pidate,
                        Pivalue = dto.Pivalue,
                        PicurrencyId = dto.PicurrencyId ?? "",
                        SupplierId = dto.SupplierId ?? "",
                        Mrbpid = dto.Mrbpid ?? "",
                        EnterFromPageName = dto.EnterFromPageName ?? "",
                        PifilePath = dto.PifilePath ?? "",
                        ModifyDate = DateTime.Now
                    };

                    await boRepo.UpdateAsync(entity);

                    // await transaction.CommitAsync();
                    return (true, UpdateSuccess);
                }
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync(); 
                return (false, $"Error: {ex.Message}");
            }
        }



        // --- 1. Carton Booking Data (Provided by User, Minor Fix) ---



        private async Task<List<RmgInvBookingReceivedDetailsCarton>> CartonBookingData()
        {
            List<RmgInvBookingReceivedDetailsCarton> newCartonBookings = new List<RmgInvBookingReceivedDetailsCarton>();

            try
            {
                // 1. Get all temp data
                var cartonBookingTempData = cartonTempRepo.All().ToList();

                if (cartonBookingTempData == null || cartonBookingTempData.Count == 0)
                {
                    return newCartonBookings;
                }

                // 2. Get current max Slno from main table
                var maxSlno = cartonRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in cartonBookingTempData)
                {
                    // 3. Check if record already exists in main table (by PoNo + IntegraJobNo + ItemId)
                    var existing = cartonRepo.All()
                        .Where(x => x.PoNo == tempItem.PoNo
                                 && x.IntegraJobNo == tempItem.IntegraJobNo
                                 && x.ItemId == tempItem.ItemId)
                        .ToList();

                    if (existing.Any())
                    {
                        // 4. Delete old records before inserting new
                        await cartonRepo.DeleteRangeAsync(existing);
                    }

                    // 5. Create new main record from temp
                    var mainItem = new RmgInvBookingReceivedDetailsCarton
                    {
                        PurchaseReceiveNo = tempItem.PurchaseReceiveNo,
                        ItemId = tempItem.ItemId,
                        ItemDescription = tempItem.ItemDescription,
                        OrderQty = tempItem.OrderQty,
                        OrderUnitId = tempItem.OrderUnitId,
                        RequiredQty = tempItem.RequiredQty,
                        RequiredQtyUnitId = tempItem.RequiredQtyUnitId,
                        ConsumptionUnitId = tempItem.ConsumptionUnitId,
                        Consumption = tempItem.Consumption,
                        UnitPrice = tempItem.UnitPrice,
                        TotalPrice = tempItem.TotalPrice,
                        PoNo = tempItem.PoNo,
                        IntegraJobNo = tempItem.IntegraJobNo,
                        Slno = sNo++,

                        ColorId = tempItem.ColorId,
                        SizeId = tempItem.SizeId,
                        Refcode = tempItem.Refcode,
                        CartonLeangth = tempItem.CartonLeangth,
                        LeangthUnitId = tempItem.LeangthUnitId,
                        CartonWidth = tempItem.CartonWidth,
                        WidthUnitId = tempItem.WidthUnitId,
                        CatonHeight = tempItem.CatonHeight,
                        HeightUnitId = tempItem.HeightUnitId,
                        CartonPercent = int.TryParse(tempItem.CartonPercent, out int cartonPercent)
                            ? cartonPercent
                            : 0,
                        TotalReceivedQty = tempItem.TotalReceivedQty,
                        CurrentReceiveQty = tempItem.CurrentReceiveQty,
                        ReceivedUnitPrice = tempItem.ReceivedUnitPrice,
                        TotalReceivedQtyPre = tempItem.TotalReceivedQtyPre,
                        PendingReceiveQty = tempItem.PendingReceiveQty,
                        PendingReceiveQtyPre = tempItem.PendingReceiveQtyPre,

                        Brdid = tempItem.Brdid,
                        ReceivedUnitType = tempItem.ReceivedUnitType,
                        CurrencyId = tempItem.CurrencyId,
                        Remarks = tempItem.Remarks,
                        EmployeeId = tempItem.EmployeeId,
                    };

                    newCartonBookings.Add(mainItem);
                }

                // 6. Save new records to main table
                await cartonRepo.AddRangeAsync(newCartonBookings);

                // 7. Clear temp table after migration
                await cartonTempRepo.DeleteRangeAsync(cartonBookingTempData);

                return newCartonBookings;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving carton booking data: {ex.Message}");
                throw;
            }
        }

        // --- 2. Thread Booking Data (Provided by User, Complete) ---
        private async Task<List<RmgInvBookingReceivedDetailsThread>> ThreadBookingData()
        {
            List<RmgInvBookingReceivedDetailsThread> newThreadBookings = new List<RmgInvBookingReceivedDetailsThread>();

            try
            {
                var threadBookingTempData = threadTempRepo.All().ToList();

                if (threadBookingTempData == null || threadBookingTempData.Count == 0)
                {
                    return newThreadBookings;
                }

                var maxSlno = threadRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in threadBookingTempData)
                {
                    // 🔹 Check if record already exists in main table
                    var existing = threadRepo.All()
                        .Where(x => x.PoNo == tempItem.PoNo
                                 && x.IntegraJobNo == tempItem.IntegraJobNo
                                 && x.ItemId == tempItem.ItemId)
                        .ToList();

                    if (existing.Any())
                    {
                        // 🔹 Delete old records before inserting new
                        await threadRepo.DeleteRangeAsync(existing);
                    }

                    var mainItem = new RmgInvBookingReceivedDetailsThread
                    {
                        PurchaseReceiveNo = tempItem.PurchaseReceiveNo,
                        PoNo = tempItem.PoNo,
                        IntegraJobNo = tempItem.IntegraJobNo,
                        Slno = sNo++,
                        Brdid = tempItem.Brdid,
                        ItemId = tempItem.ItemId,
                        ColorId = tempItem.ColorId,

                        FebricDetail = tempItem.FebricDetail,
                        ThreadColorId = tempItem.ThreadColorId,
                        ThreadCountId = tempItem.ThreadCountId,
                        Refcodepantone = tempItem.Refcodepantone,
                        ThreadReqUnit = tempItem.ThreadReqUnit,
                        Threadpercent = tempItem.Threadpercent,

                        OrderQty = tempItem.OrderQty,
                        QtyUnitId = tempItem.QtyUnitId,
                        Consumption = tempItem.Consumption,
                        ConsumtionUnitId = tempItem.ConsumtionUnitId,
                        TotalQty = tempItem.TotalQty,
                        TotalQtyUnitId = tempItem.TotalQtyUnitId,
                        ReqQty = tempItem.ReqQty,

                        UnitPrice = tempItem.UnitPrice,
                        TotalPrice = tempItem.TotalPrice,
                        CurrencyId = tempItem.CurrencyId,

                        TotalReceivedQty = tempItem.TotalReceivedQty,
                        CurrentReceiveQty = tempItem.CurrentReceiveQty,
                        ReceivedUnitType = tempItem.ReceivedUnitType,
                        ReceivedUnitPrice = tempItem.ReceivedUnitPrice,
                        TotalReceivedQtyPre = tempItem.TotalReceivedQtyPre,
                        PendingReceiveQty = tempItem.PendingReceiveQty,
                        PendingReceiveQtyPre = tempItem.PendingReceiveQtyPre,

                        Remarks = tempItem.Remarks,
                        EmployeeId = tempItem.EmployeeId,
                    };

                    newThreadBookings.Add(mainItem);
                }

                // 🔹 Save new records to main table
                await threadRepo.AddRangeAsync(newThreadBookings);

                // 🔹 Clear temp table after migration
                await threadTempRepo.DeleteRangeAsync(threadBookingTempData);

                return newThreadBookings;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving thread booking data: {ex.Message}");
                throw;
            }
        }
        // --- 3. Poly Booking Data (Case "03") ---

        private async Task<List<RmgInvBookingReceivedDetailsPoly>> PolyBookingData()
        {
            List<RmgInvBookingReceivedDetailsPoly> newPolyBookings = new List<RmgInvBookingReceivedDetailsPoly>();

            try
            {
                var polyBookingTempData = polyTempRepo.All().ToList();

                if (polyBookingTempData == null || polyBookingTempData.Count == 0)
                {
                    return newPolyBookings;
                }

                // SerialNo is used here
                var maxSlno = polyRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in polyBookingTempData)
                {
                    // 🔹 Check if record already exists in main table
                    var existing = polyRepo.All()
                        .Where(x => x.PoNo == tempItem.PoNo
                                 && x.IntegraJobNo == tempItem.IntegraJobNo
                                 && x.ItemId == tempItem.ItemId)
                        .ToList();

                    if (existing.Any())
                    {
                        // 🔹 Delete old records before inserting new
                        await polyRepo.DeleteRangeAsync(existing);
                    }

                    var mainItem = new RmgInvBookingReceivedDetailsPoly
                    {
                        // Core Data
                        PurchaseReceiveNo = tempItem.PurchaseReceiveNo,
                        PoNo = tempItem.PoNo,
                        IntegraJobNo = tempItem.IntegraJobNo,
                        SerialNo = sNo++,

                        // Item Details
                        Brdid = tempItem.Brdid,
                        ItemId = tempItem.ItemId,
                        ItemDescription = tempItem.ItemDescription,
                        ColorId = tempItem.ColorId,
                        RefernceCode = tempItem.RefernceCode,

                        // Dimensions
                        Length = tempItem.Length,
                        LengthUnitId = tempItem.LengthUnitId,
                        Width = tempItem.Width,
                        WidthUnitId = tempItem.WidthUnitId,
                        Flap = tempItem.Flap,
                        FlapUnitId = tempItem.FlapUnitId,
                        Guest = tempItem.Guest,
                        GuestUnitId = tempItem.GuestUnitId,

                        // Quantity and Consumption
                        GarmentQty = tempItem.GarmentQty,
                        GarmentQtyUnitId = tempItem.GarmentQtyUnitId,
                        Consumption = tempItem.Consumption,
                        ConsumptionUnitId = tempItem.ConsumptionUnitId,
                        TotalQty = tempItem.TotalQty,
                        TotalQtyUnitId = tempItem.TotalQtyUnitId,
                        Percentage = tempItem.Percentage,

                        // Price and Received
                        TotalReceivedQty = tempItem.TotalReceivedQty,
                        CurrentReceiveQty = tempItem.CurrentReceiveQty,
                        ReceivedUnitType = tempItem.ReceivedUnitType,
                        UnitPrice = tempItem.UnitPrice,
                        ReceivedUnitPrice = tempItem.ReceivedUnitPrice,
                        TotalPrice = tempItem.TotalPrice,
                        CurrencyId = tempItem.CurrencyId,
                        TotalReceivedQtyPre = tempItem.TotalReceivedQtyPre,
                        PendingReceiveQty = tempItem.PendingReceiveQty,
                        PendingReceiveQtyPre = tempItem.PendingReceiveQtyPre,

                        // Other
                        Remarks = tempItem.Remarks,
                        EmployeeId = tempItem.EmployeeId,
                    };

                    newPolyBookings.Add(mainItem);
                }

                // 🔹 Save new records to main table
                await polyRepo.AddRangeAsync(newPolyBookings);

                // 🔹 Clear temp table after migration
                await polyTempRepo.DeleteRangeAsync(polyBookingTempData);

                return newPolyBookings;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving poly booking data: {ex.Message}");
                throw;
            }
        }

        // --- 4. Button Booking Data (Case "02") ---

        private async Task<List<RmgInvBookingReceivedDetailsButton>> ButtonBookingData()
        {
            List<RmgInvBookingReceivedDetailsButton> newButtonBookings = new List<RmgInvBookingReceivedDetailsButton>();

            try
            {
                var ButtonBookingTempData = buttonTempRepo.All().ToList();

                if (ButtonBookingTempData == null || ButtonBookingTempData.Count == 0)
                {
                    return newButtonBookings;
                }

                // SerialNo is used here
                //var maxSlno = buttonRepo.All().Select(x => (int?)x.SerialNo).Max() ?? 0;
                var count = buttonRepo.All().Count();

                var sNo = count + 1;

                foreach (var tempItem in ButtonBookingTempData)
                {
                    var mainItem = new RmgInvBookingReceivedDetailsButton
                    {
                        // Core Data
                        PurchaseReceiveNo = tempItem.PurchaseReceiveNo,
                        PoNo = tempItem.PoNo,
                        IntegraJobNo = tempItem.IntegraJobNo,
                        SerialNo = sNo++,

                        // Item Details
                        Brdid = tempItem.Brdid,
                        ItemId = tempItem.ItemId,
                        Description = tempItem.Description,
                        FabricColorId = tempItem.FabricColorId,
                        ColorId = tempItem.ColorId,
                        SizeId = tempItem.SizeId,
                        Idno = tempItem.Idno,

                        // Quantity and Consumption
                        GermentQty = tempItem.GermentQty,
                        GermentsQtyUnitId = tempItem.GermentsQtyUnitId,
                        Consumption = tempItem.Consumption,
                        ConsumptionUnitId = tempItem.ConsumptionUnitId,
                        TotalQty = tempItem.TotalQty,
                        TotalQtyUnitId = tempItem.TotalQtyUnitId,
                        OrderQty = tempItem.OrderQty,
                        OrderQtyUnitId = tempItem.OrderQtyUnitId,
                        Percentage = tempItem.Percentage,

                        // Price and Received
                        TotalReceivedQty = tempItem.TotalReceivedQty,
                        CurrentReceiveQty = tempItem.CurrentReceiveQty,
                        ReceivedUnitType = tempItem.ReceivedUnitType,
                        UnitPrice = tempItem.UnitPrice,
                        ReceivedUnitPrice = tempItem.ReceivedUnitPrice,
                        TotalPrice = tempItem.TotalPrice,
                        CurrencyId = tempItem.CurrencyId,
                        TotalReceivedQtyPre = tempItem.TotalReceivedQtyPre,
                        PendingReceiveQty = tempItem.PendingReceiveQty,
                        PendingReceiveQtyPre = tempItem.PendingReceiveQtyPre,

                        // Other
                        Remarks = tempItem.Remarks,
                        EmployeeId = tempItem.EmployeeId,
                    };
                    newButtonBookings.Add(mainItem);
                }

                await buttonRepo.AddRangeAsync(newButtonBookings);
                await buttonTempRepo.DeleteRangeAsync(ButtonBookingTempData);

                return newButtonBookings;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving button booking data: {ex.Message}");
                throw;
            }
        }

        // --- 5. Febric Booking Data (Case "01") ---

        private async Task<List<RmgInvBookingReceivedDetailsFebric>> FebricBookingData()
        {
            List<RmgInvBookingReceivedDetailsFebric> newFebricBookings = new List<RmgInvBookingReceivedDetailsFebric>();

            try
            {
                var febricBookingTempData = febricTempRepo.All().ToList();

                if (febricBookingTempData == null || febricBookingTempData.Count == 0)
                {
                    return newFebricBookings;
                }

                // Slno is used here
                var maxSlno = febricRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in febricBookingTempData)
                {
                    // 🔹 Check if record already exists in main table
                    var existing = febricRepo.All()
                        .Where(x => x.PoNo == tempItem.PoNo
                                 && x.IntegraJobNo == tempItem.IntegraJobNo
                                 && x.ItemId == tempItem.ItemId)
                        .ToList();

                    if (existing.Any())
                    {
                        // 🔹 Delete old records before inserting new
                        await febricRepo.DeleteRangeAsync(existing);
                    }

                    var mainItem = new RmgInvBookingReceivedDetailsFebric
                    {
                        // Core Data
                        PurchaseReceiveNo = tempItem.PurchaseReceiveNo,
                        PoNo = tempItem.PoNo,
                        IntegraJobNo = tempItem.IntegraJobNo,
                        Slno = sNo++,

                        // Item Details
                        Brdid = tempItem.Brdid,
                        ColorId = tempItem.ColorId,
                        FabricItemId = tempItem.FabricItemId,
                        ItemId = tempItem.ItemId,
                        FebricDetails = tempItem.FebricDetails,
                        Refcode = tempItem.Refcode,

                        // Quantity and Consumption
                        OrderQty = tempItem.OrderQty,
                        QtyUnit = tempItem.QtyUnit,
                        Consumption = tempItem.Consumption,
                        ConsumtionUnit = tempItem.ConsumtionUnit,
                        TotalFebricQty = tempItem.TotalFebricQty,
                        Percentage = tempItem.Percentage,

                        // Price and Received
                        TotalReceivedQty = tempItem.TotalReceivedQty,
                        CurrentReceiveQty = tempItem.CurrentReceiveQty,
                        ReceivedUnitType = tempItem.ReceivedUnitType,
                        UnitPrice = tempItem.UnitPrice,
                        ReceivedUnitPrice = tempItem.ReceivedUnitPrice,
                        TotalPrice = tempItem.TotalPrice,
                        CurrencyId = tempItem.CurrencyId,
                        TotalReceivedQtyPre = tempItem.TotalReceivedQtyPre,
                        PendingReceiveQty = tempItem.PendingReceiveQty,
                        PendingReceiveQtyPre = tempItem.PendingReceiveQtyPre,

                        // Other
                        EmployeeId = tempItem.EmployeeId,
                    };

                    newFebricBookings.Add(mainItem);
                }

                // 🔹 Save new records to main table
                await febricRepo.AddRangeAsync(newFebricBookings);

                // 🔹 Clear temp table after migration
                await febricTempRepo.DeleteRangeAsync(febricBookingTempData);

                return newFebricBookings;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving febric booking data: {ex.Message}");
                throw;
            }
        }
        // --- 6. Extra Booking Data (Default) ---

        private async Task<List<RmgInvBookingReceivedDetailsExtra>> ExtraBookingData()
        {
            List<RmgInvBookingReceivedDetailsExtra> newExtraBookings = new List<RmgInvBookingReceivedDetailsExtra>();

            try
            {
                var extraBookingTempData = extraTempRepo.All().ToList();

                if (extraBookingTempData == null || extraBookingTempData.Count == 0)
                {
                    return newExtraBookings;
                }

                // Slno is used here
                var maxSlno = extraRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in extraBookingTempData)
                {
                    // 🔹 Check if record already exists in main table
                    var existing = extraRepo.All()
                        .Where(x => x.PoNo == tempItem.PoNo
                                 && x.IntegraJobNo == tempItem.IntegraJobNo
                                 && x.ItemId == tempItem.ItemId)
                        .ToList();

                    if (existing.Any())
                    {
                        // 🔹 Delete old records before inserting new
                        await extraRepo.DeleteRangeAsync(existing);
                    }

                    var mainItem = new RmgInvBookingReceivedDetailsExtra
                    {
                        // Core Data
                        PurchaseReceiveNo = tempItem.PurchaseReceiveNo,
                        PoNo = tempItem.PoNo,
                        IntegraJobNo = tempItem.IntegraJobNo,
                        Slno = sNo++,

                        // Item Details
                        Brdid = tempItem.Brdid,
                        FabricColorId = tempItem.FabricColorId,
                        ItemId = tempItem.ItemId,
                        Description = tempItem.Description,
                        ColorId = tempItem.ColorId,

                        // Quantity and Consumption
                        OrderQty = tempItem.OrderQty,
                        OrderQtyIunitD = tempItem.OrderQtyIunitD,
                        Consumption = tempItem.Consumption,
                        ConsumptionUnitId = tempItem.ConsumptionUnitId,
                        TotalQty = tempItem.TotalQty,
                        TotalQtyUnitId = tempItem.TotalQtyUnitId,
                        ReqQty = tempItem.ReqQty,
                        ReqQtyUnitId = tempItem.ReqQtyUnitId,
                        Percentage = tempItem.Percentage,

                        // Price and Received
                        TotalReceivedQty = tempItem.TotalReceivedQty,
                        CurrentReceiveQty = tempItem.CurrentReceiveQty,
                        ReceivedUnitType = tempItem.ReceivedUnitType,
                        UnitPrice = tempItem.UnitPrice,
                        ReceivedUnitPrice = tempItem.ReceivedUnitPrice,
                        TotalPrice = tempItem.TotalPrice,
                        CurrencyId = tempItem.CurrencyId,
                        TotalReceivedQtyPre = tempItem.TotalReceivedQtyPre,
                        PendingReceiveQty = tempItem.PendingReceiveQty,
                        PendingReceiveQtyPre = tempItem.PendingReceiveQtyPre,

                        // Other
                        Remarks = tempItem.Remarks,
                        EmployeeId = tempItem.EmployeeId,
                    };

                    newExtraBookings.Add(mainItem);
                }

                // 🔹 Save new records to main table
                await extraRepo.AddRangeAsync(newExtraBookings);

                // 🔹 Clear temp table after migration
                await extraTempRepo.DeleteRangeAsync(extraBookingTempData);

                return newExtraBookings;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving extra booking data: {ex.Message}");
                throw;
            }
        }


















        public async Task<(IEnumerable<object> data, int total, int filtered)> GetBookingListAsync(
        int start, int length, string search, string sortColumn, string sortDir)
        {
            var query = boRepo.All();

            int totalData = await query.CountAsync();

            // Searching
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.BookinOrderNo.Contains(search) ||
                    x.SupplierId.Contains(search) ||
                    x.StyleId.Contains(search) ||
                    x.PoNo.Contains(search));
            }

            int filteredData = await query.CountAsync();

            // Sorting
            query = sortColumn switch
            {
                "BookinOrderNo" => (sortDir == "asc") ? query.OrderBy(x => x.BookinOrderNo) : query.OrderByDescending(x => x.BookinOrderNo),
                "BookinDate" => (sortDir == "asc") ? query.OrderBy(x => x.BookinDate) : query.OrderByDescending(x => x.BookinDate),
                "SupplierId" => (sortDir == "asc") ? query.OrderBy(x => x.SupplierId) : query.OrderByDescending(x => x.SupplierId),
                _ => query.OrderByDescending(x => x.Tc)
            };

            // Paging
            var data = await query
                .Skip(start)
                .Take(length)
                .Select(x => new
                {
                    Tc = x.Tc,
                    BookingOrderNo = x.BookinOrderNo,
                    BookingDate = x.BookinDate.ToString("dd/MM/yyyy"),
                    BuyerId = x.BuyerId,
                    StyleId = x.StyleId,
                    MasterPurchaseOrder = x.MasterPurchaseOrder,
                    PoNo = x.PoNo,
                    IntegraJobNo = x.IntegraJobNo,
                    PurchasedOfficer = x.PurchasedOfficer,
                    Remarks = x.Remarks,
                    Luser = x.Luser,
                    Ldate = x.Ldate.HasValue ? x.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    Lip = x.Lip,
                    Lmac = x.Lmac,
                    ModifyDate = x.ModifyDate.HasValue ? x.ModifyDate.Value.ToString("dd/MM/yyyy") : "",
                    EmployeId = x.EmployeId,
                    CompanyId = x.CompanyId,
                    DeliveryDate = x.DeliveryDate.HasValue ? x.DeliveryDate.Value.ToString("dd/MM/yyyy") : "",
                    DeliveryAddress = x.DeliveryAddress,
                    DeliveryMethod = x.DeliveryMethod,
                    PaymentTerms = x.PaymentTerms,
                    TermsCondition = x.TermsCondition,
                    BookingType = x.BookingType,
                    BookingEntryType = x.BookingEntryType,
                    WarehouseId = x.WarehouseId,
                    Pino = x.Pino,
                    Pidate = x.Pidate.HasValue ? x.Pidate.Value.ToString("dd/MM/yyyy") : "",
                    Pivalue = x.Pivalue,
                    PicurrencyId = x.PicurrencyId,
                    SupplierId = x.SupplierId,
                    Mrbpid = x.Mrbpid,
                    EnterFromPageName = x.EnterFromPageName,
                    PifilePath = x.PifilePath
                }).ToListAsync();

            return (data, totalData, filteredData);
        }


        //public async Task<(bool isSuccess, string message)> GetBookingItemTypesAsync(string id)
        //{
        //    try
        //    {
        //        var bookingData = boRepo.All().Where(x => x.BookinOrderNo == id).FirstOrDefault();

        //        if (bookingData == null)
        //            return (false, "Booking data not found.");

        //        try
        //        {
        //            if (bookingData.BookingType == null || !bookingData.BookingType.Any())
        //                return (false, CreateFailed);

        //            switch (bookingData.BookingType)
        //            {
        //                case "04":
        //                    await GetCartonBookingDataList(bookingData);
        //                    break;
        //                case "07":
        //                    await GetThreadBookingDataList(bookingData);
        //                    break;
        //                case "03":
        //                    await GetPolyBookingDataList(bookingData);
        //                    break;
        //                case "02":
        //                    await GetButtonBookingDataList(bookingData);
        //                    break;
        //                case "01":
        //                    await GetFebricBookingDataList(bookingData);
        //                    break;
        //                default:
        //                    await GetExtraBookingDataList(bookingData);
        //                    break;
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            // Ensure CreateFailed is defined, otherwise use a string
        //            return (false, $"Details Save Failed. Error: {ex.Message}");
        //        }
        //        return (true, "");
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}


        public async Task<(bool isSuccess, string message, object data)> GetBookingItemTypesAsync(string id)
        {
            try
            {
                var bookingData = boRepo.All().FirstOrDefault(x => x.BookinOrderNo == id);

                if (bookingData == null)
                    return (false, "Booking data not found.", null);

                try
                {
                    if (string.IsNullOrEmpty(bookingData.BookingType))
                        return (false, CreateFailed, null);

                    object data = null;

                    switch (bookingData.BookingType)
                    {
                        case "04":
                            data = await GetCartonBookingDataList(bookingData);
                            break;
                        case "07":
                            data = await GetThreadBookingDataList(bookingData);
                            break;
                        case "03":
                            data = await GetPolyBookingDataList(bookingData);
                            break;
                        case "02":
                            data = await GetButtonBookingDataList(bookingData);
                            break;
                        case "01":
                            data = await GetFebricBookingDataList(bookingData);
                            break;
                        default:
                            data = await GetExtraBookingDataList(bookingData);
                            break;
                    }

                    return (true, "Booking items copied to temp successfully.", data);
                }
                catch (Exception ex)
                {
                    return (false, $"Details Save Failed. Error: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        public async Task<List<object>> GetCartonBookingDataList(RmgBookingOrder bookingData)
        {
            try
            {
                // 1. Get main records
                var mainRecords = cartonRepo.All()
                    .Where(x => x.PoNo == bookingData.PoNo && x.IntegraJobNo == bookingData.IntegraJobNo)
                    .ToList();

                if (!mainRecords.Any()) return new List<object>();

                // 2. Clear temp table first
                var existingTemp = cartonTempRepo.All().ToList();
                if (existingTemp.Any())
                    await cartonTempRepo.DeleteRangeAsync(existingTemp);

                // 3. Copy main → temp
                var newCartonTempData = mainRecords.Select(mainItem => new RmgInvBookingReceivedDetailsCartonTemp
                {
                    PurchaseReceiveNo = mainItem.PurchaseReceiveNo,
                    ItemId = mainItem.ItemId,
                    ItemDescription = mainItem.ItemDescription,
                    OrderQty = mainItem.OrderQty,
                    OrderUnitId = mainItem.OrderUnitId,
                    RequiredQty = mainItem.RequiredQty,
                    RequiredQtyUnitId = mainItem.RequiredQtyUnitId,
                    ConsumptionUnitId = mainItem.ConsumptionUnitId,
                    Consumption = mainItem.Consumption,
                    UnitPrice = mainItem.UnitPrice,
                    TotalPrice = mainItem.TotalPrice,
                    PoNo = mainItem.PoNo,
                    IntegraJobNo = mainItem.IntegraJobNo,
                    Slno = mainItem.Slno,
                    ColorId = mainItem.ColorId,
                    SizeId = mainItem.SizeId,
                    Refcode = mainItem.Refcode,
                    CartonLeangth = mainItem.CartonLeangth,
                    LeangthUnitId = mainItem.LeangthUnitId,
                    CartonWidth = mainItem.CartonWidth,
                    WidthUnitId = mainItem.WidthUnitId,
                    CatonHeight = mainItem.CatonHeight,
                    HeightUnitId = mainItem.HeightUnitId,
                    CartonPercent = mainItem.CartonPercent.ToString() ?? "",
                    TotalReceivedQty = mainItem.TotalReceivedQty,
                    CurrentReceiveQty = mainItem.CurrentReceiveQty,
                    ReceivedUnitPrice = mainItem.ReceivedUnitPrice,
                    TotalReceivedQtyPre = mainItem.TotalReceivedQtyPre,
                    PendingReceiveQty = mainItem.PendingReceiveQty,
                    PendingReceiveQtyPre = mainItem.PendingReceiveQtyPre,
                    Brdid = mainItem.Brdid,
                    ReceivedUnitType = mainItem.ReceivedUnitType,
                    CurrencyId = mainItem.CurrencyId,
                    Remarks = mainItem.Remarks,
                    EmployeeId = mainItem.EmployeeId,
                }).ToList();

                await cartonTempRepo.AddRangeAsync(newCartonTempData);

                // 4. Return shaped projection for AJAX
                var result = cartonTempRepo.All()
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

                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying carton booking data to temp: {ex.Message}");
                throw;
            }
        }
        public async Task<List<RmgInvBookingReceivedDetailsThreadTemp>> GetThreadBookingDataList(RmgBookingOrder bookingData)
        {
            var newThreadTempData = new List<RmgInvBookingReceivedDetailsThreadTemp>();

            try
            {
                var mainRecords = threadRepo.All()
                    .Where(x => x.PoNo == bookingData.PoNo && x.IntegraJobNo == bookingData.IntegraJobNo)
                    .ToList();

                if (!mainRecords.Any()) return newThreadTempData;

                var existingTemp = threadTempRepo.All().ToList();
                if (existingTemp.Any())
                    await threadTempRepo.DeleteRangeAsync(existingTemp);

                foreach (var mainItem in mainRecords)
                {
                    var tempItem = new RmgInvBookingReceivedDetailsThreadTemp
                    {
                        PurchaseReceiveNo = mainItem.PurchaseReceiveNo,
                        PoNo = mainItem.PoNo,
                        IntegraJobNo = mainItem.IntegraJobNo,
                        Slno = mainItem.Slno,
                        Brdid = mainItem.Brdid,
                        ItemId = mainItem.ItemId,
                        ColorId = mainItem.ColorId,
                        FebricDetail = mainItem.FebricDetail,
                        ThreadColorId = mainItem.ThreadColorId,
                        ThreadCountId = mainItem.ThreadCountId,
                        Refcodepantone = mainItem.Refcodepantone,
                        ThreadReqUnit = mainItem.ThreadReqUnit,
                        Threadpercent = mainItem.Threadpercent,
                        OrderQty = mainItem.OrderQty,
                        QtyUnitId = mainItem.QtyUnitId,
                        Consumption = mainItem.Consumption,
                        ConsumtionUnitId = mainItem.ConsumtionUnitId,
                        TotalQty = mainItem.TotalQty,
                        TotalQtyUnitId = mainItem.TotalQtyUnitId,
                        ReqQty = mainItem.ReqQty,
                        UnitPrice = mainItem.UnitPrice,
                        TotalPrice = mainItem.TotalPrice,
                        CurrencyId = mainItem.CurrencyId,
                        TotalReceivedQty = mainItem.TotalReceivedQty,
                        CurrentReceiveQty = mainItem.CurrentReceiveQty,
                        ReceivedUnitType = mainItem.ReceivedUnitType,
                        ReceivedUnitPrice = mainItem.ReceivedUnitPrice,
                        TotalReceivedQtyPre = mainItem.TotalReceivedQtyPre,
                        PendingReceiveQty = mainItem.PendingReceiveQty,
                        PendingReceiveQtyPre = mainItem.PendingReceiveQtyPre,
                        Remarks = mainItem.Remarks,
                        EmployeeId = mainItem.EmployeeId,
                    };
                    newThreadTempData.Add(tempItem);
                }

                await threadTempRepo.AddRangeAsync(newThreadTempData);
                return newThreadTempData;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying thread booking data to temp: {ex.Message}");
                throw;
            }
        }
        public async Task<List<RmgInvBookingReceivedDetailsPolyTemp>> GetPolyBookingDataList(RmgBookingOrder bookingData)
        {
            var newPolyTempData = new List<RmgInvBookingReceivedDetailsPolyTemp>();

            try
            {
                var mainRecords = polyRepo.All()
                    .Where(x => x.PoNo == bookingData.PoNo && x.IntegraJobNo == bookingData.IntegraJobNo)
                    .ToList();

                if (!mainRecords.Any()) return newPolyTempData;

                var existingTemp = polyTempRepo.All().ToList();
                if (existingTemp.Any())
                    await polyTempRepo.DeleteRangeAsync(existingTemp);

                foreach (var mainItem in mainRecords)
                {
                    var tempItem = new RmgInvBookingReceivedDetailsPolyTemp
                    {
                        PurchaseReceiveNo = mainItem.PurchaseReceiveNo,
                        PoNo = mainItem.PoNo,
                        IntegraJobNo = mainItem.IntegraJobNo,
                        SerialNo = mainItem.SerialNo,
                        Brdid = mainItem.Brdid,
                        ItemId = mainItem.ItemId,
                        ItemDescription = mainItem.ItemDescription,
                        ColorId = mainItem.ColorId,
                        RefernceCode = mainItem.RefernceCode,
                        Length = mainItem.Length,
                        LengthUnitId = mainItem.LengthUnitId,
                        Width = mainItem.Width,
                        WidthUnitId = mainItem.WidthUnitId,
                        Flap = mainItem.Flap,
                        FlapUnitId = mainItem.FlapUnitId,
                        Guest = mainItem.Guest,
                        GuestUnitId = mainItem.GuestUnitId,
                        GarmentQty = mainItem.GarmentQty,
                        GarmentQtyUnitId = mainItem.GarmentQtyUnitId,
                        Consumption = mainItem.Consumption,
                        ConsumptionUnitId = mainItem.ConsumptionUnitId,
                        TotalQty = mainItem.TotalQty,
                        TotalQtyUnitId = mainItem.TotalQtyUnitId,
                        Percentage = mainItem.Percentage,
                        TotalReceivedQty = mainItem.TotalReceivedQty,
                        CurrentReceiveQty = mainItem.CurrentReceiveQty,
                        ReceivedUnitType = mainItem.ReceivedUnitType,
                        UnitPrice = mainItem.UnitPrice,
                        ReceivedUnitPrice = mainItem.ReceivedUnitPrice,
                        TotalPrice = mainItem.TotalPrice,
                        CurrencyId = mainItem.CurrencyId,
                        TotalReceivedQtyPre = mainItem.TotalReceivedQtyPre,
                        PendingReceiveQty = mainItem.PendingReceiveQty,
                        PendingReceiveQtyPre = mainItem.PendingReceiveQtyPre,
                        Remarks = mainItem.Remarks,
                        EmployeeId = mainItem.EmployeeId,
                    };
                    newPolyTempData.Add(tempItem);
                }

                await polyTempRepo.AddRangeAsync(newPolyTempData);
                return newPolyTempData;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying poly booking data to temp: {ex.Message}");
                throw;
            }
        }
        public async Task<List<RmgInvBookingReceivedDetailsButtonTemp>> GetButtonBookingDataList(RmgBookingOrder bookingData)
        {
            var newButtonTempData = new List<RmgInvBookingReceivedDetailsButtonTemp>();

            try
            {
                var mainRecords = buttonRepo.All()
                    .Where(x => x.PoNo == bookingData.PoNo && x.IntegraJobNo == bookingData.IntegraJobNo)
                    .ToList();

                if (!mainRecords.Any()) return newButtonTempData;

                var existingTemp = buttonTempRepo.All().ToList();
                if (existingTemp.Any())
                    await buttonTempRepo.DeleteRangeAsync(existingTemp);

                foreach (var mainItem in mainRecords)
                {
                    var tempItem = new RmgInvBookingReceivedDetailsButtonTemp
                    {
                        PurchaseReceiveNo = mainItem.PurchaseReceiveNo,
                        PoNo = mainItem.PoNo,
                        IntegraJobNo = mainItem.IntegraJobNo,
                        SerialNo = mainItem.SerialNo,
                        Brdid = mainItem.Brdid,
                        ItemId = mainItem.ItemId,
                        Description = mainItem.Description,
                        FabricColorId = mainItem.FabricColorId,
                        ColorId = mainItem.ColorId,
                        SizeId = mainItem.SizeId,
                        Idno = mainItem.Idno,
                        GermentQty = mainItem.GermentQty,
                        GermentsQtyUnitId = mainItem.GermentsQtyUnitId,
                        Consumption = mainItem.Consumption,
                        ConsumptionUnitId = mainItem.ConsumptionUnitId,
                        TotalQty = mainItem.TotalQty,
                        TotalQtyUnitId = mainItem.TotalQtyUnitId,
                        OrderQty = mainItem.OrderQty,
                        OrderQtyUnitId = mainItem.OrderQtyUnitId,
                        Percentage = mainItem.Percentage,
                        TotalReceivedQty = mainItem.TotalReceivedQty,
                        CurrentReceiveQty = mainItem.CurrentReceiveQty,
                        ReceivedUnitType = mainItem.ReceivedUnitType,
                        UnitPrice = mainItem.UnitPrice,
                        ReceivedUnitPrice = mainItem.ReceivedUnitPrice,
                        TotalPrice = mainItem.TotalPrice,
                        CurrencyId = mainItem.CurrencyId,
                        TotalReceivedQtyPre = mainItem.TotalReceivedQtyPre,
                        PendingReceiveQty = mainItem.PendingReceiveQty,
                        PendingReceiveQtyPre = mainItem.PendingReceiveQtyPre,
                        Remarks = mainItem.Remarks,
                        EmployeeId = mainItem.EmployeeId,
                    };
                    newButtonTempData.Add(tempItem);
                }

                await buttonTempRepo.AddRangeAsync(newButtonTempData);
                return newButtonTempData;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying button booking data to temp: {ex.Message}");
                throw;
            }
        }
        public async Task<List<RmgInvBookingReceivedDetailsFebricTemp>> GetFebricBookingDataList(RmgBookingOrder bookingData)
        {
            var newFebricTempData = new List<RmgInvBookingReceivedDetailsFebricTemp>();

            try
            {
                var mainRecords = febricRepo.All()
                    .Where(x => x.PoNo == bookingData.PoNo && x.IntegraJobNo == bookingData.IntegraJobNo)
                    .ToList();

                if (!mainRecords.Any()) return newFebricTempData;

                var existingTemp = febricTempRepo.All().ToList();
                if (existingTemp.Any())
                    await febricTempRepo.DeleteRangeAsync(existingTemp);

                foreach (var mainItem in mainRecords)
                {
                    var tempItem = new RmgInvBookingReceivedDetailsFebricTemp
                    {
                        PurchaseReceiveNo = mainItem.PurchaseReceiveNo,
                        PoNo = mainItem.PoNo,
                        IntegraJobNo = mainItem.IntegraJobNo,
                        Slno = mainItem.Slno,
                        Brdid = mainItem.Brdid,
                        ColorId = mainItem.ColorId,
                        FabricItemId = mainItem.FabricItemId,
                        ItemId = mainItem.ItemId,
                        FebricDetails = mainItem.FebricDetails,
                        Refcode = mainItem.Refcode,
                        OrderQty = mainItem.OrderQty,
                        QtyUnit = mainItem.QtyUnit,
                        Consumption = mainItem.Consumption,
                        ConsumtionUnit = mainItem.ConsumtionUnit,
                        TotalFebricQty = mainItem.TotalFebricQty,
                        Percentage = mainItem.Percentage,
                        TotalReceivedQty = mainItem.TotalReceivedQty,
                        CurrentReceiveQty = mainItem.CurrentReceiveQty,
                        ReceivedUnitType = mainItem.ReceivedUnitType,
                        UnitPrice = mainItem.UnitPrice,
                        ReceivedUnitPrice = mainItem.ReceivedUnitPrice,
                        TotalPrice = mainItem.TotalPrice,
                        CurrencyId = mainItem.CurrencyId,
                        TotalReceivedQtyPre = mainItem.TotalReceivedQtyPre,
                        PendingReceiveQty = mainItem.PendingReceiveQty,
                        PendingReceiveQtyPre = mainItem.PendingReceiveQtyPre,
                        EmployeeId = mainItem.EmployeeId,
                    };
                    newFebricTempData.Add(tempItem);
                }

                await febricTempRepo.AddRangeAsync(newFebricTempData);
                return newFebricTempData;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying fabric booking data to temp: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RmgInvBookingReceivedDetailsExtraTemp>> GetExtraBookingDataList(RmgBookingOrder bookingData)
        {
            var newExtraTempData = new List<RmgInvBookingReceivedDetailsExtraTemp>();

            try
            {
                var mainRecords = extraRepo.All()
                    .Where(x => x.PoNo == bookingData.PoNo && x.IntegraJobNo == bookingData.IntegraJobNo)
                    .ToList();

                if (!mainRecords.Any()) return newExtraTempData;

                var existingTemp = extraTempRepo.All().ToList();
                if (existingTemp.Any())
                    await extraTempRepo.DeleteRangeAsync(existingTemp);

                foreach (var mainItem in mainRecords)
                {
                    var tempItem = new RmgInvBookingReceivedDetailsExtraTemp
                    {
                        PurchaseReceiveNo = mainItem.PurchaseReceiveNo,
                        PoNo = mainItem.PoNo,
                        IntegraJobNo = mainItem.IntegraJobNo,
                        Slno = mainItem.Slno,
                        Brdid = mainItem.Brdid,
                        FabricColorId = mainItem.FabricColorId,
                        ItemId = mainItem.ItemId,
                        Description = mainItem.Description,
                        ColorId = mainItem.ColorId,
                        OrderQty = mainItem.OrderQty,
                        OrderQtyIunitD = mainItem.OrderQtyIunitD,
                        Consumption = mainItem.Consumption,
                        ConsumptionUnitId = mainItem.ConsumptionUnitId,
                        TotalQty = mainItem.TotalQty,
                        TotalQtyUnitId = mainItem.TotalQtyUnitId,
                        ReqQty = mainItem.ReqQty,
                        ReqQtyUnitId = mainItem.ReqQtyUnitId,
                        Percentage = mainItem.Percentage,
                        TotalReceivedQty = mainItem.TotalReceivedQty,
                        CurrentReceiveQty = mainItem.CurrentReceiveQty,
                        ReceivedUnitType = mainItem.ReceivedUnitType,
                        UnitPrice = mainItem.UnitPrice,
                        ReceivedUnitPrice = mainItem.ReceivedUnitPrice,
                        TotalPrice = mainItem.TotalPrice,
                        CurrencyId = mainItem.CurrencyId,
                        TotalReceivedQtyPre = mainItem.TotalReceivedQtyPre,
                        PendingReceiveQty = mainItem.PendingReceiveQty,
                        PendingReceiveQtyPre = mainItem.PendingReceiveQtyPre,
                        Remarks = mainItem.Remarks,
                        EmployeeId = mainItem.EmployeeId,
                    };
                    newExtraTempData.Add(tempItem);
                }

                await extraTempRepo.AddRangeAsync(newExtraTempData);
                return newExtraTempData;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying extra booking data to temp: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool success, string message)> DeleteBookingOrderAsync(List<decimal> deleteBookingIds)
        {
            try
            {
                var orders = boRepo.All().Where(o => deleteBookingIds.Contains(o.Tc)).ToList();

                if (!orders.Any())
                    return (false, DeleteFailed);

                await boRepo.DeleteRangeAsync(orders);

                return (true, DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (false, $"Error occurred: {ex.Message}");
            }
        }

    }
}
