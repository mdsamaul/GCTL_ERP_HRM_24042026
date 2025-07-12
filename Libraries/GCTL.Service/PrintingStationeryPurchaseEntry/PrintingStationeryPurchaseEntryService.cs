using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Core.ViewModels.PrintingStationeryPurchaseEntry;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.PrintingStationeryPurchaseEntry
{
    public class PrintingStationeryPurchaseEntryService:AppService<RmgPurchaseOrderReceive> , IPrintingStationeryPurchaseEntryService
    {
        private readonly IRepository<RmgPurchaseOrderReceive> PurchaseOrderReceive;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;

        public PrintingStationeryPurchaseEntryService(
            IRepository<RmgPurchaseOrderReceive> PurchaseOrderReceive,
            IRepository<CoreAccessCode> accessCodeRepository
            ) : base(PurchaseOrderReceive)
        {
            this.PurchaseOrderReceive = PurchaseOrderReceive;
            this.accessCodeRepository = accessCodeRepository;
        }

        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";

        #region Permission all type

        public async Task<bool> PagePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Catagory Info" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Catagory Info" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Catagory Info" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Catagory Info" && x.CheckDelete);

        }

        #endregion

        //public async Task<List<PrintingStationeryPurchaseEntrySetupViewModel>> GetAllAsync()
        //{
        //    //await Task.Delay(100);
        //    try
        //    {
        //        return PurchaseOrderReceive.All().Select(c => new PrintingStationeryPurchaseEntrySetupViewModel
        //        {
        //            TC = c.Tc,

        //        }).ToList();
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}
        public async Task<List<PrintingStationeryPurchaseEntrySetupViewModel>> GetAllAsync()
        {
            try
            {
                return PurchaseOrderReceive.All().Select(c => new PrintingStationeryPurchaseEntrySetupViewModel
                {
                    TC = c.Tc,
                    MainCompanyCode = c.MainCompanyCode,
                    PurchaseReceiveNo = c.PurchaseReceiveNo,
                    ReceiveDate = c.ReceiveDate,
                    DepartmentCode = c.DepartmentCode,
                    SupplierID = c.SupplierId,
                    InvoiceNo = c.InvoiceNo,
                    InvoiceDate = c.InvoiceDate,
                    InvoiceValue = c.InvoiceValue,
                    ChallanNo = c.ChallanNo,
                    ChallanDate = c.ChallanDate,
                    EmployeeID_ReceiveBy = c.EmployeeIdReceiveBy,
                    Remarks = c.Remarks,
                    TotalAmount = c.TotalAmount,
                    LUser = c.Luser,
                    LDate = c.Ldate,
                    LIP = c.Lip,
                    LMAC = c.Lmac,
                    ModifyDate = c.ModifyDate,
                    UserInfoEmployeeID = c.UserInfoEmployeeId,
                    CompanyCode = c.CompanyCode
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PrintingStationeryPurchaseEntrySetupViewModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = PurchaseOrderReceive.All().FirstOrDefault(x => x.PurchaseReceiveNo == id);
                if (entity == null) return null;

                return new PrintingStationeryPurchaseEntrySetupViewModel
                {
                    TC = entity.Tc,
                    MainCompanyCode = entity.MainCompanyCode,
                    PurchaseReceiveNo = entity.PurchaseReceiveNo,
                    ReceiveDate = entity.ReceiveDate,
                    DepartmentCode = entity.DepartmentCode,
                    SupplierID = entity.SupplierId,
                    InvoiceNo = entity.InvoiceNo,
                    InvoiceDate = entity.InvoiceDate,
                    InvoiceValue = entity.InvoiceValue,
                    ChallanNo = entity.ChallanNo,
                    ChallanDate = entity.ChallanDate,
                    EmployeeID_ReceiveBy = entity.EmployeeIdReceiveBy,
                    Remarks = entity.Remarks,
                    TotalAmount = entity.TotalAmount,
                    LUser = entity.Luser,
                    LDate = entity.Ldate,
                    LIP = entity.Lip,
                    LMAC = entity.Lmac,
                    ModifyDate = entity.ModifyDate,
                    UserInfoEmployeeID = entity.UserInfoEmployeeId,
                    CompanyCode = entity.CompanyCode,

                    // Formatted string dates for UI display
                    ShowCreateDate = entity.Ldate.HasValue ? entity.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    ShowModifyDate = entity.ModifyDate.HasValue ? entity.ModifyDate.Value.ToString("dd/MM/yyyy") : ""
                };
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(PrintingStationeryPurchaseEntrySetupViewModel model)
        {
            try
            {
                if (model.TC == 0)
                {
                    var entity = new RmgPurchaseOrderReceive
                    {
                        MainCompanyCode = model.MainCompanyCode,
                        PurchaseReceiveNo = model.PurchaseReceiveNo,
                        ReceiveDate = model.ReceiveDate,
                        DepartmentCode = model.DepartmentCode,
                        SupplierId = model.SupplierID,
                        InvoiceNo = model.InvoiceNo,
                        InvoiceDate = model.InvoiceDate,
                        InvoiceValue = model.InvoiceValue,
                        ChallanNo = model.ChallanNo,
                        ChallanDate = model.ChallanDate,
                        EmployeeIdReceiveBy = model.EmployeeID_ReceiveBy,
                        Remarks = model.Remarks,
                        TotalAmount = model.TotalAmount,
                        Luser = model.LUser,
                        Ldate = DateTime.Now,
                        Lip = model.LIP,
                        Lmac = model.LMAC,
                        UserInfoEmployeeId = model.UserInfoEmployeeID,
                        CompanyCode = model.CompanyCode
                    };

                    await PurchaseOrderReceive.AddAsync(entity);
                    return (true, CreateSuccess, entity);
                }

                var exData = await PurchaseOrderReceive.GetByIdAsync(model.TC);
                if (exData != null)
                {
                    exData.MainCompanyCode = model.MainCompanyCode;
                    exData.PurchaseReceiveNo = model.PurchaseReceiveNo;
                    exData.ReceiveDate = model.ReceiveDate;
                    exData.DepartmentCode = model.DepartmentCode;
                    exData.SupplierId = model.SupplierID;
                    exData.InvoiceNo = model.InvoiceNo;
                    exData.InvoiceDate = model.InvoiceDate;
                    exData.InvoiceValue = model.InvoiceValue;
                    exData.ChallanNo = model.ChallanNo;
                    exData.ChallanDate = model.ChallanDate;
                    exData.EmployeeIdReceiveBy = model.EmployeeID_ReceiveBy;
                    exData.Remarks = model.Remarks;
                    exData.TotalAmount = model.TotalAmount;
                    exData.Luser = model.LUser;
                    exData.ModifyDate = DateTime.Now;
                    exData.Lip = model.LIP;
                    exData.Lmac = model.LMAC;
                    exData.UserInfoEmployeeId = model.UserInfoEmployeeID;
                    exData.CompanyCode = model.CompanyCode;

                    await PurchaseOrderReceive.UpdateAsync(exData);
                    return (true, UpdateSuccess, exData);
                }

                return (false, UpdateFailed, null);
            }
            catch (Exception)
            {
                return (false, CreateFailed, null);
            }
        }


        public async Task<(bool isSuccess, string message, object data)> DeleteAsync(List<string> ids)
        {
            foreach (var id in ids)
            {

                try
                {
                    var entity = await PurchaseOrderReceive.GetByIdAsync(decimal.Parse(id));
                    if (entity == null)
                    {
                        continue;
                    }

                    await PurchaseOrderReceive.DeleteAsync(entity);
                }
                catch (Exception)
                {
                    return (true, DeleteFailed, null);
                }
            }

            return (true, DeleteSuccess, null);
        }

        public async Task<string> AutoPrintingStationeryPurchaseIdAsync()
        {
            var currentYearShort = DateTime.Now.Year.ToString().Substring(2); // "22"
            var prefix = $"PO_{currentYearShort}_";

            var PrintingStationeryPurchaseList = (await PurchaseOrderReceive.GetAllAsync()).ToList();

            int newIdNumber = 1;

            if (PrintingStationeryPurchaseList != null && PrintingStationeryPurchaseList.Count > 0)
            {
                var lastId = PrintingStationeryPurchaseList
                    .Select(x => x.PurchaseReceiveNo)
                    .Where(id => id != null && id.StartsWith(prefix))
                    .OrderByDescending(id => id)
                    .FirstOrDefault();


                if (!string.IsNullOrEmpty(lastId))
                {
                    var numericPart = lastId.Substring(prefix.Length);
                    if (int.TryParse(numericPart, out int lastNumber))
                    {
                        newIdNumber = lastNumber + 1;
                    }
                }
            }

            return $"{prefix}{newIdNumber.ToString("D6")}";
        }

        public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string catagoryValue)
        {
            bool Exists = PurchaseOrderReceive.All().Any(x => x.DepartmentCode == catagoryValue);
            return (Exists, DataExists, null);
        }
    }
}
