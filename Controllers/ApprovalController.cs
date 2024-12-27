using EmpAttendance.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EmpAttendance.Controllers
{
    [AuthorizationFilter]
    public class ApprovalController : Controller
    {
        EmployeeAttendanceDBEntities db = new EmployeeAttendanceDBEntities();
        // GET: Approval
        public ActionResult Create()
        {
            var model = new Approval();
            model.ApprovalDate = GlobalFunction.GetDateTimeNow();
            FillEmployee(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(Approval mdl)
        {
            if (mdl.IsApproved == true)
            {
                mdl.ApprovedTime = Convert.ToDateTime(GlobalFunction.GetDateTimeNow().ToString("yyyy-MM-dd HH:mm ")); 
            }
            else
            {
                mdl.ApprovedTime = null;
            }
            mdl.CreatedTime = Convert.ToDateTime(GlobalFunction.GetDateTimeNow().ToString("HH:mm:ss "));
            mdl.ApprovedById = GlobalVariable.UserId;

            if (mdl.Id > 0)
            {
                Approval objExsisting = db.Approval.Find(mdl.Id);
                objExsisting.Id = mdl.Id;
                objExsisting.EmpId = mdl.EmpId;
                objExsisting.ApprovalDate = mdl.ApprovalDate;
                objExsisting.WorkingMinutes = mdl.WorkingMinutes;
                objExsisting.ApprovedById = GlobalVariable.UserId;
              
                if(GlobalVariable.IsAdmin == true)
                {
                    objExsisting.ApprovedTime = mdl.ApprovedTime;
                    objExsisting.IsApproved = mdl.IsApproved;
                }
                objExsisting.Remark = mdl.Remark;

                db.Entry(objExsisting).State = EntityState.Modified;
            }
            else
            {
                var data = db.Approval.Where(n => n.ApprovalDate == mdl.ApprovalDate && n.EmpId == mdl.EmpId).FirstOrDefault();
                if (data == null)
                {
                    db.Approval.Add(mdl);
                }
                else
                {
                    return Json("Approval For The Selected Date Already Exist");
                }
            }
            db.SaveChanges();
            return Json("success");
        }

        public ActionResult ApprovalList()
        {
            ViewBag.TodayDate = GlobalFunction.GetDateTimeNow();

            var model = new Approval();
            FillEmployee(model);
            List<Approval> lst = GetApprovallist(null ,null ,GlobalVariable.UserId);
           
            return View(lst);
        }

        [HttpPost]
        public PartialViewResult _ApprovalList(string FromDate, string ToDate, int EId)
        {
            List<Approval> lst = GetApprovallist(FromDate, ToDate, EId);
            return PartialView("_ApprovalList", lst);
        }

        private List<Approval> GetApprovallist(string FromDate, string ToDate, int EId)
        {
            var ApprovalData = from qd in db.Approval
                               join E in db.Employee on qd.EmpId equals E.Id
                               orderby qd.ApprovalDate descending
                               select new
                               {
                                   qd.Id,
                                   qd.EmpId,
                                   qd.WorkingMinutes,
                                   qd.ApprovalDate,
                                   qd.CreatedTime,
                                   qd.ApprovedTime,
                                   qd.IsApproved,
                                   E.EmployeeCode,
                                   E.Name
                               };

            if(EId != 0 && (string.IsNullOrEmpty(FromDate) || string.IsNullOrEmpty(ToDate)))
            {
                if (GlobalVariable.IsAdmin == false)
                {
                    ApprovalData = ApprovalData.Where(x => x.EmpId == EId);
                }
            }
            else
            {
                DateTime fromdate = DateTime.ParseExact(FromDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime todate = DateTime.ParseExact(ToDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                if (EId != 0)
                {
                    ApprovalData = ApprovalData.Where(x => x.EmpId == EId && x.ApprovalDate.Day >= fromdate.Day && x.ApprovalDate.Month >= fromdate.Month && x.ApprovalDate.Year >= fromdate.Year &&
                               x.ApprovalDate.Day <= todate.Day && x.ApprovalDate.Month <= todate.Month && x.ApprovalDate.Year <= todate.Year);
                }
                else
                {
                    ApprovalData = ApprovalData.Where(x => x.ApprovalDate.Day >= fromdate.Day && x.ApprovalDate.Month >= fromdate.Month && x.ApprovalDate.Year >= fromdate.Year &&
                              x.ApprovalDate.Day <= todate.Day && x.ApprovalDate.Month <= todate.Month && x.ApprovalDate.Year <= todate.Year);
                }
            }

            List<Approval> lst = new List<Approval>();

            if (ApprovalData != null)
            {
                foreach (var item in ApprovalData)
                {
                    Approval modelQC = new Approval();
                    modelQC.Id = item.Id;
                    modelQC.EmpId = item.EmpId;
                    modelQC.WorkingMinutes = item.WorkingMinutes;
                    modelQC.ApprovalDate = item.ApprovalDate;
                    modelQC.ApprovedTime = item.ApprovedTime;
                    modelQC.CreatedTime = item.CreatedTime;
                    modelQC.IsApproved = item.IsApproved;
                    modelQC.EmpCode = item.EmployeeCode;
                    modelQC.EmpName = item.Name;
                    lst.Add(modelQC);
                }
            }

            return lst;
        }

        public ActionResult Edit(int id)
        {
            var data = db.Approval.Where(n => n.Id == id).FirstOrDefault();
            if(GlobalVariable.IsAdmin == false)
            {
                if(data.IsApproved == true)
                {
                    return RedirectToAction("ApprovalList");
                }
            }

           // software pvt. ltd ecom

            FillEmployee(data);
            var worktime = data.WorkingMinutes;
            if (data.WorkingMinutes < 0)
            {
                data.Time = true;
            }
            data.WorkingMinutes = Math.Abs(worktime);
            return View("Create", data);
        }

        public ActionResult Delete(int id)
        {
            var data = db.Approval.Find(id);
            db.Approval.Remove(data);
            db.SaveChanges();
            return RedirectToAction("ApprovalList");
        }

        private void FillEmployee(Approval model)
        {
            List<SelectListItem> drpEmp = db.Employee.OrderBy(m => m.Name).Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
            }).ToList();

            if (model != null && model.EmpId > 0)
            {
                ViewBag.EmpId = new SelectList(drpEmp, "Value", "Text", model.EmpId.ToString());
            }
            else
            {
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
}