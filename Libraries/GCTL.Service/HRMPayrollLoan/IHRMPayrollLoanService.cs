using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HRMPayrollLoan;
using Org.BouncyCastle.Ocsp;

namespace GCTL.Service.HRMPayrollLoan
{
    public interface IHRMPayrollLoanService
    {
        Task<PayrollLoanFilterResultListDto> GetFilaterDataAsync(PayrollLoanFilterEntryDto filterEntryDto);
        Task<PayrollLoanFilterResultListDto> GetFilterPaymentReceiveAsync(PayrollLoanFilterEntryDto filterReceiveEntryDto);
        Task<List<HrmPayrollPaymentReceiveDto>> GetPaymentReceiveAsync();
        Task<(bool isSuccess, string message , PayrollLoanFilterResultDto)> EmployeeGetById(string empId);
        Task<(bool isSuccess, string message, HrmPayrollPaymentReceiveListDto)> PaymentReciveEmployeeGetById(string empId);
        Task<(bool isSuccess, string message, List<LoanTypeDto> data)> GetLoanTypeAsync();
        Task<List<PaymentModeDto>> getPaymentModeAsync();
        Task<List<PayHeadNameDto>> GetPayHeadDeductionAsync();
        Task<string> createLoanIdAsync();
        Task<string> createPaymentReceiveIdAsync();
        Task<List<SalesDefBankInfoDto>> GetBankAsync();
        Task<(bool isSuccess, string message, object data)> CreateEditLoanAsycn(HRMPayrollLoanSetupViewModel modelData);
        Task<List<HRMPayrollLoanSetupViewModel>> GetLoanDataAsync();
        Task<(bool isSuccess, string message)> deleteLoanAsync(List<decimal> autoIds);
        Task<HRMPayrollLoanSetupViewModel> getLoanIdAsync(string loanId);
    }
}
