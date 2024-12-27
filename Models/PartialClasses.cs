using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmpAttendance.Models
{
    public partial class Approval
    {
        public string EmpName { get; set; }
        public string EmpCode { get; set; }
        public bool Time { get; set; }
    }

    [MetadataType(typeof(InOutDetails_Metadata))]
    public partial class InOutDetails
    {
        public string InOut { get; set; }
        public string EmpName { get; set; }
        public string EmpCode { get; set; }
    }

    [MetadataType(typeof(Employee_Metadata))]
    public partial class Employee
    {
    }
    public partial class Attendance
    {
        public string EmpName { get; internal set; }
        public string PresentStatus { get; internal set; }
    }
}