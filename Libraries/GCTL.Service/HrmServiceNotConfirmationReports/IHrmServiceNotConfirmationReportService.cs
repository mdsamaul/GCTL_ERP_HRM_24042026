using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceNotConfirmationReports
{
    public interface IHrmServiceNotConfirmationReportService
    {
        Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter);
        Task<byte[]> GeneratePdfReport(List<ReportFilterResultViewModel> data, BaseViewModel model);
        Task<byte[]> GenerateExcelReport(List<ReportFilterResultViewModel> data);
    }
}
