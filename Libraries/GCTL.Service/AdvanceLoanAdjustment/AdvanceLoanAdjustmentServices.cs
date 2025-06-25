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
        private readonly IRepository<HrmPayMonth> monthRepo;
        private readonly IRepository<HrmPayPayHeadName> headRepo;

        public AdvanceLoanAdjustmentServices(
            IRepository<HrmPayAdvancePay> advancePayRepo,
            IConfiguration configuration,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmPayrollLoan> payrollLoanRepo,
            IRepository<HrmPayLoanTypeEntry> LoanTypeRepo,
            IRepository<HrmPayPayHeadName> payHeadRepo,
            IRepository<HrmPayMonth> monthRepo,
            IRepository<HrmPayPayHeadName> headRepo
        ) : base(advancePayRepo)
        {
            this.advancePayRepo = advancePayRepo;
            this.configuration = configuration;
            this.empRepo = empRepo;
            this.payrollLoanRepo = payrollLoanRepo;
            this.loanTypeRepo = LoanTypeRepo;
            this.payHeadRepo = payHeadRepo;
            this.monthRepo = monthRepo;
            this.headRepo = headRepo;
        }
        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";

        public async Task<List<CompanyDto>> GetAllAndFilterCompanyAsync(string searchCompanyName)
        {
            List<CompanyDto> companies = new List<CompanyDto>();

            using (SqlConnection conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection")))
            {
                await conn.OpenAsync(); 

                using (SqlCommand cmd = new SqlCommand("GetCompanyNamesBySearch", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchCompanyName", searchCompanyName ?? "");

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) 
                    {
                        while (await reader.ReadAsync()) 
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
        
        public async Task<List<EmployeeAdjustmentDto>> GetEmployeesByFilterAsync(string employeeStatusId, string companyCode, string employeeName, bool loanAdjustment)
        {
            List<EmployeeAdjustmentDto> employees = new List<EmployeeAdjustmentDto>();

            try
            {
                var sPName = loanAdjustment ? "GetEmployeesByCompanyLoanAdjustment" : "GetEmployeesByCompanyAdvanceLoanAdjustment";

                using (SqlConnection conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand(sPName, conn))
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
                                var employee = new EmployeeAdjustmentDto
                                {
                                    EmployeeId = reader["EmployeeID"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    DepartmentName = reader["DepartmentName"].ToString(),
                                    DesignationName = reader["DesignationName"].ToString(),
                                    JoiningDate = Convert.ToDateTime(reader["JoiningDate"]).ToString("dd/MM/yyyy")
                                };

                                if (loanAdjustment && reader["LoanId"] != DBNull.Value)
                                {
                                    employee.LoanId = reader["LoanId"].ToString();
                                }

                                employees.Add(employee);
                            }
                        }
                    }
                }

                return employees;
            }
            catch (Exception)
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
                   PayHeadNameName = x.PayHeadNameId != null ? payHeadRepo.All().Where(x=> x.PayHeadNameId == x.PayHeadNameId).Select(x=>x.Name).FirstOrDefault():"",
                   PayHeadNameId = x.PayHeadNameId
                }).FirstOrDefault();
                return loan;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Create loan adjustment installments
        public async Task<(bool isSuccess, string message, object data)> SaveUpdateLoanAdjustmentAsync(AdvanceLoanAdjustmentSetupViewModel modelData)
        {
            if (modelData == null|| modelData.AdjustmentType == "" || modelData.AdjustmentType == null || modelData.EmployeeID == "" || modelData.EmployeeID == null || modelData.AdvanceAmount < 0 || modelData.AdvanceAmount == null)
                return (false, CreateFailed, null);

            bool isExist;
          
            if(modelData.AdjustmentType == "Advance" && modelData.AdvancePayCode == 0)
            {
                isExist = advancePayRepo.All().Any(x =>  x.EmployeeId == modelData.EmployeeID && x.SalaryMonth == modelData.SalaryMonth && x.SalaryYear == modelData.SalaryYear);
                if (isExist)
                {
                    return (false, DataExists, null);
                }
            }
            

            if(modelData.AdvancePayCode != 0)
            {

                try
                {
                    var existing = advancePayRepo.All().FirstOrDefault(x => x.AdvancePayId == modelData.AdvancePayId);
                    if (existing == null)
                        return (false, "Data not found to update", null);

                    if (modelData.AdjustmentType == "Loan" )
                    {
                        isExist = advancePayRepo.All().Any(x =>x.LoanId == modelData.LoanID && x.EmployeeId == modelData.EmployeeID && x.SalaryMonth == modelData.SalaryMonth && x.SalaryYear == modelData.SalaryYear && x.AdvancePayId != modelData.AdvancePayId);
                        if (isExist)
                        {
                            return (false, DataExists, null);
                        }
                    }

                    // ✅ Update the existing data
                    existing.EmployeeId = modelData.EmployeeID ?? "";
                    existing.AdvanceAdjustStatus = modelData.AdvanceAdjustStatus ?? "";
                    existing.AdvanceAmount = modelData.AdvanceAmount ;
                    existing.MonthlyDeduction = modelData.MonthlyDeduction;
                    existing.SalaryMonth = modelData.SalaryMonth ?? "";
                    existing.SalaryYear = modelData.SalaryYear ?? "";
                    existing.NoOfPaymentInstallment = modelData.NoOfPaymentInstallment ?? "";
                    existing.PayHeadNameId = modelData.PayHeadNameId ?? "";
                    existing.Remarks = modelData.Remarks ?? "";
                    existing.ModifyDate = DateTime.Now ;
                    existing.AdjustmentType = modelData.AdjustmentType ?? "";
                    existing.LoanId = modelData.LoanID??"";
                    existing.CompanyCode = "001";
                    existing.PayHeadNameId = modelData.PayHeadNameId ?? "";
                    await advancePayRepo.UpdateAsync(existing);
                    return (true, UpdateSuccess, existing);
                }
                catch (Exception)
                {
                    throw;
                }
               

            }
            int advancePayId =int.Parse(modelData.AdvancePayId);
            List<HrmPayAdvancePay> installments = new List<HrmPayAdvancePay>();
            if(modelData.AdvanceAdjustStatus == "By Month")
            {
               

                var installment = new HrmPayAdvancePay
                {
                    AdvancePayId = advancePayId.ToString("D8"),
                    EmployeeId = modelData.EmployeeID,
                    AdvanceAdjustStatus = modelData.AdvanceAdjustStatus,
                    AdvanceAmount = modelData.AdvanceAmount,
                    MonthlyDeduction = modelData.MonthlyDeduction,

                    SalaryMonth = modelData.SalaryMonth, 
                    SalaryYear = modelData.SalaryYear,

                    NoOfPaymentInstallment = modelData.NoOfPaymentInstallment,
                    PayHeadNameId = modelData.PayHeadNameId,
                    Remarks = modelData.Remarks,
                    Luser = modelData.Luser,
                    Ldate = DateTime.Now,
                    Lip = modelData.Lip,
                    Lmac = modelData.Lmac,
                    AdjustmentType = modelData.AdjustmentType,
                    LoanId = modelData.LoanID,
                    CompanyCode = "001"
                };

                installments.Add(installment);
                advancePayId++;
            }

            if (modelData.AdvanceAdjustStatus == "By Date")
            {
                if (modelData.FromDate == null || modelData.Todate == null)
                    return (false, CreateFailed, null);

                DateTime fromDate = modelData.FromDate.Value;
                DateTime toDate = modelData.Todate.Value;
                while (fromDate <= toDate)
                {


                   
                        isExist = advancePayRepo.All().Any(x => x.LoanId == modelData.LoanID && x.EmployeeId == modelData.EmployeeID && x.SalaryMonth == fromDate.ToString("MMMM") && x.SalaryYear == fromDate.Year.ToString());
                        if (!isExist)
                        {
                            var installment = new HrmPayAdvancePay
                            {
                                AdvancePayId = advancePayId.ToString("D8"),
                                EmployeeId = modelData.EmployeeID,
                                AdvanceAdjustStatus = modelData.AdvanceAdjustStatus,
                                AdvanceAmount = modelData.AdvanceAmount,
                                MonthlyDeduction = modelData.MonthlyDeduction,

                                SalaryMonth = fromDate.ToString("MMMM"), 
                                SalaryYear = fromDate.Year.ToString(),

                                NoOfPaymentInstallment = modelData.NoOfPaymentInstallment,
                                PayHeadNameId = modelData.PayHeadNameId,
                                Remarks = modelData.Remarks,
                                Luser = modelData.Luser,
                                Ldate = DateTime.Now,
                                Lip = modelData.Lip,
                                Lmac = modelData.Lmac,
                                AdjustmentType = modelData.AdjustmentType,
                                LoanId = modelData.LoanID,
                                CompanyCode = "001"
                            };

                            installments.Add(installment);
                            fromDate = fromDate.AddMonths(1);
                            advancePayId++;
                    }
                    else
                    {
                        fromDate = fromDate.AddMonths(1);
                    }
                    

                    
                }
            }

          

            try
            {
                foreach (var item in installments)
                {
                    await advancePayRepo.AddAsync(item);
                }

                return (true,CreateSuccess, null);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to save installments: {ex.Message}", null);
            }
        }

        //auto id 
        public async Task<string> AdjustmentAutoGanarateIdAsync()
        {
            var lastItem = advancePayRepo.All().OrderByDescending(x => x.AdvancePayId).FirstOrDefault();

            string newId;
            if (lastItem != null && !string.IsNullOrEmpty(lastItem.AdvancePayId))
            {
                string numericPart = lastItem.AdvancePayId.Substring(1); 
                int number = int.Parse(numericPart) + 1;
                newId = number.ToString("D8");
            }
            else
            {
                newId = "00000001";
            }

            return newId;
        }

        //get month
      public async  Task<List<MonthDto>> GetMonthAsync()
        {
            var monthAll = monthRepo.All().OrderBy(x => x.MonthId).ToList();
            var months = monthAll.Select(x => new MonthDto
            {
                MonthId = x.MonthId,
                MonthName = x.MonthName,
            }).ToList();
            return months;
        }
        //get Deduction Heads
        public async  Task<List<PayHeadNameDto>> GetHeadDeductionAsync()
        {
            var deductionHeadAll = headRepo.All().OrderBy(x => x.PayHeadNameId).ToList();
            var head = deductionHeadAll.Select(x => new PayHeadNameDto
            {
                 PayHeadNameId= x.PayHeadNameId,
                Name = x.Name,
            }).ToList();
            return head;
        }

        // Helper method for safe conversion
        private static int SafeConvertToInt32(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return defaultValue;
        }

        private static decimal SafeConvertToDecimal(object value, decimal defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;

            return defaultValue;
        }

        public async Task<DataTableResponse<AdvancePayViewModel>> GetAdvancePayPaged(DataTableRequest request)
        {
            var response = new DataTableResponse<AdvancePayViewModel>
            {
                Data = new List<AdvancePayViewModel>(),
                TotalRecords = 0,
                FilteredRecords = 0
            };

            try
            {
                using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("GetAdvancePayPagedWithFilter", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PageNumber", request.Page);
                        cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
                        cmd.Parameters.AddWithValue("@SearchValue", request.SearchValue ?? "");
                        cmd.Parameters.AddWithValue("@Department", request.Department ?? "");
                        cmd.Parameters.AddWithValue("@Month", request.Month ?? "");
                        cmd.Parameters.AddWithValue("@Year", request.Year ?? "");

                        await con.OpenAsync();

                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync())
                        {
                            // First result set: Data
                            while (await rdr.ReadAsync())
                            {
                                response.Data.Add(new AdvancePayViewModel
                                {
                                    AdvancePayId = rdr["AdvancePayId"]?.ToString() ?? "",
                                    EmployeeID = rdr["EmployeeID"]?.ToString() ?? "",
                                    LoanID = rdr["LoanID"]?.ToString() ?? "",
                                    NoOfPaymentInstallment = rdr["NoOfPaymentInstallment"]?.ToString() ?? "",
                                    PayHeadNameId = rdr["PayHeadNameId"]?.ToString() ?? "",
                                    AdvancePayCode = SafeConvertToInt32(rdr["AdvancePayCode"]),
                                    FullName = rdr["FullName"]?.ToString() ?? "",
                                    JoiningDate = rdr["JoiningDate"] != DBNull.Value? ((DateTime)rdr["JoiningDate"]).ToString("dd/MM/yyyy") : "",
                                    LoanDate = rdr["LoanDate"] != DBNull.Value? ((DateTime)rdr["LoanDate"]).ToString("dd/MM/yyyy") : "",
                                    CreateDate = rdr["LDate"] != DBNull.Value? ((DateTime)rdr["LDate"]).ToString("dd/MM/yyyy") : "",
                                    ModifyDate = rdr["ModifyDate"] != DBNull.Value? ((DateTime)rdr["ModifyDate"]).ToString("dd/MM/yyyy") : "",
                                    LoanTypeId = rdr["LoanTypeId"]?.ToString() ?? "",
                                    LoanTypeName = loanTypeRepo.All().Where(x=> x.LoanTypeId == rdr["LoanTypeId"].ToString()).Select(x=> x.LoanType).FirstOrDefault() ?? "",
                                    LoanStartDate = rdr["StartDate"] != DBNull.Value? ((DateTime)rdr["StartDate"]).ToString("dd/MM/yyyy"):"",
                                    LoanEndDate = rdr["EndDate"] != DBNull.Value ? ((DateTime)rdr["EndDate"]).ToString("dd/MM/yyyy") : "",
                                    DepartmentName = rdr["DepartmentName"]?.ToString() ?? "",
                                    AdjustmentType = rdr["AdjustmentType"]?.ToString() ?? "",
                                    AdvanceAdjustStatus = rdr["AdvanceAdjustStatus"]?.ToString() ?? "",
                                    DesignationName = rdr["DesignationName"]?.ToString() ?? "",
                                    AdvanceAmount = SafeConvertToDecimal(rdr["AdvanceAmount"]),
                                    MonthlyDeduction = rdr["MonthlyDeduction"] == DBNull.Value
                                        ? (decimal?)null
                                        : SafeConvertToDecimal(rdr["MonthlyDeduction"]),
                                    SalaryMonth = rdr["SalaryMonth"]?.ToString() ?? "",
                                    SalaryYear = rdr["SalaryYear"]?.ToString() ?? "",
                                   
                                    Remarks = rdr["Remarks"]?.ToString() ?? "",
                                });
                            }

                            // Second result set: Total count
                            if (await rdr.NextResultAsync())
                            {
                                if (await rdr.ReadAsync())
                                {
                                    response.TotalRecords = SafeConvertToInt32(rdr["TotalRecords"]);
                                    response.FilteredRecords = SafeConvertToInt32(rdr["FilteredRecords"]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Data = new List<AdvancePayViewModel>();
                response.TotalRecords = 0;
                response.FilteredRecords = 0;
            }

            return response;
        }

        //delete
        public async Task<(bool isSuccess, string message)> DeleteAdvancePayAsync(List<decimal> ids)
        {
            if (ids == null || ids.Count == 0)
                return (false, DeleteFailed);

            var itemsToDelete = new List<HrmPayAdvancePay>(); 

            foreach (decimal id in ids)
            {
                var item = await advancePayRepo.GetByIdAsync(id);
                if (item != null)
                {
                    itemsToDelete.Add(item);
                }
            }

            if (itemsToDelete.Count == 0)
                return (false, DeleteFailed);

            await advancePayRepo.DeleteRangeAsync(itemsToDelete);

            return (true, DeleteSuccess);
        }

    }

}
