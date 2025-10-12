using GCTL.Core.Data;
using GCTL.Data.Models;
using GCTL.UI.Core.ViewModels.RMGProdOrderInformationEntry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GCTL.UI.Core.Controllers
{
    public class RMGProdOrderInformationEntryController : BaseController
    {
        private readonly IRepository<RmgProdDefBuyer> buyerRepo;
        private readonly IRepository<RmgProdDefBrand> buyerBrandRepo;
        private readonly IRepository<ProdDefStyle> styleRepo;
        private readonly IRepository<RmgProdDefUnitType> unitTypeRepo;
        private readonly IRepository<CaDefCurrency> currencyRepo;
        private readonly IRepository<SalesDefBankInfo> bankRepo;
        private readonly IRepository<SalesDefBankBranchInfo> bankBranchRepo;
        private readonly IRepository<SalesContactPerson> buyerContactPersonRepo;
        private readonly IRepository<RmgProdDefSeason> seasonRepo;

        public RMGProdOrderInformationEntryController(
            IRepository<RmgProdDefBuyer> buyerRepo,
            IRepository<RmgProdDefBrand> buyerBrandRepo,
            IRepository<ProdDefStyle> styleRepo,
            IRepository<RmgProdDefSeason> seasonRepo,
            IRepository<RmgProdDefUnitType> unitTypeRepo,
            IRepository<CaDefCurrency> currencyRepo,
            IRepository<SalesDefBankInfo> bankRepo,
            IRepository<SalesDefBankBranchInfo> bankBranchRepo,
            IRepository<SalesContactPerson> buyerContactPersonRepo
            )
        {
            this.buyerRepo = buyerRepo;
            this.buyerBrandRepo = buyerBrandRepo;
            this.styleRepo = styleRepo;
            this.unitTypeRepo = unitTypeRepo;
            this.currencyRepo = currencyRepo;
            this.bankRepo = bankRepo;
            this.bankBranchRepo = bankBranchRepo;
            this.buyerContactPersonRepo = buyerContactPersonRepo;
            this.seasonRepo = seasonRepo;
        }
        public IActionResult Index()
        {

            ViewBag.buyerList = new SelectList(buyerRepo.All().Select(x => new { id = x.BuyerId, name = x.BuyerName }), "id", "name");
            ViewBag.buyerBrandList = new SelectList(buyerBrandRepo.All().Select(x => new { id = x.BrandId, name = x.Name }), "id", "name");
            ViewBag.styleList = new SelectList(styleRepo.All().Select(x => new { id = x.StyleId, name = x.Style }), "id", "name");
            ViewBag.seactionList = new SelectList(seasonRepo.All().Select(x => new { id = x.SeasonId, name = x.Season }), "id", "name");
            ViewBag.unitTypeList = new SelectList(unitTypeRepo.All().Select(x => new { id = x.UnitTypId, name = x.UnitTypeName }), "id", "name");
            ViewBag.currentcyList = new SelectList(currencyRepo.All().Select(x => new { id = x.CurrencyId, name = x.ShortName }), "id", "name");
            ViewBag.bankList = new SelectList(bankRepo.All().Select(x => new { id = x.BankId, name = x.BankName }), "id", "name");
            ViewBag.bankBranchList = new SelectList(bankBranchRepo.All().Select(x => new { id = x.BankBranchId, name = x.BankBranchName }), "id", "name");
            ViewBag.buyerContactList = new SelectList(buyerContactPersonRepo.All().Select(x => new { id = x.EmployeeId, name = x.ContactPersonName }), "id", "name");
            RMGProdOrderInformationEntryViewModel model = new RMGProdOrderInformationEntryViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }
    }
}
