﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmWorkingDayDeclarations
{
    public class HrmWorkingDayDeclarationSetupViewModel:BaseViewModel
    {
        public decimal Tc { get; set; }
        public string WorkingDayCode { get; set; }
        
        public string? EmployeeId { get; set; }
        public List<string>? EmployeeIds { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? WorkingDayDate { get; set; }
        public string? Remarks { get; set; }
        public string CompanyCode { get; set; }
    }
}
