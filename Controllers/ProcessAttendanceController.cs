using EmpAttendance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using System.Data.Entity;
using static EmpAttendance.GlobalVariable;

namespace EmpAttendance.Controllers
{
    public class ProcessAttendanceController : Controller
    {
        EmployeeAttendanceDBEntities db = new EmployeeAttendanceDBEntities();

        public ActionResult AttendanceReport()
        {
            ViewBag.TodayDate = GlobalFunction.GetDateTimeNow().ToString("MM/dd/yyyy");

            FillInOutDrp();
            return View();
        }
        [HttpPost]
        public ActionResult AttendanceSearch(DateTime FromDate, DateTime ToDate, int EmpId)
        {
            var lstAllDates = new List<DateTime>();
            string sql = "DELETE FROM Attendance WHERE AttendanceDate >= @p0 AND AttendanceDate <= @p1";

            if (EmpId != 0)
            {
                sql += " AND EmpId = @p2";
            }

            db.Database.ExecuteSqlCommand(sql, FromDate, ToDate, EmpId);

            for (var dt = FromDate; dt <= ToDate; dt = dt.AddDays(1))
            {
                lstAllDates.Add(dt);
            }

            List<int> distinctVal = db.Employee.Where(m => (m.Id == EmpId || EmpId == 0) && m.UserType == UserTypeEmployee).Select(x => x.Id).Distinct().OrderBy(x => x).ToList();

            foreach (var singleEmp in distinctVal)
            {
                CalculateAndRecordAttendance(singleEmp, lstAllDates);
            }

            List<Attendance> lstAttendance = GetAttlist(FromDate, ToDate, EmpId);
            return PartialView("_FilterAttendanceReport", lstAttendance);
        }
        private void CalculateAndRecordAttendance(int empId, List<DateTime> allDates)
        {
            try
            {
                int loopCount = 0;
                int weeklyTime = 0;
                List<DateTime> lstDate = new List<DateTime>();
                var empData = db.Employee.Where(m => m.Id == empId).FirstOrDefault();

                foreach (var item in allDates)
                {
                    TimeSpan workingTime = TimeSpan.Zero;
                    var holiday = db.tblMst_Holiday.Where(m => m.Day.Day == item.Day && m.Month == item.Month && m.Year == item.Year).FirstOrDefault();
                    List<InOutDetails> lstInOut = GetInOutDetailsForDate(empId, item);

                    if (holiday != null)
                    {
                        bool isSandwich = CheckForSandwichDay(empId, item);
                        var Approval = db.Approval.Where(m => m.ApprovalDate == item && m.EmpId == empId).FirstOrDefault();
                        if (Approval != null && Approval.IsApproved == true)
                        {
                            int Approvemin = 0;
                            Approvemin = (int)(Approval.ApprovedMinutes * Approval.ConvFactor);
                            UpdateOrInsertAttendanceAndApproveMin(empId, item, (int)PresentStatusId.Present, null, null, Approvemin);
                        }
                        else if (isSandwich)
                        {
                            UpdateAttendance(empId, item, (int)PresentStatusId.Sandwich, empData.DailyWorkingMin);
                        }
                        else
                        {
                            UpdateAttendance(empId, item, (int)PresentStatusId.Holiday, empData.DailyWorkingMin);
                        }
                    }
                    else if (lstInOut != null && lstInOut.Count > 0)
                    {
                        workingTime = TimeSpan.FromMinutes(CalculateWorkingMinutes(empId, item, lstInOut, empData));
                    }


                    if (workingTime != TimeSpan.Zero)
                    {
                        weeklyTime = weeklyTime + (int)workingTime.TotalMinutes;
                        lstDate.Add(item);
                    }

                    if (weeklyTime != 0 && ((item.DayOfWeek != DayOfWeek.Saturday &&
                        (allDates.Count - 1) == loopCount) || item.DayOfWeek == DayOfWeek.Saturday))
                    {
                        UpdateSettledMinutes(lstDate, empId, weeklyTime, empData.DailyWorkingMin);
                        if (item.DayOfWeek == DayOfWeek.Saturday)
                        {
                            weeklyTime = 0;
                            lstDate.Clear();
                        }
                    }
                    loopCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
        public List<InOutDetails> GetInOutDetailsForDate(int empId, DateTime itemDate)
        {
            return db.InOutDetails
                .Where(r => DbFunctions.TruncateTime(r.PunchDateTime).Value == itemDate.Date && r.EmpId == empId)
                .ToList();
        }
        public int CalculateWorkingMinutes(int empId, DateTime item, List<InOutDetails> lstInout, Employee empData)
        {
            TimeSpan logInTime = TimeSpan.Zero;
            TimeSpan workingTime = TimeSpan.Zero;
            try
            {
                if (lstInout != null)
                {
                    foreach (var record in lstInout)
                    {
                        switch (record.InOutTypeId)
                        {
                            case (int)InOutTypeId.SuddenLeave:
                            case (int)InOutTypeId.Leave:
                                UpdateAttendance(empId, item, record.InOutTypeId, empData.DailyWorkingMin);
                                break;
                            case (int)InOutTypeId.In:
                                logInTime = record.PunchDateTime.TimeOfDay;
                                break;
                            case (int)InOutTypeId.Out:
                                workingTime = workingTime + (record.PunchDateTime.TimeOfDay - logInTime);
                                logInTime = TimeSpan.Zero;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            int workingTimeValue = Convert.ToInt32(workingTime.TotalMinutes);
            try
            {
                if (workingTime != TimeSpan.Zero)
                {
                    ProcessDailyAttendanceAndApproveMin(empId, item, empData.DailyWorkingMin,
                        Convert.ToInt32(empData.GapingMinutes), workingTimeValue, empData.DailyWorkingMin);
                }
                else if (logInTime != TimeSpan.Zero)
                {
                    UpdateAttendance(empId, item, (int)InOutTypeId.Leave, empData.DailyWorkingMin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            return workingTimeValue;
        }
        public bool CheckForSandwichDay(int empId, DateTime item)
        {
            DateTime previousDate = item.AddDays(-1);
            var checkHolidayDate = db.tblMst_Holiday.Where(m => m.Day == previousDate).FirstOrDefault();
            var checkAttendanceDate = db.Attendance.Where(m => m.AttendanceDate == previousDate && m.EmpId == empId).FirstOrDefault();
            try
            {
                if (checkHolidayDate != null && checkAttendanceDate != null && checkAttendanceDate.PresentStatusId == (int)PresentStatusId.Sandwich)
                {
                    return true;
                }
                else if (checkAttendanceDate == null || checkAttendanceDate.PresentStatusId != (int)PresentStatusId.Present)
                {
                    TimeSpan workingTime = TimeSpan.Zero;
                    DateTime nextDate = item.AddDays(1);
                    checkHolidayDate = db.tblMst_Holiday.Where(m => m.Day == nextDate).FirstOrDefault();

                    while (checkHolidayDate != null)
                    {
                        nextDate = nextDate.AddDays(1);
                        checkHolidayDate = db.tblMst_Holiday.Where(m => m.Day == nextDate).FirstOrDefault();
                    }
                    List<InOutDetails> lstInOut = GetInOutDetailsForDate(empId, nextDate);
                    var empData = db.Employee.Where(m => m.Id == empId).FirstOrDefault();
                    if (lstInOut != null && lstInOut.Count > 0)
                    {
                        workingTime = TimeSpan.FromMinutes(CalculateWorkingMinutes(empId, nextDate, lstInOut, empData));
                    }

                    var checkInOutDate = db.InOutDetails.Where(m => m.PunchDateTime.Day == nextDate.Day && m.EmpId == empId).FirstOrDefault();
                    if (checkInOutDate == null || checkInOutDate.InOutTypeId == (int)PresentStatusId.Leave
                        || checkInOutDate.InOutTypeId == (int)PresentStatusId.SuddenLeave ||
                        workingTime == TimeSpan.Zero)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            return false;
        }
        private void ProcessDailyAttendanceAndApproveMin(int empId, DateTime attendanceDate, int dailyWorkingMin, int gapingMinutes, int totalMinutes, int calculatedMinutes)
        {
            int? approveMin = null;
            try
            {
                if (dailyWorkingMin > totalMinutes)
                {
                    var roundOfMinutes = dailyWorkingMin - (totalMinutes + RoundOffTime);

                    if (roundOfMinutes > 0)
                    {
                        if (roundOfMinutes <= gapingMinutes)
                        {
                            calculatedMinutes = dailyWorkingMin - gapingMinutes;
                        }
                        else if (roundOfMinutes > gapingMinutes && roundOfMinutes <= dailyWorkingMin / 2)
                        {
                            calculatedMinutes = dailyWorkingMin / 2;
                        }
                        else
                        {
                            calculatedMinutes = totalMinutes;
                        }
                    }
                }
                else
                {
                    var approval = db.Approval.Where(m => m.ApprovalDate == attendanceDate && m.EmpId == empId).FirstOrDefault();
                    if (approval != null && approval.IsApproved == true)
                    {
                        approveMin = (int)(approval.ApprovedMinutes * approval.ConvFactor);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            UpdateOrInsertAttendanceAndApproveMin(empId, attendanceDate, (int)PresentStatusId.Present, totalMinutes, calculatedMinutes, approveMin);
        }
        private void UpdateAttendance(int empId, DateTime attendanceDate, int presentStatusId, int dailyMin)
        {
            string columnName = "";

            switch (presentStatusId)
            {
                case (int)PresentStatusId.Holiday:
                    columnName = "HolidayMinutes";
                    break;
                case (int)PresentStatusId.SuddenLeave:
                    columnName = "SuddenLeaveMinutes";
                    dailyMin *= 2;
                    break;
                case (int)PresentStatusId.Leave:
                case (int)PresentStatusId.Sandwich:
                    columnName = "LeaveMinutes";
                    break;
                default:
                    break;
            }
            string sql = $"IF EXISTS (SELECT * FROM dbo.Attendance WHERE AttendanceDate=@p1 AND EmpId=@p0)" +
                         $"UPDATE dbo.Attendance SET EmpId=@p0, AttendanceDate=@p1, PresentStatusId=@p2, CreatedDate=@p3, {columnName}=@p4 " +
                         $"WHERE EmpId = @p0 AND AttendanceDate = @p1 " +
                         $"ELSE " +
                         $"INSERT dbo.Attendance (EmpId, AttendanceDate, PresentStatusId, CreatedDate, CreatedById, {columnName}) " +
                         $"VALUES(@p0, @p1, @p2, @p3, @p0, @p4);";
            db.Database.ExecuteSqlCommand(sql, empId, attendanceDate, presentStatusId, GlobalFunction.GetDateTimeNow(), dailyMin);
        }
        public void UpdateOrInsertAttendanceAndApproveMin(int empId, DateTime attendanceDate, int presentStatusId, int? workingMinutes, int? calculatedMinutes, int? overtimeMinutes)
        {
            string sql = "IF EXISTS (SELECT * FROM dbo.Attendance WHERE AttendanceDate=@p1 and EmpId=@p0)" +
              "UPDATE dbo.Attendance SET EmpId=@p0, AttendanceDate=@p1, PresentStatusId=@p2, WorkingMin=@p3, CreatedDate=@p4, " +
              "CreatedById=@p0, CalculatedMinutes=@p5 ,OvertimeMinutes=@p6 " +
              "WHERE EmpId = @p0 AND AttendanceDate = @p1 " +
              "ELSE " +
              "INSERT dbo.Attendance (EmpId, AttendanceDate, PresentStatusId, WorkingMin, CreatedDate, CreatedById, CalculatedMinutes, OvertimeMinutes) " +
              "VALUES(@p0, @p1, @p2, @p3, @p4, @p0, @p5, @p6);";

            db.Database.ExecuteSqlCommand(sql, empId, attendanceDate, presentStatusId, workingMinutes, GlobalFunction.GetDateTimeNow(), calculatedMinutes, overtimeMinutes);

        }
        private void UpdateSettledMinutes(List<DateTime> lstDate, int empId, int weeklyTime, int dailyWorkingMin)
        {
            int? settledTime = null;
            try
            {
                int ttlCountMinutes = dailyWorkingMin * lstDate.Count;
                if (dailyWorkingMin / 2 > (ttlCountMinutes - weeklyTime)
                    && (ttlCountMinutes - weeklyTime) > 0)
                {
                    settledTime = (dailyWorkingMin / 2) / lstDate.Count;
                }
                else if (dailyWorkingMin / 2 < (ttlCountMinutes - weeklyTime))
                {
                    settledTime = dailyWorkingMin / lstDate.Count;
                }
                foreach (var result in lstDate)
                {
                    string sql = "UPDATE Attendance SET SetteledMinutes = {0} WHERE AttendanceDate ={1} and EmpId = {2}";
                    db.Database.ExecuteSqlCommand(sql, settledTime, result, empId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        private List<Attendance> GetAttlist(DateTime fromdate, DateTime todate, int EId)
        {
            var inOutData = from attendance in db.Attendance
                            join employee in db.Employee on attendance.EmpId equals employee.Id
                            join presentstatus in db.tblMst_PresentStatus on attendance.PresentStatusId equals presentstatus.PresentStatusId  // Add this line for the additional join
                            orderby attendance.AttendanceDate ascending
                            select new
                            {
                                attendance.EmpId,
                                attendance.AttendanceDate,
                                attendance.WorkingMin,
                                attendance.CalculatedMinutes,
                                attendance.SetteledMinutes,
                                attendance.HolidayMinutes,
                                attendance.SuddenLeaveMinutes,
                                attendance.LeaveMinutes,
                                attendance.OvertimeMinutes,
                                employee.Name,
                                presentstatus.PresentStatus,

                            };

            List<Attendance> lst = new List<Attendance>();

            if (inOutData != null)
            {
                foreach (var item in inOutData)
                {
                    if ((EId == item.EmpId || EId == 0) && item.AttendanceDate >= fromdate && item.AttendanceDate <= todate)
                    {
                        Attendance modelQC = new Attendance
                        {
                            AttendanceDate = item.AttendanceDate,
                            WorkingMin = item.WorkingMin,
                            EmpName = item.Name,
                            PresentStatus = item.PresentStatus,
                            CalculatedMinutes = item.CalculatedMinutes,
                            SetteledMinutes = item.SetteledMinutes,
                            HolidayMinutes = item.HolidayMinutes,
                            SuddenLeaveMinutes = item.SuddenLeaveMinutes,
                            LeaveMinutes = item.LeaveMinutes,
                            OvertimeMinutes = item.OvertimeMinutes
                        };
                        lst.Add(modelQC);
                    }
                }
            }
            return lst;
        }
        public void FillInOutDrp()
        {
            List<SelectListItem> drpEmp = db.Employee.Where(m => m.UserType == UserTypeEmployee).OrderBy(m => m.Name).Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
            }).ToList();
            ViewBag.EmpId = new SelectList(drpEmp, "Value", "Text");
        }
    }
}