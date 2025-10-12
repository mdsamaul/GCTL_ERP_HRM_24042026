using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.RMGProdOrderInformationEntry;

namespace GCTL.UI.Core.ViewModels.RMGProdOrderInformationEntry
{
    public class RMGProdOrderInformationEntryViewModel : BaseViewModel
    {
        public RMG_Prod_OrderDto OrderDto { get; set; } = new RMG_Prod_OrderDto();
        public RMG_BookingOrderDto BookingOrderDto { get; set; } = new RMG_BookingOrderDto();
    }
}
