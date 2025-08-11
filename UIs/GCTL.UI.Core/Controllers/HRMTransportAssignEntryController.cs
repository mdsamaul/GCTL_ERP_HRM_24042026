using GCTL.Core.Data;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HRMTransportAssignEntry;
using GCTL.Core.ViewModels.SalesDefVehicleType;
using GCTL.Data.Models;
using GCTL.Service.HRMTransportAssignEntryService;
using GCTL.UI.Core.ViewModels.HRMTransportAssignEntry;
using GCTL.UI.Core.ViewModels.SalesDefVehicleType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.UI.Core.Controllers
{
    public class HRMTransportAssignEntryController : BaseController
    {
        private readonly IHRMTransportAssignEntryService transportAssignService;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<SalesDefVehicle> transportRepo;
        private readonly IRepository<SalesDefVehicleType> transportTypeRepo;

        public HRMTransportAssignEntryController(
            IHRMTransportAssignEntryService transportAssignService,
            IRepository<HrmEmployee> empRepo,
           IRepository<SalesDefVehicle> transportRepo,
           IRepository<SalesDefVehicleType> transportTypeRepo
            )
        {
            this.transportAssignService = transportAssignService;
            this.empRepo = empRepo;
            this.transportRepo = transportRepo;
            this.transportTypeRepo = transportTypeRepo;
        }
        public IActionResult Index()
        {
            ViewBag.EmpList = new SelectList(empRepo.All().Select(x=> new {x.EmployeeId, fullName = x.FirstName +" "+x.LastName }), "EmployeeId", "fullName");
            ViewBag.TransportList = new SelectList(transportRepo.All().Select(x=> new {x.VehicleId, x.VehicleNo}), "VehicleId", "VehicleNo");
            ViewBag.TransportTypeList = new SelectList(transportTypeRepo.All().Select(x=> new {x.VehicleTypeId, x.VehicleType}), "VehicleTypeId", "VehicleType");

            HRMTransportAssignEntryViewModel model = new HRMTransportAssignEntryViewModel()
            {                
                PageUrl = Url.Action(nameof(Index))
            };
           
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> GetEmpDetailsId([FromBody] string Emp)
        {
            var result = await transportAssignService.GetEmpDetailsIdAsync(Emp);
            return Json(new {data = result});
        }

        [HttpGet]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var data = await transportAssignService.GetAllAsync();
                return Json(new { data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AutoId()
        {
            try
            {
                var newCategoryId = await transportAssignService.AutoIdAsync();
                return Json(new { data = newCategoryId });
            }
            catch (Exception)
            {
                throw;
            }

        }


        [HttpPost]
        public async Task<IActionResult> CreateUpdate([FromBody] HRMTransportAssignEntrySetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false });
            }
            try
            {
                model.ToAudit(LoginInfo);
                if (model.AutoId == 0)
                {
                    bool hasParmision = await transportAssignService.SavePermissionAsync(LoginInfo.AccessCode);
                    if (hasParmision)
                    {
                        var result = await transportAssignService.CreateUpdateAsync(model, LoginInfo.CompanyCode, LoginInfo.EmployeeId);
                        return Json(new { isSuccess = result.isSuccess, message = result.message, data = result.data });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access.", noSavePermission = true });
                    }
                }
                else
                {
                    var hasUpdatePermission = await transportAssignService.UpdatePermissionAsync(LoginInfo.AccessCode);
                    if (hasUpdatePermission)
                    {
                        var result = await transportAssignService.CreateUpdateAsync(model, LoginInfo.CompanyCode, LoginInfo.EmployeeId);
                        return Json(new { isSuccess = result.isSuccess, message = result.message, data = result.data });
                    }
                    else
                    {
                        return Json(new { isSuccess = false, message = "You have no access.", noUpdatePermission = true });
                    }
                }
            }
            catch (Exception)
            {

                return Json(new { isSuccess = false, message = "Faild" });
            }


        }

        [HttpGet]
        public async Task<IActionResult> PopulatedDataForUpdate(string id)
        {
            var result = await transportAssignService.GetByIdAsync(id);
            return Json(new { result });
        }

        [HttpPost]
        public async Task<IActionResult> deleteTransport([FromBody] List<decimal> selectedIds)
        {
            var hasUpdatePermission = await transportAssignService.DeletePermissionAsync(LoginInfo.AccessCode);
            if (hasUpdatePermission)
            {
                var result = await transportAssignService.DeleteAsync(selectedIds);
                return Json(new { isSuccess = result.isSuccess, message = result.message, data = result });
            }
            else
            {
                return Json(new { isSuccess = false, message = "You have no access.", noUpdatePermission = true });
            }

        }
        [HttpPost]
        public async Task<IActionResult> alreadyExist([FromBody] string TransportValue)
        {
            var result = await transportAssignService.AlreadyExistAsync(TransportValue);
            return Json(new { isSuccess = result.isSuccess, message = result.message, data = result });
        }
    }
}
