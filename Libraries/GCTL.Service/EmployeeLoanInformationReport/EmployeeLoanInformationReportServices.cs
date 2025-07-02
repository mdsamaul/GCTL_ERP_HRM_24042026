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
        public async Task<EmployeeLoanReportResponseVM> GetLoanDetailsAsync(LoanFilterVM filter)
        {
            try
            {
                var response = new EmployeeLoanReportResponseVM
                {
                    LoanReports = new List<EmployeeLoanInformationReportVM>(),
                    Companies = new List<CompanyBasicInfoVM>(),
                    Employees = new List<EmployeeBasicInfoVM>(),
                    LoanIDs = new List<string>()
                };

                using var conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection"));
                await conn.OpenAsync();

                using var cmd = new SqlCommand("EmployeeLoanInformationReport", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CompanyID", (object)filter.CompanyID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeID", (object)filter.EmployeeID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LoanID", (object)filter.LoanID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateFrom", (object)filter.DateFrom ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateTo", (object)filter.DateTo ?? DBNull.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                var loanDict = new Dictionary<string, EmployeeLoanInformationReportVM>();
                var companySet = new Dictionary<string, CompanyBasicInfoVM>();
                var employeeSet = new HashSet<string>();
                var loanIdSet = new HashSet<string>();

                while (await reader.ReadAsync())
                {
                    string loanId = reader["LoanID"].ToString();
                    string empId = reader["EmployeeID"].ToString();
                    string fullName = reader["FullName"].ToString();
                    string CompanyCode = reader["CompanyCode"].ToString();
                    string companyName = reader["CompanyName"].ToString();                   
                    
                    if (!companySet.ContainsKey(companyName))
                    {
                        companySet.Add(companyName, new CompanyBasicInfoVM
                        {
                            CompanyCode = CompanyCode,
                            CompanyName = companyName
                        });
                    }

                    if (!employeeSet.Contains(empId))
                        {
                            employeeSet.Add(empId);
                            response.Employees.Add(new EmployeeBasicInfoVM
                            {
                                EmployeeID = empId,
                                FullName = fullName,
                                DepartmentName = reader["DepartmentName"].ToString(),
                                DesignationName = reader["DesignationName"].ToString()
                            });
                        }
                        if (!loanIdSet.Contains(loanId))
                        {
                            loanIdSet.Add(loanId);
                        }
                        if (!loanDict.ContainsKey(loanId))
                        {
                            var loanVm = new EmployeeLoanInformationReportVM
                            {
                                LoanID = loanId,
                                EmployeeID = empId,
                                FullName = fullName,
                                DepartmentName = reader["DepartmentName"].ToString(),
                                DesignationName = reader["DesignationName"].ToString(),
                                Reason = reader["Reason"]?.ToString() ?? "",
                                Remarks = reader["Remarks"]?.ToString() ?? "",
                                TotalLoans = Convert.ToDecimal(reader["TotalLoans"]),
                                LoanAmount = Convert.ToDecimal(reader["LoanAmount"]),
                                //PaymentMode = reader["LoanRepaymentMethod"].ToString(),
                                CompanyName = companyName,
                                StartDate = Convert.ToDateTime(reader["StartDate"]?.ToString() ?? ""),
                                EndDate = Convert.ToDateTime(reader["EndDate"]?.ToString() ?? ""),
                                InstallmentDetails = reader["Installment Details"]?.ToString(),
                                LoanRepaymentMethod = reader["LoanRepaymentMethod"].ToString() ?? "",
                                Installments = new List<InstallmentVM>()
                            };
                            loanDict.Add(loanId, loanVm);
                        }
                        var installment = new InstallmentVM
                        {
                            InstallmentNo = Convert.ToInt32(reader["InstallmentNo"]),                           
                            InstallmentDate = reader.IsDBNull(reader.GetOrdinal("InstallmentDate"))? "": reader.GetString(reader.GetOrdinal("InstallmentDate")).ToString(),
                            PaymentMode = reader["PaymentMode"].ToString() ?? "",
                            Deposit = Convert.ToDecimal(reader["Deposit"]),
                            OutstandingBalance = Convert.ToDecimal(reader["Outstanding Balance"])
                        };
                    loanDict[loanId].Installments.Add(installment);                  

                }

                response.LoanReports = loanDict.Values.ToList();
                    response.Companies = companySet.Values.ToList();
                    response.LoanIDs = loanIdSet.ToList();
                    return response;
                

            }catch(Exception e)
            {
                throw e;
            }
        }
    }
}
