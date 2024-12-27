using EmpAttendance.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace EmpAttendance.Controllers
{
    [AuthorizationFilter]

    public class EmployeeController : Controller
    {
        EmployeeAttendanceDBEntities db = new EmployeeAttendanceDBEntities();

        public ActionResult List()
        {
            if(GlobalVariable.IsAdmin == false)
            {
                GlobalFunction.ResetSession();
                return RedirectToAction("Login");
            }
            return View(db.Employee.ToList());
        }

        public ActionResult Create()
        {
            if (GlobalVariable.IsAdmin == false)
            {
                GlobalFunction.ResetSession();
                return RedirectToAction("Login");
            }
            var model = new Employee();
            FillScheme(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee mdl)
        {
            FillScheme(mdl);
            if (ModelState.IsValid == false)
            {
                return View(mdl);
            }
           
            try
            {
                var dataExist = db.Employee.Where(n => n.Id != mdl.Id && 
                (n.Email == mdl.Email || 
                n.Phone == mdl.Phone || 
                n.EmployeeCode == mdl.EmployeeCode)).FirstOrDefault();
           

                if (dataExist != null)
                {
                    if (dataExist.Email == mdl.Email)
                    {
                        ModelState.AddModelError("Email", "User with this email already exists.");
                    }
                    if (dataExist.Phone == mdl.Phone)
                    {
                        ModelState.AddModelError("Phone", "User with this Phone already exists.");
                    }
                    if (dataExist.EmployeeCode == mdl.EmployeeCode)
                    {
                        ModelState.AddModelError("EmployeeCode", "User with this EmployeeCode already exists.");
                    }
                    return View(mdl);

                }
                if (mdl.Id > 0)
                {
                    Employee objExsisting = db.Employee.Find(mdl.Id);
                    objExsisting.Name = mdl.Name;
                    objExsisting.Phone = mdl.Phone;
                    objExsisting.EmployeeCode = mdl.EmployeeCode;
                    objExsisting.Password = mdl.Password;
                    objExsisting.Email = mdl.Email;
                    objExsisting.Salary = mdl.Salary;
                    objExsisting.DailyWorkingMin = mdl.DailyWorkingMin;
                    objExsisting.GapingMinutes = mdl.GapingMinutes;
                    objExsisting.SchemeId = mdl.SchemeId;
                    objExsisting.Remark = mdl.Remark;
                    db.Entry(objExsisting).State = EntityState.Modified;
                }
                else
                {
                    mdl.UserType = "Employee";
                    db.Employee.Add(mdl);
                }
                db.SaveChanges();
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ViewBag.FileStatus = "Error on save employee data. Error: " + ex.Message;
            }

            return View(mdl);
        }

        public ActionResult Delete(int id)
        {
            var data = db.Employee.Find(id);
            db.Employee.Remove(data);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public ActionResult Edit(int id)
        {
            var data = db.Employee.Where(n => n.Id == id).FirstOrDefault();
            FillScheme(data);
            return View("Create", data);
        }

        private void FillScheme(Employee model)
        {
            List<SelectListItem> drpScheme = db.tblMst_Scheme.OrderBy(m => m.Scheme).Select(m => new SelectListItem
            {
                Value = m.SchemeId.ToString(),
                Text = m.Scheme,
            }).ToList();
            if (model != null && model.SchemeId > 0)
            {
                ViewBag.SchemeId = new SelectList(drpScheme, "Value", "Text", model.SchemeId.ToString());
            }
            else
            {
                ViewBag.SchemeId = new SelectList(drpScheme, "Value", "Text");
            }
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            GlobalFunction.ResetSession();
            return View();
        }
        [AllowAnonymous]
        public ActionResult CheckLogin(string Username, string Password)
        {
            try
            {
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    return Json("Please enter your log in details");
                }
                var UserRes = db.Employee.Where(u => u.EmployeeCode == Username).ToList().Where(n => n.Password.Equals(Password, StringComparison.Ordinal)).FirstOrDefault();
                if (UserRes == null)
                {
                    return Json("Authentication faild");
                }

                User objUser = new User();
                objUser.UserType = UserRes.UserType;
                objUser.UserId = UserRes.Id;
                objUser.UserName = UserRes.Name;

                Session["User"] = objUser;

                return Json("success");
            } 
            catch (Exception ex)
            {
             return Json("Error : " + ex.Message);
            }
            
        }
    }
}