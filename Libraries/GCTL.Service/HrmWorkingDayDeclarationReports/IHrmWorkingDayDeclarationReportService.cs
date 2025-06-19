using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmWorkingDayDeclarationReports
{
    public interface IHrmWorkingDayDeclarationReportService
    {
        Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter);

        Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model, bool isDate = false);

        Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data, bool isDate = true);

    }
}
