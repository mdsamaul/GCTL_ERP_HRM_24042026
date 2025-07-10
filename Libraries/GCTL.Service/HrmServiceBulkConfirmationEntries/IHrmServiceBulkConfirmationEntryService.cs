using GCTL.Core.ViewModels.HrmServiceBulkConfimationEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceBulkConfirmationEntries
{
    public interface IHrmServiceBulkConfirmationEntryService
    {
        Task<bool> BulkDeleteAsync(List<decimal> autoIds);
        Task<bool> BulkEditAsync(HrmServiceBulkConfirmationViewModel model);
        Task<EmployeeFilterResultViewModel> GetFilterEmployeeAsync(EmployeeFilterViewModel model);
    }
}
