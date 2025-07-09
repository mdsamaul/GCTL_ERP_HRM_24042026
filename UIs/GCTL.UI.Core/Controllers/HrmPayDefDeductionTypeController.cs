             using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using GCTL.Service.HrmPayDefDeductionTypes;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.Helpers;
using AutoMapper;
//using GCTL.Core.ViewModels.Religions;
using GCTL.Data.Models;
using GCTL.Service.Common;
//using GCTL.Service.Religions;
//using GCTL.UI.Core.ViewModels.Religions;
using GCTL.UI.Core.ViewModels.HrmPayDeductionTypes;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPayDefDeductionTypeController : BaseController
    {
        private readonly IHrmPayDefDeductionTypeService deductionTypeService;
        private readonly ICommonService commonService;
        private readonly IMapper mapper;
        string strMaxNO = "";
        public HrmPayDefDeductionTypeController(IHrmPayDefDeductionTypeService deductionTypeService,
                                   ICommonService commonService,
                                   IMapper mapper)
        {
            this.deductionTypeService = deductionTypeService;
            this.commonService = commonService;
            this.mapper = mapper;
        }

        public IActionResult Index(bool child = false)
        {
            HrmPayDeductionTypePageViewModel model = new HrmPayDeductionTypePageViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            commonService.FindMaxNo(ref strMaxNO, "DeductionTypeId", "HRM_Pay_Def_DeductionType", 1);
            model.Setup = new DeductionTypeSetupViewModel
            {
                DeductionTypeId = int.Parse(strMaxNO).ToString("D2")
            };
            if (child)
                return PartialView(model);

            return View(model);
        }


        public IActionResult Setup(decimal id)
        {
            DeductionTypeSetupViewModel model = new DeductionTypeSetupViewModel();
            commonService.FindMaxNo(ref strMaxNO, "DeductionTypeId", "HRM_Pay_Def_DeductionType", 1);
            var result = deductionTypeService.GetDeductionType(id);
            if (result != null)
            {
                model = mapper.Map<DeductionTypeSetupViewModel>(result);
                model.Code = result.DeductionTypeId;
                model.Id = (decimal)result.Tc;
            }
            else
            {
                model.DeductionTypeId = strMaxNO;
            }

            return PartialView($"_{nameof(Setup)}", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Setup(DeductionTypeSetupViewModel model)
        {
            if (deductionTypeService.IsDeductionTypeExist(model.DeductionType, model.DeductionTypeId))
            {
                return Json(new { isSuccess = false, message = "Already Exists" });
            }

            if (ModelState.IsValid)
            {
                if (deductionTypeService.IsDeductionTypeExistByCode(model.DeductionTypeId))
                {
                    //var hasPermission = deductionTypeService.UpdatePermission(LoginInfo.AccessCode);
                    //if (hasPermission)
                    //{
                    HrmPayDefDeductionType type = deductionTypeService.GetDeductionType(model.Id) ?? new HrmPayDefDeductionType();
                        model.ToAudit(LoginInfo, model.Id > 0);
                        mapper.Map(model, type);
                        deductionTypeService.SaveDeductionType(type);
                        return Json(new { isSuccess = true, message = "Update Successfully", lastCode = type.DeductionTypeId });
                    //}
                    //else
                    //{

                        //return Json(new { isSuccess = false, message = "You have no access" });
                    //}

                }
                else
                {
                    //var hasPermission = deductionTypeService.SavePermission(LoginInfo.AccessCode);
                    //if (hasPermission)
                    //{
                    HrmPayDefDeductionType type = deductionTypeService.GetDeductionType(model.Id) ?? new HrmPayDefDeductionType();
                        model.ToAudit(LoginInfo, model.Id > 0);
                        mapper.Map(model, type);
                        deductionTypeService.SaveDeductionType(type);
                        return Json(new { isSuccess = true, message = "Saved Successfully", lastCode = type.DeductionTypeId });
                    //}
                    //else
                    //{
                    //
                    //    return Json(new { isSuccess = false, message = "You have no access" });
                    //}
                }

            }

            return Json(new { success = false, message = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage });
        }

        public ActionResult Grid()
        {
            var resutl = deductionTypeService.GetDeductionTypes();
            return Json(new { data = resutl });
        }


        [HttpPost]
        public ActionResult Delete(string id)
        {
            //var hasPermission = deductionTypeService.DeletePermission(LoginInfo.AccessCode);
            //if (hasPermission)
            //{
            bool success = false;
            foreach (var item in id.Split(",", StringSplitOptions.RemoveEmptyEntries))
            {
                if (decimal.TryParse(item, out var decimalId))
                {
                    success = deductionTypeService.DeleteDeductionType(decimalId);
                }
                else
                {
                    return Json(new { success = false, message = $"Invalid ID: {item}" });
                }
            }

            return Json(new { success = success, message = "Deleted Successfully" });
            //}
            //else
            //{
            //    return Json(new { isSuccess = false, message = "You have no access" });
            //}

        }


        [HttpPost]
        public JsonResult CheckAvailability(string name, string code)
        {
            if (deductionTypeService.IsDeductionTypeExist(name, code))
            {
                return Json(new { isSuccess = true, message = "Already Exists" });
            }

            return Json(new { isSuccess = false });
        }

        #region My Type
        //private readonly IHrmPayDefDeductionTypeService deductionTypeService;

        //public HrmPayDefDeductionTypeController(IHrmPayDefDeductionTypeService deductionTypeService)
        //{
        //    this.deductionTypeService = deductionTypeService;
        //}

        //public IActionResult Index()
        //{
        //    return View();
        //}

        //public async Task<ActionResult> GenerateNewid()
        //{
        //    var newId = await deductionTypeService.GenarateDeductionTypeIdAsync();
        //    return Json(newId);
        //}

        //public async Task<IActionResult> GetById(decimal id)
        //{
        //    var result = await deductionTypeService.GetByIdAsync(id);

        //    return Json(new { data = result });
        //}

        //public async Task<IActionResult> GetSalaryDeductionType()
        //{
        //    var result = await deductionTypeService.GetAllAsync();

        //    return Json(new { data = result });
        //}

        //public async Task<IActionResult> SaveSalaryDeduction([FromBody] HrmPayDefDeductionTypeViewModel model)
        //{
        //    if (model.Tc == 0)
        //    {
        //        model.ToAudit(LoginInfo);
        //        var result = await deductionTypeService.SaveAsync(model);
        //        return Json(new
        //        {
        //            success = result,
        //            message = "Saved Successfully"
        //        });
        //    }
        //    else
        //    {
        //        model.ToAudit(LoginInfo, true);
        //        var result = await deductionTypeService.EditAsync(model);
        //        return Json(new
        //        {
        //            success = result,
        //            message = "Updated Successfully"
        //        });
        //    }

        //}

        //public async Task<ActionResult> BulkDelete(List<decimal> ids)
        //{
        //    try
        //    {
        //        if (ids == null || !ids.Any() || ids.Count == 0)
        //        {
        //            return Json(new { isSuccess = false, message = "No salary deduction is selected to delete" });
        //        }

        //        var result = await deductionTypeService.BulkDeleteAsync(ids);

        //        if (!result)
        //        {
        //            return Json(new { isSuccess = false, message = "No salary deduction type found to delete" });
        //        }
        //        return Json(new { isSuccess = true, message = $"Deleted {ids.Count} salary deduction type Successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        #endregion
    }
}
