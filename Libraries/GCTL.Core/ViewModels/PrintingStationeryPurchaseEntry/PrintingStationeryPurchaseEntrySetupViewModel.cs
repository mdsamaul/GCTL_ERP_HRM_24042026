using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PrintingStationeryPurchaseEntry
{
    public class PrintingStationeryPurchaseEntrySetupViewModel : BaseViewModel
    {
        public decimal TC { get; set; }
        public string? MainCompanyCode { get; set; }
        public string PurchaseReceiveNo { get; set; } = string.Empty;
        public DateTime? ReceiveDate { get; set; }
        public string? DepartmentCode { get; set; }
        public string? SupplierID { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal? InvoiceValue { get; set; }
        public string? ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string? EmployeeID_ReceiveBy { get; set; }
        public string? Remarks { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? LUser { get; set; }
        public DateTime? LDate { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string UserInfoEmployeeID { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string ShowCreateDate { get; set; }
        public string ShowModifyDate { get; set; }
    }
}
