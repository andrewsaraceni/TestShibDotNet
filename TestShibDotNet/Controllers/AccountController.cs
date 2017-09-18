using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using edu.upenn.Shibboleth;

namespace TestShibDotNet.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl))
                return Redirect(Url.Content("~/"));
            else
                return Redirect(returnUrl);
        }

        [HttpPost]
        [ShibbolethAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return Redirect(ConfigurationManager.AppSettings["ShibLogoutPath"]);
        }
    }
}