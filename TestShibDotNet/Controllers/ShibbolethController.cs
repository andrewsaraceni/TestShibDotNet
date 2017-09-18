using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using edu.upenn.Shibboleth;
using System.Globalization;

namespace TestShibDotNet.Controllers
{
    [ShibbolethAuthorize]
    public class ShibbolethController : Controller
    {
        public ActionResult Index()
        {
            ShibbolethPrincipal principal = new ShibbolethPrincipal();
            if (!String.IsNullOrEmpty(principal.givenName))
                ViewBag.ShibUser = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    principal.givenName.ToLower()
                );
            else
                ViewBag.ShibUser = principal.eduPersonPrincipalName;
            return View();
        }

        public ActionResult Attributes()
        {
            ShibbolethPrincipal principal = new ShibbolethPrincipal();
            List<KeyValuePair<string, string>> shibAttributes = new List<KeyValuePair<string, string>>();
            shibAttributes.Add(new KeyValuePair<string, string>("eduPersonPrincipalName", principal.eduPersonPrincipalName));
            shibAttributes.Add(new KeyValuePair<string, string>("givenName", principal.givenName));
            shibAttributes.Add(new KeyValuePair<string, string>("surname", principal.surname));
            shibAttributes.Add(new KeyValuePair<string, string>("displayName", principal.displayName));
            shibAttributes.Add(new KeyValuePair<string, string>("mail", principal.mail));
            shibAttributes.Add(new KeyValuePair<string, string>("eduPersonAffiliation", principal.eduPersonAffiliation));
            shibAttributes.Add(new KeyValuePair<string, string>("eduPersonScopedAffiliation", principal.eduPersonScopedAffiliation));
            ViewBag.ShibAttributes = shibAttributes;
            return View();
        }
    }
}