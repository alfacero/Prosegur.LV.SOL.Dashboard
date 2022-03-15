using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Prosegur.LV.SOL.Dashboard.Core.Excepciones;

namespace Prosegur.LV.SOL.Dashboard.Core
{
    public class SOLOrdenDeTrabajo
    {
        /// <summary>
        /// Retorna los mensajes de integracion Pendientes y c/Error a partir de cierta hora
        /// </summary>
        /// <param name="delegaciones">Listado de delegaciones</param>
        /// <param name="desdeHoras">Cantidad de horas a partir de cuando se deben retornar los mensajes</param>
        /// <returns>Listado de mensajes de la cola JMS insertados por el servicio de integracion</returns>
        public IList<Entities.JMSEnvioMensaje> GetMensajesIntegracion(string[] delegaciones, int desdeHoras)
        {
            List<Entities.JMSEnvioMensaje> mensajes = new List<Entities.JMSEnvioMensaje>();

            DAO.JMSEnvio dao = new DAO.JMSEnvio();

            try
            {
                // 1º Retornar mensajes con estado pendiente
                mensajes.AddRange(dao.GetMensajes(desdeHoras, delegaciones, "OT", Configuracion.CodigoEstadoPendiente));

                // 2º Retornar ultimos mensajes con estado error
                mensajes.AddRange(dao.GetUltimosMensajesError(desdeHoras, delegaciones, "OT", Configuracion.CodigoEstadoError));
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(SOLOrdenDeTrabajo));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "delegaciones", string.Join(", ", delegaciones) },
                    { "desdeHoras", desdeHoras.ToString() }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return mensajes;
        }

        /// <summary>
        /// Retorna los mensajes de integracion con Error a partir de cierta hora
        /// </summary>
        /// <param name="delegaciones">Listado de delegaciones</param>
        /// <param name="desdeHoras">Cantidad de horas a partir de cuando se deben retornar los mensajes</param>
        /// <returns>Listado de mensajes de la cola JMS insertados por el servicio de integracion</returns>
        public IList<Entities.JMSEnvioMensaje> GetMensajesIntegracionError(string[] delegaciones, int desdeHoras)
        {
            IList<Entities.JMSEnvioMensaje> mensajes = new List<Entities.JMSEnvioMensaje>();

            DAO.JMSEnvio dao = new DAO.JMSEnvio();

            try
            {
                mensajes = dao.GetUltimosMensajesError(desdeHoras, delegaciones, "OT", Configuracion.CodigoEstadoError);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(SOLOrdenDeTrabajo));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "delegaciones", string.Join(", ", delegaciones) },
                    { "desdeHoras", desdeHoras.ToString() }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return mensajes;
        }

        /// <summary>
        /// Retorna un listado de Ordenes de trabajo
        /// </summary>
        /// <param name="oidOTs">Los OID_OT de las ordenes a buscar</param>
        /// <returns>Listado de Ordenes de Trabajo</returns>
        public IList<Entities.SOLOrdenDeTrabajo> GetListado(string[] oidOTs)
        { 
            DAO.SOLOrdenDeTrabajo dao = new DAO.SOLOrdenDeTrabajo();

            IList<Entities.SOLOrdenDeTrabajo> solOrdenesDeTrabajo = new List<Entities.SOLOrdenDeTrabajo>();

            try
            {
                solOrdenesDeTrabajo = dao.GetDetalleOrdenesDeTrabajo(oidOTs);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(SOLOrdenDeTrabajo));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "oidOTs", string.Join(", ", oidOTs) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return solOrdenesDeTrabajo;
        }
    }
}
