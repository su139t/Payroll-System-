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
    
    public partial class Approval
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public System.DateTime ApprovalDate { get; set; }
        public int WorkingMinutes { get; set; }
        public Nullable<int> ApprovedById { get; set; }
        public Nullable<System.DateTime> ApprovedTime { get; set; }
        public int CreatedById { get; set; }
        public System.DateTime CreatedTime { get; set; }
        public bool IsApproved { get; set; }
        public string Remark { get; set; }
        public Nullable<int> ApprovedMinutes { get; set; }
        public Nullable<decimal> ConvFactor { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}
