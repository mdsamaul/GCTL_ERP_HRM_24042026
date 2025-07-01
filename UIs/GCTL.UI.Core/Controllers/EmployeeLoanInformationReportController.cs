using GCTL.Core.ViewModels.EmployeeLoanInformationReport;
using GCTL.Service.EmployeeLoanInformationReport;
using GCTL.UI.Core.ViewModels.EmployeeLoanInformationReport;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class EmployeeLoanInformationReportController : BaseController
    {
        private readonly IEmployeeLoanInformationReportServices employeeLoanInformationReportServices;

        public EmployeeLoanInformationReportController(
                IEmployeeLoanInformationReportServices employeeLoanInformationReportServices
            )
        {
            this.employeeLoanInformationReportServices = employeeLoanInformationReportServices;
        }
        public IActionResult Index()
        {
            EmployeeLoanInformationReportSetupVM model = new EmployeeLoanInformationReportSetupVM()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetLoanDetails([FromQuery] LoanFilterVM filter)
        {
            var data = await employeeLoanInformationReportServices.GetLoanDetailsAsync(filter);
            return Json(data);
        }



        //[HttpGet]
        //public async Task<IActionResult> GetLoanDetails(string employeeId)
        //{
        //    //if (string.IsNullOrEmpty(employeeId))
        //    //    return BadRequest("Employee ID is required");

        //    var data = await employeeLoanInformationReportServices.GetLoanDetailsByEmployeeIdAsync(employeeId);
        //    return Json(data);
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetLoanEmployees()
        //{
        //    var data = await employeeLoanInformationReportServices.GetDistinctLoanEmployeesAsync(); 
        //    return Json(data);
        //}

    }
}
