using GCTL.Core.Data;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Service.INV_Catagory;
using GCTL.UI.Core.ViewModels.INV_Catagory;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class INV_CatagoryController : BaseController
    {
        private readonly IINV_CatagoryService InvCatagoryService;

        public INV_CatagoryController(
            IINV_CatagoryService InvCatagoryService
            )
        {
            this.InvCatagoryService = InvCatagoryService;
        }
        public IActionResult Index()
        {
            INV_CatagoryViewModel model = new INV_CatagoryViewModel() { 
            PageUrl=Url.Action(nameof(Index))
            };
            return View(model);
        }
              
        [HttpGet]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var data = await InvCatagoryService.GetAllAsync();
                return Json(new { data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AutoCatagoryId()
        {
            try
            {
                var newCategoryId = await InvCatagoryService.AutoCatagoryIdAsync();
                return Json(new { data = newCategoryId });
            }
            catch (Exception)
            {
                throw;
            }
           
        }
        //[HttpPost]
        //public async Task<IActionResult> Create(decimal? id)
        //{
        //    var model = new INV_CatagoryViewModel();

        //    if (id != null && id > 0)
        //    {
        //        var data = await InvCatagoryService.GetByIdAsync(id.Value);
        //        if (data != null)
        //        {
        //            model.Setup = data;
        //        }
        //    }

        //    return View(model); // Create.cshtml
        //}


        [HttpPost]
        public async Task<IActionResult> CreateUpdate([FromBody] INV_CatagorySetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false });
            }
                var result = await InvCatagoryService.CreateUpdateAsync(model);
            return Json(new { isSuccess = result.isSuccess, message = result.message, data = result.data });
        }

        [HttpGet]
        public async Task<IActionResult> PopulatedDataForUpdate(string id)
        {
            var result = await InvCatagoryService.GetByIdAsync(id);
            return Json(new {result});
        }

        //[HttpPost]
        //public async Task<IActionResult> Edit(INV_CatagorySetupViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        await InvCatagoryService.UpdateAsync(model);
        //        return RedirectToAction("Index");
        //    }
        //    return View(model);
        //}

        public async Task<IActionResult> Delete(long id)
        {
            await InvCatagoryService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
