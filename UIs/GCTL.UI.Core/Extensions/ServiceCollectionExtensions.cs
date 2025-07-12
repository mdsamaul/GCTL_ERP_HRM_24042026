﻿using GCTL.Core.Data;
using GCTL.Data;
using GCTL.Data.Models;
using GCTL.Service.Common;
using GCTL.Service.Companies;

using GCTL.Service.UnitTypes;
using GCTL.Service.Users;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.Currencies;
using GCTL.Service.PaymentModes;
//using GCTL.Service.PaymentTypes;
using GCTL.Service.Doctors;
using GCTL.Service.DoctorTypes;
using GCTL.Service.Specialities;
using GCTL.Service.Qualifications;
using GCTL.Service.Shifts;
using GCTL.Service.BloodGroups;
using GCTL.Service.Religions;
using GCTL.Service.Relation;
using GCTL.Service.Designations;
using GCTL.Service.Departments;

//using GCTL.Service.ReferencePersons;

using GCTL.Service.Reports;
using GCTL.Core.Configurations;
using Microsoft.Extensions.Options;

using GCTL.Service.LogsLoggers;
using GCTL.Service.Loggers;

using GCTL.Service.AdmissionTypes;

using GCTL.Service.Units;

using GCTL.Service.People;
using GCTL.Service.Periods;

using GCTL.Service.DeliveryPeriods;
using Microsoft.AspNetCore.Http.Features;
using GCTL.Service.Surnames;

using GCTL.Service.DoctorWorkingPalace;
using GCTL.Service.LeaveTypes;
using GCTL.Service.HrmAtdShifts;
using GCTL.Service.HRMCompanyWeekEnds;
using GCTL.Service.HolidayTypes;
using GCTL.Service.HrmAtdHolidays;
using GCTL.Service.HRMAttWorkingDayDeclarations;
using GCTL.Service.HRMATDAttendanceTypes;
using GCTL.Service.HrmDefEmpTypes;
using GCTL.Service.HrmEmployees2;
//using GCTL.Service.BankInformations;
//using GCTL.Service.BankBranchInformations;
//using GCTL.Service.CoreBankAccountInformations;
//using GCTL.Service.HrmEmployeeAdditionalInfos;
using GCTL.Service.HrmDefDegrees;
using GCTL.Service.HRMDefBoardCountryNames;
using GCTL.Service.HRMDefExamTitles;
using GCTL.Service.HrmDefInstitutes;
//using GCTL.Service.HrmEmployeeEducations;
using GCTL.Service.HrmDefExamGroupInfos;
//using GCTL.Service.EmployeeOfficialInfo;
using GCTL.Service.BranchesTypeInfo;
using GCTL.Service.Districts;
//using GCTL.Service.EmployeeContactInfos;
using GCTL.Service.Relationships;
using GCTL.Service.HrmDefPerformances;
using GCTL.Service.PerformancesType;
using GCTL.Service.HrmDefSeparationTypes;
using GCTL.Service.JobTitles;
using GCTL.Service.HrmDefGradeTypes;
using GCTL.Service.Grades;
using GCTL.Service.HrmDefOccupations;
//using GCTL.Service.HrmEmployeeFamilys;
//using GCTL.Service.HrmEmployeeQualifications;
//using GCTL.Service.HrmEmployeeDocumentInfos;
using GCTL.Service.CompanyInfos;
//using GCTL.Service.EmployeeReferenceInfos;
using GCTL.Service.ExperianceInfos;
using GCTL.Service.Nationalitys;
using GCTL.Service.SeparationInfos;
//using GCTL.Service.ManualAttendances;
//using GCTL.Service.ManualAttendanceBulks;
using GCTL.Service.HrmAtdMachineDatas;
//using GCTL.Service.HrmDefBankAndNomineeInfos;
using GCTL.Service.Branches;
using GCTL.Service.CoreBranches;
using GCTL.Service.EmailService;
using GCTL.Service.FileHandle;
//using GCTL.Service.HrmLeaveApplicationEntrys;
//using GCTL.Service.IncrementService;
//using GCTL.Service.LeaveAppDay;
using GCTL.Service.HrmEmployeeOfficialInfoServe;
using GCTL.Service.Employees;
//using GCTL.Service.EducationalInfoReport;
using GCTL.Service.EmployeeGeneralInfoReport;
//using GCTL.Service.EmployeeReferenceInformationReport;
//using GCTL.Service.WorkingExperienceReport;
//using GCTL.Service.TaxAdjustmentEntry;
//using GCTL.Service.Reposition;
using GCTL.Service.SeparationTypes;
using GCTL.Service.EmployeeWeekendDeclaration;
using GCTL.Service.ManualEarnLeaveEntry;
using GCTL.Service.PFAssignEntry;
using GCTL.Service.RosterScheduleApproval;
using GCTL.Service.RosterScheduleEntry;
using GCTL.Service.RosterScheduleReport;
using GCTL.Service.EmployeeWeekendDeclarationReport;
using GCTL.Service.PFAssignEntryReport;
using GCTL.Service.HRMPayrollLoan;
using GCTL.Service.AdvanceLoanAdjustment;
using GCTL.Service.AdvanceLoanAdjustmentReport;
using GCTL.Service.EmployeeLoanInformationReport;
using GCTL.Service.INV_Catagory;
using GCTL.Service.HRM_Brand;
using GCTL.Service.ItemMasterInformation;
using GCTL.Service.HRM_Size;
using GCTL.Service.RMG_Prod_Def_UnitType;
using GCTL.Service.PrintingStationeryPurchaseEntry;


