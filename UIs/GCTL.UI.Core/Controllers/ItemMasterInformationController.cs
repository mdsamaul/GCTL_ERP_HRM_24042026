
using GCTL.Core.Data;
using GCTL.Data.Models;
using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustmentReport;
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
        public IActionResult Index()
        {
            ViewBag.CategoryAll =new SelectList(catagoryRepo.All().Select(d=> new {d.CatagoryId, d.CatagoryName}), "CatagoryId", "CatagoryName");
            ViewBag.BrandAll =new SelectList(brandRepo.All().Select(d=> new {d.BrandId, d.BrandName}), "BrandId", "BrandName");
            ItemMasterInformationViewModel model = new ItemMasterInformationViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };

            return View(model);
        }


    }
}
