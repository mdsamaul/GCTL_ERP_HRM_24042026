using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.ProductStockHistoryReport;

namespace GCTL.Service.ProductStockHistoryReport
{
    public interface IProductStockHistoryReportService
    {
        Task<List<ProductStockHistoryReportSetupViewModel>> GetStockReportAsync(StockReportFilterViewModel filter);
        Task<ProductStockHistoryReportDropdownDto> GetFilteredDropdownAsync(StockReportFilterViewModel model);
    }
}
