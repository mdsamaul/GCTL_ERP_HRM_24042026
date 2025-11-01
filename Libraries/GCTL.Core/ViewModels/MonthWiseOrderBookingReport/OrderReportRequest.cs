namespace GCTL.Core.ViewModels.MonthWiseOrderBookingReport
{
    //public class OrderReportRequest
    //{
    //    public DateTime? FromDate { get; set; }
    //    public DateTime? ToDate { get; set; }
    //    public int? FromYear { get; set; }
    //    public int? ToYear { get; set; }
    //    public List<string> BuyerIds { get; set; }
    //    public List<string> StyleIds { get; set; }
    //    public List<string> PurchaseOrders { get; set; }
    //    public List<string> ColorIds { get; set; }
    //    public List<string> SizeIds { get; set; }
    //    public int PageNumber { get; set; } = 1;
    //    public int PageSize { get; set; } = 10;
    //}
    public class OrderReportRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        public List<string> BuyerIds { get; set; }
        public List<string> StyleIds { get; set; }
        public List<string> PurchaseOrders { get; set; }
        public List<string> ColorIds { get; set; }
        public List<string> SizeIds { get; set; }
    }
}
