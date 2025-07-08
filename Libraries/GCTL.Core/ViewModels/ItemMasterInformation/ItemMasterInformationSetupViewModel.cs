using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.ItemMasterInformation
{
    public class ItemMasterInformationSetupViewModel :BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string UnitID { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string LUser { get; set; }
        public DateTime? LDate { get; set; }
        public string LIP { get; set; }
        public string LMAC { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string UserInfoEmployeeID { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CatagoryID { get; set; }
        public string BrandID { get; set; }
    }
}
