using GCTL.Core.Data;
using GCTL.Core.ViewModels.Dosages;
using GCTL.Core.ViewModels.HrmAtdMachineData;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.HrmAtdMachineDatas
{
    public class HrmAtdMachineDataService : AppService<HrmAtdMachineData>, IHrmAtdMachineDataService
    {
        private readonly IRepository<HrmAtdMachineData> _hrmAtdMachineDataRepository;

        public HrmAtdMachineDataService(IRepository<HrmAtdMachineData> hrmAtdMachineDataRepository) : base(hrmAtdMachineDataRepository)
        {
            _hrmAtdMachineDataRepository = hrmAtdMachineDataRepository;
        }

        public async Task<(List<HrmAtdMachineData> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = _hrmAtdMachineDataRepository.All().AsQueryable();

            // Apply global search
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(d =>
                    d.FingerPrintId.Contains(searchValue)
                    //|| d.MachineId.Contains(searchValue)
                    || d.Date.ToString().Contains(searchValue)
                    /*|| d.Time.ToString().Contains(searchValue)*/);
            }

            // Get total record count after filtering
            var totalRecords = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                query = sortColumn switch
                {
                    "fingerPrintId" => sortDirection == "asc" ? query.OrderBy(d => d.FingerPrintId) : query.OrderByDescending(d => d.FingerPrintId),
                    "machineId" => sortDirection == "asc" ? query.OrderBy(d => d.MachineId) : query.OrderByDescending(d => d.MachineId),
                    "date" => sortDirection == "asc" ? query.OrderBy(d => d.Date) : query.OrderByDescending(d => d.Date),
                    "time" => sortDirection == "asc" ? query.OrderBy(d => d.Time) : query.OrderByDescending(d => d.Time),
                    _ => query.OrderBy(d => d.AutoId), // Default sorting
                };
            }

            // Apply pagination
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}
