﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HrmAtdShifts
{
    public class HrmAtdShiftSetupViewModel:BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string ShiftCode { get; set; }
        [Required(ErrorMessage ="Please enter shif type name")]
        public string ShiftName { get; set; }
        public string ShiftShortName { get; set; }
        [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime ShiftStartTime { get; set; }
       
        [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime ShiftEndTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime LateTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime AbsentTime { get; set; }
        public string Description { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Wef { get; set; }
        public string Remarks { get; set; }
        public string ShiftTypeId { get; set; }

    }
}
