using GCTL.Core.ViewModels.Departments;
using GCTL.Data.Models;
using GCTL.Service.Common;
using GCTL.Service.Departments;
using GCTL.UI.Core.ViewModels.Departments;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.Helpers;
using ClosedXML.Excel;
using iText.Kernel.Pdf;

using iText.IO.Image;
using iText.Kernel.Colors;

using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;




namespace GCTL.UI.Core.Controllers
{
    public class DepartmentsController : BaseController
    {
        private readonly IDepartmentService departmentService;
        private readonly ICommonService commonService;
        private readonly IMapper mapper;
        string strMaxNO = "";
        public DepartmentsController(IDepartmentService departmentService,
                                     ICommonService commonService,
                                     IMapper mapper)
        {
            this.departmentService = departmentService;
            this.commonService = commonService;
            this.mapper = mapper;
        }

        public IActionResult Index(bool child = false)
        {
            DepartmentPageViewModel model = new DepartmentPageViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            commonService.FindMaxNo(ref strMaxNO, "DepartmentCode", "HRM_Def_Department", 3);
            model.Setup = new DepartmentSetupViewModel
            {
                DepartmentCode = strMaxNO
            };

            if (child)
                return PartialView(model);

            return View(model);
        }


        public IActionResult Setup(string id)
        {
            DepartmentSetupViewModel model = new DepartmentSetupViewModel();
            commonService.FindMaxNo(ref strMaxNO, "DepartmentCode", "HRM_Def_Department", 3);
            var result = departmentService.GetDepartment(id);
            if (result != null)
            {
                model = mapper.Map<DepartmentSetupViewModel>(result);
                model.Code = id;
                model.Id = (int)result.AutoId;
            }
            else
            {
                model.DepartmentCode = strMaxNO;
            }

            return PartialView($"_{nameof(Setup)}", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Setup(DepartmentSetupViewModel model)
        {
            if (departmentService.IsDepartmentExist(model.DepartmentName, model.DepartmentCode))
            {
                return Json(new { isSuccess = false, message = "Already Exists" });
            }

            if (ModelState.IsValid)
            {
                if (departmentService.IsDepartmentExistByCode(model.DepartmentCode))
                {
                    var hasPermission = departmentService.UpdatePermission(LoginInfo.AccessCode);
                    if (hasPermission)
                    {
                        HrmDefDepartment department = departmentService.GetDepartment(model.DepartmentCode) ?? new HrmDefDepartment();
                        model.ToAudit(LoginInfo, model.Id > 0);
                        mapper.Map(model, department);
                        
                        departmentService.SaveDepartment(department);
                        return Json(new { isSuccess = true, message = "Saved Successfully", lastCode = department.DepartmentCode });
                    }
                    else
                    {

                        return Json(new { isSuccess = false, message = "You have no access" });
                    }

                }
                else
                {
                    var hasPermission = departmentService.SavePermission(LoginInfo.AccessCode);
                    if (hasPermission)
                    {
                        HrmDefDepartment department = departmentService.GetDepartment(model.DepartmentCode) ?? new HrmDefDepartment();
                        model.ToAudit(LoginInfo, model.Id > 0);
                        mapper.Map(model, department);
                        departmentService.SaveDepartment(department);
                        return Json(new { isSuccess = true, message = "Saved Successfully", lastCode = department.DepartmentCode });
                    }
                    else
                    {

                        return Json(new { isSuccess = false, message = "You have no access" });
                    }

                }

               
            }

            return Json(new { success = false, message = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage });
        }

        public ActionResult Grid()
        {
            var result = departmentService.GetDepartments();
            return Json(new { data = result });
        }


        [HttpPost]
        public ActionResult Delete(string id)
        {
            var hasPermission = departmentService.DeletePermission(LoginInfo.AccessCode);
            if (hasPermission)
            {
                bool success = false;
                foreach (var item in id.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    success = departmentService.DeleteDepartment(item);
                }

                return Json(new { success = success, message = "Deleted Successfully" });
            }
            else
            {
                return Json(new { isSuccess = false, message = "You have no access" });
            }
           
        }


        [HttpPost]
        public JsonResult CheckAvailability(string name, string code)
        {
            if (departmentService.IsDepartmentExist(name, code))
            {
                return Json(new { isSuccess = true, message = "Already Exists!" });
            }

            return Json(new { isSuccess = false });
        }


        #region Xls
        public async Task<IActionResult> ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Departments");

                // Add title
                worksheet.Cell(1, 1).Value = "Department Information";
                worksheet.Range(1, 1, 1, 4).Merge(); // Merge cells across the header columns
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Font.FontSize = 14;
                worksheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range(2, 1, 2, 4).Merge();
                // Leave an empty row
                int dataStartRow = 3;

                // Add headers
                worksheet.Cell(dataStartRow, 1).Value = "Department Id";
                worksheet.Cell(dataStartRow, 2).Value = "Department Name";
                worksheet.Cell(dataStartRow, 3).Value = "Short Name";
                worksheet.Cell(dataStartRow, 4).Value = "Department (বাংলা)";
                worksheet.Row(dataStartRow).Style.Font.Bold = true;
                worksheet.Row(dataStartRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(dataStartRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                // Add data
                var designations = departmentService.GetDepartments();
                int row = dataStartRow + 1;
                foreach (var designation in designations)
                {
                   // worksheet.Cell(row, 1).Value = designation.DepartmentCode;
                    //worksheet.Cell(row, 1).Value = designation.DepartmentCode.PadLeft(2, '0');
                    worksheet.Cell(row, 1).Value = "'" + designation.DepartmentCode.PadLeft(2, '0');


                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 2).Value = designation.DepartmentName;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    worksheet.Cell(row, 3).Value = designation.DepartmentShortName;
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 4).Value = designation.BanglaDepartment;
                    worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                // Save to a stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Departments.xlsx");
                }
            }
        }
        #endregion
    }
}