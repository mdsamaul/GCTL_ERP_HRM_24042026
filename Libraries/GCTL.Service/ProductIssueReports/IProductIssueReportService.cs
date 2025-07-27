using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.ProductIssueReport;

namespace GCTL.Service.ProductIssueReports
{
    public interface IProductIssueReportService
    {
        Task<List<ProductIssueReportSetupViewModel>> GetProductIssueReportAsync(ProductIssueReportFilterViewModel filter);
        Task<ProductIssueDropdownDto> GetProductIssueDropdownAsync(ProductIssueReportFilterViewModel model);
    }
}
