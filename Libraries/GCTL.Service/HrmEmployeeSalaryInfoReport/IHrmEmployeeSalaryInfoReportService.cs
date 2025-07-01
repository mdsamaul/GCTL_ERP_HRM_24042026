using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmEmployeeSalaryInfoEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmEmployeeSalaryInfoReport
{
    public interface IHrmEmployeeSalaryInfoReportService
    {
        Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter);
        Task<byte[]> GeneratePdfReport(List<ReportFilterResultViewModel> data, BaseViewModel model);
        Task<byte[]> GenerateExcelReport(List<ReportFilterResultViewModel> data);
    }
}
