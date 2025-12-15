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


        public async Task<(bool isSuccess, string message)> SaveBookingAsync(RMGBookingOrderEntryBuklDto dto)
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

                    // Create object mapping
                    var entity = new RmgBookingOrder
                    {

                        Tc = dto.Tc,
                        BookinOrderNo = dto.BookinOrderNo ?? "",
                        BookinDate = dto.BookinDate,
                        BuyerId = dto.BuyerId ?? "",
                        StyleId = dto.StyleId ?? "",
                        MasterPurchaseOrder = dto.MasterPurchaseOrder ?? "",
                        PoNo = dto.PoNo ?? "",
                        IntegraJobNo = dto.IntegraJobNo ?? "",
                        PurchasedOfficer = dto.PurchasedOfficer ?? "",
                        Remarks = dto.Remarks ?? "",
                        EmployeId = dto.EmployeId ?? "",
                        CompanyId = dto.CompanyId ?? "",
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
                        Ldate = DateTime.Now
                    };

                    await boRepo.AddAsync(entity);

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
                        Tc = dto.Tc,
                        BookinOrderNo = dto.BookinOrderNo ?? "",
                        BookinDate = dto.BookinDate,
                        BuyerId = dto.BuyerId ?? "",
                        StyleId = dto.StyleId ?? "",
                        MasterPurchaseOrder = dto.MasterPurchaseOrder ?? "",
                        PoNo = dto.PoNo ?? "",
                        IntegraJobNo = dto.IntegraJobNo ?? "",
                        PurchasedOfficer = dto.PurchasedOfficer ?? "",
                        Remarks = dto.Remarks ?? "",
                        EmployeId = dto.EmployeId ?? "",
                        CompanyId = dto.CompanyId ?? "",
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
                var CartonBookingTempData = cartonTempRepo.All().ToList();

                if (CartonBookingTempData == null || CartonBookingTempData.Count == 0)
                {
                    return newCartonBookings;
                }

                var maxSlno = cartonRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in CartonBookingTempData)
                {
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
                        // Assuming CartonPercent needs to be stored as a string or int in the main table
                        // I am correcting the mapping to string if the property type allows, or safe conversion
                        //CartonPercent = tempItem.CartonPercent, // Reverting to string/original type
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

                await cartonRepo.AddRangeAsync(newCartonBookings);
                await cartonTempRepo.DeleteRangeAsync(CartonBookingTempData);

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
            // ... (This function is already complete and correct as per the last response)
            try
            {
                var ThreadBookingTempData = threadTempRepo.All().ToList();

                if (ThreadBookingTempData == null || ThreadBookingTempData.Count == 0)
                {
                    return newThreadBookings;
                }

                var maxSlno = threadRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in ThreadBookingTempData)
                {
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

                await threadRepo.AddRangeAsync(newThreadBookings);
                await threadTempRepo.DeleteRangeAsync(ThreadBookingTempData);

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
                var PolyBookingTempData = polyTempRepo.All().ToList();

                if (PolyBookingTempData == null || PolyBookingTempData.Count == 0)
                {
                    return newPolyBookings;
                }

                // SerialNo is used here
                var maxSlno = polyRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in PolyBookingTempData)
                {
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

                await polyRepo.AddRangeAsync(newPolyBookings);
                await polyTempRepo.DeleteRangeAsync(PolyBookingTempData);

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
                var FebricBookingTempData = febricTempRepo.All().ToList();

                if (FebricBookingTempData == null || FebricBookingTempData.Count == 0)
                {
                    return newFebricBookings;
                }

                // Slno is used here
                var maxSlno = febricRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in FebricBookingTempData)
                {
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

                await febricRepo.AddRangeAsync(newFebricBookings);
                await febricTempRepo.DeleteRangeAsync(FebricBookingTempData);

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
                var ExtraBookingTempData = extraTempRepo.All().ToList();

                if (ExtraBookingTempData == null || ExtraBookingTempData.Count == 0)
                {
                    return newExtraBookings;
                }

                // Slno is used here
                var maxSlno = extraRepo.All().Count();
                var sNo = maxSlno + 1;

                foreach (var tempItem in ExtraBookingTempData)
                {
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

                await extraRepo.AddRangeAsync(newExtraBookings);
                await extraTempRepo.DeleteRangeAsync(ExtraBookingTempData);

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
    }
}
