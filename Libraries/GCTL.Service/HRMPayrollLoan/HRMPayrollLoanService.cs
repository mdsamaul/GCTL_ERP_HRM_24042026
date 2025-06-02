using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HRMPayrollLoan;
using GCTL.Data.Models;
using GCTL.Service.EmployeeWeekendDeclaration;
using Microsoft.EntityFrameworkCore;

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
            IRepository<HrmPayPayHeadName> payHeadRepo
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

        public async Task<string> createLoanIdAsync()
        {
            var lastLoanId = await payrollLoanRepo.All().OrderByDescending(x=>x.LoanId).Select(x=>x.LoanId).FirstOrDefaultAsync();
            string newLoanId;
            if (!string.IsNullOrEmpty(lastLoanId))
            {
                int lastNumber = int.Parse(lastLoanId.Substring(1));
                newLoanId = "L" + (lastNumber + 1).ToString("D6");
            }
            else
            {
                newLoanId = "L000001";
            }
            return newLoanId;
        }
    }
}