//using GCTL.Service.AccountReport;



namespace GCTL.UI.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {      
        public static void ConfigureContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GCTL_ERP_DB_DatapathContext>(x =>
            x.UseSqlServer(configuration.GetConnectionString("ApplicationDbConnection"),
            x => x.UseDateOnlyTimeOnly()).EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information));
        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IRelationService, RelationService>();
            services.AddScoped<IDoctorWorkingPlaceService, DoctorWorkingPlaceService>();
            services.AddScoped<IBloodGroupService, BloodGroupService>();
            services.AddScoped<IReligionService, ReligionService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccessCodeService, AccessCodeService>();
            services.AddScoped<IEncoderService, EncoderService>();
            services.AddScoped<IUnitTypeService, UnitTypeService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IPaymentModeService, PaymentModeService>();
            
           



            services.AddScoped<IDoctorTypeService, DoctorTypeService>();
            services.AddScoped<ISpecialityService, SpecialityService>();
            services.AddScoped<IQualificationService, QualificationService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IDoctorService, DoctorService>();
            

            //services.AddScoped<IReferencePersonService, ReferencePersonService>();


            services.AddScoped<IAdmissionTypeService, AdmissionTypeService>();
      
            services.AddScoped<ISurnameService, SurnameService>();


            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IPeriodService, PeriodService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IMeasurementUnitService, MeasurementUnitService>();
        
            services.AddScoped<IDeliveryPeriodService, DeliveryPeriodService>();

            //services.AddScoped<IAccountReportService, AccountReportService>();

          
            //samaul

