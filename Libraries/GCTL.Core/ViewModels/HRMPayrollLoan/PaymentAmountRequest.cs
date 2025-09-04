using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HRMPayrollLoan
{
    public class PaymentAmountRequest
    {
        public string AmountValue { get; set; }
        public string LoanId { get; set; }
        public string LoanAmount { get; set; }
    }
}
