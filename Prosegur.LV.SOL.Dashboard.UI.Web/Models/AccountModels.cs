using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Models
{
    public class LoginModel
    {
        public LoginModel()
        {
            RememberMe = true;
        }

        [Required]
        [Display(Name = "Usuario AD")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Recordar cuenta?")]
        public bool RememberMe { get; set; }
    }

    public class SerializedUser
    {
        public string UserCode { set; get; }

        public string UserName { set; get; }

        public string ADUser { set; get; }

        public SerializedRol[] Roles { set; get; }

        public class SerializedRol
        {
            public string RolCode { set; get; }

            public string RolId { set; get; }

            public string RolDescription { set; get; }
        }
    }

    public class UserData : IPrincipal
    {
        public string UserCode { set; get; }
        
        public string UserName { set; get; }

        public string ADUser { set; get; }

        public IEnumerable<Role> Roles { set; get; }

        public IIdentity Identity
        {
            get {
                return new GenericIdentity(UserName);
            }
        }

        public bool IsInRole(string roleCode)
        {
            bool isInRole = false;

            if (Roles != null && Roles.Count() > 0)
            {
                isInRole = Roles.Where(r => r.RoleCode == roleCode).Any();
            }

            return isInRole;
        }
    }

    public class Role
    {
        public string RoleCode { set; get; }

        public string RoleId { set; get; }

        public string RoleDescription { set; get; }
    }

    public class DashboardMenuViewModel
    {
        public string Id { get; set; }

        public string LinkText { get; set; }
        
        public string ActionName { get; set; }

        public string ControllerName { get; set; }
    }
}
