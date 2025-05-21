using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HRMATDAttendanceTypes
{
    public class HRMATDAttendanceTypeSetupViewModel:BaseViewModel
    {
        public decimal AutoId { get; set; }
        public string AttendanceTypeCode { get; set; }
        [Required(ErrorMessage ="{0} is required")]
        public string AttendanceTypeName { get; set; }
        public string ShortName { get; set; }
    }
}
