using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;
using GCTL.Service.HrmWorkingDayDeclarations;
using GCTL.UI.Core.ViewModels.HrmWorkingDayDeclarations;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmWorkingDayDeclarationController : BaseController
    {
        private readonly IHrmWorkingDayDeclarationService repo;

        public HrmWorkingDayDeclarationController(IHrmWorkingDayDeclarationService repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            var model = new HrmWorkingDayDeclarationPageViewModel
            {
                PageUrl = Url.Action(nameof(Index)),
            };

            try
            {
                model.Setup = new HrmWorkingDayDeclarationViewModel();
            }
            catch (Exception)
            {
                model.Setup = new HrmWorkingDayDeclarationViewModel();
            }
            return View(model);
        }

        public IActionResult Setup()
        {
            return View();
        }

        public async Task<IActionResult> getFilterEmp([FromBody] EmployeeFilterViewModel model)
        {
            var data = await repo.GetFilterDataAsync(model);
            return Json(data);
        }

        public async Task<IActionResult> GetById(decimal id)
        {
            var result = await repo.GetByIdAsync(id);

            return Json(new { data = result });
        }

        public async Task<IActionResult> GetPaginatedData()
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

            var result = await repo.GetPaginatedDataAsync(searchValue, page, pageSize, sortColumn, sortDirection);

            Console.WriteLine(result);

            var resultTest = result;
            var response = new
            {
                draw = draw,
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.TotalRecords,
                Data = result.Data
            };

            return Json(response);
        }

        public async Task<IActionResult> Save([FromBody] HrmWorkingDayDeclarationViewModel model)
        {
            if (model.Tc == 0)
            {
                model.ToAudit(LoginInfo);
                var result = await repo.SaveAsync(model);
                return Json(new
                {
                    success = result.isSuccess,
                    message = result.message,
                });
            }
            else
            {
                model.ToAudit(LoginInfo,true);
                var result = await repo.EditAsync(model);
                return Json(new
                {
                    success = result.isSuccess,
                    message = result.message
                });
            }
        }

        public async Task<IActionResult> BulkDelete([FromBody] HrmWorkingDayDeclarationViewModel model)
        {
            try
            {
                if(model.Tcs == null||!model.Tcs.Any()||model.Tcs.Count==0)
                    return Json(new{ isSuccess = false, message = "No entry is selected to delete"});

                var result = await repo.BulkDeleteAsync(model.Tcs);

                if (!result)
                    return Json(new { isSuccess = false, message = "No found to delete" });

                return Json(new { isSuccess = true, message = $"Deleted {model.Tcs.Count} Entry Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
