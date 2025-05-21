using GCTL.Core.ViewModels.BranchesTypeInfo;
using GCTL.Core.ViewModels.Common;
using GCTL.Data.Models;


namespace GCTL.Service.BranchesTypeInfo
{
    public interface IBranchTypeInfoService
    {
        List<CoreBranch> GetBranches();
        CoreBranch GetBranch(string code);
        BranchTypeSetupViewModel GetBranchTypeSetupView(string code);
       Task <List<BranchTypeSetupViewModel>> GetCompaniess(string CompanyCode);
        CoreBranch SaveBranchTypeInfo(CoreBranch entity);
        bool IsBranchTypeInfoExistByCode(string code);
        bool IsBranchTypeInfoExist(string name);
        bool IsBranchTypeInfoExist(string name, string BranchCode);
        bool DeleteBranchTypeInfo(string id);
        bool PagePermission(string accessCode);
        bool SavePermission(string accessCode);
        bool UpdatePermission(string accessCode);
        bool DeletePermission(string accessCode);
       Task<IEnumerable<CommonSelectModel>> GetCompanieBranchSelections();
        IEnumerable<CommonSelectModel> DropSelection();
    }
}
