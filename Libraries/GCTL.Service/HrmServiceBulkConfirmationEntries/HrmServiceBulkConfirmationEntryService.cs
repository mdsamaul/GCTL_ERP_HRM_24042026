using DocumentFormat.OpenXml.Wordprocessing;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmServiceBulkConfimationEntry;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceBulkConfirmationEntries
{
    public class HrmServiceBulkConfirmationEntryService:AppService<HrmEmployeeOfficialInfo>,IHrmServiceBulkConfirmationEntryService
    {
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOfficialInfoRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<HrmDefDepartment> departmentRepo;
        private readonly IRepository<HrmDefDesignation> designationRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmDefEmployeeStatus> empStatusRepo;
        private readonly IRepository<HrmDefEmpType> empTypeRepo;
        private readonly IRepository<HrmEisDefEmploymentNature> empNatureRepo;
        private readonly IRepository<CorePeriodInfo> periodInfoRepo;
        private readonly IRepository<HrmDefProbationPeriodExtension> ppExtensionRepo;

        public HrmServiceBulkConfirmationEntryService(
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmEmployeeOfficialInfo> empOfficialInfoRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<HrmDefDepartment> departmentRepo,
            IRepository<HrmDefDesignation> designationRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmDefEmployeeStatus> empStatusRepo,
            IRepository<HrmDefEmpType> empTypeRepo,
            IRepository<HrmEisDefEmploymentNature> empNatureRepo,
            IRepository<CorePeriodInfo> periodInfoRepo,
            IRepository<HrmDefProbationPeriodExtension> ppExtensionRepo):base(empOfficialInfoRepo)
        {
            this.companyRepo = companyRepo;
            this.empOfficialInfoRepo = empOfficialInfoRepo;
            this.branchRepo = branchRepo;
            this.departmentRepo = departmentRepo;
            this.designationRepo = designationRepo;
            this.empRepo = empRepo;
            this.empStatusRepo = empStatusRepo;
            this.empTypeRepo = empTypeRepo;
            this.empNatureRepo = empNatureRepo;
            this.periodInfoRepo = periodInfoRepo;
            this.ppExtensionRepo = ppExtensionRepo;
        }


        public Task<bool> BulkDeleteAsync(List<decimal> autoIds)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> BulkEditAsync(HrmServiceBulkConfirmationViewModel model)
        {
            if (model?.confirmInfo?.Count == 0)
                return false;

            await empOfficialInfoRepo.BeginTransactionAsync();

            try
            {
                var empIds = model.confirmInfo.Select(x => x.EmployeeId).ToHashSet();

                var existingEntries = empOfficialInfoRepo.FindBy(x => empIds.Contains(x.EmployeeId)).ToList();

                var recordLookup = existingEntries.ToDictionary(x => x.EmployeeId);

                List<HrmEmployeeOfficialInfo> entryToUpgrade = new List<HrmEmployeeOfficialInfo>();

                foreach(var row in model.confirmInfo)
                {
                    if (!recordLookup.TryGetValue(row.EmployeeId, out var exRecord))
                        continue;

                    if (model.confirmInfo != null)
                    {
                        if (DateTime.TryParseExact(model.ConfirmeDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var confirmeDate))
                        {
                            exRecord.ConfirmeDate = confirmeDate;
                        }


                        exRecord.ConfirmationRefNo = row.RefLetterNo;
                        //exRecord.EmploymentNatureId = "01";
                        exRecord.EmpTypeCode = "01";

                        exRecord.ModifyDate = row.ModifyDate;
                        exRecord.Luser = row.Luser;
                        exRecord.Lip = row.Lip;
                        exRecord.Lmac = row.Lmac;

                        entryToUpgrade.Add(exRecord);
                    }
                }

                if (entryToUpgrade.Count>0)
                {
                    await empOfficialInfoRepo.UpdateRangeAsync(entryToUpgrade);
                }

                await empOfficialInfoRepo.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await empOfficialInfoRepo.RollbackTransactionAsync();
                return false; 
            }
        }

        public async Task<EmployeeFilterResultViewModel> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {
            var result = new EmployeeFilterResultViewModel();

            var query = from e in empOfficialInfoRepo.All().AsNoTracking()
                        join emp in empRepo.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepo.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join b in branchRepo.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchJoin
                        from b in branchJoin.DefaultIfEmpty()
                        join dept in departmentRepo.All().AsNoTracking() on e.DepartmentCode equals dept.DepartmentCode into deptJoin
                        from dept in deptJoin.DefaultIfEmpty()
                        join desig in designationRepo.All().AsNoTracking() on e.DesignationCode equals desig.DesignationCode into desigJoin
                        from desig in desigJoin.DefaultIfEmpty()
                        join status in empStatusRepo.All().AsNoTracking() on e.EmployeeStatus equals status.EmployeeStatus into statusJoin
                        from status in statusJoin.DefaultIfEmpty()
                        join type in empTypeRepo.All().AsNoTracking() on e.EmpTypeCode equals type.EmpTypeCode into typeJoin
                        from type in typeJoin.DefaultIfEmpty()
                        join nature in empNatureRepo.All().AsNoTracking() on e.EmploymentNatureId equals nature.EmploymentNatureId into natureJoin
                        from nature in natureJoin.DefaultIfEmpty()

                        where ((e.ConfirmeDate == null || e.ConfirmeDate == new DateTime(1900, 1, 1)) && e.EmployeeStatus.Equals("01") && e.EmpTypeCode.Equals("02"))

                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            e.BranchCode,
                            BranchName = b != null ? b.BranchName : null,
                            e.DepartmentCode,
                            DepartmentName = dept.DepartmentName,
                            e.DesignationCode,
                            DesignationName = desig.DesignationName,
                            e.EmployeeStatus,
                            EmployeeStatusName = status != null ? status.EmployeeStatus : null,
                            e.EmpTypeCode,
                            EmpTypename = type != null ? type.EmpTypeName : null,
                            e.EmploymentNatureId,
                            EmpNatureName = nature != null ? nature.EmploymentNature : null,
                            ConfirmationDate = e.ConfirmeDate,
                            ConfirmationRefNo = e.ConfirmationRefNo,
                            e.GrossSalary,
                            e.JoiningDate,
                            e.ProbationPeriod,
                            e.ProbationPeriodType
                        };

            query = query.Where(x => x.CompanyCode == "001");

            if (model.BranchCodes?.Any() == true)
                query = query.Where(x => model.BranchCodes.Contains(x.BranchCode));

            if (model.DepartmentCodes?.Any() == true)
                query = query.Where(x => model.DepartmentCodes.Contains(x.DepartmentCode));

            if (model.DesignationCodes?.Any() == true)
                query = query.Where(x => model.DesignationCodes.Contains(x.DesignationCode));

            if (model.EmployeeTypes?.Any() == true)
                query = query.Where(x => model.EmployeeTypes.Contains(x.EmpTypeCode));

            if (!string.IsNullOrEmpty(model.ProbationEndDays))
            {
                if (int.TryParse(model.ProbationEndDays, out int probationDays))
                {
                    var cutoffDate = DateTime.Today.AddDays(-probationDays); // Use negative to go back in time
                    query = query.Where(x =>
                        (x.JoiningDate != null && x.JoiningDate != new DateTime(1900, 1, 1)) &&
                        (x.ProbationPeriod != null && x.ProbationPeriodType != null) &&
                        x.JoiningDate.Value <= cutoffDate // Service date must be before cutoff (i.e., service length > probationDays)
                    );
                }
            }

            var allFilteredData = await query.ToListAsync();
            var empIds = allFilteredData.Select(x => x.EmployeeId).ToList();

            var extensionData = await ppExtensionRepo.All().AsNoTracking()
                .Where(x => empIds.Contains(x.EmployeeId))
                .OrderBy(x => x.EmployeeId)
                .ThenByDescending(x => x.Ppeid) 
                .ToListAsync();

            var extensionsByEmployee = extensionData.GroupBy(x => x.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var latestExtensionDict = extensionsByEmployee.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.FirstOrDefault()
            );

            var allBranch = await branchRepo.All().Where(b => b.BranchCode != null && b.BranchName != null)
                .OrderBy(b => b.BranchCode)
                .Select(b => new LookupItemDto
                {
                    Code = b.BranchCode,
                    Name = b.BranchName
                }).Distinct().ToListAsync();

            result.Employees = allFilteredData.Select(x =>
            {
                string serviceLength = "";
                string probationPeriod = "";
                string extPeriod = "";
                DateTime? extPeriodEndOn = null;
                DateTime? endDate = null;

                var latestExtension = latestExtensionDict.ContainsKey(x.EmployeeId) ? latestExtensionDict[x.EmployeeId] : null;

                var allExtensions = extensionsByEmployee.GetValueOrDefault(x.EmployeeId) ?? new List<GCTL.Data.Models.HrmDefProbationPeriodExtension>();

                if (x.JoiningDate.HasValue && x.JoiningDate.Value != new DateTime(1900, 1, 1))
                {
                    serviceLength = CalculateDateLengthInDays(x.JoiningDate.Value, DateTime.Today);

                    if (x.ProbationPeriod != null && x.ProbationPeriodType != null)
                    {
                        endDate = CalculateProbationEndDate(x.JoiningDate.Value, x.ProbationPeriod, x.ProbationPeriodType);
                        if (endDate != null)
                            probationPeriod = CalculateDateLengthInDays(x.JoiningDate.Value, endDate.Value);
                    }
                }

                if (latestExtension != null && latestExtension.ExtendedPeriod != null)
                {
                    DateTime? currentEndDate = endDate;
                    DateTime? finalExtensionEndDate = currentEndDate;

                    foreach (var item in allExtensions)
                    {
                        if (item.ExtendedPeriod != null && !string.IsNullOrEmpty(item.PeriodInfoId) && finalExtensionEndDate.HasValue)
                        {
                            switch (item.PeriodInfoId)
                            {
                                case "01":
                                    finalExtensionEndDate = finalExtensionEndDate.Value.AddYears(int.Parse(item.ExtendedPeriod));
                                    break;
                                case "02": 
                                    finalExtensionEndDate = finalExtensionEndDate.Value.AddMonths(int.Parse(item.ExtendedPeriod) * 6);
                                    break;
                                case "03":
                                    finalExtensionEndDate = finalExtensionEndDate.Value.AddMonths(int.Parse(item.ExtendedPeriod) * 3);
                                    break;
                                case "04":
                                    finalExtensionEndDate = finalExtensionEndDate.Value.AddMonths(int.Parse(item.ExtendedPeriod));
                                    break;
                                case "05":
                                    finalExtensionEndDate = finalExtensionEndDate.Value.AddDays(int.Parse(item.ExtendedPeriod) * 7);
                                    break;
                                case "06":
                                    finalExtensionEndDate = finalExtensionEndDate.Value.AddDays(int.Parse(item.ExtendedPeriod));
                                    break;
                            }
                        }
                    }

                    if (finalExtensionEndDate.HasValue && currentEndDate.HasValue)
                    {
                        extPeriod = CalculateDateLengthInDays(currentEndDate.Value, finalExtensionEndDate.Value);
                        extPeriodEndOn = finalExtensionEndDate;
                    }
                }

                return new EmployeeListItemViewModel
                {
                    EmployeeId = x.EmployeeId,
                    EmployeeName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                    DesignationName = x.DesignationName,
                    DepartmentName = x.DepartmentName,
                    GrossSalary = x.GrossSalary.ToString("N2") ?? "0.00",
                    JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                    ProbationPeriod = probationPeriod,
                    ProbationPeriodEndOn = endDate.HasValue ? endDate.Value.ToString("dd/MM/yyyy") : "",
                    ExtenPeriod = extPeriod,
                    ExtenPeriodEndOn = extPeriodEndOn.HasValue ? extPeriodEndOn.Value.ToString("dd/MM/yyyy") : "",
                    ServiceLength = serviceLength,
                    RefLetterNo = x.ConfirmationRefNo,
                };
            }).ToList();

            result.LookupData["companies"] = allFilteredData
                .Where(x => x.CompanyCode != null && x.CompanyName != null)
                .GroupBy(x => new { x.CompanyCode, x.CompanyName })
                .Select(x => new LookupItemDto { Code = x.Key.CompanyCode, Name = x.Key.CompanyName })
                .ToList();

            result.LookupData["branches"] = allBranch;

            result.LookupData["departments"] = allFilteredData
                .Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .GroupBy(x => new { x.DepartmentCode, x.DepartmentName })
                .Select(x => new LookupItemDto { Code = x.Key.DepartmentCode, Name = x.Key.DepartmentName })
                .ToList();

            result.LookupData["designations"] = allFilteredData
                .Where(x => x.DesignationCode != null && x.DesignationName != null)
                .GroupBy(x => new { x.DesignationCode, x.DesignationName })
                .Select(x => new LookupItemDto { Code = x.Key.DesignationCode, Name = x.Key.DesignationName })
                .ToList();

            result.LookupData["employeeType"] = allFilteredData
                .Where(x => !string.IsNullOrWhiteSpace(x.EmpTypeCode) && !string.IsNullOrWhiteSpace(x.EmpTypename))
                .GroupBy(x => new { x.EmpTypeCode, x.EmpTypename })
                .Select(x => new LookupItemDto { Code = x.Key.EmpTypeCode, Name = x.Key.EmpTypename })
                .ToList();

            return result;
        }

        private DateTime? CalculateProbationEndDate(DateTime? value, string probationPeriod, string probationPeriodType)
        {
            if (value == null)
                return null;

            switch (probationPeriodType)
            {
                case "01": // Years
                    return value.Value.AddYears(int.Parse(probationPeriod));
                case "02": // Half-Years
                    return value.Value.AddMonths(int.Parse(probationPeriod) * 6);
                case "03": // Quarter-Years
                    return value.Value.AddMonths(int.Parse(probationPeriod) * 3);
                case "04": // Months
                    return value.Value.AddMonths(int.Parse(probationPeriod));
                case "05": // Weeks
                    return value.Value.AddDays(int.Parse(probationPeriod) * 7);
                case "06": // Days
                    return value.Value.AddDays(int.Parse(probationPeriod));
                default:
                    return null;
            }
        }

        private string CalculateDateLengthInDays(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || startDate == new DateTime(1900, 1, 1) || endDate == null)
                return "";

            if (startDate <= endDate)
            {
                if (startDate == new DateTime(1900, 1, 1) || endDate == new DateTime(1900, 1, 1))
                    return "";

                var totalDays = (endDate - startDate).Value.Days;
                return $"{totalDays} {(totalDays == 1 ? "day" : "days")}";
            }
            else
            {
                return "";
            }
        }

    }
}
