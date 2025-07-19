using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Brand;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Core.ViewModels.ItemMasterInformation;
using GCTL.Core.ViewModels.ItemModel;
using GCTL.Core.ViewModels.PrintingStationeryPurchaseEntry;
using GCTL.Core.ViewModels.SalesSupplier;
using GCTL.Data.Models;
using iText.Commons.Bouncycastle.Cert.Ocsp;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace GCTL.Service.PrintingStationeryPurchaseEntry
{
    public class PrintingStationeryPurchaseEntryService:AppService<RmgPurchaseOrderReceive> , IPrintingStationeryPurchaseEntryService
    {
        private readonly IRepository<RmgPurchaseOrderReceive> PurchaseOrderReceive;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<SalesSupplier> salesSupRepo;
        private readonly IRepository<HrmItemMasterInformation> productRepo;
        private readonly IRepository<HrmModel> modelRepo;
        private readonly IRepository<HrmBrand> brandRepo;
        private readonly IRepository<RmgPurchaseOrderReceiveDetails> purchaseOrderReceiveDetailsRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<CoreCompany> comRepo;
        private readonly IRepository<HrmSize> sizeRepo;
        private readonly IRepository<HmsLtrvPeriod> periodRepo;
        private readonly IRepository<RmgProdDefUnitType> unitRepo;

        public PrintingStationeryPurchaseEntryService(
            IRepository<RmgPurchaseOrderReceive> PurchaseOrderReceive,
            IRepository<CoreAccessCode> accessCodeRepository,
            IRepository<SalesSupplier> salesSupRepo,
            IRepository<HrmItemMasterInformation> productRepo,
            IRepository<HrmModel> modelRepo,
            IRepository<HrmBrand> brandRepo,
            IRepository<RmgPurchaseOrderReceiveDetails> PurchaseOrderReceiveDetailsRepo,
            IRepository<HrmDefDepartment> depRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<CoreCompany> comRepo,
            IRepository<HrmSize> sizeRepo,
            IRepository<HmsLtrvPeriod> periodRepo,
            IRepository<RmgProdDefUnitType> unitRepo
            ) : base(PurchaseOrderReceive)
        {
            this.PurchaseOrderReceive = PurchaseOrderReceive;
            this.accessCodeRepository = accessCodeRepository;
            this.salesSupRepo = salesSupRepo;
            this.productRepo = productRepo;
            this.modelRepo = modelRepo;
            this.brandRepo = brandRepo;
           this.purchaseOrderReceiveDetailsRepo = PurchaseOrderReceiveDetailsRepo;
            this.depRepo = depRepo;
            this.empRepo = empRepo;
            this.comRepo = comRepo;
            this.sizeRepo = sizeRepo;
            this.periodRepo = periodRepo;
            this.unitRepo = unitRepo;
        }

        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";

        #region Permission all type

        public async Task<bool> PagePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.CheckDelete);

        }

        #endregion


        public async Task<List<PrintingStationeryPurchaseEntrySetupViewModel>> GetAllAsync()
        {
            try
            {
                return PurchaseOrderReceive.All().Select(c => new PrintingStationeryPurchaseEntrySetupViewModel
                {
                    TC = c.Tc,
                    MainCompanyCode = c.MainCompanyCode,
                    PurchaseReceiveNo = c.PurchaseReceiveNo,
                    ReceiveDate = c.ReceiveDate,
                    ShowReceiveDate= c.ReceiveDate.HasValue? c.ReceiveDate.Value.ToString("dd/MM/yyyy  hh:mm tt"):"",
                    DepartmentCode = c.DepartmentCode,
                    DepartmentName = depRepo.All().Where(x=>x.DepartmentCode== c.DepartmentCode).Select(x=>x.DepartmentName).FirstOrDefault(),
                    SupplierID = c.SupplierId,
                    SupplierName= salesSupRepo.All().Where(x=>x.SupplierId== c.SupplierId).Select(x=> x.SupplierName).FirstOrDefault(),
                    InvoiceNo = c.InvoiceNo,
                    InvoiceDate = c.InvoiceDate,
                    InvoiceValue = c.InvoiceValue,
                    ChallanNo = c.ChallanNo,
                    ChallanDate = c.ChallanDate,
                    EmployeeID_ReceiveBy = empRepo.All().Where(x=>x.EmployeeId == c.EmployeeIdReceiveBy).Select(x=> x.FirstName+" "+x.LastName).FirstOrDefault(),
                    Remarks = c.Remarks,
                    //TotalAmount = c.TotalAmount,
                    Luser = c.Luser,
                    Ldate = c.Ldate,
                    Lip = c.Lip,
                    Lmac = c.Lmac,
                    ModifyDate = c.ModifyDate,
                    UserInfoEmployeeId = c.UserInfoEmployeeId,
                    CompanyCode = comRepo.All().Where(x=>x.CompanyCode== c.CompanyCode).Select(x=>x.CompanyName).FirstOrDefault(),

                    // 🔽 Details DTO mapping
                    TotalAmount = purchaseOrderReceiveDetailsRepo.All().Where(x => x.PurchaseReceiveNo == c.PurchaseReceiveNo).Select(x =>x.TotalPrice).FirstOrDefault()
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<PrintingStationeryPurchaseEntrySetupViewModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = PurchaseOrderReceive.All().FirstOrDefault(x => x.PurchaseReceiveNo == id);
                if (entity == null) return null;

                // Caching all lookup data to avoid repeated DB calls
                var allProducts = productRepo.All().ToList();
                var allBrands = brandRepo.All().ToList();
                var allModels = modelRepo.All().ToList();
                var allSizes = sizeRepo.All().ToList();
                var allPeriods = periodRepo.All().ToList();
                var allUnits = unitRepo.All().ToList();

                var detailsList = purchaseOrderReceiveDetailsRepo.All()
                    .Where(x => x.PurchaseReceiveNo == entity.PurchaseReceiveNo)
                    .ToList() // Important: materialize query first, then project
                    .Select(x =>
                    {
                        var product = allProducts.FirstOrDefault(p => p.ProductCode == x.ProductCode);
                        var brand = allBrands.FirstOrDefault(b => b.BrandId == x.BrandId);
                        var model = allModels.FirstOrDefault(m => m.ModelId == x.ModelId);
                        var size = allSizes.FirstOrDefault(s => s.SizeId == x.SizeId);
                        var period = allPeriods.FirstOrDefault(p => p.PeriodId == x.WarrentyTypeId);
                        var unit = allUnits.FirstOrDefault(u => u.UnitTypId == x.UnitTypId);

                        return new PurchaseOrderReceiveDetailsDTO
                        {
                            TC = x.Tc,
                            PurchaseOrderReceiveDetailsID = x.PurchaseOrderReceiveDetailsId ?? "",
                            PurchaseReceiveNo = x.PurchaseReceiveNo ?? "",
                            ProductCode = x.ProductCode ?? "",
                            ProductName = product?.ProductName ?? "",
                            Description = x.Description ?? "",
                            BrandID = x.BrandId ?? "",
                            BrandName = brand?.BrandName ?? "",
                            ModelID = x.ModelId ?? "",
                            ModelName = model?.ModelName ?? "",
                            SizeID = x.SizeId ?? "",
                            SizeName = size?.SizeName ?? "",
                            WarrantyPeriod = x.WarrantyPeriod ?? "",
                            WarrantyPeriodName = period?.PeriodName ?? "", // ✅ fixed
                            WarrentyTypeID = x.WarrentyTypeId ?? "",
                            ReqQty = x.ReqQty,
                            UnitTypID = x.UnitTypId ?? "",
                            UnitTypName = unit?.UnitTypeName ?? "",
                            UnitPrice = x.UnitPrice,
                            TotalPrice = x.TotalPrice,
                            SLNO = x.Slno
                        };
                    }).ToList();


                return new PrintingStationeryPurchaseEntrySetupViewModel
                {
                    TC = entity.Tc,
                    MainCompanyCode = entity.MainCompanyCode,
                    PurchaseReceiveNo = entity.PurchaseReceiveNo,
                    ReceiveDate = entity.ReceiveDate,
                    DepartmentCode = entity.DepartmentCode,
                    SupplierID = entity.SupplierId,
                    InvoiceNo = entity.InvoiceNo,
                    InvoiceDate = entity.InvoiceDate,
                    InvoiceValue = entity.InvoiceValue,
                    ChallanNo = entity.ChallanNo,
                    ChallanDate = entity.ChallanDate,
                    EmployeeID_ReceiveBy = entity.EmployeeIdReceiveBy,
                    Remarks = entity.Remarks,
                    TotalAmount = entity.TotalAmount,
                    Luser = entity.Luser,
                    Ldate = entity.Ldate,
                    Lip = entity.Lip,
                    Lmac = entity.Lmac,
                    ModifyDate = entity.ModifyDate,
                    UserInfoEmployeeId = entity.UserInfoEmployeeId,
                    CompanyCode = entity.CompanyCode,
                    ShowCreateDate = entity.Ldate?.ToString("dd/MM/yyyy") ?? "",
                    ShowModifyDate = entity.ModifyDate?.ToString("dd/MM/yyyy") ?? "",
                    purchaseOrderReceiveDetailsDTOs = detailsList
                };
            }
            catch (Exception ex)
            {
                // Optional: log ex here
                throw;
            }
        }



        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(PrintingStationeryPurchaseEntrySetupViewModel model, string companyCode)
        {
            try
            {
                if (model.TC == 0) // Create
                {
                    if (model.purchaseOrderReceiveDetailsDTOs == null || model.purchaseOrderReceiveDetailsDTOs.Count == 0)
                    {
                        return (false, CreateFailed, null);
                    }

                    // Main Entity Create
                    var entity = new RmgPurchaseOrderReceive
                    {
                        MainCompanyCode = model.MainCompanyCode,
                        PurchaseReceiveNo = model.PurchaseReceiveNo,
                        ReceiveDate = model.ReceiveDate,
                        DepartmentCode = model.DepartmentCode,
                        SupplierId = model.SupplierID,
                        InvoiceNo = model.InvoiceNo,
                        InvoiceDate = model.InvoiceDate,
                        InvoiceValue = model.InvoiceValue,
                        ChallanNo = model.ChallanNo,
                        ChallanDate = model.ChallanDate,
                        EmployeeIdReceiveBy = model.EmployeeID_ReceiveBy,
                        Remarks = model.Remarks,
                        TotalAmount = model.TotalAmount,
                        Luser = model.Luser,
                        Ldate = DateTime.Now,
                        Lip = model.Lip,
                        Lmac = model.Lmac,
                        UserInfoEmployeeId = model.UserInfoEmployeeId,
                        CompanyCode = companyCode
                    };

                    await PurchaseOrderReceive.AddAsync(entity); // Save main entity

                    // Get last inserted PurchaseOrderReceiveDetailsId
                    var lastDetail = purchaseOrderReceiveDetailsRepo.All()
                                            .OrderByDescending(x => x.PurchaseOrderReceiveDetailsId)
                                            .FirstOrDefault();

                    int nextId = 1;
                    if (lastDetail != null && int.TryParse(lastDetail.PurchaseOrderReceiveDetailsId, out int lastId))
                    {
                        nextId = lastId + 1;
                    }

                    // Map details list
                    List<RmgPurchaseOrderReceiveDetails> detailsList = new List<RmgPurchaseOrderReceiveDetails>();

                    foreach (var item in model.purchaseOrderReceiveDetailsDTOs)
                    {
                        var detail = new RmgPurchaseOrderReceiveDetails
                        {
                            PurchaseOrderReceiveDetailsId = nextId.ToString("D3"),
                            PurchaseReceiveNo = entity.PurchaseReceiveNo??"",
                            ProductCode = item.ProductCode ?? "",
                            Description = item.Description ?? "",
                            BrandId = item.BrandID ?? "",
                            ModelId = item.ModelID ?? "",
                            SizeId = item.SizeID ?? "",
                            WarrantyPeriod = item.WarrantyPeriod ?? "",
                            WarrentyTypeId = item.WarrentyTypeID ?? "",
                            ReqQty = item.ReqQty ?? 0,
                            UnitTypId = item.UnitTypID ?? "",
                            UnitPrice = item.UnitPrice  ??0,
                            TotalPrice = item.TotalPrice ?? 0,
                            Slno = item.SLNO ?? 0,
                            Luser = model.Luser ?? "",                           
                        };

                        detailsList.Add(detail);
                        nextId++;
                    }

                    // Bulk Insert
                    await purchaseOrderReceiveDetailsRepo.AddRangeAsync(detailsList);

                    return (true, CreateSuccess, entity);
                }
                else // Update
                {
                    var exData = await PurchaseOrderReceive.GetByIdAsync(model.TC);
                    if (exData != null)
                    {
                        exData.MainCompanyCode = model.MainCompanyCode;
                        exData.PurchaseReceiveNo = model.PurchaseReceiveNo;
                        exData.ReceiveDate = model.ReceiveDate;
                        exData.DepartmentCode = model.DepartmentCode;
                        exData.SupplierId = model.SupplierID;
                        exData.InvoiceNo = model.InvoiceNo;
                        exData.InvoiceDate = model.InvoiceDate;
                        exData.InvoiceValue = model.InvoiceValue;
                        exData.ChallanNo = model.ChallanNo;
                        exData.ChallanDate = model.ChallanDate;
                        exData.EmployeeIdReceiveBy = model.EmployeeID_ReceiveBy;
                        exData.Remarks = model.Remarks;
                        exData.TotalAmount = model.TotalAmount;
                        exData.Luser = model.Luser;
                        exData.ModifyDate = DateTime.Now;
                        exData.Lip = model.Lip;
                        exData.Lmac = model.Lmac;
                        exData.UserInfoEmployeeId = model.UserInfoEmployeeId;
                        exData.CompanyCode = companyCode;

                        await PurchaseOrderReceive.UpdateAsync(exData);

                        var detailstList = purchaseOrderReceiveDetailsRepo.All().Where(x => x.PurchaseReceiveNo == exData.PurchaseReceiveNo).ToList();
                        await purchaseOrderReceiveDetailsRepo.DeleteRangeAsync(detailstList);
                        //if (item != null || item.TC != null)
                        //{
                        //    var details = await purchaseOrderReceiveDetailsRepo.GetByIdAsync(item.TC);
                        //    if (details == null)
                        //    {
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        await purchaseOrderReceiveDetailsRepo.DeleteAsync(details);
                        //    }
                        //}




                        // Get last inserted PurchaseOrderReceiveDetailsId
                        var lastDetail = purchaseOrderReceiveDetailsRepo.All()
                                                .OrderByDescending(x => x.PurchaseOrderReceiveDetailsId)
                                                .FirstOrDefault();

                        int nextId = 1;
                        if (lastDetail != null && int.TryParse(lastDetail.PurchaseOrderReceiveDetailsId, out int lastId))
                        {
                            nextId = lastId + 1;
                        }

                        // Map details list
                        List<RmgPurchaseOrderReceiveDetails> detailsList = new List<RmgPurchaseOrderReceiveDetails>();

                        foreach (var item in model.purchaseOrderReceiveDetailsDTOs)
                        {
                            
                            var detail = new RmgPurchaseOrderReceiveDetails
                            {
                                PurchaseOrderReceiveDetailsId = nextId.ToString("D3"),
                                PurchaseReceiveNo = model.PurchaseReceiveNo ?? "",
                                ProductCode = item.ProductCode ?? "",
                                Description = item.Description ?? "",
                                BrandId = item.BrandID ?? "",
                                ModelId = item.ModelID ?? "",
                                SizeId = item.SizeID ?? "",
                                WarrantyPeriod = item.WarrantyPeriod ?? "",
                                WarrentyTypeId = item.WarrentyTypeID ?? "",
                                ReqQty = item.ReqQty ?? 0,
                                UnitTypId = item.UnitTypID ?? "",
                                UnitPrice = item.UnitPrice ?? 0,
                                TotalPrice = item.TotalPrice ?? 0,
                                Slno = item.SLNO ?? 0,
                                Luser = model.Luser ?? "",
                            };

                            detailsList.Add(detail);
                            nextId++;
                        }

                        // Bulk Insert
                        await purchaseOrderReceiveDetailsRepo.AddRangeAsync(detailsList);
                        // 🔁 Optional: Update এর সময় পুরানো details delete করে নতুন গুলো insert করতে চাইলে এই অংশ যোগ করুন:
                        //await purchaseOrderReceiveDetailsRepo.DeleteAsync(exData.PurchaseReceiveNo);

                        return (true, UpdateSuccess, exData);
                    }

                    return (false, UpdateFailed, null);
                }
            }
            catch (Exception ex)
            {
                return (false, CreateFailed, null);
            }
        }



        public async Task<(bool isSuccess, string message, object data)> DeleteAsync(List<string> ids)
        {
            foreach (var id in ids)
            {

                try
                {
                    var entity = await PurchaseOrderReceive.GetByIdAsync(decimal.Parse(id));
                    if (entity == null)
                    {
                        continue;
                    }

                    await PurchaseOrderReceive.DeleteAsync(entity);
                }
                catch (Exception)
                {
                    return (true, DeleteFailed, null);
                }
            }

            return (true, DeleteSuccess, null);
        }

        public async Task<string> AutoPrintingStationeryPurchaseIdAsync()
        {
            var currentYearShort = DateTime.Now.Year.ToString().Substring(2); // "22"
            var prefix = $"PO_{currentYearShort}_";

            var PrintingStationeryPurchaseList = (await PurchaseOrderReceive.GetAllAsync()).ToList();

            int newIdNumber = 1;

            if (PrintingStationeryPurchaseList != null && PrintingStationeryPurchaseList.Count > 0)
            {
                var lastId = PrintingStationeryPurchaseList
                    .Select(x => x.PurchaseReceiveNo)
                    .Where(id => id != null && id.StartsWith(prefix))
                    .OrderByDescending(id => id)
                    .FirstOrDefault();


                if (!string.IsNullOrEmpty(lastId))
                {
                    var numericPart = lastId.Substring(prefix.Length);
                    if (int.TryParse(numericPart, out int lastNumber))
                    {
                        newIdNumber = lastNumber + 1;
                    }
                }
            }

            return $"{prefix}{newIdNumber.ToString("D6")}";
        }

        public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string catagoryValue)
        {
            bool Exists = PurchaseOrderReceive.All().Any(x => x.DepartmentCode == catagoryValue);
            return (Exists, DataExists, null);
        }

        public async Task<SalesSupplierSetupViewModel> getSupplierByIdAsync(string supplierId)
        {
            try
            {
                var suplier = salesSupRepo.All().Where(x => x.SupplierId == supplierId).FirstOrDefault();
                if (suplier == null) return null;
                var supplierData = new SalesSupplierSetupViewModel
                {
                    SupplierID = suplier.SupplierId,
                    SupplierAddress = suplier.SupplierAddress
                };
                return supplierData;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<ItemMasterInformationSetupViewModel> productSelectIdDetailsAsync(string ProductCode)
        {
            try
            {
                var product = productRepo.All().Where(x => x.ProductCode == ProductCode).FirstOrDefault();
                if (product == null) return null;
                var supplierData = new ItemMasterInformationSetupViewModel
                {
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    PurchaseCost = product.PurchaseCost,
                    UnitID = product.UnitId,
                    BrandList = brandRepo.All().Where(x => x.BrandId == product.BrandId).Select(x => new BrandSetupViewModel
                    {
                        BrandID = x.BrandId,
                        BrandName = x.BrandName
                    }).ToList()
                };
                return supplierData;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ItemModelSetupViewModel>> brandIdAsync(string brandId)
        {
            var listModel = await Task.Run(() =>
                modelRepo.All()
                .Where(x => x.BrandId == brandId)
                .Select(x => new ItemModelSetupViewModel
                {
                    AutoId = x.AutoId,
                    ModelID = x.ModelId,
                    ModelName = x.ModelName
                })
                .ToList()
            );

            return listModel;
        }
    }
}
