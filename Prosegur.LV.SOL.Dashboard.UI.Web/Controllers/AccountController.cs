using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using log4net;
using Prosegur.LV.SOL.Dashboard.Core;
using Prosegur.LV.SOL.Dashboard.Entities;
using Prosegur.LV.SOL.Dashboard.UI.Web.Models;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel());
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SOLUsuario entUsuario = null;
                    UserData user = null;
                    string userName = string.Empty;
                    bool usuarioValido = false;

                    if (model.UserName.Equals(Configuracion.UsuarioAdmin))
                    {
                        if (model.Password.Equals(Configuracion.PasswordAdmin))
                        {
                            usuarioValido = true;

                            userName = Configuracion.UsuarioSolSuplantacionAdmin;
                        }
                    }
                    else
                    {
                        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, Configuracion.DominioProsegur))
                        {
                            if (pc.ValidateCredentials(model.UserName, model.Password))
                            {
                                usuarioValido = true;

                                userName = model.UserName;
                            }
                        }
                    }

                    if (usuarioValido)
                    {
                        Usuario coreUsuario = new Usuario();

                        entUsuario = coreUsuario.GetUsuario(userName);

                        if (entUsuario != null)
                        {
                            user = new UserData()
                            {
                                UserCode = entUsuario.Oid,
                                UserName = entUsuario.Nombre,
                                ADUser = userName
                            };

                            SerializedUser userToSerialize = new SerializedUser()
                            {
                                ADUser = user.ADUser,
                                UserCode = user.UserCode,
                                UserName = user.UserName,
                            };

                            JavaScriptSerializer serializer = new JavaScriptSerializer();

                            string userData = serializer.Serialize(userToSerialize);

                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                     1,
                                     model.UserName,
                                     DateTime.Now,
                                     DateTime.Now + FormsAuthentication.Timeout,
                                     model.RememberMe,
                                     userData);

                            string encTicket = FormsAuthentication.Encrypt(authTicket);

                            int cookieSize = System.Text.ASCIIEncoding.ASCII.GetByteCount(encTicket);

                            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);

                            faCookie.Expires = model.RememberMe ? authTicket.Expiration : DateTime.MinValue;

                            Response.Cookies.Add(faCookie);

                            return RedirectToLocal(returnUrl);
                        }
                    }
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                { 
                    ILog log = LogManager.GetLogger(typeof(AccountController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }

                // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
                ModelState.AddModelError("", "El nombre de usuario o la contraseña especificados son incorrectos.");
            }

            return View(model);
        }

        public ActionResult UserMenuPartial()
        { 
            IEnumerable<DashboardMenuViewModel> listMenu = new List<DashboardMenuViewModel>();

            if (Request.IsAuthenticated)
            {
                UserData userData = (UserData)HttpContext.User;
                
                listMenu = ResolveUserMenues(userData.Roles);
            }

            return PartialView(listMenu);
        }
                
        //
        // POST: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
        
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private IEnumerable<DashboardMenuViewModel> ResolveUserMenues(IEnumerable<Role> roles)
        {
            // Available Menues
            List<DashboardMenuViewModel> availableListMenues = new List<DashboardMenuViewModel>()
            {
                new DashboardMenuViewModel() {
                    Id = "Dashboard_1",
                    LinkText = "Control de Rutas",
                    ActionName = "Route",
                    ControllerName = "Dashboard"
                },
                //new DashboardMenuViewModel() {
                //    Id = "Dashboard_2",
                //    LinkText = "Programacion",
                //    ActionName = "Programmation",
                //    ControllerName = "Dashboard"
                //},
                new DashboardMenuViewModel() {
                    Id = "Dashboard_3",
                    LinkText = "Prog. por Delegacion",
                    ActionName = "ProgrammationBar",
                    ControllerName = "Dashboard"
                },
                new DashboardMenuViewModel() {
                    Id = "Dashboard_4",
                    LinkText = "Cierres de Facturación",
                    ActionName = "CierresFacturacionBar",
                    ControllerName = "Dashboard"
                }
            };

            List<DashboardMenuViewModel> listMenues = new List<DashboardMenuViewModel>();

            foreach (DashboardMenuViewModel availableMenu in availableListMenues)
            {
                string rolesNecesarios = ConfigurationManager.AppSettings[availableMenu.Id];

                if (!string.IsNullOrEmpty(rolesNecesarios))
                {
                    DashboardMenuViewModel listMenu = (from rol in rolesNecesarios.Split(';')
                                                      where roles.Any(r => r.RoleId.Equals(rol))
                                                      select availableMenu).FirstOrDefault();

                    if (listMenu != null)
                    {
                        listMenues.Add(listMenu);
                    }
                }                
            }

            return listMenues;
        }
    }
}
