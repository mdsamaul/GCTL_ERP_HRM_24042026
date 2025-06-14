using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Companies;
using GCTL.Core.ViewModels.HRMPayrollLoan;
using GCTL.Data.Models;
using GCTL.Service.EmployeeWeekendDeclaration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HRMPayrollLoan
{
    public class HRMPayrollLoanService : AppService<HrmPayrollLoan>, IHRMPayrollLoanService
    {
        private readonly IRepository<HrmPayrollLoan> payrollLoanRepo;
        private readonly IRepository<CoreCompany> comRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> dpRepo;
        private readonly IRepository<HrmPayLoanTypeEntry> payTypeRepo;
        private readonly IRepository<HrmPayPayHeadName> payHeadRepo;
        private readonly IRepository<SalesDefBankInfo> bankRepo;
        private readonly IRepository<HrmPayrollPaymentReceive> paymentRepo;

        public IRepository<SalesDefPaymentMode> PayModeRepo { get; }

        public HRMPayrollLoanService(
            IRepository<HrmPayrollLoan> payrollLoanRepo,
            IRepository<CoreCompany> comRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmDefDesignation> desiRepo,
            IRepository<HrmDefDepartment> dpRepo,
            IRepository<HrmPayLoanTypeEntry> payTypeRepo,
            IRepository<SalesDefPaymentMode> PayModeRepo,
            IRepository<HrmPayPayHeadName> payHeadRepo,
            IRepository<SalesDefBankInfo> bankRepo,
            IRepository<HrmPayrollPaymentReceive> paymentRepo
            ):base(payrollLoanRepo)
        {
            this.payrollLoanRepo = payrollLoanRepo;
            this.comRepo = comRepo;
            this.empRepo = empRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.dpRepo = dpRepo;
            this.payTypeRepo = payTypeRepo;
            this.PayModeRepo = PayModeRepo;
            this.payHeadRepo = payHeadRepo;
            this.bankRepo = bankRepo;
            this.paymentRepo = paymentRepo;
        }
        public async Task<PayrollLoanFilterResultListDto> GetFilaterDataAsync(PayrollLoanFilterEntryDto filterEntryDto)
        {
            var queary = from eoi in empOffRepo.All()
                         join e in empRepo.All() on eoi.EmployeeId equals e.EmployeeId into e_join
                         from e in e_join.DefaultIfEmpty()
                         join c in comRepo.All() on eoi.CompanyCode equals c.CompanyCode into c_join
                         from c in c_join.DefaultIfEmpty()
                         join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dg_join
                         from dg in dg_join.DefaultIfEmpty()
                         join dp in dpRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dp_join
                         from dp in dp_join.DefaultIfEmpty()
                         select new
                         {
                             EmpId = e.EmployeeId,
                             CompanyCode = c.CompanyCode,
                             EmpName = e.FirstName + " " + e.LastName,
                             CompaneName = c.CompanyName,
                             DesignationName = dg.DesignationName,
                             DepartmentName = dp.DepartmentName,
                             JoinDate =  eoi.JoiningDate.HasValue? eoi.JoiningDate.Value.ToString("dd/MM/yyyy"):""
                         };
            if(filterEntryDto.CompanyCodes?.Any() == true)
            {
                queary = queary.Where(x => x.CompanyCode != null && filterEntryDto.CompanyCodes.Contains(x.CompanyCode));
            }
            if(filterEntryDto.EmployeeIds?.Any() == true)
            {
                queary = queary.Where(x => x.EmpId != null && x.EmpName != null && filterEntryDto.EmployeeIds.Contains(x.EmpId));
            }
           var result =new PayrollLoanFilterResultListDto
           {
               Company = await queary.Where(x => x.CompanyCode != null && x.CompaneName != null).Select(x => new PayrollLoanFilterResultDto
               {
                   Code = x.CompanyCode,
                   Name = x.CompaneName
               }).Distinct().ToListAsyncSafe(),
               Employees = await queary.Where(x=> x.EmpId != null && x.EmpName != null).Select(x=> new PayrollLoanFilterResultDto
               {
                   Code = x.EmpId, 
                   Name = x.EmpName,
                   DesignationName=x.DesignationName,
                   DepartmentName=x.DepartmentName,
                   joinDate= x.JoinDate,
               }).Distinct().ToListAsyncSafe(),
           };
            return result;
        }
        public async Task<PayrollLoanFilterResultListDto> GetFilterPaymentReceiveAsync(PayrollLoanFilterEntryDto filterEntryDto)
        {
            //var queary = from eoi in empOffRepo.All()
            //             join le in payrollLoanRepo.All() on eoi.EmployeeId equals le.EmployeeId into le_join
            //             from le in le_join.DefaultIfEmpty()
            //             join e in empRepo.All() on eoi.EmployeeId equals e.EmployeeId into e_join
            //             from e in e_join.DefaultIfEmpty()
            //             join c in comRepo.All() on eoi.CompanyCode equals c.CompanyCode into c_join
            //             from c in c_join.DefaultIfEmpty()
            //             join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dg_join
            //             from dg in dg_join.DefaultIfEmpty()
            //             join dp in dpRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dp_join
            //             from dp in dp_join.DefaultIfEmpty()
            var queary = from le in payrollLoanRepo.All()
                         join eoi in empOffRepo.All() on le.EmployeeId equals eoi.EmployeeId into eoi_join
                         from eoi in eoi_join.DefaultIfEmpty()
                         join e in empRepo.All() on eoi.EmployeeId equals e.EmployeeId into e_join
                         from e in e_join.DefaultIfEmpty()
                         join c in comRepo.All() on eoi.CompanyCode equals c.CompanyCode into c_join
                         from c in c_join.DefaultIfEmpty()
                         join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dg_join
                         from dg in dg_join.DefaultIfEmpty()
                         join dp in dpRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dp_join
                         from dp in dp_join.DefaultIfEmpty()


                         select new
                         {
                             EmpId = e.EmployeeId,
                             CompanyCode = c.CompanyCode,
                             EmpName = e.FirstName + " " + e.LastName,
                             CompaneName = c.CompanyName,
                             DesignationName = dg.DesignationName,
                             DepartmentName = dp.DepartmentName,
                             JoinDate = eoi.JoiningDate.HasValue ? eoi.JoiningDate.Value.ToString("dd/MM/yyyy") : ""
                         };
            if (filterEntryDto.CompanyCodes?.Any() == true)
            {
                queary = queary.Where(x => x.CompanyCode != null && filterEntryDto.CompanyCodes.Contains(x.CompanyCode));
            }
            if (filterEntryDto.EmployeeIds?.Any() == true)
            {
                queary = queary.Where(x => x.EmpId != null && x.EmpName != null && filterEntryDto.EmployeeIds.Contains(x.EmpId));
            }
            var result = new PayrollLoanFilterResultListDto
            {
                Company = await queary.Where(x => x.CompanyCode != null && x.CompaneName != null).Select(x => new PayrollLoanFilterResultDto
                {
                    Code = x.CompanyCode,
                    Name = x.CompaneName
                }).Distinct().ToListAsyncSafe(),
                Employees = await queary.Where(x => x.EmpId != null && x.EmpName != null).Select(x => new PayrollLoanFilterResultDto
                {
                    Code = x.EmpId,
                    Name = x.EmpName,
                    DesignationName = x.DesignationName,
                    DepartmentName = x.DepartmentName,
                    joinDate = x.JoinDate,
                }).Distinct().ToListAsyncSafe(),
            };
            return result;
        }

        public async Task<(bool isSuccess, string message, PayrollLoanFilterResultDto)> EmployeeGetById(string empId)
        {
            if (string.IsNullOrWhiteSpace(empId))
            {
                return (false, "Employee not found", null);
            }
            try
            {
                var e = empRepo.GetById(empId);
                if (e == null)
                {
                    return (false, "Employee not found", null);
                }

                var eoi = await empOffRepo.All().FirstOrDefaultAsync(x => x.EmployeeId == e.EmployeeId);
                var c = await comRepo.All().FirstOrDefaultAsync(x => x.CompanyCode == e.CompanyCode);
                var dg = await desiRepo.All().FirstOrDefaultAsync(x => x.DesignationCode == eoi.DesignationCode);
                var dp = await dpRepo.All().FirstOrDefaultAsync(x => x.DepartmentCode == eoi.DepartmentCode);

                var employee = new PayrollLoanFilterResultDto
                {
                    Code = e?.EmployeeId,
                    Name = (e?.FirstName + " " + e?.LastName)?.Trim(),
                    DesignationName = dg?.DesignationName,
                    DepartmentName = dp?.DepartmentName,
                    joinDate = eoi.JoiningDate?.ToString("dd/MM/yyyy") ?? ""
                };
                return (true, "Employee found", employee);

            }
            catch(Exception ex)
            {
                return (true, ex.Message, null);
            }
        }
        public async Task<(bool isSuccess, string message, List<LoanTypeDto> data)> GetLoanTypeAsync()
        {
            var result = await payTypeRepo.GetAllAsync();
           
            var dtoList = result.Select(x => new LoanTypeDto
            {
                LoanTypeId = x.LoanTypeId,
                LoanType= x.LoanType,
                ShortName = x.ShortName,
                CompanyCode= x.CompanyCode
            }).ToList();
            return (true, "Loan Type fetched successfully", dtoList);
        }
        public async Task<List<PaymentModeDto>> getPaymentModeAsync()
        {
            var result = await PayModeRepo.GetAllAsync();
            var ModeList = result.Select(x => new PaymentModeDto
            {
                PaymentModeId = x.PaymentModeId,
                PaymentModeName = x.PaymentModeName,
                PaymentModeShortName = x.PaymentModeShortName
            }).ToList();
            return ModeList;
        }
        public async Task<List<PayHeadNameDto>> GetPayHeadDeductionAsync()
        {
            var result = await payHeadRepo.GetAllAsync();
            var PayHeadList = result.Select(x=> new PayHeadNameDto
            {
                LoanTypeId=x.LoanTypeId,
                Name=x.Name,
                PayHeadNameCode=x.PayHeadNameCode,
                PayHeadNameId=x.PayHeadNameId
            }).ToList();
            return PayHeadList;
        }
        public async Task<List<SalesDefBankInfoDto>> GetBankAsync()
        {
            var banks = await bankRepo.GetAllAsync();
            var bankList = banks.Select(x => new SalesDefBankInfoDto
            {
                BankId = x.BankId,
                BankName = x.BankName,
                AutoId = x.AutoId,
                Ldate = x.Ldate,
                Lip = x.Lip,
                Lmac = x.Lmac,
                Luser = x.Luser,
                ModifyDate = x.ModifyDate,
                ShortName = x.ShortName,
            }).ToList();
            return bankList;
        }
        public async Task<string> createLoanIdAsync()
        {
            var lastLoanId = await payrollLoanRepo.All().OrderByDescending(x=>x.LoanId).Select(x=>x.LoanId).FirstOrDefaultAsync();
            string newLoanId;
            if (!string.IsNullOrEmpty(lastLoanId))
            {
                int lastNumber = int.Parse(lastLoanId.Substring(1));
                newLoanId = "L" + (lastNumber + 1).ToString("D8");
            }
            else
            {
                newLoanId = "L00000001";
            }
            return newLoanId;
        }
        
        public async Task<string> createPaymentReceiveIdAsync()
        {
            var lastPaymentReceiveId = await paymentRepo.All().OrderByDescending(x=>x.PaymentId).Select(x=>x.PaymentId).FirstOrDefaultAsync();
            string newPaymentReceiveId;
            if (!string.IsNullOrEmpty(lastPaymentReceiveId))
            {
                int lastNumber = int.Parse(lastPaymentReceiveId.Substring(1));
                newPaymentReceiveId = (lastNumber + 1).ToString("D8");
            }
            else
            {
                newPaymentReceiveId = "00000001";
            }
            return newPaymentReceiveId;
        }
       
        //create and edit
        public async Task<(bool isSuccess, string message, object data)> CreateEditLoanAsycn(HRMPayrollLoanSetupViewModel modelData)
        {
            if (string.IsNullOrWhiteSpace(modelData.CompanyCode) || string.IsNullOrWhiteSpace(modelData.EmployeeId) || modelData.LoanAmount == null)
            {
                return (false, "Data Invalid", null);
            }
            if(modelData.LoanAmount <= 0)
            {
                return (false, "Data Invalid", null);
            }

            if (modelData.StartDate == null || modelData.EndDate == null)
            {
                return (false, "Start Date or End Date is missing", null);
            }

            DateTime startDate = modelData.StartDate.Value;
            DateTime endDate = modelData.EndDate.Value;

            if (endDate < startDate)
            {
                return (false, "End Date cannot be before Start Date", null);
            }

            // Calculate total months like JavaScript logic
            int yearDiff = endDate.Year - startDate.Year;
            int monthDiff = endDate.Month - startDate.Month;
            int totalMonths = (yearDiff * 12) + monthDiff;

            if (totalMonths < 0)
            {
                return (false, "Invalid date range", null);
            }

            modelData.NoOfInstallment = totalMonths.ToString();

            if (totalMonths == 0)
            {
                modelData.MonthlyDeduction = modelData.LoanAmount;
            }
            else
            {
                decimal loanAmount = modelData.LoanAmount ?? 0;
                decimal monthlyDeduction = Math.Ceiling(loanAmount / totalMonths);
                modelData.MonthlyDeduction = monthlyDeduction;
            }
            if (modelData.AutoId == 0)
            {
                var entity = new HrmPayrollLoan
                {
                    LoanId = modelData.LoanId,
                    EmployeeId = modelData.EmployeeId,
                    LoanDate = modelData.LoanDate,
                    LoanTypeId = modelData.LoanTypeId,
                    StartDate = modelData.StartDate,
                    EndDate = modelData.EndDate,
                    LoanAmount = modelData.LoanAmount,
                    NoOfInstallment = modelData.NoOfInstallment,
                    MonthlyDeduction = modelData.MonthlyDeduction,
                    PayHeadNameId = modelData.PayHeadNameId,
                    PaymentModeId = modelData.PaymentModeId,
                    ChequeNo = modelData.ChequeNo,
                    ChequeDate = modelData.ChequeDate,
                    BankId = modelData.BankId,
                    BankAccount = modelData.BankAccount,
                    Remarks = modelData.Remarks,
                    CompanyCode = modelData.CompanyCode,
                    Luser = modelData.Luser,
                    Lip = modelData.Lip,
                    Lmac = modelData.Lmac,
                    Ldate = modelData.Ldate
                };

                await payrollLoanRepo.AddAsync(entity);

                return (true, "Loan Saved Successfully", modelData);
            }
            else
            {
                var loanData = await payrollLoanRepo.GetByIdAsync(modelData.AutoId);
                if (loanData == null)
                {
                    return (false, "Update Faild", modelData);
                }
                    loanData.LoanId = modelData.LoanId;
                    loanData.EmployeeId = modelData.EmployeeId;
                    loanData.LoanDate = modelData.LoanDate;
                    loanData.LoanTypeId = modelData.LoanTypeId;
                    loanData.StartDate = modelData.StartDate;
                    loanData.EndDate = modelData.EndDate;
                    loanData.LoanAmount = modelData.LoanAmount;
                    loanData.NoOfInstallment = modelData.NoOfInstallment;
                    loanData.MonthlyDeduction = modelData.MonthlyDeduction;
                    loanData.PayHeadNameId = modelData.PayHeadNameId;
                    loanData.PaymentModeId = modelData.PaymentModeId;
                    loanData.ChequeNo = modelData.ChequeNo;
                    loanData.ChequeDate = modelData.ChequeDate;
                    loanData.BankId = modelData.BankId;
                    loanData.BankAccount = modelData.BankAccount;
                    loanData.Remarks = modelData.Remarks;
                    loanData.CompanyCode = modelData.CompanyCode;
                //    loanData.Luser = modelData.Luser;
                //    loanData.Lip = modelData.Lip;
                //    loanData.Lmac = modelData.Lmac;
                //loanData.Ldate = modelData.Ldate;
                loanData.ModifyDate = modelData.Ldate;
                await payrollLoanRepo.UpdateAsync(loanData);
                return (true, "Update Successfully", modelData);
            }
           
        }
        
        public async Task<List<HRMPayrollLoanSetupViewModel>> GetLoanDataAsync()
        {
            var queary = from lon in payrollLoanRepo.All()
                         join eoi in empOffRepo.All() on lon.EmployeeId equals eoi.EmployeeId
                         join e in empRepo.All() on lon.EmployeeId equals e.EmployeeId into eJoin
                         from e in eJoin.DefaultIfEmpty()
                         join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                         from dg in dgJoin.DefaultIfEmpty()
                         join lType in payTypeRepo.All() on lon.LoanTypeId equals lType.LoanTypeId into lTypeJoin
                         from lType in lTypeJoin.DefaultIfEmpty()
                         join pType in PayModeRepo.All() on lon.PaymentModeId equals pType.PaymentModeId into pTypeJoin
                         from pType in pTypeJoin.DefaultIfEmpty()
                         join dp in dpRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                         from dp in dpJoin.DefaultIfEmpty()
                         select new
                         {
                             lon.AutoId,
                             empId = e.EmployeeId ?? "",
                             loanId = lon.LoanId ?? "",
                             loanDate = lon.LoanDate,
                             showLoanDate = lon.LoanDate.HasValue ? lon.LoanDate.Value.ToString("dd/MM/yyyy") : "",
                             loanTypeName = lType.LoanType ?? "",
                             loanTypeId = lType.LoanTypeId ?? "",
                             empName = (e != null ? e.FirstName + " " + e.LastName : ""),
                             desiName = dg.DesignationName ?? "",
                             loanAmount = lon.LoanAmount,
                             startDate = lon.StartDate,
                             startShowDate = lon.StartDate.HasValue ? lon.StartDate.Value.ToString("dd/MM/yyyy") : "",
                             endDate = lon.EndDate,
                             endShowDate = lon.EndDate.HasValue ? lon.EndDate.Value.ToString("dd/MM/yyyy") : "",
                             noOfInstallments = lon.NoOfInstallment ?? "",
                             monthlyDeduction = lon.MonthlyDeduction,
                             paymentModeId = lon.PaymentModeId ?? "",
                             paymentMode = pType.PaymentModeName ?? "",
                             chequeNo = lon.ChequeNo ?? "",
                             chequeDate = lon.ChequeDate,
                             bankId = lon.BankId ?? "",
                             bankAccount = lon.BankAccount ?? "",
                             remarks = lon.Remarks ?? "",
                             companyCode = lon.CompanyCode ?? "",
                             dpName = dp.DepartmentName ?? "",
                             joiningDate = eoi.JoiningDate.HasValue? eoi.JoiningDate.Value.ToString("dd/MM/yyyy"):"",
                             payHeadId = lon.PayHeadNameId ??"",
                             createDate= lon.Ldate.HasValue ? lon.Ldate.Value.ToString("dd/MM/yyyy"):"",
                             updateDate= lon.ModifyDate.HasValue ? lon.ModifyDate.Value.ToString("dd/MM/yyyy"):"",
                         };

            var loanDataList = queary.Select(x => new HRMPayrollLoanSetupViewModel
            {
                AutoId = x.AutoId,
                EmployeeId = x.empId,
                LoanId = x.loanId,
                LoanDate = x.loanDate,
                ShowLoanDate = x.showLoanDate,
                LoanTypeId = x.loanTypeId,
                LoanTypeName = x.loanTypeName,
                EmpName = x.empName,
                DesignationName = x.desiName,
                LoanAmount = x.loanAmount,
                StartDate = x.startDate,
                StartShowDate = x.startShowDate,
                EndDate = x.endDate,
                EndShowDate = x.endShowDate,
                NoOfInstallment = x.noOfInstallments,
                MonthlyDeduction = x.monthlyDeduction,
                PaymentModeId = x.paymentModeId,
                PaymentModeName = x.paymentMode,
                ChequeNo = x.chequeNo,
                ChequeDate = x.chequeDate,
                BankId = x.bankId,
                BankAccount = x.bankAccount,
                Remarks = x.remarks,
                CompanyCode = x.companyCode,
                DepartmentName = x.dpName,
                ShowJoiningDate = x.joiningDate,
                PayHeadNameId = x.payHeadId,
                showCreateDate = x.createDate,
                showModifyDate = x.updateDate,
            }).ToList();

            return loanDataList.OrderBy(x=>x.AutoId).ToList();
        }
        public async Task<(bool isSuccess, string message)> deleteLoanAsync(List<decimal> autoIds)
        {
            foreach (var autoId in autoIds)
            {
                 var loanToDelete = await payrollLoanRepo.GetByIdAsync(autoId);
                    if (loanToDelete == null)
                    {
                        return (false, "Delete Failed");
                    }
                    await payrollLoanRepo.DeleteAsync(loanToDelete);
                }
           
            //var loanToDelete = await payrollLoanRepo.GetByIdAsync(loanId);
            return (true, "Delete Successfully");
        }
        public async Task<HRMPayrollLoanSetupViewModel> getLoanIdAsync(string loanId)
        {
            var LoanData= payrollLoanRepo.All().Where(x => x.LoanId == loanId).FirstOrDefault();
            //var LoanData = await payrollLoanRepo.GetByIdAsync(loanId);
            if (LoanData == null)
                return null;

            // Manual mapping
            var result = new HRMPayrollLoanSetupViewModel
            {
                LoanId = LoanData.LoanId,
                EmployeeId = LoanData.EmployeeId,
                LoanAmount = LoanData.LoanAmount,
                NoOfInstallment = LoanData.NoOfInstallment,
                MonthlyDeduction = LoanData.MonthlyDeduction,
                ShowLoanDate = LoanData.LoanDate.HasValue? LoanData.LoanDate.Value.ToString("dd/MM/yyyy"):"",
                LoanTypeName= payTypeRepo.All().Where(x=> x.LoanTypeId ==LoanData.LoanTypeId).Select(x=> x.LoanType).FirstOrDefault()?.ToString(),
                StartShowDate= LoanData.StartDate.HasValue? LoanData.StartDate.Value.ToString("dd/MM/yyyy"):"",
                EndShowDate = LoanData.EndDate.HasValue? LoanData.EndDate.Value.ToString("dd/MM/yyyy"):"",
            };
            return result;
        }


    }
}
