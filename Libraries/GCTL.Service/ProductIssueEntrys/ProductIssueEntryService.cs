using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Brand;
using GCTL.Core.ViewModels.ItemMasterInformation;
using GCTL.Core.ViewModels.ItemModel;
using GCTL.Core.ViewModels.ProductIssueEntry;
using GCTL.Core.ViewModels.SalesSupplier;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.ProductIssueEntrys
{
    public class ProductIssueEntryService : AppService<InvProductIssueInformation>, IProductIssueEntryService
    {
        private readonly IRepository<InvProductIssueInformation> ProductIssueInformation;
    private readonly IRepository<CoreAccessCode> accessCodeRepository;
    private readonly IRepository<SalesSupplier> salesSupRepo;
    private readonly IRepository<HrmItemMasterInformation> productRepo;
    private readonly IRepository<HrmModel> modelRepo;
    private readonly IRepository<HrmBrand> brandRepo;
    private readonly IRepository<RmgPurchaseOrderReceiveDetails> purchaseOrderReceiveDetailsRepo;
        private readonly IRepository<InvProductIssueInformationDetails> productIssueInformationDetailRepo;
        private readonly IRepository<InvProductIssueInformationDetailsTemp> productIssueInformationDetailTempRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
    private readonly IRepository<HrmEmployee> empRepo;
    private readonly IRepository<CoreCompany> comRepo;
    private readonly IRepository<HrmSize> sizeRepo;
    private readonly IRepository<HmsLtrvPeriod> periodRepo;
    private readonly IRepository<RmgProdDefUnitType> unitRepo;

    public ProductIssueEntryService(
        IRepository<InvProductIssueInformation> ProductIssueInformation,
        IRepository<CoreAccessCode> accessCodeRepository,
        IRepository<SalesSupplier> salesSupRepo,
        IRepository<HrmItemMasterInformation> productRepo,
        IRepository<HrmModel> modelRepo,
        IRepository<HrmBrand> brandRepo,
        IRepository<RmgPurchaseOrderReceiveDetails> PurchaseOrderReceiveDetailsRepo,
        IRepository<InvProductIssueInformationDetails> ProductIssueInformationDetailRepo,
        IRepository<InvProductIssueInformationDetailsTemp> ProductIssueInformationDetailTempRepo,
        IRepository<HrmDefDepartment> depRepo,
        IRepository<HrmEmployee> empRepo,
        IRepository<CoreCompany> comRepo,
        IRepository<HrmSize> sizeRepo,
        IRepository<HmsLtrvPeriod> periodRepo,
        IRepository<RmgProdDefUnitType> unitRepo
        ) : base(ProductIssueInformation)
    {
        this.ProductIssueInformation = ProductIssueInformation;
        this.accessCodeRepository = accessCodeRepository;
        this.salesSupRepo = salesSupRepo;
        this.productRepo = productRepo;
        this.modelRepo = modelRepo;
        this.brandRepo = brandRepo;
        this.purchaseOrderReceiveDetailsRepo = PurchaseOrderReceiveDetailsRepo;
        this.productIssueInformationDetailRepo = ProductIssueInformationDetailRepo;
        this.productIssueInformationDetailTempRepo = ProductIssueInformationDetailTempRepo;
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


    public async Task<List<ProductIssueEntrySetupViewModel>> GetAllAsync()
    {
        try
        {
            return ProductIssueInformation.All().Select(c => new ProductIssueEntrySetupViewModel
            {
               
                Remarks = c.Remarks,
                Luser = c.Luser,
                Ldate = c.Ldate,
                Lip = c.Lip,
                Lmac = c.Lmac,
                ModifyDate = c.ModifyDate,
                UserInfoEmployeeId = c.UserInfoEmployeeId,
                CompanyCode = comRepo.All().Where(x => x.CompanyCode == c.CompanyCode).Select(x => x.CompanyName).FirstOrDefault(),

              
            }).ToList();
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task<ProductIssueEntrySetupViewModel> GetByIdAsync(string id)
    {
        try
        {
            var entity = ProductIssueInformation.All().FirstOrDefault(x => x.IssueNo == id);
            if (entity == null) return null;

            // Caching all lookup data to avoid repeated DB calls
            var allProducts = productRepo.All().ToList();
            var allBrands = brandRepo.All().ToList();
            var allModels = modelRepo.All().ToList();
            var allSizes = sizeRepo.All().ToList();
            var allPeriods = periodRepo.All().ToList();
            var allUnits = unitRepo.All().ToList();

            var detailsList = ProductIssueInformation.All()
                .Where(x => x.IssueNo == entity.IssueNo)
                .ToList() // Important: materialize query first, then project
                .Select(x =>
                {
                   

                    return new ProductIssueEntrySetupViewModel
                    {
                        TC = x.Tc,
                       
                    };
                }).ToList();


            return new ProductIssueEntrySetupViewModel
            {
                TC = entity.Tc,
                
            };
        }
        catch (Exception ex)
        {
            // Optional: log ex here
            throw;
        }
    }



    public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(ProductIssueEntrySetupViewModel model, string companyCode)
    {
        try
        {
            if (model.TC == 0) // Create
            {
                if (model.Details == null || model.Details.Count == 0)
                {
                    return (false, CreateFailed, null);
                }

                // Main Entity Create
                var entity = new InvProductIssueInformation
                {
                    Tc = model.TC,
                };


                    await ProductIssueInformation.AddAsync(entity); // Save main entity

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

                foreach (var item in model.Details)
                {
                    var detail = new RmgPurchaseOrderReceiveDetails
                    {
                        PurchaseOrderReceiveDetailsId = nextId.ToString("D3"),
                       
                        BrandId = item.BrandID ?? "",
                        ModelId = item.ModelID ?? "",
                        SizeId = item.SizeID ?? "",
                      
                        UnitTypId = item.UnitTypID ?? "",
                      
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
                var exData = await ProductIssueInformation.GetByIdAsync(model.TC);
                if (exData != null)
                {
                    exData.MainCompanyCode = model.MainCompanyCode;
                 
                    exData.Luser = model.Luser;
                    exData.ModifyDate = DateTime.Now;
                    exData.Lip = model.Lip;
                    exData.Lmac = model.Lmac;
                    exData.UserInfoEmployeeId = model.UserInfoEmployeeId;
                    exData.CompanyCode = companyCode;

                    await ProductIssueInformation.UpdateAsync(exData);

                    var detailstList = productIssueInformationDetailRepo.All().Where(x => x.IssueNo == exData.IssueNo).ToList();
                    await productIssueInformationDetailRepo.DeleteRangeAsync(detailstList);
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

                    foreach (var item in model.Details)
                    {

                        var detail = new RmgPurchaseOrderReceiveDetails
                        {
                            PurchaseOrderReceiveDetailsId = nextId.ToString("D3"),
                          
                            Luser = model.Luser ?? "",
                        };

                        detailsList.Add(detail);
                        nextId++;
                    }

                    // Bulk Insert
                    await purchaseOrderReceiveDetailsRepo.AddRangeAsync(detailsList);


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

                var entity = await ProductIssueInformation.GetByIdAsync(decimal.Parse(id));
                var detailsEntity = productIssueInformationDetailRepo.All().Where(x => x.IssueNo == entity.IssueNo).ToList();
                if (detailsEntity != null)
                {
                    await productIssueInformationDetailRepo.DeleteRangeAsync(detailsEntity);
                }
                if (entity == null)
                {
                    continue;
                }

                await ProductIssueInformation.DeleteAsync(entity);
            }
            catch (Exception)
            {
                return (true, DeleteFailed, null);
            }
        }

        return (true, DeleteSuccess, null);
    }

    public async Task<string> AutoProductIssueEntryIdAsync()
    {
        var currentYearShort = DateTime.Now.Year.ToString(); // "22"
        var prefix = $"PI_{currentYearShort}_";

        var productIssueList = (await ProductIssueInformation.GetAllAsync()).ToList();

        int newIdNumber = 1;

        if (productIssueList != null && productIssueList.Count > 0)
        {
            var lastId = productIssueList
                .Select(x => x.IssueNo)
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

    public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string productIssueValue)
    {
        bool Exists = ProductIssueInformation.All().Any(x => x.DepartmentCode == productIssueValue);
        return (Exists, DataExists, null);
    }


        public Task<(bool isSuccess, string message, object data)> CreateUpdateDetailsAsync(ProductIssueInformationDetailViewModel model, string companyCode)
        {
            throw new NotImplementedException();
        }
    }
}
