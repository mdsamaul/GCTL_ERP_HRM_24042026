using DocumentFormat.OpenXml.Spreadsheet;
using GCTL.Core.Data;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.HrmAtdMachineData;
//using GCTL.Core.ViewModels.HrmEmployeeOfficialInfo;
using GCTL.Data.Models;
using GCTL.Service.HrmAtdMachineDatas;
using GCTL.UI.Core.ViewModels.HrmAtdMachineData;
//using GCTL.UI.Core.ViewModels.HrmEmployeeOfficialInfo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL.UI.Core.Controllers
{
    public class MachineDataController : BaseController
    {
        private readonly IHrmAtdMachineDataService _hrmAtdMachineDataService;
        private readonly IRepository<HrmAtdMachineData> _hrmAtdMachineDataRepository;

        public MachineDataController(IHrmAtdMachineDataService hrmAtdMachineDataService, IRepository<HrmAtdMachineData> hrmAtdMachineDataRepository)
        {
            _hrmAtdMachineDataService = hrmAtdMachineDataService;
            _hrmAtdMachineDataRepository = hrmAtdMachineDataRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetDataTableData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            // Calculate pagination values
            var pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            var page = string.IsNullOrEmpty(start) ? 1 : (Convert.ToInt32(start) / pageSize) + 1;

            // Fetch data from service
            var result = await _hrmAtdMachineDataService.GetPaginatedDataAsync(searchValue, page, pageSize, sortColumn, sortDirection);

            // Prepare response for DataTables
            var response = new
            {
                draw = draw,
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.TotalRecords,
                data = result.Data
            };

            return Ok(response);
        }
    }
}