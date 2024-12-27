using EmpAttendance.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace EmpAttendance.Controllers
{
    [AuthorizationFilter]

    public class InOutController : Controller
    {
        EmployeeAttendanceDBEntities db = new EmployeeAttendanceDBEntities();
        // GET: InOut
        public ActionResult Create()
        {
            var model = new InOutDetails();
            model.PunchDateTime = GlobalFunction.GetDateTimeNow();
            FillLogType();
            FillEmployee();
            return View(model);
        }
       
        [HttpPost]
        public ActionResult Create(InOutDetails model)
        {
            if (GlobalVariable.IsAdmin == false)
            {
                model.EmpId = GlobalVariable.UserId;
                model.PunchDateTime = GlobalFunction.GetDateTimeNow();
            }
            model.CreatedDate = GlobalFunction.GetDateTimeNow();

            var lastLoggedInData = db.InOutDetails.Where(n => n.EmpId == model.EmpId &&
                            n.PunchDateTime.Day == model.PunchDateTime.Day &&
                            n.PunchDateTime.Month == model.PunchDateTime.Month &&
                            n.PunchDateTime.Year == model.PunchDateTime.Year)
                          .OrderByDescending(d => d.Id).FirstOrDefault();

            #region validations  
            if (lastLoggedInData == null)
            {
                if (model.InOutTypeId == Convert.ToInt32(GlobalVariable.InOutTypeId.Out))
                {
                    return Json("Please first login.");
                }
            }
            if (lastLoggedInData != null)
            {
                if (model.InOutTypeId == lastLoggedInData.InOutTypeId)
                {
                    if (model.InOutTypeId == Convert.ToInt32(GlobalVariable.InOutTypeId.In))
                        return Json("Logout pending for last login.");
                    else if (model.InOutTypeId == Convert.ToInt32(GlobalVariable.InOutTypeId.Out))
                        return Json("Please first login.");
                }
                if (lastLoggedInData.PunchDateTime.TimeOfDay > model.PunchDateTime.TimeOfDay)
                {
                    return Json(string.Format("Entry should be greater then {0}:{1}", lastLoggedInData.PunchDateTime.TimeOfDay.Hours,
                        lastLoggedInData.PunchDateTime.TimeOfDay.Minutes));

                }
            }
            #endregion validations  
            //Assign values to model based on the conditions
            if (model.InOutTypeId == (int)GlobalVariable.InOutTypeId.Out)
            {
                model.InId = lastLoggedInData.InId;
            }

            db.InOutDetails.Add(model);
            db.SaveChanges();

            if (model.InOutTypeId != (int)GlobalVariable.InOutTypeId.Out)
            {
                model.InId = model.Id;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json("Success");
        }

        public ActionResult InOutList()
        {
            ViewBag.TodayDate = GlobalFunction.GetDateTimeNow();

            FillEmployee();

            List<InOutDetails> lst = GetInOutlist(null, null, GlobalVariable.UserId);

            return View(lst);
        }

        private List<InOutDetails> GetInOutlist(string FromDate, string ToDate, int EId)
        {
            var InOutData = from qd in db.InOutDetails
                            join I in db.tblMst_InOutType on qd.InOutTypeId equals I.InOutTypeId
                            join E in db.Employee on qd.EmpId equals E.Id
                            orderby qd.Id descending
                            select new
                            {
                                qd.Id,
                                qd.EmpId,
                                qd.InId,
                                qd.PunchDateTime,
                                qd.Remark,
                                I.InOut,
                                E.Name,
                                E.EmployeeCode
                            };

            if (EId != 0 && (string.IsNullOrEmpty(FromDate) || string.IsNullOrEmpty(ToDate)))
            {
                if(GlobalVariable.IsAdmin == false)
                {
                    InOutData = InOutData.Where(x => x.EmpId == EId);
                }
            }
            else
            {
                DateTime fromdate = DateTime.ParseExact(FromDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime todate = DateTime.ParseExact(ToDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                if (EId != 0)
                {
                    InOutData = InOutData.Where(x => x.EmpId == EId && x.PunchDateTime.Day >= fromdate.Day && x.PunchDateTime.Month >= fromdate.Month && x.PunchDateTime.Year >= fromdate.Year &&
                               x.PunchDateTime.Day <= todate.Day && x.PunchDateTime.Month <= todate.Month && x.PunchDateTime.Year <= todate.Year);
                }
                else
                {
                    InOutData = InOutData.Where(x => x.PunchDateTime.Day >= fromdate.Day && x.PunchDateTime.Month >= fromdate.Month && x.PunchDateTime.Year >= fromdate.Year &&
                              x.PunchDateTime.Day <= todate.Day && x.PunchDateTime.Month <= todate.Month && x.PunchDateTime.Year <= todate.Year);
                }
            }

            List<InOutDetails> lst = new List<InOutDetails>();

            if (InOutData != null)
            {
                foreach (var item in InOutData)
                {
                    InOutDetails modelQC = new InOutDetails();
                    modelQC.Id = item.Id;
                    modelQC.EmpId = item.EmpId;
                    modelQC.InId = item.InId;
                    modelQC.PunchDateTime = item.PunchDateTime;
                    modelQC.InOut = item.InOut;
                    modelQC.Remark = item.Remark;
                    modelQC.EmpName = item.Name;
                    modelQC.EmpCode = item.EmployeeCode;
                    lst.Add(modelQC);
                }
            }
            return lst;
        }

        [HttpPost]
        public PartialViewResult _InOutList(string FromDate, string ToDate, int EId)
        {
            List<InOutDetails> lst = GetInOutlist(FromDate, ToDate, EId);
            return PartialView("_InOutList", lst);
        }

        public ActionResult Delete(int id)
        {
            var data = db.InOutDetails.Where(n => n.InId == id).ToList();
            db.InOutDetails.RemoveRange(data);
            db.SaveChanges();
            return RedirectToAction("InOutList");
        }

        private void FillLogType()
        {
            List<SelectListItem> drpInOut = db.tblMst_InOutType.OrderBy(m => m.InOut).Select(m => new SelectListItem
            {
                Value = m.InOutTypeId.ToString(),
                Text = m.InOut,
            }).ToList();

            ViewBag.InOutTypeId = new SelectList(drpInOut, "Value", "Text");
        }

        private void FillEmployee()
        {
            List<SelectListItem> drpEmp = db.Employee.OrderBy(m => m.Name).Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
            }).ToList();
            if (GlobalVariable.IsAdmin == false)
            {
                ViewBag.EmpId = new SelectList(drpEmp, "Value", "Text", GlobalVariable.UserId.ToString());
            }
            else
            {
                ViewBag.EmpId = new SelectList(drpEmp, "Value", "Text");
            }
        }
    }
}