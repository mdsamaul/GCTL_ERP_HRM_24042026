using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.AdvanceLoanAdjustmentReport;

namespace GCTL.Service.AdvanceLoanAdjustmentReport
{
    public interface IAdvanceLoanAdjustmentReportServices
    {
        Task<List<AdvanceLoanAdjustmentReportSetupViewModel>> GetAdvancePayReportAsync(HrmAdvancePayReportFilter filter);
        Task<List<DepartmentGroupedData>> GetAdvancePayReportGroupedAsync(HrmAdvancePayReportFilter filter);
        Task<AdvanceLoanFilterData> GetAdvancePayFiltersAsync(HrmAdvancePayReportFilter filter);

    }
}
