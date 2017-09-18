TestShibDotNet
==============

This repository contains a conceptual ASP.NET MVC web application that demonstrates integrating authentication and authorization with Shibboleth into any .NET web application.

**Developer:** Andrew Saraceni (saraceni@wharton.upenn.edu)

Integrating Shibboleth Into Your Application
--------------------------------------------

Looking to get up and running quickly?  Here's a quick way to integrate Shibboleth with your .NET application:

1. Add the [Shibboleth Service](TestShibDotNet/Services/ShibbolethService.cs) class into your .NET project for communicating with the web server and the Shibboleth service provider (SP).

2. Add the [Account Controller](TestShibDotNet/Controllers/AccountController.cs) into your project as well, to provide basic login and logout functionality.

3. In your _web.config_ file, define an appSettings key/value pair for **ShibLogoutPath** to define the IdP's logout URL; in Penn/Wharton's case, this is: [https://idp.pennkey.upenn.edu/logout](https://idp.pennkey.upenn.edu/logout)

	```xml
    <configuration>
      <appSettings>
        ...
        <add key="ShibLogoutPath" value="https://idp.pennkey.upenn.edu/logout" />
      </appSettings>
    </configuration>
	```

4. Decorate any actions/controllers you wish to only allow Shibboleth-authenticated access to using the `[ShibbolethAuthorize]` attribute, e.g.:

	```c#
    using edu.upenn.Shibboleth
    
    public class NewAppController : Controller
    {
        [ShibbolethAuthorize]
        public ActionResult DoSomething()
        {
            ...
        }
    }
    
    [ShibbolethAuthorize]
    public class AnotherNewAppController : Controller
    {
        ...
    }
	```

    * You can also authorize views based on users' eduPersonPrincipalName (Users) and eduPersonAffiliation (Roles) membership, much as you would with the common `[Authorize]` attribute, e.g.
  
        ```c#
        [ShibbolethAuthorize(Users = "saraceni@upenn.edu")]
        [ShibbolethAuthorize(Roles = "staff")]
        [ShibbolethAuthorize(Roles = "student")]
        // etc.
        ```
  
    * Feel free to make use of the ShibbolethPrincipal class directly within your controllers when retrieving additional information about the authenticated user:
  
	    ```c#
        [ShibbolethAuthorize]
        public ActionResult ShibEmail()
        {
	        ShibbolethPrincipal principal = new ShibbolethPrincipal();
	        ViewBag.Message = String.Format("Your email address is: {0}", principal.mail);
        }
	    ```
  
    * ...and/or use common principal-related checks in your views:
  
	    ```html
        @if (User.Identity.IsAuthenticated)
        {
	        <p>Looks like you're logged in!</p>
        }
        else
        {
	        <p>You're not logged in.</p>
        }
	    ```
	
5. On the system(s) where your app is hosted, ensure the Shibboleth SP, via the _shibboleth2.xml_ file, is configured for both the IIS site for the URL, and the path(s) to directly protect with Shibboleth.  In this app's case, we only directly protect the main login path, and rely on `[ShibbolethAuthorize]` to handle requiring Shibboleth authentication/authorization overall:

	```xml
    <InProcess logger="native.logger">
        <ISAPI normalizeRequest="true" safeHeaderNames="true">
            <Site id="1" name="someurl.wharton.upenn.edu"/>
        </ISAPI>
    </InProcess>

    <RequestMapper type="Native">
        <RequestMap>
            <Host name="someurl.wharton.upenn.edu" authType="shibboleth" requireSession="false"> 
                <Path name="TestShibDotNet/Account/Login" authType="shibboleth" requireSession="true"/> 
            </Host>
        </RequestMap>
    </RequestMapper>
    ```

6.  Congrats!  You should be ready to test Shibboleth authentication and authorization with your .NET application.

Other Shibboleth Integration Options
------------------------------------

Are there other ways to protect your .NET application with Shibboleth?  Yes!  The `[ShibbolethAuthorize]` attribute provides a happy medium between vastly simplified Shibboleth protection cases, and more elaborate login/authentication scenarios.

If you're looking to put an entire app behind Shibboleth protection, and generally ignore granular control or obtaining information about the active user, you can place the root URL and path of the application as the RequestMap entry in your SP's _shibboleth2.xml_ file:

```xml
<RequestMapper type="Native">
    <RequestMap>
        <!-- Protects the entire path of the app and all of it's associated URLs. -->
        <Host name="someurl.wharton.upenn.edu" authType="shibboleth" requireSession="false"> 
            <Path name="NewApp" authType="shibboleth" requireSession="true"/> 
        </Host>
    </RequestMap>
</RequestMapper>
```

For more complex scenarios with creating, updating, etc. user models, or other login and authentication-related processes that need to be integrated, feel free to extend or alter the ShibbolethService or AccountController classes to meet your needs.
	
While this example app is built upon the .NET Framework, the concepts demonstrated within the app should also be generally applicable to .NET Core as well.

How It Works
------------

A [Shibboleth Service](TestShibDotNet/Services/ShibbolethService.cs) defines two classes which facilitate adding Shibboleth authentication and authorization to your app:

  * **ShibbolethAuthorizeAttribute**
    * Inherits from the base [AuthorizeAttribute](https://msdn.microsoft.com/en-us/library/system.web.mvc.authorizeattribute(v=vs.118).aspx) class
	* Provides a means of only allowing authenticated users' access to actions/controllers
	* For a request decorated with the `ShibbolethAuthorize` attribute:
	  * The "HTTP_EPPN" server variable (Shibboleth session) and the .NET session variable "eduPersonPrincipalName" are checked to see if the user has authenticated with the IdP/SP and the app, respectively
	    * If a Shibboleth session specifically does not exist for the user, the request is redirected to the default Login controller, which is Shibboleth protected and handles authenticating the user
	    * If a Shibboleth session does exist, the ShibbolethPrincipal is instantiated and added to the request, with the .NET session variable populated via the information from the principal
  * **ShibbolethPrincipal**
    * Inherits from the base [GenericPrincipal](https://msdn.microsoft.com/en-us/library/system.security.principal.genericprincipal(v=vs.110).aspx) class
    * When instantiated:
	  * The current user is pulled from the HTTP_EPPN server variable (eduPersonPrincipalName)
	  * The current roles of the user are generated/parsed from the HTTP_UNSCOPEDAFFILIATION server variable (eduPersonAffiliation)
	  * Property lookups for the user are available on the principal for the following Shibboleth attributes, utilizing [Penn's attributes available to all SPs](http://www.upenn.edu/computing/weblogin/shibboleth/attribute.html#affiliations):
	    * eduPersonPrincipalName (HTTP_EPPN)
		* givenName (HTTP_GIVENNAME)
		* surname (HTTP_SN)
		* displayName (HTTP_DISPLAYNAME)
		* mail (HTTP_MAIL)
		* eduPersonAffiliation (HTTP_UNSCOPEDAFFILIATION)
		* eduPersonScopedAffiliation (HTTP_AFFILIATION)

A basic [Account Controller](TestShibDotNet/Controllers/AccountController.cs) defines two action methods for handling login and logout functionality:
  * **Login**
	* On login, the SP will take over the login process (due to the path for the controller being protected), and pass the call back to the controller once completed
	  * This merely serves as a redirect point post-Shibboleth authentication to the app's home page (if a returnURL is not passed), or the desired returnUrl
  * **Logout**
    * On logout, the .NET session variables are cleared, the app session is destroyed and the user is redirected to the path specified in the **ShibLogoutPath** _web.config_ appSettings section
	  * The IdP handles the logout of the overall Shibboleth session, but the app's session is ended prior to the redirect
