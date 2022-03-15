using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Prosegur.LV.SOL.Dashboard.Core.Excepciones;

namespace Prosegur.LV.SOL.Dashboard.Core
{
    public class Usuario
    {
        #region "Metodos Publicos"

        public Entities.SOLUsuario GetUsuario(string desLogin)
        {
            DAO.SOLUsuario dao = new DAO.SOLUsuario();

            Entities.SOLUsuario entUsuario = null;

            try
            {
                entUsuario = dao.GetUsuario(desLogin);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(Usuario));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "desLogin",  desLogin.ToString() }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n {0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return entUsuario;
        }

        /// <summary>
        /// Retorna el usuario SOL con sus Roles en una lista
        /// </summary>
        /// <param name="desLogin">El codigo de usuario</param>
        /// <returns>Retorna un lista con una unica entidad usuario</returns>
        public IList<Entities.SOLUsuario> GetUsuarioConRoles(string desLogin)
        {
            DAO.SOLUsuario dao = new DAO.SOLUsuario();

            IList<Entities.SOLUsuario> entUsuario = null;

            try
            {
                entUsuario = dao.GetUsuarioConRoles(desLogin);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(Usuario));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "desLogin",  desLogin.ToString() }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n {0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return entUsuario;
        }

        /// <summary>
        /// Retorna el listado de delegaciones a las cuales tiene acceso el usuario
        /// </summary>
        /// <param name="oidUsuario">El OID_OT del usuario</param>
        /// <returns>El listado de delegaciones</returns>
        public IList<Entities.SOLDelegacion> GetUsuarioDelegaciones(string oidUsuario)
        {
            IList<Entities.SOLDelegacion> delegaciones = null;

            DAO.SOLUsuario dao = new DAO.SOLUsuario();

            ILog log = LogManager.GetLogger(typeof(Usuario));

            log.Info(string.Format("Se buscaron las delegaciones para el OID Usuario SOL \"{0}\"", oidUsuario));

            try
            {
                delegaciones = dao.GetUsuarioDelegaciones(oidUsuario);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "oidUsuario",  oidUsuario }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n {0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return delegaciones;
        }

        #endregion
    }
}
