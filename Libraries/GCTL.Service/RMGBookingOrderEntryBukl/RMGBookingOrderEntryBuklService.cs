using GCTL.Core.Data;
using GCTL.Core.ViewModels.RMGBookingOrderEntryBukl;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.RMGBookingOrderEntryBukl
{
    public class RMGBookingOrderEntryBuklService : AppService<RmgBookingOrder>, IRMGBookingOrderEntryBuklService
    {
        private readonly IRepository<RmgBookingOrder> boRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;

        public RMGBookingOrderEntryBuklService(
            IRepository<RmgBookingOrder> boRepo,
             IRepository<CoreAccessCode> accessCodeRepository
            ) : base(boRepo)
        {
            this.boRepo = boRepo;
            this.accessCodeRepository = accessCodeRepository;
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
                // basic validation
                if (dto == null || string.IsNullOrEmpty(dto.BookinOrderNo))
                {
                    return (false, "Booking Order No cannot be empty!");
                }

                // *** CREATE ***
                if (dto.Tc == 0)
                {
                    // check duplicate BookingOrderNo
                    bool exists = await IsExistByCodeAsync(dto.BookinOrderNo);
                    if (exists)
                        return (false, "This Booking Order No already exists!");

                    // Create object mapping
                    var entity = new RmgBookingOrder
                    {
                        Tc = 0,
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


                    return (true, CreateSuccess);


                }

                // *** UPDATE ***
                else
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


                    return (true, UpdateSuccess);

                }
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
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
