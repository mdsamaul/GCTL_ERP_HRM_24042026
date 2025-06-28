using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.AdvanceLoanAdjustmentReport;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;

namespace GCTL.Service.AdvanceLoanAdjustmentReport
{
    public class AdvanceLoanAdjustmentReportServices :AppService<HrmPayAdvancePay>, IAdvanceLoanAdjustmentReportServices
    {
        private readonly IRepository<HrmPayAdvancePay> advancePayRepo;
        private readonly IConfiguration configuration;

        public AdvanceLoanAdjustmentReportServices(
              IRepository<HrmPayAdvancePay> advancePayRepo,
            IConfiguration configuration
            ) :base(advancePayRepo)
        {
            this.advancePayRepo = advancePayRepo;
            this.configuration = configuration;
        }
        public async Task<List<AdvanceLoanAdjustmentReportSetupViewModel>> GetAdvancePayReportAsync(HrmAdvancePayReportFilter filter)
        {
            try
            {
                using var connection = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection"));
                var parameters = new DynamicParameters();

                parameters.Add("@CompanyCodes", filter.CompanyCodes != null && filter.CompanyCodes.Any()
                    ? string.Join(",", filter.CompanyCodes) : null);

                parameters.Add("@BranchCodes", filter.BranchCodes != null && filter.BranchCodes.Any()
                    ? string.Join(",", filter.BranchCodes) : null);

                parameters.Add("@DepartmentCodes", filter.DepartmentCodes != null && filter.DepartmentCodes.Any()
                    ? string.Join(",", filter.DepartmentCodes) : null);

                parameters.Add("@DesignationCodes", filter.DesignationCodes != null && filter.DesignationCodes.Any()
                    ? string.Join(",", filter.DesignationCodes) : null);

                parameters.Add("@EmployeeIDs", filter.EmployeeIDs != null && filter.EmployeeIDs.Any()
                    ? string.Join(",", filter.EmployeeIDs) : null);

                parameters.Add("@PayHeadIDs", filter.PayHeadIDs != null && filter.PayHeadIDs.Any()
                    ? string.Join(",", filter.PayHeadIDs) : null);

                parameters.Add("@MonthIDs", filter.MonthIDs != null && filter.MonthIDs.Any()
                    ? string.Join(",", filter.MonthIDs) : null);

                parameters.Add("@YearIDs", filter.YearIDs != null && filter.YearIDs.Any()
                    ? string.Join(",", filter.YearIDs) : null);


                var result = await connection.QueryAsync<AdvanceLoanAdjustmentReportSetupViewModel>(
                    "SP_HRM_AdvancePayReport",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        public async Task<AdvanceLoanFilterData> GetAdvancePayFiltersAsync(HrmAdvancePayReportFilter filter)
        {
            using var conn = new SqlConnection(configuration.GetConnectionString("ApplicationDbConnection"));
            await conn.OpenAsync();

            // Convert list to comma separated string or null
            string ToCsv(List<string> list) =>
                (list != null && list.Any()) ? string.Join(",", list) : null;

            var param = new
            {
                CompanyCodes = ToCsv(filter.CompanyCodes),
                BranchCodes = ToCsv(filter.BranchCodes),
                DepartmentCodes = ToCsv(filter.DepartmentCodes),
                DesignationCodes = ToCsv(filter.DesignationCodes),
                EmployeeIDs = ToCsv(filter.EmployeeIDs),
                PayHeadIDs = ToCsv(filter.PayHeadIDs),
                MonthIDs = ToCsv(filter.MonthIDs),
                YearIDs = ToCsv(filter.YearIDs)
            };

            using var multi = await conn.QueryMultipleAsync(
                "SP_HRM_AdvancePayFilterData",
                param,
                commandType: CommandType.StoredProcedure);

            var result = new AdvanceLoanFilterData
            {
                Companies = (await multi.ReadAsync<IdNamePair>()).ToList(),
                Branches = (await multi.ReadAsync<IdNamePair>()).ToList(),
                Departments = (await multi.ReadAsync<IdNamePair>()).ToList(),
                Designations = (await multi.ReadAsync<IdNamePair>()).ToList(),
                Employees = (await multi.ReadAsync<IdNamePair>()).ToList(),
                PayHeads = (await multi.ReadAsync<IdNamePair>()).ToList(),
                Months = (await multi.ReadAsync<IdNamePair>()).ToList(),
                Years = (await multi.ReadAsync<IdNamePair>()).ToList()
            };

            return result;
        }


        
        public async Task<List<DepartmentGroupedData>> GetAdvancePayReportGroupedAsync(HrmAdvancePayReportFilter filter)
        {
            try
            {
                var allData = await GetAdvancePayReportAsync(filter);

                var groupedData = allData
                    .GroupBy(x => new { x.DepartmentCode, x.DepartmentName })
                    .Select(g => new DepartmentGroupedData
                    {
                        DepartmentCode = g.Key.DepartmentCode,
                        DepartmentName = g.Key.DepartmentName,
                        TotalEmployees = g.Select(x => x.EmployeeID).Distinct().Count(),
                        Employees = g.OrderBy(x => x.EmployeeID).ToList(),
                        Luser= filter.Luser
                    })
                    .OrderBy(x => x.DepartmentName)
                    .ToList();

                return groupedData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving grouped advance pay report: {ex.Message}", ex);
            }
        }

    }
}
