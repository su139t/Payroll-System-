using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmpAttendance.Models
{
    public class User
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; }
        public bool IsAdmin()
        { 
            if (UserType.Trim().ToUpper() == "ADMIN")
            {
                return true;
            }
            return false;
        }
       
    }
}