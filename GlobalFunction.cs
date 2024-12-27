using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmpAttendance
{
    public static  class GlobalFunction
    {
        public static void ResetSession()
        {
            HttpContext.Current.Session.Abandon();
        }
        public static DateTime GetDateTimeNow()
        {
            DateTime utcTime = DateTime.UtcNow;
            TimeZoneInfo myZone = TimeZoneInfo.CreateCustomTimeZone("INDIA", new TimeSpan(+5, +30, 0), "India", "India");
            DateTime custDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, myZone);
            return custDateTime;
        }
    }
}