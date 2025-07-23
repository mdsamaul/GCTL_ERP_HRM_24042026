using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.ProductIssueEntry
{
    public class ProductIssueEntrySetupViewModel : BaseViewModel
    {
        public decimal TC { get; set; }
        public string? MainCompanyCode { get; set; }
        public string IssueNo { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public string? DepartmentCode { get; set; }
        public string? EmployeeID { get; set; }
        public string? IssuedBy { get; set; }
        public string? Remarks { get; set; }
        public string? LUser { get; set; }
        public DateTime? LDate { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string UserInfoEmployeeID { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string? FloorCode { get; set; }

        // Details list: INV_ProductIssueInformationDetails
        public List<ProductIssueInformationDetailViewModel> Details { get; set; } = new();
    }
}
