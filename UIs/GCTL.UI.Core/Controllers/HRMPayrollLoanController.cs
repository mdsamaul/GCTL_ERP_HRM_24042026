using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HRMPayrollLoan;
using GCTL.Service.HRMPayrollLoan;
using GCTL.UI.Core.ViewModels.HRMPayrollLoan;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HRMPayrollLoanController : BaseController
    {
        private readonly IHRMPayrollLoanService hRMPayrollLoanService;

        public HRMPayrollLoanController(IHRMPayrollLoanService hRMPayrollLoanService)
        {
            this.hRMPayrollLoanService = hRMPayrollLoanService;
        }
        public IActionResult Index()
        {
            HRMPayrollLoanViewModel model = new HRMPayrollLoanViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
        //get company
        [HttpPost]
        public async Task<IActionResult> GetFilterData([FromBody] PayrollLoanFilterEntryDto entryDto)
        {
            var result = await hRMPayrollLoanService.GetFilaterDataAsync(entryDto);
            return Json(new {isSuccess =true, messate="Successed data load", data=result});
        }
        //get employee id 
        [HttpGet]
        public async Task<IActionResult> GetEmpById(string empId)
        {
            if (string.IsNullOrWhiteSpace(empId))
            {
                return Json(new { isSuccess = false, message = "Employee not found" });
            }

            var result = await hRMPayrollLoanService.EmployeeGetById(empId);

            if (result.isSuccess)
            {
                return Json(new { result.isSuccess, result.message, data = result.Item3 });
            }
            else
            {
                return Json(new { result.isSuccess, result.message });
            }
        }

        //get loan type 

        public async Task<IActionResult> getLoanType()
        {
            var result = await hRMPayrollLoanService.GetLoanTypeAsync();
            if(result.isSuccess)
            {
                return Json(new {isSuccess= result.isSuccess, message=result.message, data = result.data});
            }
            return Json(new { isSuccess = true, message = "Loan Type loaded" });
        }

        //get payment mode
        [HttpGet]
        public async Task<IActionResult> GetPaymentMode()
        {
            var result = await hRMPayrollLoanService.getPaymentModeAsync();
            return Json(new { data= result});
        }

        //get pay head
        [HttpGet]
        public async Task<IActionResult> getPayHeadDeduction()
        {
            var result = await hRMPayrollLoanService.GetPayHeadDeductionAsync();
            return Json(new { data = result });
        }

        //loan id
        [HttpGet]
        public async Task<IActionResult> CreateLoanId()
        {
            var LoanId = await hRMPayrollLoanService.createLoanIdAsync();
            return Json(new {LoanId});
        }
        //get bank
        [HttpGet]
        public async Task<IActionResult> GetBank()
        {
            var banks = await hRMPayrollLoanService.GetBankAsync();
            return Json(new { data=banks});
        }

        //create loan 
        [HttpPost]
        public async Task<IActionResult> CreateEditLoan([FromBody] HRMPayrollLoanSetupViewModel modelData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }
            modelData.ToAudit(LoginInfo);
            var result = await hRMPayrollLoanService.CreateEditLoanAsycn(modelData);
            //if(result.I)
            // await _loanService.SaveLoanAsync(modelData);

            return Ok(new {isSuccess = result.isSuccess, message = result.message, data= result});
        }
        //get loan data
        [HttpGet]
        public async Task<IActionResult> GetLoanData()
        {
            var result =await hRMPayrollLoanService.GetLoanDataAsync();
            return Json(new { data= result});
        }
    }
}
