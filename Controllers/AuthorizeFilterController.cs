using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EmpAttendance.Controllers
{
    public class AuthorizationFilter : AuthorizeAttribute
    {
        public bool AllowAnonymous { get; set; }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Pass the current action descriptor to the AuthorizeCore
            // method on the same thread by using HttpContext.Items
            filterContext.HttpContext.Items["ActionDescriptor"] = filterContext.ActionDescriptor;
            base.OnAuthorization(filterContext);
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var actionDescriptor = httpContext.Items["ActionDescriptor"] as ActionDescriptor;
            if (actionDescriptor != null)
            {
                AuthorizationFilter attribute = GetAuthorizePublicAttribute(actionDescriptor);
                if (attribute.AllowAnonymous)
                    return true;
                // Logic
            }
           
            if (GlobalVariable.UserId > 0)
            {
                return true;
            }
            return false;
           

            //return true;
        }
        // Gets the Attribute instance of this class from an action method or contoroller.
        // An action method will override a controller.
        private AuthorizationFilter GetAuthorizePublicAttribute(ActionDescriptor actionDescriptor)
        {
            AuthorizationFilter result = null;

            // Check if the attribute exists on the action method
            result = (AuthorizationFilter)actionDescriptor
                .GetCustomAttributes(attributeType: typeof(AuthorizationFilter), inherit: true)
                .SingleOrDefault();

            if (result != null)
            {
                return result;
            }

            // Check if the attribute exists on the controller
            result = (AuthorizationFilter)actionDescriptor
                .ControllerDescriptor
                .GetCustomAttributes(attributeType: typeof(AuthorizationFilter), inherit: true)
                .SingleOrDefault();

            return result;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary
               {
                    { "controller", "Employee" },
                    { "action", "Login" }
               });
        }
        //protected override bool AuthorizeCore(HttpContextBase filterContext)
        //{

        //    if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
        //        || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
        //    {
        //        // Don't check for authorization as AllowAnonymous filter is applied to the action or controller
        //        return false;
        //    }

        //    // Check for authorization
        //    if (HttpContext.Current.Session["User"] == null || GlobalVariable.UserId==0)
        //    {
        //        filterContext.Result = new RedirectResult("~/Employee/login");
        //        return false;
        //        //filterContext.Result = new filterContext.Result =();
        //    }
        //    if (GlobalVariable.UserId >0)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}