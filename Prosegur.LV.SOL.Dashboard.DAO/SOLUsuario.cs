using System.Collections.Generic;
using Prosegur.LV.SOL.Dashboard.DAO.Base;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SOLUsuario : ConexionSOL
    {
        #region "Metodos Publicos"

        public Entities.SOLUsuario GetUsuario(string des_login)
        {
            CommandText = Consultas.SOLGetUsuario;
            AddParam(":DES_LOGIN", des_login.ToLower());
            return GetOne<Entities.SOLUsuario>();
        }

        public IList<Entities.SOLUsuario> GetUsuarioConRoles(string des_login)
        {
            CommandText = Consultas.SOLGetUsuarioConRoles;
            AddParam(":DES_LOGIN", des_login.ToLower());
            return GetMany<Entities.SOLUsuario, Entities.SOLRolUsuario, string>
                (
                    u => CastTo<string>(u["oid"]),
                    p => p.Roles,
                    u => string.IsNullOrEmpty(CastTo<string>(u["oidRol"]))
                );
        }

        public IList<Entities.SOLDelegacion> GetUsuarioDelegaciones(string oid_usuario)
        {
            CommandText = Consultas.SOLGetUsuarioDelegaciones;
            AddParam(":OID_USUARIO", oid_usuario);
            return GetMany<Entities.SOLDelegacion>();
        }

        #endregion
    }
}
