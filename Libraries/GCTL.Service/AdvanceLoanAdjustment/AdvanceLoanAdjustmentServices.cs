using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.AdvanceLoanAdjustment;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GCTL.Service.AdvanceLoanAdjustment
{
    public class AdvanceLoanAdjustmentServices : AppService<HrmPayAdvancePay>, IAdvanceLoanAdjustmentServices
    {
        private readonly IRepository<HrmPayAdvancePay> advancePayRepo;
        private readonly IConfiguration configuration;

        public AdvanceLoanAdjustmentServices(
            IRepository<HrmPayAdvancePay> advancePayRepo,
            IConfiguration configuration
        ) : base(advancePayRepo)
        {
            this.advancePayRepo = advancePayRepo;
            this.configuration = configuration;
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
                                    JoiningDate = reader["JoiningDate"].ToString()
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
    }

}
