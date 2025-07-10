using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.ItemModel
{
    public class ItemModelSetupViewModel:BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string CompanyCode { get; set; }
        public string CatagoryName { get; set; }
        public string BrandName { get; set; }
    }
}
