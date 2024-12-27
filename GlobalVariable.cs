using EmpAttendance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls.WebParts;
namespace EmpAttendance
{
    public static class GlobalVariable
    {
        public static int UserId { get => GetUserId(); }
         
        public static bool IsAdmin { get => IsAdminUser(); }

        private static bool IsAdminUser()
        {
            if (HttpContext.Current.Session["User"] == null)
            {
                return false;
            }
            if (GetUserData().UserType.Trim().ToUpper().Equals("ADMIN"))
            {
                return true;
            }
            return false;
        }
        private static int GetUserId()
        {
            if (HttpContext.Current.Session["User"] == null)
            {
                return 0;
            }
            return GetUserData().UserId;
        }
        private static User GetUserData()
        {
            User _user = new User();
            if(HttpContext.Current.Session["User"]==null)
            {
                return _user;
            }
            _user = (User)HttpContext.Current.Session["User"];
            return _user;
        }

        public static int RoundOffTime = 10;

        public enum InOutTypeId
        {
            In = 1,
            Out = 2,
            SuddenLeave = 3,
            Leave = 4,
        }
        public enum SchemeId
        {
            Daily = 1,
            Weekly = 2,
        }

        public enum PresentStatusId
        {
            Present = 1,
            Holiday = 2,
            SuddenLeave = 3,
            Leave = 4,
            Sandwich = 5,

        }

        public const String UserTypeEmployee = "Employee";
    }
}