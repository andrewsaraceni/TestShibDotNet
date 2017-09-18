using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Principal;
using System.Configuration;

namespace edu.upenn.Shibboleth
{
    public class ShibbolethAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (AuthorizeCore(filterContext.HttpContext))
            {
                var principal = new ShibbolethPrincipal();
                filterContext.HttpContext.User = principal;
                filterContext.HttpContext.Session["eduPersonPrincipalName"] = principal.eduPersonPrincipalName;
            }
            else
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!String.IsNullOrEmpty(httpContext.Request.ServerVariables["HTTP_EPPN"]) ||
                httpContext.Session["eduPersonPrincipalName"] != null)
            {
                return true;
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new {
                        controller = "Account",
                        action = "Login",
                        returnUrl = filterContext.HttpContext.Request.Url.GetComponents(
                            UriComponents.PathAndQuery, UriFormat.SafeUnescaped
                        )
                    }
                )
            );
        }
    }
    
    public class ShibbolethPrincipal : GenericPrincipal
    {
        public string eduPersonPrincipalName
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_EPPN"]; }
        }

        public string givenName
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_GIVENNAME"]; }
        }

        public string surname
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_SN"]; }
        }

        public string displayName
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_DISPLAYNAME"]; }
        }

        public string mail
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_MAIL"]; }
        }

        public string eduPersonAffiliation
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_UNSCOPEDAFFILIATION"]; }
        }

        public string eduPersonScopedAffiliation
        {
            get { return HttpContext.Current.Request.ServerVariables["HTTP_AFFILIATION"]; }
        }

        public ShibbolethPrincipal()
            : base(new GenericIdentity(GetUserIdentity()), GetRoles())
        {
        }

        public static string GetUserIdentity()
        {            
            return HttpContext.Current.Request.ServerVariables["HTTP_EPPN"];
        }

        public static string[] GetRoles()
        {
            string[] roles = null;
            string shibbolethRoles = HttpContext.Current.Request.ServerVariables["HTTP_UNSCOPEDAFFILIATION"];
            if (shibbolethRoles != null)
            {
                roles = shibbolethRoles.Split(';');
            }
            return roles;
        }
    }
}