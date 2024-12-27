//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EmpAttendance.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Attendance
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public System.DateTime AttendanceDate { get; set; }
        public Nullable<int> PresentStatusId { get; set; }
        public Nullable<int> WorkingMin { get; set; }
        public string Remark { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<int> CalculatedMinutes { get; set; }
        public Nullable<decimal> SetteledMinutes { get; set; }
        public Nullable<int> HolidayMinutes { get; set; }
        public Nullable<int> SuddenLeaveMinutes { get; set; }
        public Nullable<int> LeaveMinutes { get; set; }
        public Nullable<int> OvertimeMinutes { get; set; }
    
        public virtual Employee Employee { get; set; }
        public virtual tblMst_PresentStatus tblMst_PresentStatus { get; set; }
    }
}
