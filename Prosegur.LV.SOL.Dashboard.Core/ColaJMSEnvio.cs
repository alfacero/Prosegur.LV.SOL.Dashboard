using log4net;
using Prosegur.LV.SOL.Dashboard.Core.Excepciones;
using Prosegur.LV.SOL.Dashboard.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.Core
{
    public class ColaJMSEnvio
    {
        public IList<JMSEnvioMensaje> GetMensajes(int[] jmsId)
        {
            DAO.JMSEnvio daoJmsEnvio = new DAO.JMSEnvio();

            IList<JMSEnvioMensaje> jmsEnvioMensajes = null;

            try
            {
                jmsEnvioMensajes = daoJmsEnvio.GetMensajes(jmsId);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(ColaJMSEnvio));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "jmsId", string.Join(", ", jmsId) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return jmsEnvioMensajes;
        }
    }
}
