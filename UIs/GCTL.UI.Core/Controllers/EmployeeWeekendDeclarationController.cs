using System.Data;
using System.Diagnostics;
using AutoMapper;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HRM_EmployeeWeekendDeclaration;
using GCTL.Data.Models;
using GCTL.Service.Common;
using GCTL.Service.Companies;
using GCTL.Service.EmployeeWeekendDeclaration;
using GCTL.Service.HrmDefEmpTypes;
using GCTL.UI.Core.ViewModels.HRM_EmployeeWeekendDeclaration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using GCTL.UI.Core.Controllers;
using System.Threading.Tasks;
using OfficeOpenXml;
using GCTL.Core.ViewModels.Companies;
using Org.BouncyCastle.Ocsp;
using DocumentFormat.OpenXml.Spreadsheet;
namespace GCTL.UI.Core.Controllers
{
    public class EmployeeWeekendDeclarationController : BaseController
    {
        private readonly ICompanyService companyService;
        private readonly ICommonService commonService;
        private readonly IMapper mapper;
        private readonly IEmployeeWeekendDeclarationService employeeWeekendDeclarationService;
        private readonly IHrmDefEmpTypeService service;
        public EmployeeWeekendDeclarationController(ICompanyService companyService,
                                          ICommonService commonService,
                                          IMapper mapper,
                                           IEmployeeWeekendDeclarationService employeeWeekendDeclarationService,
                                             IHrmDefEmpTypeService service
                                          )
        {
            this.companyService = companyService;
            this.commonService = commonService;
            this.mapper = mapper;
            this.employeeWeekendDeclarationService = employeeWeekendDeclarationService;
            this.service = service;
        }
        public IActionResult Index()
        {
            HRM_EmployeeWeekendDeclarationViewModel model = new HRM_EmployeeWeekendDeclarationViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }
     

        public IActionResult GetAllCompany()
        {
            var result = employeeWeekendDeclarationService.GetAllCompany();
            return Json(new {data=result});
        }

        [HttpPost]
        public async Task<IActionResult> getFilterEmp([FromBody]  EmployeeFilterDto filter)
        {
            var data = await employeeWeekendDeclarationService.GetFilterDataAsync(filter);
            return Json(new { data = data });
        }



        [HttpPost]
        public async Task<IActionResult> SaveSelectedDatesAndEmployees([FromBody] HRM_EmployeeWeekendDeclarationDto modelVM)
        {
            modelVM.ToAudit(LoginInfo);
            var result = await employeeWeekendDeclarationService.SaveSelectedDatesAndEmployeesAsync(modelVM);

            return Json(new
            {
                success = result.isSuccess,
                message = result.message,
                data = result.data
            });
        }
        public IActionResult GetWeekendEmployeeDeclaration()        {            
            var result = employeeWeekendDeclarationService.GetWeekendEmpDecService();
            return Json(new {data=result});
        }
        public async Task<ActionResult> BulkDeleteEmpWeelend(List<decimal> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "Employee not selected" });
                }

                var result = await employeeWeekendDeclarationService.BulkDeleteAsync(ids);
                if (!result)
                {
                    return Json(new { isSuccess = false, message = "Employee not found" });
                }
                return Json(new { isSuccess = true, message = $"Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
       
        [HttpPost]
        public async Task<IActionResult> editEmpWeekDec(string Id)
        {          
            var emp = await employeeWeekendDeclarationService.GetEmployeeWeekendDeclarationByIdAsync(Id);

            if (emp == null)
                return NotFound();

            return Json(new
            {
                weekendDate = emp.WeekendDate,
                remarks = emp.Remarks,
                id = emp.ID
            });            
        }
        [HttpPost]
        public async Task<IActionResult> UpdateEmpWeekDec(string Id, String WeekendDate, string Remarks)
        {           
            var (isSuccess, message) = await employeeWeekendDeclarationService
                                    .UpdateEmployeeWeekendDeclarationAsync(Id, WeekendDate, Remarks);
            return Json(new { success = isSuccess, message = message });
        }


        [HttpPost]
        [Route("/EmployeeWeekendDeclaration/UploadExcelAsync")]
        public async Task<IActionResult> UploadExcelAsync(IFormFile excelFile, HRM_EmployeeWeekendDeclarationDto modelVM)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { isSuccess = false, message = "Please select a valid Excel file." });
            }

            modelVM.ToAudit(LoginInfo);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    modelVM.WeekendEmployeeIds = new List<string>();
                    modelVM.WeekendDates = new List<string>();
                    modelVM.ExcelRemark = new List<string>(); 

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var empId = worksheet.Cells[row, 1].Text.Trim();
                        var date = worksheet.Cells[row, 2].Text.Trim();
                        var remark = worksheet.Cells[row, 3].Text.Trim();

                        modelVM.WeekendEmployeeIds.Add(empId);
                        modelVM.WeekendDates.Add(date);
                        modelVM.ExcelRemark.Add(remark); 
                    }
                }
            }

            var result = await employeeWeekendDeclarationService.SaveSelectedDatesAndEmployeesFromExcelAsync(modelVM);

            return Json(new { isSuccess = result.isSuccess, message = result.message });
        }

        public async Task<IActionResult> DownloadExcel()
        { 
            var fileBytes = await employeeWeekendDeclarationService.GenerateEmployeeWeekendDeclarationExcelAsync();
            string excelName = $"EmployeeData.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);      
        }      
    }
}
