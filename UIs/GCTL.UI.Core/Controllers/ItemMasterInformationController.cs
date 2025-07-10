
using GCTL.Core.Data;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Brand;
using GCTL.Core.ViewModels.ItemMasterInformation;
using GCTL.Data.Models;
using GCTL.Service.HRM_Brand;
using GCTL.Service.ItemMasterInformation;
using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustmentReport;
using GCTL.UI.Core.ViewModels.Brand;
using GCTL.UI.Core.ViewModels.INV_Catagory;
using GCTL.UI.Core.ViewModels.ItemMasterInformation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.UI.Core.Controllers
{
    public class ItemMasterInformationController : BaseController
    {
        private readonly IRepository<InvCatagory> catagoryRepo;
        private readonly IRepository<HrmBrand> brandRepo;
        private readonly IRepository<RmgProdDefUnitType> UOMRepo;
        private readonly IItemMasterInformationService itemMasterInformationService;

        public ItemMasterInformationController(
            IRepository<InvCatagory> CatagoryRepo,
            IRepository<HrmBrand> BrandRepo,
            IRepository<RmgProdDefUnitType> UOMRepo,
            IItemMasterInformationService itemMasterInformationService
            )
        {
            this.catagoryRepo = CatagoryRepo;
            this.brandRepo = BrandRepo;
            this.itemMasterInformationService = itemMasterInformationService;
            this.UOMRepo = UOMRepo;
        }        
        public IActionResult Index(bool isPartial)
        {
            try
            {
                ViewBag.CategoryAll = new SelectList(catagoryRepo.All().Select(d => new { d.CatagoryId, d.CatagoryName }), "CatagoryId", "CatagoryName");
                ViewBag.BrandAll = new SelectList(brandRepo.All().Select(d => new { d.BrandId, d.BrandName }), "BrandId", "BrandName");
                ViewBag.UnitType = new SelectList(UOMRepo.All().Select(x => new { x.UnitTypId, x.UnitTypeName }), "UnitTypId", "UnitTypeName");
                var model = new ItemMasterInformationViewModel()
                {
                    PageUrl = Url.Action(nameof(Index))
                };
                if (isPartial)
                {
                    return PartialView(model);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return Content("Error occurred: " + ex.Message);
            }
        }

        
        public async Task<IActionResult> categoryList()
        {
            var result = catagoryRepo.All().Where(x => x.AutoId != null).OrderByDescending(x=>x.AutoId);
            return Json(new {data=result});
        }
        public async Task<IActionResult> brandList()
        {
            var result = brandRepo.All().Where(x => x.AutoId != null).OrderByDescending(x=>x.AutoId);
            return Json(new {data=result});
        }



        [HttpGet]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var data = await itemMasterInformationService.GetAllAsync();
                return Json(new { data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AutoProductId()
        {
            try
            {
                var newProductId = await itemMasterInformationService.AutoProductIdAsync();
                return Json(new { data = newProductId });
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateUpdate([FromBody] ItemMasterInformationSetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false });
            }
            try
            {
                model.ToAudit(LoginInfo);
                if (model.AutoId == 0)
                {
                    bool hasParmision = await itemMasterInformationService.SavePermissionAsync(LoginInfo.AccessCode);
                    if (hasParmision)
                    {
                        var result = await itemMasterInformationService.CreateUpdateAsync(model);
                        return Json(new { isSuccess = result.isSuccess, message = result.message, data = result.data });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access.", noSavePermission = true });
                    }
                }
                else
                {
                    var hasUpdatePermission = await itemMasterInformationService.UpdatePermissionAsync(LoginInfo.AccessCode);
                    if (hasUpdatePermission)
                    {
                        var result = await itemMasterInformationService.CreateUpdateAsync(model);
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
            var result = await itemMasterInformationService.GetByIdAsync(id);
            return Json(new { result });
        }

        //[HttpPost]
        //public async Task<IActionResult> deleteBrand([FromBody] List<string> selectedIds)
        //{
        //    var hasUpdatePermission = await itemMasterInformationService.DeletePermissionAsync(LoginInfo.AccessCode);
        //    if (hasUpdatePermission)
        //    {
        //        var result = await itemMasterInformationService.DeleteAsync(selectedIds);
        //        return Json(new { isSuccess = result.isSuccess, message = result.message, data = result });
        //    }
        //    else
        //    {
        //        return Json(new { isSuccess = false, message = "You have no access.", noUpdatePermission = true });
        //    }

        //}
        [HttpPost]
        public async Task<IActionResult> alreadyExist([FromBody] string BrandValue)
        {
            var result = await itemMasterInformationService.AlreadyExistAsync(BrandValue);
            return Json(new { isSuccess = result.isSuccess, message = result.message, data = result });
        }

    }
}
