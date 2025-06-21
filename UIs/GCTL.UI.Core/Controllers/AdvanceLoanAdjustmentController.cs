using GCTL.Service.AdvanceLoanAdjustment;
using GCTL.UI.Core.ViewModels.AdvanceLoanAdjustment;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class AdvanceLoanAdjustmentController : BaseController
    {
        private readonly IAdvanceLoanAdjustmentServices advanceLoanAdjustmentServices;

        public AdvanceLoanAdjustmentController(IAdvanceLoanAdjustmentServices advanceLoanAdjustmentServices)
        {
            this.advanceLoanAdjustmentServices = advanceLoanAdjustmentServices;
        }
        public IActionResult Index()
        {
            AdvanceLoanAdjustmentViewModel model = new AdvanceLoanAdjustmentViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };

            return View(model);
        }

        //get all company
        [HttpGet]
        public async Task<IActionResult> GetAllAndFilterCompany(string companyName)
        {
            var companess = await advanceLoanAdjustmentServices.GetAllAndFilterCompanyAsync(companyName);
            return Json(companess);
        }

        //get employee by company
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByFilter(string employeeStatusId, string companyCode, string EmployeeName)
        {
            var employees = await advanceLoanAdjustmentServices.GetEmployeesByFilterAsync(employeeStatusId, companyCode, EmployeeName);
            return Json(employees);
        }
    }
}
