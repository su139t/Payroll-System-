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
    
    public partial class InOutDetails
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public System.DateTime PunchDateTime { get; set; }
        public int InOutTypeId { get; set; }
        public Nullable<int> InId { get; set; }
        public bool IsLocked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public string Remark { get; set; }
    
        public virtual tblMst_InOutType tblMst_InOutType { get; set; }
    }
}
