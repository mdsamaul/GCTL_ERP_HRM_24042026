﻿using GCTL.Core.Data;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Accounts;
using GCTL.Core.ViewModels.Common;
using GCTL.Core.ViewModels.LeaveTypes;
using GCTL.Data.Models;
using GCTL.Service.Common;
using GCTL.Service.Departments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.LeaveTypes
{
    public class LeaveTypeService : AppService<HrmAtdLeaveType>, ILeaveTypeService
    {
        private readonly IRepository<HrmAtdLeaveType> leaveTypeRepository;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly ICommonService commonService;
     
        public LeaveTypeService(IRepository<HrmAtdLeaveType> leaveTypeRepository, ICommonService commonService,IRepository<CoreAccessCode> accessCodeRepository)
      : base(leaveTypeRepository) 
        {
            this.leaveTypeRepository = leaveTypeRepository;
            this.accessCodeRepository = accessCodeRepository;
            this.commonService = commonService;
        }




        //async 

       // model.EmployeAddInfoId = commonService.NextCode("EmployeAddInfoID", "HRM_EmployeeAdditionalInfo", 4);
        public async Task<List<LeaveTypeSetupViewModel>> GetLeaveTypesAsync()
        {
            var entity = await leaveTypeRepository.GetAllAsync();
            return entity.Select(leave => new LeaveTypeSetupViewModel
            {  
                AutoId=leave.AutoId,
                LeaveTypeCode = leave.LeaveTypeCode,
                Name = leave.Name,
                ShortName = leave.ShortName,
                Ldate = leave.Ldate,
                ModifyDate = leave.ModifyDate,
                RulePolicy = leave.RulePolicy,
                NoOfDay = leave.NoOfDay,
                For = leave.For,
                Ymwd = leave.Ymwd,
                Wef = leave.Wef,
                Luser = leave.Luser,
                Lip = leave.Lip,
                Lmac = leave.Lmac
            }).ToList();

        }


        public async Task<LeaveTypeSetupViewModel> GetLeaveTypeAsync(string id)
        {
            var leave = await leaveTypeRepository.GetByIdAsync(id);
            if (leave == null)
            {
                return null;
            }
            return new LeaveTypeSetupViewModel

            {
                AutoId = leave.AutoId, //id
                LeaveTypeCode = leave.LeaveTypeCode,
                Name = leave.Name,
                ShortName = leave.ShortName,
                Ldate = leave.Ldate,
                ModifyDate = leave.ModifyDate,
                RulePolicy = leave.RulePolicy,
                NoOfDay = leave.NoOfDay,
                For = leave.For,
                Ymwd = leave.Ymwd,
                Wef = leave.Wef, //.ToString("dd/MM/yyyy"),
                Luser = leave.Luser,
                Lip = leave.Lip,
                Lmac = leave.Lmac

            };
        }




        public async Task<bool> SaveLeaveTypeAsync(LeaveTypeSetupViewModel entityVM)
        {
            await leaveTypeRepository.BeginTransactionAsync();
            try
            {
              

                HrmAtdLeaveType entity = new HrmAtdLeaveType();
                entity.LeaveTypeCode = await GenerateNextLeaveTypeCode();   
                entity.Name = entityVM.Name;
                entity.ShortName = entityVM.ShortName ?? string.Empty;
                entity.RulePolicy = entityVM.RulePolicy ?? string.Empty;
                entity.NoOfDay = entityVM.NoOfDay ?? decimal.Zero;
                entity.For = entityVM.For ?? decimal.Zero;
                entity.Ymwd = entityVM.Ymwd ?? string.Empty;
                entity.Luser = entityVM.Luser ?? string.Empty;
                entity.Lip = entityVM.Lip ?? string.Empty;
                entity.Lmac=entityVM.Lmac ?? string.Empty;
                entity.Wef = entityVM.Wef;
                entity.Ldate = DateTime.Now;
                await leaveTypeRepository.AddAsync(entity);
                await leaveTypeRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await leaveTypeRepository.RollbackTransactionAsync();
                return false;
            }
        }





        public async Task<bool> UpdateLeaveTypeAsync(LeaveTypeSetupViewModel entityVM)
        {
            await leaveTypeRepository.BeginTransactionAsync();
            try
            {
                
                var entity = await leaveTypeRepository.GetByIdAsync(entityVM.LeaveTypeCode); //autoId
                if (entity == null)
                {
                    return false;
                }

                entity.LeaveTypeCode = entityVM.LeaveTypeCode;
                entity.Name = entityVM.Name;
                entity.ShortName = entityVM.ShortName ?? string.Empty;
                entity.RulePolicy = entityVM.RulePolicy ?? string.Empty;
                entity.NoOfDay = entityVM.NoOfDay ?? decimal.Zero;
                entity.For = entityVM.For ?? decimal.Zero;
                entity.Ymwd = entityVM.Ymwd ?? string.Empty;
                entity.Luser = entityVM.Luser ?? string.Empty;
                entity.Lip = entityVM.Lip ?? string.Empty;
                entity.Lmac = entityVM.Lmac ?? string.Empty;
                entity.Wef = entityVM.Wef;
                entity.ModifyDate = DateTime.Now;
                await leaveTypeRepository.UpdateAsync(entity);
                await leaveTypeRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred : {ex.Message}");
                await leaveTypeRepository.RollbackTransactionAsync();
                return false;
            }
        }



        public async Task<bool> DeleteLeaveTypeAsync(string id)
        {
            try
            {
                var entity = await GetLeaveTypeAsync(id);
             
                if (entity != null)
                {
                    await leaveTypeRepository.DeleteAsync(entity);
                    return true; 
                }
                else
                {
                    Console.WriteLine($"Leave type with ID {id} was not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deleting leave type: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            return false;
        }

        public   bool DeleteLeaveType(string id)
        {
            var entity = GetLeaveType(id); 
            if (entity != null)
            {
                leaveTypeRepository.Delete(entity);
                return true;
            }
            return false;
        }

        public HrmAtdLeaveType GetLeaveType(string code)
        {
            return leaveTypeRepository.GetById(code);
        }


        public async Task<bool> IsLeaveTypeExistByCodeAsync(string code)
        {
            return await leaveTypeRepository.All().AnyAsync(x => x.LeaveTypeCode == code);
        }

        public async Task<bool> IsLeaveTypeExistAsync(string name)
        {
            return await leaveTypeRepository.All().AnyAsync(x => x.Name == name);
        }

      
     
        public async Task<bool> IsLeaveTypeExistAsync(string name, string typeCode)
        {
            return await leaveTypeRepository.All().AnyAsync(x => x.Name == name && x.LeaveTypeCode != typeCode);
        }

      
        public async Task<IEnumerable<CommonSelectModel>> LeaveTypeSelectionAsync()
        {
            return await leaveTypeRepository.All()
                      .Select(x => new CommonSelectModel
                      {
                          Code = x.LeaveTypeCode,
                          Name = x.Name
                      })
                      .ToListAsync();  
        }


        public async Task<bool> PagePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Leave Type" && x.TitleCheck);
        }

        public async Task<bool> SavePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Leave Type" && x.CheckAdd);
        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Leave Type" && x.CheckEdit);
        }

     

        public async Task<bool> DeletePermissionAsync(string accessCode)
        {
            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Leave Type" && x.CheckDelete);
        }

        public async Task<string> GenerateNextLeaveTypeCode()
        {
           
                var leaveTypeCode = await leaveTypeRepository.GetAllAsync();
                var lastCode = leaveTypeCode.Max(b => b.LeaveTypeCode);
                int nextCode = 1;
                if (!string.IsNullOrEmpty(lastCode))
                {
                    int lastNumber = int.Parse(lastCode.TrimStart('0'));
                    lastNumber++;
                    nextCode = lastNumber;
                }
                return nextCode.ToString("D2");
            
        }
    }
}



