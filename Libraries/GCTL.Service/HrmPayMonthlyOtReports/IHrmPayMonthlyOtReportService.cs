using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmPayMonthlyOtReports
{
    public interface IHrmPayMonthlyOtReportService
    {
        Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter);
        Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model);
        Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data);
    }
}
