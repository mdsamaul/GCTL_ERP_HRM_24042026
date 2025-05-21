using GCTL.Core.ViewModels.Common;
using GCTL.Core.ViewModels.JobTitles;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.JobTitles
{
    public interface IJobTitleService
    {
        HrmDefJobTitle GetLeaveType(string code);
        bool DeleteLeaveType(string id);
        Task<List<JobTitleSetupViewModel>> GetAllAsync();
        Task<JobTitleSetupViewModel> GetByIdAsync(string code);
        Task<bool> SaveAsync(JobTitleSetupViewModel entityVM);
        Task<bool> UpdateAsync(JobTitleSetupViewModel entityVM);

        Task<string> GenerateNextCode();
        Task<bool> IsExistByCodeAsync(string code);
        Task<bool> IsExistAsync(string name);
        Task<bool> IsExistAsync(string name, string typeCode);
        Task<IEnumerable<CommonSelectModel>> SelectionHrmDefJobTitleAsync();

        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
    }
}
