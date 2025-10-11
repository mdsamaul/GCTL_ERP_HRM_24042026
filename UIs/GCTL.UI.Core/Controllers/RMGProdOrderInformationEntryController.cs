using GCTL.Core.Data;
using GCTL.Data.Models;
using GCTL.UI.Core.ViewModels.SalesDefInvMainItem;
using Microsoft.AspNetCore.Mvc;

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
            SalesDefInvMainItemDto model = new SalesDefInvMainItemDto()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }
    }
}
