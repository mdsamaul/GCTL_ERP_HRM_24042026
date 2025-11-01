using GCTL.Core.ViewModels.MonthWiseOrderBookingReport;

namespace GCTL.Service.MonthWiseOrderBookingReport
{
    public interface IOrderReportService
    {
        //Task<OrderReportResponse> GetOrderReportAsync(OrderReportRequest request);
        //Task<OrderReportResponse> GetOrderReportAsync(OrderReportRequest request);

        //Task<List<OrderReportData>> GetOrderReportAsync(OrderReportRequest request);
        Task<OrderReportAllStyleResponse> GetOrderReportAllStyleAsync(OrderReportRequest request);
        //Task<List<BuyerMaster>> GetBuyersAsync();
        Task<List<StyleMaster>> GetStylesAsync();
        Task<List<ColorMaster>> GetColorsAsync();
        Task<List<SizeMaster>> GetSizesAsync();
    }
}
