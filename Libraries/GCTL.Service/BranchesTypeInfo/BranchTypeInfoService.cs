using GCTL.Core.Data;
using GCTL.Core.ViewModels.BranchesTypeInfo;
using GCTL.Core.ViewModels.Common;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using static Dapper.SqlMapper;

namespace GCTL.Service.BranchesTypeInfo
{
    public class BranchTypeInfoService : AppService<CoreBranch>, IBranchTypeInfoService
    {
        private readonly IRepository<CoreBranch> _coreBranchrepository;
        private readonly IRepository<CoreAccessCode> _accessCodeRepository;
        private readonly IRepository<CoreCompany> _companyRepository;
       

        public BranchTypeInfoService(IRepository<CoreBranch> coreBranchrepository, IRepository<CoreAccessCode> accessCodeRepository, IRepository<CoreCompany> companyRepository) : base(coreBranchrepository)
        {
            _coreBranchrepository = coreBranchrepository;
            _accessCodeRepository = accessCodeRepository;
            _companyRepository = companyRepository;
        }

        public List<CoreBranch> GetBranches()
        {
            return GetAll();
        }


        public async Task <List<BranchTypeSetupViewModel>> GetCompaniess(string CompanyCode)
        {
            var data = await(from emp in _coreBranchrepository.All().AsNoTracking()
                        where emp.CompanyCode == CompanyCode
                        join empComp in _companyRepository.All().AsNoTracking()
                        on emp.CompanyCode equals empComp.CompanyCode into empComJoin
                        from empComp in empComJoin.DefaultIfEmpty()
                        select new BranchTypeSetupViewModel
                        {
                            BranchCode = emp.BranchCode,
                            BranchName = emp.BranchName,
                            CompanyCode = emp.CompanyCode,
                            CompanyName = empComp.CompanyName,
                            Address = emp.Address,
                            Phone = emp.Phone,
                            Email = emp.Email,
                            AddressBangla = emp.AddressBangla,
                            BanglaBranch = emp.BanglaBranch,
                            Fax = emp.Fax,
                        }).ToListAsync();

           
            Debug.WriteLine($"Records retrieved for CompanyCode {CompanyCode}: {data.Count}");

            return data;
        }


        public CoreBranch GetBranch(string id)
        {
            return _coreBranchrepository.GetById(id);
        }

        public BranchTypeSetupViewModel GetBranchTypeSetupView(string code)
        {
            var query = (from branch in _coreBranchrepository.All()
                         join company in _companyRepository.All()
                         on branch.CompanyCode equals company.CompanyCode into companyComJoin
                         from company in companyComJoin.DefaultIfEmpty()
                         where branch.BranchCode == code
                         select new BranchTypeSetupViewModel
                         {
                             BranchName = branch.BranchName,
                             BranchCode = branch.BranchCode,
                             CompanyCode = branch.CompanyCode,
                             Address = branch.Address,
                             AddressBangla = branch.AddressBangla,
                             BanglaBranch = branch.BanglaBranch,
                             Phone = branch.Phone,
                             Ldate = branch.Ldate,
                             ModifyDate = branch.ModifyDate,
                             Email = branch.Email,
                             Fax = branch.Fax,
                             Company = company.CompanyName

                         }).FirstOrDefault(); 

            return query;
        }


        public bool DeleteBranchTypeInfo(string id)
        {
            var entity = GetBranch(id);
            if (entity != null)
            {
                _coreBranchrepository.Delete(entity);
                return true;
            }
            return false;
        }

        

        public CoreBranch SaveBranchTypeInfo(CoreBranch entity)
        {
            if (IsBranchTypeInfoExistByCode(entity.BranchCode))
                Update(entity);
            else
                Add(entity);
            return entity;
        }

        public async Task< IEnumerable<CommonSelectModel>> GetCompanieBranchSelections()
        {
            return await _coreBranchrepository.All()
                 .Select(x => new CommonSelectModel
                 {
                     Code = x.BranchCode,
                     Name = x.BranchName,
                 }).ToListAsync();
        }


        public IEnumerable<CommonSelectModel> DropSelection()
        {
            return _companyRepository.All().Select(x => new CommonSelectModel
            {
                Code = x.CompanyCode,
                Name = x.CompanyName
            });
        }

        public IEnumerable<CommonSelectModel> GetCompaniesSelections()
        {
            return (from company in _companyRepository.All()
                    select new CommonSelectModel
                    {
                        Code = company.CompanyCode,
                        Name = company.CompanyName
                    }).Distinct().ToList();
        }

        public bool IsBranchTypeInfoExistByCode(string code)
        {
            return _coreBranchrepository.All().Any(x => x.BranchCode == code);
        }

        public bool IsBranchTypeInfoExist(string name)
        {
           return _coreBranchrepository.All().Any(x => x.BranchName == name);
        }

        public bool IsBranchTypeInfoExist(string name, string BranchCode)
        {
            return _coreBranchrepository.All().Any(x => x.BranchName == name && x.BranchCode != BranchCode);
        }

        public bool PagePermission(string accessCode)
        {
            return _accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Branch Type Information" && x.TitleCheck);
        }

        public bool SavePermission(string accessCode)
        {
            return _accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Branch Type Information" && x.CheckAdd);
        }

        public bool UpdatePermission(string accessCode)
        {
            return _accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Branch Type Information" && x.CheckEdit);
        }

        public bool DeletePermission(string accessCode)
        {
            return _accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Branch Type Information" && x.CheckDelete);
        }
    }
}
