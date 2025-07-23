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
            IRepository<HrmEmployeeOfficialInfo> OfficialInfo
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
        public async Task<IActionResult> SupplierCloseList()
        {
            var result = supplier.All().Where(x => x.SupplierId != null).OrderByDescending(x => x.SupplierId).ToList();
            return Json(new { data = result });
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
        public async Task<IActionResult> AutoPrintingStationeryPurchaseId()
        {
            try
            {
                var newIssueId = await productIssueEntryService.AutoProductIssueEntryIdAsync();
                return Json(new { data = newIssueId });
            }
            catch (Exception)
            {
                throw;
            }

        }


        [HttpPost]
        public async Task<IActionResult> CreateUpdate([FromBody] ProductIssueEntrySetupViewModel model)
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
        public async Task<IActionResult> deletePrintingStationeryPurchase([FromBody] List<string> selectedIds)
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
       
        [HttpGet]
        public async Task<IActionResult> addMoreLoadProduct()
        {
            var sizeList = sizeRepo.All().Select(x => new { value = x.SizeId, text = x.SizeName }).ToList();
            var periodList = periodRepo.All().Select(x => new { value = x.PeriodId, text = x.PeriodName }).ToList();
            var unitList = unitRepo.All().Select(x => new { value = x.UnitTypId, text = x.UnitTypeName }).ToList();

            var productList = productRepo.All()
                .Select(x => new
                {
                    value = x.ProductCode,
                    text = x.ProductName
                }).ToList();

            return Json(new
            {
                productList = productList,
                sizeList = sizeList,
                periodList = periodList,
                unitList = unitList
            });
        }
        [HttpGet]
        public IActionResult productItemClose()
        {
            //var result = productRepo.All().Select(x => new { x.ProductCode, x.ProductName }).OrderByDescending(x=> x.ProductCode).ToList();
            var result = productRepo.All().Where(x => x.AutoId != null).OrderByDescending(x => x.AutoId).ToList();
            return Json(new { data = result });
        }

        [HttpGet]
        public IActionResult BrandListClose()
        {
            var result = brandRepo.All().Where(x => x.AutoId != null).OrderByDescending(x => x.AutoId).ToListAsync();
            return Json(new { data = result });
        }
    }
}
