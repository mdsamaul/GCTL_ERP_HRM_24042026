using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;

namespace GCTL.Service.HrmPayOthersAdjustmentReports
{
    public interface IHrmPayOthersAdjustmentReportService
    {
        Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter);

        Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model);
        
        Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data);


    }
}
