using GCTL.Core.ViewModels.Common;
using GCTL.Data.Models;

namespace GCTL.Service.Designations
{
    public interface IDesignationService
    {
        List<HrmDefDesignation> GetDesignations();
        HrmDefDesignation GetDesignation(string code); 
        bool DeleteDesignation(string id);    
        HrmDefDesignation SaveDesignation(HrmDefDesignation entity);
        bool IsDesignationExistByCode(string code);
        bool IsDesignationExist(string name);
        bool IsDesignationExist(string name, string typeCode);
        IEnumerable<CommonSelectModel> DesignationSelection();
        bool SavePermission(string accessCode);
        bool UpdatePermission(string accessCode);
        bool DeletePermission(string accessCode);
    }
}