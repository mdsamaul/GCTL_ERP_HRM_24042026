using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmEmployeePPAlertReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmEmployeePPAlertReports
{
    public interface IHrmEmployeePPAlertReportService
    {
        Task<EmployeeFilterResultViewModel> GetDataAsync(EmployeeFilterViewModel filter);
        Task<byte[]> GeneratePdfReport(List<HrmEmployeePPAlertReportViewModel> data, BaseViewModel model);
        Task<byte[]> GenerateExcelReport(List<HrmEmployeePPAlertReportViewModel> data);
    }
}
