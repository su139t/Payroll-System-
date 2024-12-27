using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmpAttendance.Models
{
    public class Employee_Metadata
    {
        [Required(ErrorMessage = "Please Enter EmployeeCode")]
        [Display(Name = "EmployeeCode")]
        public string EmployeeCode { get; set; }
        [StringLength(100, MinimumLength = 3)]
        [Required(ErrorMessage = "Please Enter Name")]
        public string Name { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number Required!")]
        // For Indian Phone No. Use This :-  "^(\+91[\-\s]?)?[0]?(91)?[789]\d{9}$"
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})", ErrorMessage = "Entered phone format is not valid.")]
        public string Phone { get; set; }
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Please Enter Email")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail id is not valid")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Salary")]
        [Range(100, 1000000, ErrorMessage = "Salary must be between 100 and 1000000.")]
        public decimal Salary { get; set; }
        [Required(ErrorMessage = "Please Enter Password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please Enter DailyWorkingMin")]
        [Range(1, 720, ErrorMessage = "Range 1 to 720 minutes")]
        public int DailyWorkingMin { get; set; }
        [Required(ErrorMessage = "Please Select Scheme")]
        public Nullable<int> SchemeId { get; set; }
    }

    public class InOutDetails_Metadata
    {
        [Required(ErrorMessage = "Please Enter InOutType")]
        public int InOutTypeId { get; set; }
    }
}