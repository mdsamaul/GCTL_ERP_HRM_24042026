using GCTL.Core.Data;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PrintingStationeryPurchaseEntry;
using GCTL.Core.ViewModels.ProductIssueEntry;
using GCTL.Data.Models;
using GCTL.Service.PrintingStationeryPurchaseEntry;
using GCTL.Service.ProductIssueEntrys;
using GCTL.UI.Core.ViewModels;
using GCTL.UI.Core.ViewModels.PrintingStationeryPurchaseEntry;
using GCTL.UI.Core.ViewModels.ProductIssueEntry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL.UI.Core.Controllers
{
    public class ProductIssueEntryController : BaseController
    {
        private readonly IProductIssueEntryService productIssueEntryService;
        private readonly IRepository<HrmItemMasterInformation> productRepo;
        private readonly IRepository<HrmBrand> brandRepo;
        private readonly IRepository<HrmSize> sizeRepo;
        private readonly IRepository<HmsLtrvPeriod> periodRepo;
        private readonly IRepository<RmgProdDefUnitType> unitRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<HrmModel> modelRepo;
        private readonly IRepository<SalesSupplier> supplier;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> officialInfo;
        private readonly IRepository<HrmDefFloor> floorRepo;

        public ProductIssueEntryController(
           IProductIssueEntryService productIssueEntryService,
            IRepository<HrmItemMasterInformation> productRepo,
            IRepository<HrmBrand> brandRepo,
            IRepository<HrmSize> sizeRepo,
            IRepository<HmsLtrvPeriod> periodRepo,
            IRepository<RmgProdDefUnitType> unitRepo,
            IRepository<HrmDefDepartment> depRepo,
            IRepository<HrmModel> modelRepo,
            IRepository<SalesSupplier> supplier,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmEmployeeOfficialInfo> OfficialInfo,
            IRepository<HrmDefFloor> floorRepo
            )
        {
            this.productIssueEntryService = productIssueEntryService;
            this.productRepo = productRepo;
            this.brandRepo = brandRepo;
            this.sizeRepo = sizeRepo;
            this.periodRepo = periodRepo;
            this.unitRepo = unitRepo;
            this.depRepo = depRepo;
            this.modelRepo = modelRepo;
            this.supplier = supplier;
            this.empRepo = empRepo;
            this.officialInfo = OfficialInfo;
            this.floorRepo = floorRepo;
        }
        public IActionResult Index()
        {
            ViewBag.ProductList = new SelectList(productRepo.All().Select(x => new { x.ProductCode, x.ProductName }), "ProductCode", "ProductName");
            ViewBag.BrandList = new SelectList(brandRepo.All().Select(x => new { x.BrandId, x.BrandName }), "BrandId", "BrandName");
            ViewBag.SizeList = new SelectList(sizeRepo.All().Select(x => new { x.SizeId, x.SizeName }), "SizeId", "SizeName");
            ViewBag.periodList = new SelectList(periodRepo.All().Select(x => new { x.PeriodId, x.PeriodName }), "PeriodId", "PeriodName");
            ViewBag.unitList = new SelectList(unitRepo.All().Select(x => new { x.UnitTypId, x.UnitTypeName }), "UnitTypId", "UnitTypeName");
            ViewBag.departmentList = new SelectList(depRepo.All().Select(x => new { x.DepartmentCode, x.DepartmentName }), "DepartmentCode", "DepartmentName");
            ViewBag.modelList = new SelectList(modelRepo.All().Select(x => new { x.ModelId, x.ModelName }), "ModelId", "ModelName");
            ViewBag.SuppleirList = new SelectList(supplier.All().Select(x => new { x.SupplierId, x.SupplierName }), "SupplierId", "SupplierName");
            ViewBag.FloorList = new SelectList(floorRepo.All().Select(x => new { x.FloorCode, x.FloorName }), "FloorCode", "FloorName");
            var employeeList = (from emp in empRepo.All()
                                join offi in officialInfo.All()
                                on emp.EmployeeId equals offi.EmployeeId
                                where offi.EmployeeStatus == "01"
                                select new
                                {
                                    emp.EmployeeId,
                                    FullName = emp.FirstName + " " + emp.LastName
                                }).ToList();

            ViewBag.EmployeeList = new SelectList(employeeList, "EmployeeId", "FullName");
            ProductIssueEntryViewModel model = new ProductIssueEntryViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }

    

        [HttpGet]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var data = await productIssueEntryService.GetAllAsync();
                return Json(new { data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AutoProdutIssueId()
        {
            try
            {
                var newIssueId = await productIssueEntryService.AutoProdutIssueIdAsync();
                return Json(new { data = newIssueId });
            }
            catch (Exception)
            {
                throw;
            }

        }


        [HttpPost]
        public async Task<IActionResult> CreateEditProductIssue([FromBody] ProductIssueEntrySetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false });
            }
            try
            {
                model.ToAudit(LoginInfo);
                if (model.TC == 0)
                {
                    bool hasParmision = await productIssueEntryService.SavePermissionAsync(LoginInfo.AccessCode);
                    if (hasParmision)
                    {
                        var result = await productIssueEntryService.CreateUpdateAsync(model, LoginInfo.CompanyCode);
                        return Json(new { isSuccess = result.isSuccess, message = result.message, data = result.data });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access.", noSavePermission = true });
                    }
                }
                else
                {
                    var hasUpdatePermission = await productIssueEntryService.UpdatePermissionAsync(LoginInfo.AccessCode);
                    if (hasUpdatePermission)
                    {
                        var result = await productIssueEntryService.CreateUpdateAsync(model, LoginInfo.CompanyCode);
                        return Json(new { isSuccess = result.isSuccess, message = result.message, data = result.data });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access.", noUpdatePermission = true });
                    }
                }
            }
            catch (Exception)
            {

                return Json(new { isSuccess = false, message = "Faild" });
            }


        }

        [HttpGet]
        public async Task<IActionResult> PopulatedDataForUpdate(string id)
        {
            ViewBag.ProductList = new SelectList(productRepo.All().Select(x => new { x.ProductCode, x.ProductName }), "ProductCode", "ProductName");
            ViewBag.BrandList = new SelectList(brandRepo.All().Select(x => new { x.BrandId, x.BrandName }), "BrandId", "BrandName");
            ViewBag.SizeList = new SelectList(sizeRepo.All().Select(x => new { x.SizeId, x.SizeName }), "SizeId", "SizeName");
            ViewBag.periodList = new SelectList(periodRepo.All().Select(x => new { x.PeriodId, x.PeriodName }), "PeriodId", "PeriodName");
            ViewBag.unitList = new SelectList(unitRepo.All().Select(x => new { x.UnitTypId, x.UnitTypeName }), "UnitTypId", "UnitTypeName");
            ViewBag.departmentList = new SelectList(depRepo.All().Select(x => new { x.DepartmentCode, x.DepartmentName }), "DepartmentCode", "DepartmentName");
            ViewBag.modelList = new SelectList(modelRepo.All().Select(x => new { x.ModelId, x.ModelName }), "ModelId", "ModelName");
            ViewBag.SuppleirList = new SelectList(supplier.All().Select(x => new { x.SupplierId, x.SupplierName }), "SupplierId", "SupplierName");
            var result = await productIssueEntryService.GetByIdAsync(id);
            return Json(new { result });
        }

        [HttpPost]
        public async Task<IActionResult> deleteIssue([FromBody] List<string> selectedIds)
        {
            var hasUpdatePermission = await productIssueEntryService.DeletePermissionAsync(LoginInfo.AccessCode);
            if (hasUpdatePermission)
            {
                var result = await productIssueEntryService.DeleteAsync(selectedIds);
                return Json(new { isSuccess = result.isSuccess, message = result.message, data = result });
            }
            else
            {
                return Json(new { isSuccess = false, message = "You have no access.", noUpdatePermission = true });
            }

        }
        [HttpPost]
        public async Task<IActionResult> alreadyExist([FromBody] string PrintingStationeryPurchaseValue)
        {
            var result = await productIssueEntryService.AlreadyExistAsync(PrintingStationeryPurchaseValue);
            return Json(new { isSuccess = result.isSuccess, message = result.message, data = result });
        }
        [HttpPost]
        public async Task<IActionResult> SelectBrandByProductId([FromBody] string productId)
        {
            var BrandList = (from p in productRepo.All().Where(x => x.ProductCode == productId)
                             join b in brandRepo.All()
                             on p.BrandId equals b.BrandId
                             select new
                             {
                                 b.BrandId,
                                 b.BrandName
                             }).Distinct().ToList();

            return Json(new { BrandList });
        }
        [HttpPost]
        public async Task<IActionResult> SelectModalByBrandId([FromBody] string brandId)
        {
            var ModelList = (from m in modelRepo.All()
                             join b in brandRepo.All().Where(x => x.BrandId == brandId)
                             on m.BrandId equals b.BrandId
                             select new
                             {
                                 m.ModelId,
                                 m.ModelName
                             }).Distinct().ToList();

            return Json(new { ModelList });
        }
        public async Task<IActionResult> PurchaseIssueAddmoreCreateEditDetails([FromBody] ProductIssueInformationDetailViewModel model)
        {
            model.ToAudit(LoginInfo);
            var result = await productIssueEntryService.PurchaseIssueAddmoreCreateEditDetailsAsync(model);
            return Json(new {isSuccess= result.isSuccess, message = result.message, data = result.data });
        }

        [HttpGet]
        public async Task<IActionResult> LoadTempData()
        {
            var resutl = await productIssueEntryService.LoadTempDataAsync();
            return Json(new { data = resutl });
        }
        [HttpPost]
        public async Task<IActionResult> detailsDeleteById([FromBody] decimal id)
        {
            var result = await productIssueEntryService.detailsDeleteByIdAsync(id);
            return Json(new { data = result });
        }
        [HttpPost]
        public async Task<IActionResult> detailsEditById([FromBody] decimal id)
        {
            var result = await productIssueEntryService.detailsEditByIdAsync(id);
            return Json(new { isSuccess = result.isSuccess, data = result.data });
        }

        [HttpPost]
        public async Task<IActionResult> EditPopulateIssueid([FromBody] decimal issueId)
        {
            var result = await productIssueEntryService.EditPopulateIssueidAsync(issueId);
            return Json(new {data = result});
        }
    }
}
