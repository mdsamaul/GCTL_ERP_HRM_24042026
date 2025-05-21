using GCTL.Core.ViewModels.Dosages;
using GCTL.Core.ViewModels.HrmAtdMachineData;
using GCTL.Core.ViewModels.HrmAtdShifts;
using GCTL.Data.Models;

namespace GCTL.Service.HrmAtdMachineDatas
{
    public interface IHrmAtdMachineDataService
    {
        Task<(List<HrmAtdMachineData> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection);
    }
}
