using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Companies;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceNotConfirmationEntries
{
    public class HrmServiceNotConfirmationEntryService:AppService<HrmServiceNotConfirmationEntry>, IHrmServiceNotConfirmationEntryService
    {
        private readonly IRepository<CoreCompany> companyRepository;
        private readonly IRepository<HrmDefDivision> divisionRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
        private readonly IRepository<CoreBranch> branchRepository;
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<HrmDefDesignation> designationRepository;
        private readonly IRepository<HrmEmployee> employeeRepository;
        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;
        private readonly IRepository<HrmDefEmpType> empTypeRepository;
        private readonly IRepository<HrmEisDefEmploymentNature> employmentNatureRepository;
        private readonly IRepository<HrmPayMonth> payMonthRepository;
        private readonly IRepository<HrmSeparation> separationRepository;
        private readonly IRepository<CorePeriodInfo> periodRepository;

        private readonly IRepository<HrmServiceNotConfirmationEntry> entryRepository;

        public HrmServiceNotConfirmationEntryService(
            IRepository<HrmServiceNotConfirmationEntry> entryRepository,
            IRepository<CoreCompany> companyRepository,
            IRepository<HrmDefDivision> divisionRepository,
            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository,
            IRepository<CoreBranch> branchRepository,
            IRepository<HrmDefDepartment> departmentRepository,
            IRepository<HrmDefDesignation> designationRepository,
            IRepository<HrmEmployee> employeeRepository,
            IRepository<HrmDefEmployeeStatus> employeeStatusRepository,
            IRepository<HrmDefEmpType> empTypeRepository,
            IRepository<HrmEisDefEmploymentNature> employmentNatureRepository,
            IRepository<HrmPayMonth> payMonthRepository,
            IRepository<HrmSeparation> separationRepository,
            IRepository<CorePeriodInfo> periodRepository
            ) : base(entryRepository)
        {
            this.entryRepository = entryRepository;
            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.branchRepository = branchRepository;
            this.designationRepository = designationRepository;
            this.departmentRepository = departmentRepository;
            this.employeeRepository = employeeRepository;
            this.empTypeRepository = empTypeRepository;
            this.payMonthRepository = payMonthRepository;
            this.employmentNatureRepository = employmentNatureRepository;
            this.employeeStatusRepository = employeeStatusRepository;
            this.separationRepository = separationRepository;
            this.periodRepository = periodRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> tcs)
        {
            if (tcs == null || !tcs.Any())
                return false;

            const int batchSize = 500;

            await entryRepository.BeginTransactionAsync();
            try
            {
                for (int i = 0; i < tcs.Count; i += batchSize)
                {
                    var batch = tcs.Skip(i).Take(batchSize).ToList();
                    var entries = await entryRepository.All()
                        .Where(e => batch.Contains(e.Tc))
                        .AsNoTracking()
                        .ToListAsync();

                    if (!entries.Any()) continue;

                    var employeeIds = entries.Select(e => e.EmployeeId).Distinct().ToList();
                    var effectiveDates = entries.Select(e => e.EffectiveDate).Distinct().ToList();

                    var separations = await separationRepository.All()
                        .Where(s => employeeIds.Contains(s.EmployeeId) &&
                                   effectiveDates.Contains(s.SeparationDate) &&
                                   s.SeparationTypeId == "04")
                        .ToListAsync();

                    var officialInfos = await employeeOfficialInfoRepository.All()
                        .Where(o => employeeIds.Contains(o.EmployeeId) && o.EmployeeStatus == "02")
                        .ToListAsync();

                    foreach (var entry in entries)
                    {
                        try
                        {
                            var existingOfficialInfo = officialInfos
                                .Where(e => e.EmployeeId == entry.EmployeeId)
                                .FirstOrDefault();

                            if (existingOfficialInfo != null)
                            {
                                existingOfficialInfo.EmployeeStatus = "01";
                                await employeeOfficialInfoRepository.UpdateAsync(existingOfficialInfo);
                            }

                            var separationsToDelete = separations
                                .Where(s => s.EmployeeId == entry.EmployeeId && s.SeparationDate == entry.EffectiveDate)
                                .ToList();

                            if (separationsToDelete.Any())
                            {
                                await separationRepository.DeleteRangeAsync(separationsToDelete);
                            }

                            await entryRepository.DeleteAsync(entry.Tc);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                await entryRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await entryRepository.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> EditAsync(HrmServiceNotConfirmViewModel model)
        {
            if (model == null || model.Tc == 0 || model.Sncid == null)
            {
                return false;
            }

            await entryRepository.BeginTransactionAsync();

            try
            {
                var exRecord = await entryRepository.GetByIdAsync(model.Tc);

                if (exRecord == null)
                    return false;

                exRecord.EmployeeId = model.EmployeeId;
                exRecord.EffectiveDate = model.EffectiveDate;
                exRecord.DuePaymentDate = model.DuePaymentDate;
                exRecord.RefLetterNo = model.RefLetterNo ?? "";
                exRecord.RefLetterDate = model.RefLetterDate;
                exRecord.Remarks = model.Remarks ?? "";
                exRecord.ModifyDate = model.ModifyDate;
                exRecord.Lip = model.Lip;
                exRecord.Lmac = model.Lmac;
                exRecord.Luser = model.Luser;
                exRecord.CompanyCode = model.CompanyCode;

                await entryRepository.UpdateAsync(exRecord);

                var exSeparation = await separationRepository.All()
                    .Where(s => s.EmployeeId == model.EmployeeId && s.SeparationTypeId == "04")
                    .FirstOrDefaultAsync();

                if (exSeparation != null)
                {
                    exSeparation.SeparationDate = model.EffectiveDate.HasValue ? model.EffectiveDate.Value : default;
                    exSeparation.ModifyDate = model.ModifyDate;
                    await separationRepository.UpdateAsync(exSeparation);
                }
                else
                {
                    string newSeparationId = await GenerateSeparationIdAsync();
                    HrmSeparation seprecord = new HrmSeparation
                    {
                        SeparationId = newSeparationId,
                        EmployeeId = model.EmployeeId,
                        SeparationDate = model.EffectiveDate.HasValue ? model.EffectiveDate.Value : default,
                        SeparationTypeId = "04",
                        FinalPayment = 0,
                        IsPaid = "Y",
                        Remark = "",
                        Ldate = model.Ldate,
                        Lip = model.Lip,
                        Lmac = model.Lmac,
                        Luser = model.Luser,
                        CompanyCode = model.CompanyCode,
                        RefLetterNo = null,
                        RefLetterDate = null
                    };
                    await separationRepository.AddAsync(seprecord);
                }
                
                await entryRepository.CommitTransactionAsync();

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return false;
            }
        }

        public Task<byte[]> GenerateExcelSampleAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateSNCIdAsync()
        {
            try
            {
                var maxId = entryRepository.All()
                    .AsEnumerable() // Switch to LINQ-to-Objects
                    .Where(e => !string.IsNullOrEmpty(e.Sncid) && e.Sncid.All(char.IsDigit))
                    .Select(e => int.Parse(e.Sncid))
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString("D8");
            }
            catch
            {
                return "00000001";
            }
        }


        public async Task<HrmServiceNotConfirmViewModel> GetByIdAsync(decimal id)
        {
            var service = await entryRepository.GetByIdAsync(id);
            var emp = await employeeRepository.GetByIdAsync(service.EmployeeId);

            if (service == null) return null;

            var serviceModel = new HrmServiceNotConfirmViewModel
            {
                Tc = service.Tc,
                Sncid = service.Sncid,
                EmployeeId = service.EmployeeId,
                Id = service.EmployeeId,
                Code = service.EmployeeId,

                Name = string.Join(" ", new[] { emp.FirstName, emp.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({emp.EmployeeId})",

                EffectiveDate = service.EffectiveDate.Value.Date,

                DuePaymentDate = service.DuePaymentDate.HasValue ? service.DuePaymentDate.Value.Date : null,
                RefLetterDate = service.RefLetterDate.HasValue ? service.RefLetterDate.Value.Date : null,
                RefLetterNo = service.RefLetterNo,
                Remarks = service.Remarks
            };
            return serviceModel;
        }

        public async Task<EmployeeListItemViewModel> GetDataByEmpId(string selectedEmpId)
        {
            var result = new EmployeeListItemViewModel();

            var query = from emp in employeeRepository.All().AsNoTracking()
                        join e in employeeOfficialInfoRepository.All().AsNoTracking() on emp.EmployeeId equals e.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join b in branchRepository.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join dep in departmentRepository.All().AsNoTracking()
                        on e.DepartmentCode equals dep.DepartmentCode into departmentGroup
                        from dep in departmentGroup.DefaultIfEmpty()
                        join des in designationRepository.All().AsNoTracking() on e.DesignationCode equals des.DesignationCode into designationGroup
                        from des in designationGroup.DefaultIfEmpty()
                        join eType in empTypeRepository.All().AsNoTracking() on e.EmpTypeCode equals eType.EmpTypeCode into eTypeGroup
                        from eType in eTypeGroup.DefaultIfEmpty()
                        join eStatus in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals eStatus.EmployeeStatusId into eStatusGroup
                        from eStatus in eStatusGroup.DefaultIfEmpty()
                        join periodInfo in periodRepository.All().AsNoTracking() on e.ProbationPeriodType equals periodInfo.PeriodInfoId into periodGroup
                        from periodInfo in periodGroup.DefaultIfEmpty()
                        where e.EmployeeId == selectedEmpId 
                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            BranchName = b.BranchName,
                            DepartmentName = dep.DepartmentName,
                            DesignationName = des.DesignationName,
                            GrossSalary = e.GrossSalary,
                            e.JoiningDate,
                            e.ProbationPeriod,
                            e.ProbationPeriodType,
                            ProbetionPeriod = "",
                            EndDate = "",
                            //ServiceLength =""
                            PeriodName = periodInfo.PeriodName,
                            ShortName = periodInfo.ShortName
                        };

            var data = await query.FirstOrDefaultAsync();

            if (data == null)
                return null;

            string serviceLength = "";
            string probetionPeriod = "";
            DateTime? endDate = null;

            if (data.JoiningDate.HasValue && data.JoiningDate.Value != new DateTime(1900, 1, 1))
            {
                serviceLength = CalculateDateLength(data.JoiningDate.Value, DateTime.Today);

                if(data.ProbationPeriod !=null && data.ProbationPeriodType != null)
                {
                    endDate = CalculateProbationEndDate(data.JoiningDate.Value, data.ProbationPeriod, data.ProbationPeriodType);
                        if(endDate != null)
                        probetionPeriod = CalculateDateLengthInDays(data.JoiningDate.Value, endDate.Value);
                }
            }


            return new EmployeeListItemViewModel
            {
                EmployeeId = data.EmployeeId,
                Name = string.Join(" ", new[] { data.FirstName, data.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                JoiningDate = data.JoiningDate.HasValue ? data.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                DesignationName = data.DesignationName,
                DepartmentName = data.DepartmentName,
                GrossSalary = data.GrossSalary.ToString(),
                //Pro
                ProbationPeriod = probetionPeriod,
                EndOn = endDate.HasValue ? endDate.Value.ToString("dd/MM/yyyy") :"",
                ServiceLength = serviceLength
            };
        }

        private DateTime? CalculateProbationEndDate(DateTime value, string probationPeriod, string probationPeriodType)
        {
            switch(probationPeriodType)
            {
                case "01": // Years
                    return value.AddYears(int.Parse(probationPeriod));
                case "02": // Half-Years
                    return value.AddMonths(int.Parse(probationPeriod) * 6);
                case "03": // Quarter-Years
                    return value.AddMonths(int.Parse(probationPeriod) * 3);
                case "04": // Months
                    return value.AddMonths(int.Parse(probationPeriod));
                case "05": // Weeks
                    return value.AddDays(int.Parse(probationPeriod) * 7);
                case "06": // Days
                    return value.AddDays(int.Parse(probationPeriod));
                default:
                    return null;
            }

            throw new NotImplementedException();
        }
        private string CalculateDateLengthInDays(DateTime startDate, DateTime endDate)
        {
            if (startDate <= endDate)
            {
                if (startDate == new DateTime(1900, 1, 1) || endDate == new DateTime(1900, 1, 1))
                    return "";

                var totalDays = (endDate - startDate).Days;
                return $"{totalDays} {(totalDays == 1 ? "day" : "days")}";
            }
            else
            {
                return "";
            }
        }
        private string CalculateDateLength(DateTime startDate, DateTime endDate)
        {
            if (startDate <= endDate)
            {
                if (startDate == new DateTime(1900, 1, 1) || endDate == new DateTime(1900, 1, 1))
                    return "";

                var years = endDate.Year - startDate.Year;
                var months = endDate.Month - startDate.Month;
                var days = endDate.Day - startDate.Day;

                if (days < 0)
                {
                    months--;
                    days += DateTime.DaysInMonth(endDate.AddMonths(-1).Year, endDate.AddMonths(-1).Month);
                }

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                var parts = new List<string>();

                if (years > 0)
                {
                    parts.Add($"{years} {(years == 1 ? "year" : "years")}");
                }

                if (months > 0)
                {
                    parts.Add($"{months} {(months == 1 ? "month" : "months")}");
                }

                if (days > 0)
                {
                    parts.Add($"{days} {(days == 1 ? "day" : "days")}");
                }
    
                return string.Join(" ", parts); 
            }
            else
            {
                return "";
            }
        }

        public async Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {
            var result = new EmployeeFilterResultDto();

            var query = from emp in employeeRepository.All().AsNoTracking()
                        join e in employeeOfficialInfoRepository.All().AsNoTracking() on emp.EmployeeId equals e.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join des in designationRepository.All().AsNoTracking() on e.DesignationCode equals des.DesignationCode into designationGroup
                        from des in designationGroup.DefaultIfEmpty()
                        join eType in empTypeRepository.All().AsNoTracking() on e.EmpTypeCode equals eType.EmpTypeCode into eTypeGroup
                        from eType in eTypeGroup.DefaultIfEmpty()
                        join eStatus in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals eStatus.EmployeeStatusId into eStatusGroup
                        from eStatus in eStatusGroup.DefaultIfEmpty()

                        where ((e.ConfirmeDate == null || e.ConfirmeDate == new DateTime(1900, 1, 1)) && e.EmployeeStatus.Equals("01") && e.EmpTypeCode.Equals("02"))

                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName
                        };

            query = query.Where(x => x.CompanyCode == "001");

            if (!string.IsNullOrWhiteSpace(model.EmployeeID))
                query = query.Where(x => x.EmployeeId == model.EmployeeID);

            if (!string.IsNullOrWhiteSpace(model.CompanyCode))
                query = query.Where(x => x.CompanyCode == model.CompanyCode);

            var filteredData = await query.ToListAsync();

            result.LookupData["companies"] = filteredData
                .Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new LookupItemDto { Code = x.CompanyCode, Name = x.CompanyName })
                .DistinctBy(x => new { x.Code, x.Name })
                .ToList();

            result.LookupData["employees"] = filteredData
                .Where(x => x.EmployeeId != null)
                .Select(x => new LookupItemDto {
                    Code = x.EmployeeId,
                    Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({x.EmployeeId})"
                })
                .Distinct()
                .ToList();

            return result;
        }

        public async Task<(List<HrmServiceNotConfirmViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from e in entryRepository.All().AsNoTracking()
                        join em in employeeRepository.All().AsNoTracking() on e.EmployeeId equals em.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on em.EmployeeId equals ei.EmployeeId
                        select new HrmServiceNotConfirmViewModel
                        {
                            Tc = e.Tc,
                            Sncid = e.Sncid,
                            EmployeeId = e.EmployeeId,
                            EmployeeName = (em.FirstName ?? "") + (em.LastName != null && em.LastName != "" ? " " + em.LastName : ""),

                            EffectiveDate = e.EffectiveDate.HasValue ? e.EffectiveDate.Value.Date : null,
                            DuePaymentDate = e.DuePaymentDate.HasValue ? e.DuePaymentDate.Value.Date : null,
                            RefLetterDate = e.RefLetterDate.HasValue ? e.RefLetterDate.Value.Date : null,
                            RefLetterNo = e.RefLetterNo ?? "",
                            Remarks = e.Remarks ?? ""
                        };

            var materializedQuery = await query.ToListAsync();

            IEnumerable<HrmServiceNotConfirmViewModel> filterQuery = materializedQuery;

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                filterQuery = filterQuery.Where(d =>
                (d.EmployeeName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.EmployeeId?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.Sncid?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.EffectiveDate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.DuePaymentDate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.RefLetterNo?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.RefLetterDate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.Remarks?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            var totalRecords = filterQuery.Count();

            if(!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                filterQuery = sortColumn.ToLower() switch
                {
                    "sncid" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(x => x.Sncid) : filterQuery.OrderByDescending(x => x.Sncid),
                    "employeeid" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.EmployeeId) : filterQuery.OrderByDescending(a => a.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.EmployeeName) : filterQuery.OrderByDescending(a => a.EmployeeName),
                    "effectivedate" => sortDirection.ToLower() == "asc" ?
                    filterQuery.OrderBy(a => a.EffectiveDate) : filterQuery.OrderByDescending(a => a.EffectiveDate),
                    "duepaymentdate" => sortDirection.ToLower() == "asc" ?
                    filterQuery.OrderBy(a => a.DuePaymentDate) : filterQuery.OrderByDescending(a => a.DuePaymentDate),
                    "refletterno" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.RefLetterNo) : filterQuery.OrderByDescending(a => a.RefLetterNo),
                    "refletterdate" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.RefLetterDate) : filterQuery.OrderByDescending(a => a.RefLetterDate),
                    "remarks" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.Remarks) : filterQuery.OrderByDescending(a => a.Remarks),
                    _ => filterQuery.OrderBy(a => a.Tc)
                };
            }
            else
            {
                filterQuery = filterQuery.OrderBy(a => a.Tc);
            }

            var data = pageSize < 0
                ? filterQuery.ToList()
                : filterQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return (data, totalRecords);
        }

        public Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ProcessExcelFileAsync(Stream stream, HrmServiceNotConfirmViewModel model)
        {
            throw new NotImplementedException();
        }


        public async Task<bool> SaveAsync(HrmServiceNotConfirmViewModel model)
        {
            if (model == null || model.EmployeeId == null)
                return false;

            await employeeOfficialInfoRepository.BeginTransactionAsync();

            try
            {
                var newId = await GenerateSNCIdAsync();
                HrmServiceNotConfirmationEntry record = new HrmServiceNotConfirmationEntry
                {
                    Sncid = newId,
                    EmployeeId = model.EmployeeId,
                    EffectiveDate = model.EffectiveDate,
                    DuePaymentDate = model.DuePaymentDate,
                    RefLetterNo = model.RefLetterNo ?? "",
                    RefLetterDate = model.RefLetterDate,
                    Remarks = model.Remarks ?? "",
                    Ldate = model.Ldate,
                    Lip = model.Lip,
                    Lmac = model.Lmac,
                    Luser = model.Luser,
                    CompanyCode = model.CompanyCode,
                };

                await entryRepository.AddAsync(record);

                string newSeparationId = await GenerateSeparationIdAsync();

                HrmSeparation seprecord = new HrmSeparation
                {
                    SeparationId = newSeparationId,
                    EmployeeId = model.EmployeeId,
                    SeparationDate = model.EffectiveDate.HasValue ? model.EffectiveDate.Value : default,
                    SeparationTypeId = "04",
                    FinalPayment = 0,
                    IsPaid = "Y",
                    Remark = "",
                    Ldate = model.Ldate,
                    Lip = model.Lip,
                    Lmac = model.Lmac,
                    Luser = model.Luser,
                    CompanyCode = model.CompanyCode,
                    RefLetterNo = null,
                    RefLetterDate = null
                };

                await separationRepository.AddAsync(seprecord);

                var existingOfficialInfo = await employeeOfficialInfoRepository.All()
                    .Where(e => e.EmployeeId == model.EmployeeId)
                    .FirstOrDefaultAsync();

                if (existingOfficialInfo == null)
                {
                    await employeeOfficialInfoRepository.RollbackTransactionAsync();
                    return false;
                }

                //if (existingOfficialInfo.EmployeeStatus == "02")
                //    return true;

                existingOfficialInfo.EmployeeStatus = "02";
                await employeeOfficialInfoRepository.UpdateAsync(existingOfficialInfo);

                await employeeOfficialInfoRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await employeeOfficialInfoRepository.RollbackTransactionAsync();
                return false;
            }
        }

        private async Task<string> GenerateSeparationIdAsync()
        {
            string newId = "00000001";
            try
            {
                var maxId = separationRepository.All()
                    .AsEnumerable() // Switch to LINQ-to-Objects
                    .Where(e => !string.IsNullOrEmpty(e.SeparationId) && e.SeparationId.All(char.IsDigit))
                    .Select(e => int.Parse(e.SeparationId))
                    .DefaultIfEmpty(0)
                    .Max();

                newId = (maxId + 1).ToString("D8");
            }
            catch (Exception ex)
            {
                newId = "00000001";
            }
            return newId;
        }
    }
}
