using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.AccessCodes;
using GCTL.Core.ViewModels.Common;
using GCTL.Data.Models;
using System.Security.Cryptography.X509Certificates;

namespace GCTL.Service.Users
{
    public class AccessCodeService : AppService<CoreAccessCode>, IAccessCodeService
    {
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<CoreMenuTab2> menuRepository;
        private readonly IMapper mapper;

        public AccessCodeService(IRepository<CoreAccessCode> accessCodeRepository,
                                 IRepository<CoreMenuTab2> menuRepository,
                                 IMapper mapper)
            : base(accessCodeRepository)
        {
            this.accessCodeRepository = accessCodeRepository;
            this.menuRepository = menuRepository;
            this.mapper = mapper;
        }

        public List<CoreAccessCode> GetAccessCodes()
        {
            return GetAll();
        }

        public List<CoreAccessCode> GetUniqueAccessCodes()
        {
            return (from accessCode in accessCodeRepository.All()
                    select new CoreAccessCode
                    {
                        AccessCodeId = accessCode.AccessCodeId,
                        AccessCodeName = accessCode.AccessCodeName
                    })
                    .Distinct()
                    .ToList();
        }

        public CoreAccessCode GetAccessCode(string id)
        {
            return accessCodeRepository.All().FirstOrDefault(x => x.AccessCodeId == id);
        }

        public async Task<List<AccessCodeModel>> GetAccessCodesAsync(string accessCode)
        {
            List<AccessCodeModel> permissions = new List<AccessCodeModel>();
            var accessCodes = await (from access in accessCodeRepository.All()
                                     where access.AccessCodeId == accessCode && access.TitleCheck && access.IsActive
                                     orderby access.OrderBy
                                     select access).ToListAsync();

            List<AccessCodeModel> accesses = new List<AccessCodeModel>();
            mapper.Map(accessCodes, accesses);

            foreach (var parent in accesses.Where(x => x.ParentId == "0"))
            {
                parent.Children = accesses.Where(x => x.ParentId != "0" && x.ParentId == parent.MenuId).ToList();
                permissions.Add(parent);
            }

            return permissions.ToList();
        }

        public CoreAccessCode SaveAccessCode(CoreAccessCode entity)
        {
            if (IsAccessCodeExistByCode(entity.AccessCodeName))
                Update(entity);
            else
                Add(entity);

            return entity;
        }

        public void UpdateAccessCode(CoreMenuTab2 entity)
        {
            var accessCodes = accessCodeRepository.All().Where(x => x.MenuId == entity.MenuId).ToList();
            foreach (var accessCode in accessCodes)
            {
                accessCode.Title = entity.Title;
                accessCode.ParentId = entity.ParentId;
                accessCode.OrderBy = entity.OrderBy;
                accessCode.ControllerName = entity.ControllerName;
                accessCode.ViewName = entity.ViewName;
                accessCode.Icon = entity.Icon;
                accessCode.IsActive = entity.IsActive;
                Update(accessCode);
            }
        }

        public bool SetPermissions(AccessCodeSetupViewModel model)
        {
            var existingPermissions = accessCodeRepository.All().Where(x => x.AccessCodeId == model.AccessCodeId).ToList();
            if (existingPermissions.Any())
            {
                accessCodeRepository.Delete(existingPermissions);
            }

            List<CoreAccessCode> accessCodes = new List<CoreAccessCode>();

            foreach (var parent in model.Accesses.Where(x => x.TitleCheck))
            {
                CoreAccessCode code = new CoreAccessCode();
                mapper.Map(parent, code);
                code.AccessCodeId = model.AccessCodeId;
                code.AccessCodeName = model.AccessCodeName;

                accessCodes.Add(code);

                foreach (var child in parent.Children.Where(x => x.TitleCheck))
                {
                    code = new CoreAccessCode();
                    mapper.Map(child, code);
                    code.AccessCodeId = model.AccessCodeId;
                    code.AccessCodeName = model.AccessCodeName;

                    accessCodes.Add(code);
                }
            }


            //mapper.Map(model.Accesses.Where(x => x.TitleCheck).ToList(), accessCodes);

            //foreach (var accessCode in accessCodes)
            //{
            //    accessCode.AccessCodeId = model.AccessCodeId;
            //    accessCode.AccessCodeName = model.AccessCodeName;
            //}
            accessCodeRepository.Add(accessCodes);

            return true;
        }

        public bool DeleteAccessCode(string id)
        {
            var entity = GetAccessCode(id);
            if (entity != null)
            {
                accessCodeRepository.Delete(entity);
                return true;
            }
            return false;
        }

        public bool IsAccessCodeExistByCode(string code)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == code);
        }

        public bool IsAccessCodeExist(string name)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeName == name);
        }

        public bool IsAccessCodeExist(string name, string typeCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeName == name && x.AccessCodeId != typeCode);
        }

        public IEnumerable<CommonSelectModel> AccessCodeSelection()
        {
            return accessCodeRepository.All()
                .Select(x => new CommonSelectModel
                {
                    Code = x.AccessCodeId,
                    Name = x.AccessCodeName
                });
        }

        public List<AccessCodeModel> GetMenus()
        {
            List<AccessCodeModel> accesses = new List<AccessCodeModel>();
            var navs = (from menu in menuRepository.All()
                        where menu.IsActive 
                        select new AccessCodeModel
                        {
                            MenuId = menu.MenuId,
                            Title = menu.Title,
                            ParentId = menu.ParentId,
                            OrderBy = menu.OrderBy,
                            ControllerName = menu.ControllerName,
                            ViewName = menu.ViewName,
                            Icon = menu.Icon,
                            IsActive = menu.IsActive
                        })
                        .AsEnumerable()
                        .OrderBy(x => x.ParentId)
                        .ThenBy(x => x.OrderBy)
                        .ToList();

            foreach (var parent in navs.Where(x => x.ParentId == "0"))
            {
                parent.Children = navs.Where(x => x.ParentId != "0" && x.ParentId == parent.MenuId).ToList();
                accesses.Add(parent);
            }

            return accesses;
        }

        public List<AccessCodeModel> GetPermissionsAccessCode(string accessCode = "")
        {
            List<AccessCodeModel> result = GetMenus();

            if (!string.IsNullOrWhiteSpace(accessCode))
            {
                foreach (var item in result)
                {
                    var permission = GetPermission(accessCode, item.MenuId);
                    if (permission != null)
                    {
                        item.TitleCheck = permission.TitleCheck;
                        item.CheckAdd = permission.CheckAdd;
                        item.CheckEdit = permission.CheckEdit;
                        item.CheckDelete = permission.CheckDelete;
                        item.CheckPrint = permission.CheckPrint;

                        if (item.Children.Any())
                        {
                            foreach (var child in item.Children)
                            {
                                permission = GetPermission(accessCode, child.MenuId);
                                if (permission != null)
                                {
                                    child.TitleCheck = permission.TitleCheck;
                                    child.CheckAdd = permission.CheckAdd;
                                    child.CheckEdit = permission.CheckEdit;
                                    child.CheckDelete = permission.CheckDelete;
                                    child.CheckPrint = permission.CheckPrint;
                                }
                            }
                        }

                    }
                }
            }

            return result;
        }

        private CoreAccessCode GetPermission(string accessCode, string menuId)
        {
            return accessCodeRepository.All().FirstOrDefault(x => x.AccessCodeId == accessCode && x.MenuId == menuId);
        }

        public bool HasPermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Access Code" && x.TitleCheck);
        }
    }
}