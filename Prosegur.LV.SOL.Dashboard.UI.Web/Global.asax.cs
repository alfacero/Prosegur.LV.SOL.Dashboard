using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using Prosegur.LV.SOL.Dashboard.UI.Web.ModelBinders;
using Prosegur.LV.SOL.Dashboard.UI.Web.Models;
using System.Runtime.Remoting.Messaging;
using log4net;
using Prosegur.LV.SOL.Dashboard.Core;

namespace Prosegur.LV.SOL.Dashboard.UI.Web
{
    // Nota: para obtener instrucciones sobre cómo habilitar el modo clásico de IIS6 o IIS7, 
    // visite http://go.microsoft.com/?LinkId=9394801

    public class GlobalAsax : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            // Removida la registracion del filtro. LINK: http://benfoster.io/blog/aspnet-mvc-custom-error-pages
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BundleTable.EnableOptimizations = false;

            // IMPORTANT!!! Add Here Chart filter models for Data Binding 
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(RouteBalanceFilterViewModel), new DashBoardModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(OTIntegrationFilterViewModel), new DashBoardModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(ProgrammationFilterViewModel), new DashBoardModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(ProgrammationBarFilterViewModel), new DashBoardModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(CierreFactFilterViewModel), new DashBoardModelBinder());

        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Usuario coreUsuario = new Usuario();

                SerializedUser serializedUser = serializer.Deserialize<SerializedUser>(authTicket.UserData);

                UserData userData = new UserData()
                {
                    UserCode = serializedUser.UserCode,
                    UserName = serializedUser.UserName,
                    ADUser = serializedUser.ADUser
                };
                                
                Entities.SOLUsuario entUsuario = coreUsuario.GetUsuarioConRoles(userData.ADUser).FirstOrDefault();

                userData.Roles = (from r in entUsuario.Roles
                                  select new Role()
                                  {
                                      RoleCode = r.CodigoRol,
                                      RoleId = r.OidRol,
                                      RoleDescription = r.DescripcionRol,
                                  });

                HttpContext.Current.User = userData;
            }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            LogicalThreadContext.Properties["requestGUID"] = new ActivityIdHelper();
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();

            if (exception is Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                Response.StatusCode = 500;
                Response.Status = string.Format("{0} Se produjo un error técnico. Comúniquese por mail con el código \"{1}\" a \"ar_lvge_lv@prosegur.com\"", Response.StatusCode, ((Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)exception).RequestGUID);
            }

            if (exception.InnerException is Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                Response.StatusCode = 500;
                Response.Status = string.Format("{0} Se produjo un error técnico. Comúniquese por mail con el código \"{1}\" a \"ar_lvge_lv@prosegur.com\"", Response.StatusCode, ((Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)exception.InnerException).RequestGUID);
            }

        }
    }
}