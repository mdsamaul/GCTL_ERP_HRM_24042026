using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmPayMonthlyOtEntries
{
    public interface IHrmPayMothlyOtEntryService
    {
        Task<HrmPayMonthlyOtEntryViewModel> GetByIdAsync(decimal id);
    }
}
