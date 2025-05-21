using GCTL.Core.ViewModels.Common;
using GCTL.Core.ViewModels.HolidayTypes;
using GCTL.Core.ViewModels.HRMATDAttendanceTypes;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HRMATDAttendanceTypes
{
    public interface IHRMATDAttendanceTypeService
    {
        HrmAtdAttendanceType GetLeaveType(string code);
        bool DeleteLeaveType(string id);
        Task<List<HRMATDAttendanceTypeSetupViewModel>> GetAllAsync();
        Task<HRMATDAttendanceTypeSetupViewModel> GetByIdAsync(string code);
        Task<bool> SaveAsync(HRMATDAttendanceTypeSetupViewModel entityVM);
        Task<bool> UpdateAsync(HRMATDAttendanceTypeSetupViewModel entityVM);
        Task<bool> DeleteAsync(string id);
        Task<string> GenerateNextCode();
        Task<bool> IsExistByCodeAsync(string code);
        Task<bool> IsExistAsync(string name);
        Task<bool> IsExistAsync(string name, string typeCode);
        Task<IEnumerable<CommonSelectModel>> SelectionHrmAttendanceTypeAsync();
        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
    }
}