services.AddScoped<IEmployeeWeekendDeclarationService, EmployeeWeekendDeclarationService>(); 
services.AddScoped<IAdvanceLoanAdjustmentReportServices, AdvanceLoanAdjustmentReportServices>(); 
services.AddScoped<IEmployeeLoanInformationReportServices, EmployeeLoanInformationReportServices>(); 
services.AddScoped<IManualEarnLeaveEntryService, ManualEarnLeaveEntryService>();
services.AddScoped<IPFAssignEntryService, PFAssignEntryService>(); 
services.AddScoped<IRosterScheduleEntryService, RosterScheduleEntryService>(); 
services.AddScoped<IRosterScheduleApprovalService, RosterScheduleApprovalService>();
services.AddScoped<IRosterScheduleReportServices, RosterScheduleReportServices>();
services.AddScoped<IEmployeeWeekendDeclarationReportServices, EmployeeWeekendDeclarationReportServices>();
services.AddScoped<IPFAssignEntryReportServices, PFAssignEntryReportServices>();
services.AddScoped<IAdvanceLoanAdjustmentServices, AdvanceLoanAdjustmentServices>();
services.AddScoped<IHRMPayrollLoanService, HRMPayrollLoanService>();
            services.AddScoped<IReportService, ReportService>();

            services.AddScoped<IINV_CatagoryService, INV_CatagoryService>();
            services.AddScoped<IHRM_BrandService, HRM_BrandService>();
            services.AddScoped<IItemMasterInformationService, ItemMasterInformationService>();
            services.AddScoped<IHRM_SizeService, HRM_SizeService>();
            services.AddScoped<IRMG_Prod_Def_UnitTypeService , RMG_Prod_Def_UnitTypeService>();
            services.AddScoped<IPrintingStationeryPurchaseEntryService , PrintingStationeryPurchaseEntryService>();


            
          

            services.AddScoped<ILogService, LogService>();

            // Start from here 
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddScoped<IHrmAtdShiftService, HrmAtdShiftService>();
            services.AddScoped<IHRMCompanyWeekEndService, HRMCompanyWeekEndService>();
            services.AddScoped<IHrmDefHolidayTypeService, HrmDefHolidayTypeService>();
            services.AddScoped<IHrmAtdHolidayService, HrmAtdHolidayService>();
            services.AddScoped<IHRMAttWorkingDayDeclarationService, HRMAttWorkingDayDeclarationService>();
            services.AddScoped<IHRMATDAttendanceTypeService, HRMATDAttendanceTypeService>();
            services.AddScoped<IHrmDefEmpTypeService, HrmDefEmpTypeService>();
            services.AddScoped<IHrmEmployee2Service, HrmEmployee2Service>();
            //services.AddScoped<IBankInformationsService, BankInformationsService>();
            //services.AddScoped<ISalesDefBankBranchInfosService, SalesDefBankBranchInfosService>();
            // services.AddScoped<ICoreBankAccountInformationService, CoreBankAccountInformationService>();
            //services.AddScoped<IHrmEmployeeAdditionalInfosService, HrmEmployeeAdditionalInfosService>();
            services.AddScoped<IHrmDefDegreesService, HrmDefDegreesService>();
            services.AddScoped<IHRMDefBoardCountryNamesService, HRMDefBoardCountryNamesService>();
            services.AddScoped<IHRMDefExamTitlesService,HRMDefExamTitlesService>();
            services.AddScoped<IHrmDefInstitutesService, HrmDefInstitutesService>();
            //services.AddScoped<IHrmEmployeeEducationsService, HrmEmployeeEducationsService>();
            services.AddScoped<IHrmDefExamGroupInfosService, HrmDefExamGroupInfosService>();
            services.AddScoped<IBranchTypeInfoService, BranchTypeInfoService>();
            services.AddScoped<IDistrictsService, DistrictsService>();
            //services.AddScoped<IEmployeeContactInfoService,  EmployeeContactInfoService>();
            services.AddScoped<IRelationshipsService, RelationshipsService>();
            services.AddScoped<IHrmDefPerformanceService,  HrmDefPerformanceService>();
            services.AddScoped<IHrmPerformancesTypeService, HrmPerformancesTypeService>();
            services.AddScoped<IHrmDefSeparationTypeService,  HrmDefSeparationTypeService>();
            services.AddScoped<IJobTitleService, JobTitleService>();
            //services.AddScoped<IHrmEmployeeFamilysService, HrmEmployeeFamilysService>();
            //services.AddScoped<IEmployeeOfficialInfoService, EmployeeOfficialInfoService>();
            services.AddScoped<IHrmDefGradeTypesService, HrmDefGradeTypesService>();
            services.AddScoped<IGradesService, GradesService>();
            services.AddScoped<IHrmDefOccupationsService, HrmDefOccupationsService>();
            //services.AddScoped<IHrmEmployeeQualificationsService, HrmEmployeeQualificationsService>();

            //services.AddScoped<IHrmEmployeeDocumentInfosService,HrmEmployeeDocumentInfosService>();
            services.AddScoped<INationalitysService, NationalitysService>();
            //services.AddScoped<IEmployeeReferenceInfosService, EmployeeReferenceInfosService>();
            services.AddScoped<ICompanyInfosService, CompanyInfosService>();
            services.AddScoped<IExperianceInfosService, ExperianceInfosService>();
            services.AddScoped<ISeparationInfosService, SeparationInfosService>();
            //services.AddScoped<IManualAttendanceService, ManualAttendanceService>();
            //services.AddScoped<IManualAttendanceBulkService, ManualAttendanceBulkService>();
            services.AddScoped<IHrmAtdMachineDataService, HrmAtdMachineDataService>();
            //services.AddScoped<IHrmDefBankAndNomineeInfosService, HrmDefBankAndNomineeInfosService>();
            //Siam Report 20-03-2025
            services.AddScoped<IEmployeeGeneralInfoReportService, EmployeeGeneralInfoReportService>();
            //services.AddScoped<IEducationalInfoReportService, EducationalInfoReportService>();
            //services.AddScoped<IEmployeeReferenceInformationReportService, EmployeeReferenceInformationReportService>();
            //services.AddScoped<IWorkingExperienceReportService, WorkingExperienceReportService>();
            // End Siam Report 20-03-2025

            services.AddScoped<IHrmEmployeeOfficialInfoService, HrmEmployeeOfficialInfoService>();
            //services.AddScoped<IHrmLeaveApplicationEntry, HrmLeaveApplicationEntryService>();
            services.AddScoped<IFileHandle, FileHandleService>();
            //services.AddScoped<ILeaveApplicationDay, LeaveAppicationDayService>();
            services.AddScoped<ICoreBranch, CoreBranchService>();
            services.AddScoped<IEmailService, EmailService>();
            //services.AddScoped<ILeaveEmailService, LeaveEmailService>();
            //services.AddScoped<IIncrementService, IncrementService>();
			//services.AddScoped<ITaxAdjustmentEntryService, TaxAdjustmentEntryService>();
			//services.AddScoped<IRepositionService, RepositionService>();
			//services.AddScoped<IRepostionExcelService, RepostionExcelService>();





			
			
			
			
			
			
			
			
			
			
		
			
			
			
			services.AddScoped<ISeparationTypesService, SeparationTypesService>();




			//with TKey reposotory for Test 

			services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepositoryTest<,>));





        }

        public static void ConfigureRequestFormSizeLimit(this IServiceCollection services)
        {
            services.Configure<FormOptions>(x => x.ValueCountLimit = 10000);
        }

        public static void ConfigureMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Program));
        }

        public static void ConfigureSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public static void ReadConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApplicationSettings>>().Value);

            services.Configure<SMSSetting>(configuration.GetSection("SMSSetting"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<SMSSetting>>().Value);
        }
    }
}
