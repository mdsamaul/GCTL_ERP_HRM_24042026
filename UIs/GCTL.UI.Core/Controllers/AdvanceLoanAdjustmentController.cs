using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdvanceLoanAdjustment;
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
        public async Task<IActionResult> GetEmployeesByFilter(string employeeStatusId, string companyCode, string EmployeeName, bool loanAdjustment) 
        {
            var employees = await advanceLoanAdjustmentServices.GetEmployeesByFilterAsync(employeeStatusId, companyCode, EmployeeName, loanAdjustment);
            return Json(employees);
        }
        [HttpPost]
        public async Task<IActionResult> GetLoadEmployeeById(string employeeId)
        {
            var employee = await advanceLoanAdjustmentServices.GetLoadEmployeeByIdAsync(employeeId);
            return Json(employee);
        }
        [HttpGet]
        public async Task<IActionResult> GetLoanByEmployeeId(string employeeId)
        {
            var Loans = await advanceLoanAdjustmentServices.GetLoanByEmployeeIdAsync(employeeId);
            return Json(Loans);
        }

        [HttpGet]
        public async Task<IActionResult> GetLoanById(string loanId)
        {
            var loan = await advanceLoanAdjustmentServices.GetLoanByIdAsync(loanId);
            return Json(loan);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUpdateLoanAdjustment([FromBody] AdvanceLoanAdjustmentSetupViewModel modelData)
        {
            if (ModelState.IsValid)
            {
                // Save to database logic goes here
                modelData.ToAudit(LoginInfo);
                var AdjustmentLoan = await advanceLoanAdjustmentServices.SaveUpdateLoanAdjustmentAsync(modelData);
                return Json(new { success = AdjustmentLoan.isSuccess, message = AdjustmentLoan.message , data = AdjustmentLoan });
            }

            return Json(new { success = false, message = "Invalid data." });
        }

        //auto ganerate id
        public async Task<IActionResult> AdjustmentAutoGanarateId()
        {
            var autoId = await advanceLoanAdjustmentServices.AdjustmentAutoGanarateIdAsync();
            return Json(autoId);
        }
        [HttpGet]
        public async Task<IActionResult> GetMonth()
        {
            var months = await advanceLoanAdjustmentServices.GetMonthAsync();
            return Json(months);
        }
        [HttpGet]
        public async Task<IActionResult> GetHeadDeduction()
        {
            var DeductionHeads = await advanceLoanAdjustmentServices.GetHeadDeductionAsync();
            return Json(DeductionHeads);
        }


        // Updated Controller Method
        [HttpPost]
        public async Task<JsonResult> GetAdvancePayData(DataTableRequest request)
        {
            try
            {
                // Validate request parameters
                if (request.Page <= 0) request.Page = 1;
                if (request.PageSize <= 0) request.PageSize = 10;
                if (request.PageSize > 100) request.PageSize = 100; // Limit max page size

                var result = await advanceLoanAdjustmentServices.GetAdvancePayPaged(request);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = result.TotalRecords,
                    recordsFiltered = result.FilteredRecords,
                    data = result.Data ?? new List<AdvancePayViewModel>()
                });
            }
            catch (Exception ex)
            {
                // Log the full exception
                Console.WriteLine($"Controller Error: {ex}");

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<AdvancePayViewModel>(),
                    error = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdvancePay([FromBody] List<decimal> selectedIds)
        {
            var result = await advanceLoanAdjustmentServices.DeleteAdvancePayAsync(selectedIds);
            return Json(new {isSuccess=result.isSuccess, message = result.message});
        }

    }
}
