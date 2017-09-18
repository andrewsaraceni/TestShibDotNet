using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using edu.upenn.Shibboleth;
using System.Configuration;

namespace TestShibDotNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string eppn = null;
            if (Session["eduPersonPrincipalName"] != null)
                eppn = Session["eduPersonPrincipalName"].ToString();

            if (!String.IsNullOrEmpty(eppn))
                return RedirectToAction("Index", "Shibboleth");
            else
                return View();
        }
    }
}