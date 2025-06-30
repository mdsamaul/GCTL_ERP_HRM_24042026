using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.EmployeeLoanInformationReport;
using GCTL.Core.ViewModels.HRMPayrollLoan;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GCTL.Service.EmployeeLoanInformationReport
{
    public class EmployeeLoanInformationReportServices:AppService<HrmPayrollLoan>, IEmployeeLoanInformationReportServices
    {
        private readonly IRepository<HrmPayrollLoan> ploanRepo;
        private readonly IRepository<HrmPayrollLoan> payrollLoanRepo;
        private readonly IRepository<CoreCompany> comRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmPayrollPaymentReceive> paymentRepo;
        private readonly IConfiguration configuration;

        public EmployeeLoanInformationReportServices(
            IRepository<HrmPayrollLoan> ploanRepo,
            IRepository<HrmPayrollLoan> payrollLoanRepo,
            IRepository<CoreCompany> comRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmPayrollPaymentReceive> paymentRepo,
            IConfiguration configuration
            ):base(ploanRepo)
        {
            this.ploanRepo = ploanRepo;
            this.payrollLoanRepo = payrollLoanRepo;
            this.comRepo = comRepo;
            this.empRepo = empRepo;
            this.empOffRepo = empOffRepo;
            this.paymentRepo = paymentRepo;
            this.configuration = configuration;
        }

        public async Task<List<EmployeeLoanInformationReportVM>> GetLoanDetailsByEmployeeIdAsync(string employeeId)
        {
            try
            {
                var result = new List<EmployeeLoanInformationReportVM>();

                using var conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection"));
                await conn.OpenAsync();

                using var cmd = new SqlCommand("EmployeeLoanInformationReport", conn);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId ?? "");

                using var reader = await cmd.ExecuteReaderAsync();

                var loandDict = new Dictionary<string, EmployeeLoanInformationReportVM>();

                while (await reader.ReadAsync())
                {
                    string loanId = reader.IsDBNull(reader.GetOrdinal("LoanID")) ? null : reader.GetString(reader.GetOrdinal("LoanID"));


                    if (!loandDict.ContainsKey(loanId))
                    {
                        var loanVm = new EmployeeLoanInformationReportVM
                        {
                            LoanID = loanId,
                            EmployeeID = reader.GetString(reader.GetOrdinal("EmployeeID")),
                            FullName = reader.GetString(reader.GetOrdinal("FullName")),
                            DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                            DesignationName = reader.GetString(reader.GetOrdinal("DesignationName")),
                            Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? "" : reader.GetString(reader.GetOrdinal("Reason")),
                            TotalLoans = reader.GetInt32(reader.GetOrdinal("TotalLoans")),
                            LoanAmount = reader.GetDecimal(reader.GetOrdinal("LoanAmount")),
                            PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            InstallmentDetails = reader.IsDBNull(reader.GetOrdinal("Installment Details")) ? "" : reader.GetString(reader.GetOrdinal("Installment Details")),
                            LoanRepaymentMethod = reader.GetString(reader.GetOrdinal("LoanRepaymentMethod")),
                            Remarks = reader.GetString(reader.GetOrdinal("Remarks")),
                            Installments = new List<InstallmentVM>()
                        };

                        loandDict.Add(loanId, loanVm);
                    }
                    var installment = new InstallmentVM
                    {
                        InstallmentNo = reader.GetInt32(reader.GetOrdinal("InstallmentNo")),
                        InstallmentDate = reader.IsDBNull(reader.GetOrdinal("InstallmentDate"))
    ? ""
    : DateTime.Parse(reader.GetString(reader.GetOrdinal("InstallmentDate"))).ToString("dd/MM/yyyy"),
                        PaymentMode = reader.IsDBNull(reader.GetOrdinal("PaymentMode")) ? "" : reader.GetString(reader.GetOrdinal("PaymentMode")),
                        Deposit = reader.GetDecimal(reader.GetOrdinal("Deposit")),
                        OutstandingBalance = reader.GetDecimal(reader.GetOrdinal("Outstanding Balance"))
                    };
                    loandDict[loanId].Installments.Add(installment);
                }
                return loandDict.Values.ToList();
            }
            catch (Exception e)
            {
                throw;
            }
       }
        public async Task<List<EmployeeBasicInfoVM>> GetDistinctLoanEmployeesAsync()
        {
            try
            {
                var result = new List<EmployeeBasicInfoVM>();

                using var conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection"));
                await conn.OpenAsync();

                string query = @"
            SELECT DISTINCT 
                em.EmployeeID, 
                em.FirstName + ' ' + em.LastName AS FullName
            FROM HRM_Payroll_Loan pl
            INNER JOIN HRM_Employee em ON em.EmployeeID = pl.EmployeeID
        ";

                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var employee = new EmployeeBasicInfoVM
                    {
                        EmployeeID = reader.GetString(reader.GetOrdinal("EmployeeID")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName"))
                    };
                    result.Add(employee);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw; 
            }
        }


    }
}
