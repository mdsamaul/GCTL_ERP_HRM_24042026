using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Category
{
    public class CategorySetupViewModel:BaseViewModel
    {
        public long AutoId { get; set; }
        public string CatagoryID { get; set; }
        public string CatagoryName { get; set; }
        public string ShortName { get; set; }
        public string LUser { get; set; }
        public DateTime? LDate { get; set; }
        public string LIP { get; set; }
        public string LMAC { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string UserInfoEmployeeID { get; set; }
        public string CompanyCode { get; set; }
    }
}
