using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Service.HrmPaySalaryDeductionReports
{
    public interface IHrmPaySalaryDeductionReportService
    {
        Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter);
        Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model);
        Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data);
        byte[] GenerateWordReport(List<ReportFilterResultDto> data);
    }
}
