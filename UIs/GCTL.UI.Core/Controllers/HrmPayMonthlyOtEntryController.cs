using AutoMapper;
using GCTL.Service.Common;
using GCTL.Service.HrmPayMonthlyOtEntries;
using GCTL.UI.Core.Views.HrmPayMonthlyOt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPayMonthlyOtEntryController : BaseController
    {
        private readonly IHrmPayMothlyOtEntryService entryService;
        private readonly ICommonService commonService;
        private readonly ICompositeViewEngine viewEngine;
        private readonly IMapper mapper;

        public HrmPayMonthlyOtEntryController(IHrmPayMothlyOtEntryService entryService,
            ICommonService commonService, IMapper mapper, ICompositeViewEngine viewEngine)
        {
            this.entryService = entryService;
            this.commonService = commonService;
            this.mapper = mapper;
            this.viewEngine = viewEngine;
        }

        public IActionResult Index()
        {
            HrmPayMonthlyOtViewModel model = new HrmPayMonthlyOtViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
                AddUrl = Url.Action(nameof(Setup))
            };

            return View(model);
        }

        public IActionResult Setup()
        {
            return View();
        }

       // public async Task<IActionResult> getFilterEmp()
    }
}
