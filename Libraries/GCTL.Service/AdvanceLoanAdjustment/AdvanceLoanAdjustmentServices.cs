using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.AdvanceLoanAdjustment;
using GCTL.Core.ViewModels.Companies;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GCTL.Service.AdvanceLoanAdjustment
{
    public class AdvanceLoanAdjustmentServices : AppService<HrmPayAdvancePay>, IAdvanceLoanAdjustmentServices
    {
        private readonly IRepository<HrmPayAdvancePay> advancePayRepo;
        private readonly IConfiguration configuration;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmPayrollLoan> payrollLoanRepo;
        private readonly IRepository<HrmPayLoanTypeEntry> loanTypeRepo;
        private readonly IRepository<HrmPayPayHeadName> payHeadRepo;

        public AdvanceLoanAdjustmentServices(
            IRepository<HrmPayAdvancePay> advancePayRepo,
            IConfiguration configuration,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmPayrollLoan> payrollLoanRepo,
            IRepository<HrmPayLoanTypeEntry> LoanTypeRepo,
            IRepository<HrmPayPayHeadName> payHeadRepo
        ) : base(advancePayRepo)
        {
            this.advancePayRepo = advancePayRepo;
            this.configuration = configuration;
            this.empRepo = empRepo;
            this.payrollLoanRepo = payrollLoanRepo;
            this.loanTypeRepo = LoanTypeRepo;
            this.payHeadRepo = payHeadRepo;
        }

        public async Task<List<CompanyDto>> GetAllAndFilterCompanyAsync(string searchCompanyName)
        {
            List<CompanyDto> companies = new List<CompanyDto>();

            using (SqlConnection conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection")))
            {
                await conn.OpenAsync(); // Async connection open

                using (SqlCommand cmd = new SqlCommand("GetCompanyNamesBySearch", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchCompanyName", searchCompanyName ?? "");

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) // Async reader
                    {
                        while (await reader.ReadAsync()) // Async read loop
                        {
                            companies.Add(new CompanyDto
                            {
                                companyCode = reader["CompanyCode"].ToString(),
                                companyName = reader["CompanyName"].ToString()
                            });
                        }
                    }
                }
            }

            return companies;
        }
        public async Task<List<EmployeeAdjustmentDto>> GetEmployeesByFilterAsync(string employeeStatusId, string companyCode, string employeeName)
        {
            List<EmployeeAdjustmentDto> employees = new List<EmployeeAdjustmentDto>();

            try
            {
                using (SqlConnection conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("GetEmployeesByCompanyAdvanceLoanAdjustment", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EmployeeStatusId", employeeStatusId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CompanyCode", companyCode ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EmployeeName", employeeName ?? (object)DBNull.Value);

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                employees.Add(new EmployeeAdjustmentDto
                                {
                                    EmployeeId = reader["EmployeeID"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    DepartmentName = reader["DepartmentName"].ToString(),
                                    DesignationName = reader["DesignationName"].ToString(),
                                    //JoiningDate = Convert.ToDateTime(reader["JoiningDate"])
                                    JoiningDate = Convert.ToDateTime(reader["JoiningDate"]).ToString("dd/MM/yyyy"),
                                });
                            }
                        }
                    }
                }

                return employees;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<EmployeeAdjustmentDto> GetLoadEmployeeByIdAsync(string employeeId)
        {
            if(employeeId == null)
            {
                return null;
            }
            EmployeeAdjustmentDto employees = new EmployeeAdjustmentDto();

            try
            {
                using (SqlConnection conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("GetEmployeesByCompanyAdvanceLoanAdjustment", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EmployeeStatusId", "01" ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CompanyCode", "001" ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EmployeeName", employeeId ?? (object)DBNull.Value);

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                employees = (new EmployeeAdjustmentDto
                                {
                                    EmployeeId = reader["EmployeeID"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    DepartmentName = reader["DepartmentName"].ToString(),
                                    DesignationName = reader["DesignationName"].ToString(),
                                    //JoiningDate = Convert.ToDateTime(reader["JoiningDate"])
                                    JoiningDate = Convert.ToDateTime(reader["JoiningDate"]).ToString("dd/MM/yyyy"),
                                });
                            }
                        }
                    }
                }

                return employees;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<List<LoanDataDto>> GetLoanByEmployeeIdAsync(string employeeId)
        {
            if(employeeId == null)
            {
                return null;
            }
            try
            {
                var payrollLoans = payrollLoanRepo.All().Where(x => x.EmployeeId == employeeId).Select(x => new LoanDataDto
                {
                    LoanId = x.LoanId,
                    LoanDate = x.LoanDate.HasValue ? x.LoanDate.Value.ToString("dd/MM/yyyy") : "",
                    LoanType = x.LoanTypeId != null ? loanTypeRepo.All().Where(x => x.LoanTypeId == x.LoanTypeId).Select(x => x.LoanType).FirstOrDefault() : "",
                    LoanStartEndDate = (x.StartDate.HasValue ? x.StartDate.Value.ToString("dd/MM/yyyy") : "") + " - " + (x.EndDate.HasValue ? x.EndDate.Value.ToString("dd/MM/yyyy") : ""),
                    NoOfInstallment = x.NoOfInstallment != "0" ? x.NoOfInstallment : ""
                }).ToList();
                return payrollLoans;
            }catch(Exception)
            {
                throw;
            }
        }

       public async Task<LoanDataDto> GetLoanByIdAsync(string loanId)
        {
            if (loanId == null)
            {
                return null;
            }
            try
            {
                var loan = payrollLoanRepo.All().Where(x => x.LoanId == loanId).Select(x => new LoanDataDto
                {
                    LoanId = x.LoanId,
                    LoanDate = x.LoanDate.HasValue ? x.LoanDate.Value.ToString("dd/MM/yyyy") : "",
                    LoanType = x.LoanTypeId != null ? loanTypeRepo.All().Where(x => x.LoanTypeId == x.LoanTypeId).Select(x => x.LoanType).FirstOrDefault() : "",
                    LoanStartEndDate = (x.StartDate.HasValue ? x.StartDate.Value.ToString("dd/MM/yyyy") : "") + " - " + (x.EndDate.HasValue ? x.EndDate.Value.ToString("dd/MM/yyyy") : ""),
                    NoOfInstallment = x.NoOfInstallment != "0" ? x.NoOfInstallment : "",
                   LoanAmount= x.LoanAmount,
                   StarDate = x.StartDate.HasValue? x.StartDate.Value.ToString("dd/MM/yyyy"):"" ,
                   EndDate = x.EndDate.HasValue? x.EndDate.Value.ToString("dd/MM/yyyy"):"" ,
                   MonthlyDeduction = x.MonthlyDeduction,
                   PayHeadNameId = x.PayHeadNameId != null ? payHeadRepo.All().Where(x=> x.PayHeadNameId == x.PayHeadNameId).Select(x=>x.Name).FirstOrDefault():"",
                }).FirstOrDefault();
                return loan;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
