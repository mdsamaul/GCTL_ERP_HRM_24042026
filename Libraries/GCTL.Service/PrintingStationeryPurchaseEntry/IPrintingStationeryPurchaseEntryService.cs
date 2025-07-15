using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HrmAtdShifts;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Core.ViewModels.ItemMasterInformation;
using GCTL.Core.ViewModels.ItemModel;
using GCTL.Core.ViewModels.PrintingStationeryPurchaseEntry;
using GCTL.Core.ViewModels.SalesSupplier;
using GCTL.Data.Models;

namespace GCTL.Service.PrintingStationeryPurchaseEntry
{
    public interface IPrintingStationeryPurchaseEntryService
    {
        Task<List<PrintingStationeryPurchaseEntrySetupViewModel>> GetAllAsync();
        Task<PrintingStationeryPurchaseEntrySetupViewModel> GetByIdAsync(string id);
        Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(PrintingStationeryPurchaseEntrySetupViewModel model);
        Task<(bool isSuccess, string message, object data)> DeleteAsync(List<string> ids);
        Task<string> AutoPrintingStationeryPurchaseIdAsync();

        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
        Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string catagoryValue);
        Task<SalesSupplierSetupViewModel> getSupplierByIdAsync(string SupplierID);
        Task<ItemMasterInformationSetupViewModel> productSelectIdDetailsAsync(string ProductCode);
        Task<List<ItemModelSetupViewModel>> brandIdAsync(string brandId);
    }
}
