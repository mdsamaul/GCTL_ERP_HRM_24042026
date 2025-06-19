//using Dapper;
//using GCTL.Core.ViewModels.EmployeeFilter;
//using System.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Data.SqlClient;

//namespace GCTL.UI.Core.Controllers
//{
//    public class HRMEmployeeHolidayDeclarationController : Controller
//    {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCTL.Data.Models;
using GCTL.UI.Core.Controllers;
using GCTL.Service.Common;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using AutoMapper;
using GCTL.UI.Core.ViewModels.HRMEmployeeHolidayDeclarations;

using GCTL.Service.HrmEmployeeHolidayDeclarations;
using GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations;
using GCTL.Core.Helpers;
using OfficeOpenXml;
using GCTL.Core.ViewModels;

namespace GCTL.UI.Core.Controllers
{
    public class HRMEmployeeHolidayDeclarationController : BaseController
    {
        private readonly IHrmEmployeeHolidayDeclarationService holidayService;
        private readonly ICommonService commonService;
        private readonly ICompositeViewEngine viewEngine;
        private readonly IMapper mapper;

        public HRMEmployeeHolidayDeclarationController(IHrmEmployeeHolidayDeclarationService holidayService,
            ICommonService commonService, ICompositeViewEngine viewEngine, IMapper mapper)
        {
            this.holidayService = holidayService;
            this.commonService = commonService;
            this.viewEngine = viewEngine;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            HRMEmployeeHolidayDeclarationViewModel model = new HRMEmployeeHolidayDeclarationViewModel()
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

        public async Task<ActionResult> BulkDelete(List<decimal> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No Holiday is selected to delete" });
                }

                var result = await holidayService.BulkDeleteAsync(ids);

                if (!result)
                {
                    return Json(new { isSuccess = false, message = "No Holiday found to delete" });
                }
                return Json(new { isSuccess = true, message = $"Deleted {ids.Count} Holiday Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHolidayByDateAndEmployees([FromBody] HrmEmployeeHolidayDeclarationViewModel model)
        {
            try
            {
                var result = await holidayService.DeleteHolidayDeclarationsByDateAndEmployees(
                    model.Date,
                    model.EmployeeIds,
                    model.CompanyCode);

                if (result)
                {
                    return Json(new { success = true, message = "Holiday deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "No holiday found for the specified date and employees." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting holiday." });
            }
        }


        public async Task<IActionResult> GetPaginatedHolidayDeclaration()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            var page = string.IsNullOrEmpty(start) ? 1 : (Convert.ToInt32(start) / pageSize) + 1;

            var result = await holidayService.GetPaginatedDataAsync(searchValue, page, pageSize, sortColumn, sortDirection);

            var response = new
            {
                draw = draw,
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.TotalRecords,
                data = result.Data
            };

            return Ok(response);
        }

        public async Task<IActionResult> GetHolidayEmployeeDeclaration()
        {
            var result = await holidayService.GetAllAsync();

            return Json(new { data = result });
        }

        public async Task<IActionResult> GetHolidayDeclarationType()
        {
            var result = await holidayService.GetAllHolidayType();

            return Json(new { data = result });
        }

        public async Task<IActionResult> Details(decimal id)
        {
            var result = await holidayService.GetByIdAsync(id);

            return Json(new { data = result });
        }

        public async Task<IActionResult> DownloadExcel()
        {
            var fileBytes = await holidayService.GenerateEmployeeHolidayDeclarationExcelAsync();
            string excelName = $"EmployeeData.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [HttpPost]
        public async Task<IActionResult> SaveHoliday([FromBody] HrmEmployeeHolidayDeclarationViewModel modelVM)
        {
            modelVM.ToAudit(LoginInfo);
            var result = await holidayService.SaveAsync(modelVM);

            return Json(new
            {
                success = result.isSuccess,
                message = result.message,
                data = result.data
            });
        }

        public async Task<IActionResult> EditHoliday(string id, [FromBody] HrmEmployeeHolidayDeclarationViewModel modelVM)
        {
            modelVM.ToAudit(LoginInfo, true);
            var result = await holidayService.EditAsync(modelVM);

            return Json(new
            {
                success = result.isSuccess,
                message = result.message
            });
        }

        [Route("/HrmEmployeeHolidayDeclaration/UploadExcelAsync")]
        public async Task<IActionResult> UploadExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "File must be an Excel (.xlsx) file" });
            }

            BaseViewModel model = new BaseViewModel();
            model.ToAudit(LoginInfo);

            try
            {
                string userName = model.Luser;
                string userIp = model.Lip;
                string userMac = model.Lmac;
                string companyCode = Request.Form["CompanyCode"];
                using (var stream = file.OpenReadStream())
                {
                    var result = await holidayService.ProcessExcelFileAsync(
                        stream,
                        userName,
                        userIp,
                        userMac,
                        companyCode);

                    if (result.isSuccess)
                    {
                        return Ok(new { success = true, message = result.message, data = result.data });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = result.message, validationErrors = result.data });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error processing Excel file", error = ex.Message });
            }
        }


        public IActionResult GetAllCompany()
        {
            var result = holidayService.GetAllCompany();
            return Json(new { data = result });
        }

        public async Task<IActionResult> getFilterEmp([FromBody] EmployeeFilterViewModel model)
        {
            var data = await holidayService.GetFilterDataAsync(model);
            return Json(data);
        }
    }
}
