﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HRM_EmployeeWeekendDeclaration
{
    public class EmployeeFilterResultDto
    {
        //public List<string>? CompanyCodes { get; set; }
        //public List<string>? BranchCodes { get; set; }
        //public List<string>? DepartmentCodes { get; set; }
        //public List<string>? DesignationCodes { get; set; }
        //public List<string>? EmployeeIDs { get; set; }
        //public List<string>? EmployeeStatuses { get; set; }
        public List<CodeNameDto> Companies { get; set; }
        public List<CodeNameDto> Branches { get; set; }
        public List<CodeNameDto> Divisions { get; set; }
        public List<CodeNameDto> Departments { get; set; }
        public List<CodeNameDto> Designations { get; set; }
        public List<CodeNameDto> Employees { get; set; }
        public List<CodeNameDto> ActivityStatuses { get; set; }
       
    }

}
