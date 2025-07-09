
using GCTL.Core.Data;
using GCTL.Data.Models;
using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustmentReport;
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

        public ItemMasterInformationController(
            IRepository<InvCatagory> CatagoryRepo,
            IRepository<HrmBrand> BrandRepo
            )
        {
            catagoryRepo = CatagoryRepo;
            brandRepo = BrandRepo;
        }
        //public IActionResult Index()
        //{
        //    ViewBag.CategoryAll =new SelectList(catagoryRepo.All().Select(d=> new {d.CatagoryId, d.CatagoryName}), "CatagoryId", "CatagoryName");
        //    ViewBag.BrandAll =new SelectList(brandRepo.All().Select(d=> new {d.BrandId, d.BrandName}), "BrandId", "BrandName");
        //    ItemMasterInformationViewModel model = new ItemMasterInformationViewModel()
        //    {
        //        PageUrl = Url.Action(nameof(Index))
        //    };

        //    return View(model);
        //}
        public IActionResult Index()
        {
            try
            {
                ViewBag.CategoryAll = new SelectList(catagoryRepo.All().Select(d => new { d.CatagoryId, d.CatagoryName }), "CatagoryId", "CatagoryName");
                ViewBag.BrandAll = new SelectList(brandRepo.All().Select(d => new { d.BrandId, d.BrandName }), "BrandId", "BrandName");

                var model = new ItemMasterInformationViewModel()
                {
                    PageUrl = Url.Action(nameof(Index)),
                    CatagoryViewModel = new INV_CatagoryViewModel()
                    {
                        PageTitle = "Category Setup",
                        Breadcrumb = "Inventory > Category"
                    }
                };

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

    }
}
