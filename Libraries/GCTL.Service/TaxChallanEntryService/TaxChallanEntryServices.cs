using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Companies;
using GCTL.Core.ViewModels.TaxChallanEntry;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.TaxChallanEntryService
{
    public class TaxChallanEntryServices:AppService<HrmPayMonthlyTaxDepositEntry>, ITaxChallanEntryService
    {
        private readonly IRepository<HrmPayMonthlyTaxDepositEntry> taxChallanRepo;
        private readonly IRepository<EmployeeTaxChallanResultViewModel> TaxChallanResultRepo;
        private readonly IRepository<SalesDefBankBranchInfo> bankBranchRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmDefDesignation> degRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmPayMonth> monthRepo;
        private readonly string _configuration;

        public TaxChallanEntryServices(
            IRepository<HrmPayMonthlyTaxDepositEntry> TaxChallanRepo,
            IRepository<EmployeeTaxChallanResultViewModel> TaxChallanResultRepo,
            IConfiguration configuration,
            IRepository<SalesDefBankBranchInfo> bankBranchRepo,
            IRepository<CoreAccessCode> accessCodeRepository,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmDefDesignation> degRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmPayMonth> monthRepo
            ) :base(TaxChallanRepo)
        {
            this.taxChallanRepo = TaxChallanRepo;
            this.TaxChallanResultRepo = TaxChallanResultRepo;
            this.bankBranchRepo = bankBranchRepo;
            this.accessCodeRepository = accessCodeRepository;
            this.empRepo = empRepo;
            this.degRepo = degRepo;
            this.empOffRepo = empOffRepo;
            this.monthRepo = monthRepo;
            _configuration = configuration.GetConnectionString("ApplicationDbConnection");
        }


        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";

        #region Permission all type

        public async Task<bool> PagePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Printing Stationery Purchase Entry" && x.CheckDelete);

        }

        #endregion



        public async Task<EmployeeTaxChallanDropdownListResultViewModel> EmployeeTaxChallanDropdownList(EmployeeTaxChallanFilterViewModel filterData)
        {
            try
            {
                // Initialize result
                var result = new EmployeeTaxChallanDropdownListResultViewModel
                {
                    CompanyesList = new List<CommonDto>(),
                    EmployeesList = new List<CommonDto>(),
                    BranchList = new List<CommonDto>(),
                    DesignationsList = new List<CommonDto>(),
                    DepartmentsList = new List<CommonDto>(),
                    EmployeeList = new List<EmployeeTaxChallanResultViewModel>()
                };

                using var connection = new SqlConnection(_configuration);
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                // Convert list filters to comma separated strings
                var companyIds = filterData.CompanyCodes?.Count > 0 ? string.Join(",", filterData.CompanyCodes) : null;
                var employeeIds = filterData.EmployeeIds?.Count > 0 ? string.Join(",", filterData.EmployeeIds) : null;
                var branchCodes = filterData.BranchCodes?.Count > 0 ? string.Join(",", filterData.BranchCodes) : null;
                var designationCodes = filterData.DesignationCodes?.Count > 0 ? string.Join(",", filterData.DesignationCodes) : null;
                var departmentCodes = filterData.DepartmentCodes?.Count > 0 ? string.Join(",", filterData.DepartmentCodes) : null;

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.ProcGetEmpTaxChallanEntry";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@CompanyIds", (object?)companyIds ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@EmployeeIds", (object?)employeeIds ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@BranchCodes", (object?)branchCodes ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@DesignationCodes", (object?)designationCodes ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@DepartmentCodes", (object?)departmentCodes ?? DBNull.Value));

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // Helper functions to safely get nullable values
                    string? GetString(string col) => reader[col] != DBNull.Value ? reader[col]?.ToString() : null;
                    DateTime? GetDateTime(string col) => reader[col] != DBNull.Value ? (DateTime?)reader[col] : null;
                    decimal? GetDecimal(string col) => reader[col] != DBNull.Value ? (decimal?)reader[col] : null;

                    var emp = new EmployeeTaxChallanResultViewModel
                    {
                        CompanyCode = GetString("CompanyCode"),
                        CompanyName = GetString("CompanyName"),
                        BranchCode = GetString("BranchCode"),
                        BranchName = GetString("BranchName"),
                        EmployeeID = GetString("EmployeeID"),
                        FullName = GetString("FullName"),
                        JoiningDate = GetDateTime("JoiningDate"),
                        DesignationCode = GetString("DesignationCode"),
                        DesignationName = GetString("DesignationName"),
                        DepartmentCode = GetString("DepartmentCode"),
                        DepartmentName = GetString("DepartmentName"),
                        GrossSalary = GetDecimal("GrossSalary")
                    };

                    result.EmployeeList.Add(emp);

                    // Populate dropdown lists without duplicates
                    if (!string.IsNullOrEmpty(emp.CompanyCode) && !result.CompanyesList.Exists(x => x.Id == emp.CompanyCode))
                        result.CompanyesList.Add(new CommonDto { Id = emp.CompanyCode, Name = emp.CompanyName ?? "" });

                    if (!string.IsNullOrEmpty(emp.EmployeeID) && !result.EmployeesList.Exists(x => x.Id == emp.EmployeeID))
                        result.EmployeesList.Add(new CommonDto { Id = emp.EmployeeID, Name = emp.FullName ?? "" });

                    if (!string.IsNullOrEmpty(emp.BranchCode) && !result.BranchList.Exists(x => x.Id == emp.BranchCode))
                        result.BranchList.Add(new CommonDto { Id = emp.BranchCode, Name = emp.BranchName ?? "" });

                    if (!string.IsNullOrEmpty(emp.DesignationCode) && !result.DesignationsList.Exists(x => x.Id == emp.DesignationCode))
                        result.DesignationsList.Add(new CommonDto { Id = emp.DesignationCode, Name = emp.DesignationName ?? "" });

                    if (!string.IsNullOrEmpty(emp.DepartmentCode) && !result.DepartmentsList.Exists(x => x.Id == emp.DepartmentCode))
                        result.DepartmentsList.Add(new CommonDto { Id = emp.DepartmentCode, Name = emp.DepartmentName ?? "" });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching Employee Tax Challan dropdown data", ex);
            }
        }

        public async Task<string> GetBankBranchAddressAsync(string branchId)
        {
            try
            {
                if (string.IsNullOrEmpty(branchId))
                {
                    return null;
                }
                var Address =await bankBranchRepo.All().Where(x=> x.BankBranchId== branchId).Select(x=> x.Address).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(Address)) return null;
                return Address;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TaxChallanEntryBankDetailsResult>> GetBankDetails(string bankId)
        {
            try
            {
                if (string.IsNullOrEmpty(bankId) || bankId == string.Empty)
                {
                    return new List<TaxChallanEntryBankDetailsResult>();
                }
                var bankBranch =  bankBranchRepo.All().Where(x=> x.BankId== bankId).ToList();
                if (!bankBranch.Any()) { 
                return new List<TaxChallanEntryBankDetailsResult>();
                }


                var result = bankBranch.Select(branch=> new TaxChallanEntryBankDetailsResult()
                {
                    BankBranchId= branch.BankBranchId??"",
                    BankBranchName= branch.BankBranchName??"",
                    BankBranchAddress= branch.Address ?? "",
                }).ToList();
                return result;
            }
            catch (Exception)
            {

                throw;
            }         
        }
        //#region create and update
        //public async Task<(bool isSuccess, string message, object)> TaxChallanSaveEditAsync(HrmPayMonthlyTaxDepositEntryDto formData, string CompanyCode)
        //{
        //    try
        //    {   
        //        if(formData.TaxDepositCode == 0)
        //        {
        //            if (formData.EmployeeIds.IsNullOrEmpty())
        //            {
        //                return (false, CreateFailed, null);
        //            }
        //            if (formData.SalaryMonth.IsNullOrEmpty())
        //            {
        //                return (false, "salaryMonth", null);
        //            }
        //            if (formData.SalaryYear.IsNullOrEmpty())
        //            {
        //                return (false, "salaryYear", null);
        //            }
        //            if (formData.FinancialCodeNo.IsNullOrEmpty())
        //            {
        //                return (false, "financialCodeNo", null);
        //            }
        //            if (formData.TaxChallanNo.IsNullOrEmpty())
        //            {
        //                return (false, "taxChallanNo", null);
        //            }
        //            if (formData.TaxChallanDate== null)
        //            {
        //                return (false, "taxChallanDate", null);
        //            }
        //            if (formData.ChallanAmount== null || formData.ChallanAmount <=0)
        //            {
        //                return (false, "challanAmount", null);
        //            }

        //            var SaveDataList = new List<HrmPayMonthlyTaxDepositEntry>();

        //            int taxDepositId = Convert.ToInt32(formData.TaxDepositId);

        //            foreach (var employeeId in formData.EmployeeIds.Where(x=> !string.IsNullOrEmpty(x)))
        //            {
        //                var entity = new HrmPayMonthlyTaxDepositEntry
        //                {
        //                    TaxDepositId = taxDepositId.ToString("D8"),
        //                    EmployeeId = employeeId,
        //                    FinancialCodeNo = formData.FinancialCodeNo,
        //                    TaxDepositAmount = formData.TaxDepositAmount,
        //                    SalaryMonth = formData.SalaryMonth,
        //                    SalaryYear = formData.SalaryYear,
        //                    TaxChallanNo = formData.TaxChallanNo,
        //                    TaxChallanDate=formData.TaxChallanDate,
        //                    BankId = formData.BankId,
        //                    BankBranchId = formData.BankBranchId,
        //                    Remark = formData.Remark,
        //                    ChallanAmount = formData.ChallanAmount,
        //                    CompanyCode= CompanyCode,
        //                    Luser= formData.Luser,
        //                    Ldate = formData.Ldate,
        //                    Lmac = formData.Lmac,
        //                    Lip = formData.Lip,
        //                    TaxChallanNoPrefix= formData.TaxChallanNoPrefix??"",
        //                };
        //                SaveDataList.Add(entity);
        //              taxDepositId++;
        //            }
        //            await taxChallanRepo.AddRangeAsync(SaveDataList);
        //            return (true, CreateSuccess, SaveDataList);
        //        }


        //        return (true, "ok", null); 
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        public async Task<(bool isSuccess, string message, object)> TaxChallanSaveEditAsync(
    HrmPayMonthlyTaxDepositEntryDto formData,
    string CompanyCode)
        {
            try
            {            
                if (formData.EmployeeIds.IsNullOrEmpty())
                    return (false, "employeeIds", null);

                if (formData.SalaryMonth.IsNullOrEmpty())
                    return (false, "salaryMonth", null);

                if (formData.SalaryYear.IsNullOrEmpty())
                    return (false, "salaryYear", null);

                if (formData.FinancialCodeNo.IsNullOrEmpty())
                    return (false, "financialCodeNo", null);

                if (formData.TaxChallanNo.IsNullOrEmpty())
                    return (false, "taxChallanNo", null);

                if (formData.TaxChallanDate == null)
                    return (false, "taxChallanDate", null);

                if (formData.ChallanAmount == null || formData.ChallanAmount <= 0)
                    return (false, "challanAmount", null);

                // -----------------------------
                //  INSERT (Create New)
                // -----------------------------
                if (formData.TaxDepositCode == 0)
                {
                    var SaveDataList = new List<HrmPayMonthlyTaxDepositEntry>();
                    int taxDepositId = Convert.ToInt32(formData.TaxDepositId);

                    foreach (var employeeId in formData.EmployeeIds.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        var entity = new HrmPayMonthlyTaxDepositEntry
                        {
                            TaxDepositId = taxDepositId.ToString("D8"),
                            EmployeeId = employeeId,
                            FinancialCodeNo = formData.FinancialCodeNo,
                            TaxDepositAmount = formData.TaxDepositAmount,
                            SalaryMonth = formData.SalaryMonth,
                            SalaryYear = formData.SalaryYear,
                            TaxChallanNo = formData.TaxChallanNo,
                            TaxChallanDate = formData.TaxChallanDate,
                            BankId = formData.BankId,
                            BankBranchId = formData.BankBranchId,
                            Remark = formData.Remark,
                            ChallanAmount = formData.ChallanAmount,
                            CompanyCode = CompanyCode,
                            Luser = formData.Luser,
                            Ldate = formData.Ldate,
                            Lmac = formData.Lmac,
                            Lip = formData.Lip,
                            TaxChallanNoPrefix = formData.TaxChallanNoPrefix ?? "",
                        };
                        SaveDataList.Add(entity);
                        taxDepositId++;
                    }

                    await taxChallanRepo.AddRangeAsync(SaveDataList);
                    return (true, CreateSuccess, SaveDataList);
                }

                // -----------------------------
                //  UPDATE (Edit Existing)
                // -----------------------------
                else
                {
                    var existingData = taxChallanRepo.All().Where(x => x.TaxDepositCode == formData.TaxDepositCode &&
                                                  x.CompanyCode == CompanyCode);

                    if (existingData == null || !existingData.Any())
                        return (false, "Data not found", null);

                    await taxChallanRepo.DeleteRangeAsync(existingData);

                    var updateDataList = new List<HrmPayMonthlyTaxDepositEntry>();
                    int taxDepositId = Convert.ToInt32(formData.TaxDepositId);

                    foreach (var employeeId in formData.EmployeeIds.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        var entity = new HrmPayMonthlyTaxDepositEntry
                        {                            
                            TaxDepositId = taxDepositId.ToString("D8"),
                            EmployeeId = employeeId,
                            FinancialCodeNo = formData.FinancialCodeNo,
                            TaxDepositAmount = formData.TaxDepositAmount,
                            SalaryMonth = formData.SalaryMonth,
                            SalaryYear = formData.SalaryYear,
                            TaxChallanNo = formData.TaxChallanNo,
                            TaxChallanDate = formData.TaxChallanDate,
                            BankId = formData.BankId,
                            BankBranchId = formData.BankBranchId,
                            Remark = formData.Remark,
                            ChallanAmount = formData.ChallanAmount,
                            CompanyCode = CompanyCode,
                            Luser = formData.Luser,
                            Ldate = formData.Ldate,
                            Lmac = formData.Lmac,
                            Lip = formData.Lip,
                            TaxChallanNoPrefix = formData.TaxChallanNoPrefix ?? "",
                        };
                        updateDataList.Add(entity);
                        taxDepositId++;
                    }

                    await taxChallanRepo.AddRangeAsync(updateDataList);
                    return (true, UpdateSuccess, updateDataList);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        //#endregion 


        public async Task<string> TaxDipositAutoIdAsync()
        {
            try
            {
                var TaxChallanList = (await taxChallanRepo.GetAllAsync()).ToList();

                int newIdNumber = 1;

                if (TaxChallanList != null && TaxChallanList.Count > 0)
                {
                    var lastId = TaxChallanList
                        .Select(x => x.TaxDepositId)
                        .Where(id => id != null)
                        .OrderByDescending(id => id)
                        .FirstOrDefault();


                    if (!string.IsNullOrEmpty(lastId))
                    {
                        if (int.TryParse(lastId, out int lastNumber))
                        {
                            newIdNumber = lastNumber + 1;
                        }
                    }
                }
                
                return $"{newIdNumber.ToString("D8")}";
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<HrmPayMonthlyTaxDepositEntryDto>> GetchallanEntryGridAsync()
        {
            try
            {
                var taxCahlanList = taxChallanRepo.All().ToList();
                if (taxCahlanList.IsNullOrEmpty())
                {
                    return new List<HrmPayMonthlyTaxDepositEntryDto>();
                }
                var result = taxCahlanList.Select(x => new HrmPayMonthlyTaxDepositEntryDto()
                {
                    TaxDepositCode = x.TaxDepositCode,
                    TaxChallanDate = x.TaxChallanDate,
                    TaxChallanNo= x.TaxChallanNo,
                    TaxChallanNoPrefix= x.TaxChallanNoPrefix??"",
                    TaxDepositAmount    = x.TaxDepositAmount,
                    TaxDepositId = x.TaxDepositId,
                    EmployeeId = x.EmployeeId,
                    EmpName = empRepo.All().Where(e=> e.EmployeeId == x.EmployeeId).Select(s=> s.FirstName+" "+s.LastName).FirstOrDefault(),
                    DesignationCode = empOffRepo.All().Where(o=> o.EmployeeId== x.EmployeeId).Select(s=> s.DesignationCode).FirstOrDefault(),
                    DesignationName = empOffRepo.All().Where(o=>o.EmployeeId == x.EmployeeId).Join(degRepo.All(), e=> e.DesignationCode , d=> d.DesignationCode,(e,d)=> d.DesignationName).FirstOrDefault(),
                    ShowTaxChallanDate = x.TaxChallanDate.HasValue? x.TaxChallanDate.Value.ToString("dd/MM/yyyy"):"",
                    SalaryMonth = x.SalaryMonth,
                    SalaryYear = x.SalaryYear,
                    FinancialCodeNo = x.FinancialCodeNo,
                    Remark= x.Remark,
                    SalaryMonthName = monthRepo.All().Where(e=>e.MonthId.ToString() == x.SalaryMonth).Select(s=> s.MonthName).FirstOrDefault(),
                    ShowCreateDate= x.Ldate.HasValue? x.Ldate.Value.ToString("dd/MM/yyyy") :"",
                    ShowModifyDate = x.ModifyDate.HasValue ? x.ModifyDate.Value.ToString("dd/MM/yyyy") : "",
                    BankId=x.BankId,
                    BankBranchId=x.BankBranchId
                }).ToList();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(bool isSuccess, string message, object)> DeleteTaxChallanEntryGridAsync(List<string> selectedIds)
        {
            try
            {
                if (selectedIds.IsNullOrEmpty())
                {
                    return (false , DeleteFailed, null);
                }
                var deletedItemList = new List<HrmPayMonthlyTaxDepositEntry>();
                foreach (var item in selectedIds) {
                    if (!item.IsNullOrEmpty())
                    {
                        var data = taxChallanRepo.GetById(Convert.ToDecimal(item));
                        if (data != null)
                        {
                            deletedItemList.Add(data);
                        }
                    }
                }
                if (deletedItemList.IsNullOrEmpty())
                {
                    return (false, DeleteFailed, null);
                }
                await taxChallanRepo.DeleteRangeAsync(deletedItemList);
                return (true, DeleteSuccess, deletedItemList);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

