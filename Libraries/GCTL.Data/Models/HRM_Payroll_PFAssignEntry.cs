using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Data.Models
{
    public class HRM_Payroll_PFAssignEntry
    {
        public decimal AutoId { get; set; } 
        public string PFAssignID { get; set; }
        public string EmployeeId { get; set; }
        public string PFApprovedStatus { get; set; }
        public string ApprovalRemark { get; set; }
        public DateTime EFDate { get; set; }
        public string LUser { get; set; }
        public DateTime? LDate { get; set; }  
        public string LIP { get; set; }
        public string LMAC { get; set; }
        public DateTime? ModifyDate { get; set; }  
        public string CompanyCode { get; set; }
    }
}
