using AutoMapper;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;
using GCTL.Service.Common;
using GCTL.Service.HrmPayDefBenefitTypes;
using GCTL.Service.Nationalitys;
using GCTL.UI.Core.ViewModels.HrmPayDefBenefitTypes;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPayDefBenefitTypeController : BaseController
    {
        private readonly IHrmPayDefBenefitTypeService benefitTypeService;
        private readonly ICommonService commonService;
        private readonly IMapper mapper;
        string strMaxNO = string.Empty;

        public HrmPayDefBenefitTypeController( IHrmPayDefBenefitTypeService benefitTypeService, ICommonService commonService, IMapper mapper)
        {
            this.benefitTypeService = benefitTypeService;
            this.commonService = commonService;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(bool child = false)
        {
            var model = new HrmPayDefBenefitTypePagesViewModel
            {
                PageUrl = Url.Action(nameof(Index)),
            };

            try
            {
                var list = await benefitTypeService.GetAllAsync();
                model.BenefitTypeList = list ?? new List<HrmPayDefBenefitTypeViewModel>();

                commonService.FindMaxNo(ref strMaxNO, "BenefitTypeId", "HRM_Pay_Def_BenefitType", 2);
                model.Setup = new HrmPayDefBenefitTypeViewModel
                {
                    BenefitTypeId = strMaxNO
                };
            }
            catch (Exception ex) 
            {
                model.BenefitTypeList = new List<HrmPayDefBenefitTypeViewModel>();
                model.Setup = new HrmPayDefBenefitTypeViewModel();
                Console.WriteLine("Error"+ex.Message);
            }

            if(child)
                return PartialView(model);

            return View(model);
        }

        public async Task<IActionResult> Setup(string id)
        {
            HrmPayDefBenefitTypeViewModel model = new HrmPayDefBenefitTypeViewModel();
            commonService.FindMaxNo(ref strMaxNO, "BenefitTypeId", "HRM_Pay_Def_BenefitType", 2);

            var result = benefitTypeService.GetBenefitType(id);
            if (result != null)
            {
                model = mapper.Map<HrmPayDefBenefitTypeViewModel>(result);
                model.Code = result.BenefitType;
                model.Id = result.Tc;
            }
            else
            {
                model.BenefitTypeId = strMaxNO;
            }

                //if (!string.IsNullOrEmpty(id))
                //{
                //    model=await benefitTypeService.GetByIdAsync(id);
                //    if(model == null)
                //    {
                //        return NotFound();
                //    }
                //}
                //else
                //{
                //    model.BenefitTypeId = strMaxNO;
                //}

                return PartialView($"_{nameof(Setup)}", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(HrmPayDefBenefitTypeViewModel model)
        {
            if (await benefitTypeService.IsExistAsync(model.BenefitType, model.BenefitTypeId))
            {
                return Json(new { isSuccess = false, message = $"Already Exists!", isDuplicate = true });
            }

            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage });
            }

            model.ToAudit(LoginInfo, model.Tc > 0);
            if (model.Tc == 0)
            {
                await benefitTypeService.SaveAsync(model);
                return Json(new { isSuccess = true, message = "Data Saved Successfully.", lastCode = model.BenefitTypeId });
            }
            else
            {
                await benefitTypeService.UpdateAsync(model);
                return Json(new { isSuccess = true, message = "Data Updated Successfully.", lastCode = model.BenefitTypeId });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckAvailability(string name, string code)
        {
            if (await benefitTypeService.IsExistAsync(name, code))
            {

                return Json(new { isSuccess = true, message = $"Already Exists!" });

            }

            return Json(new { isSuccess = false });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<decimal> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new { success = false, message = "No IDs provided for delete." });
            }

            bool success = await benefitTypeService.DeleteTab(ids);
            if (success)
            {
                return Json(new { success = true, message = "Data Deleted Successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Deletion failed. Some entities may still exists." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTableData()
        {
            try
            {
                var list =await benefitTypeService.GetAllAsync();
                return PartialView("_Grid",list);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}
