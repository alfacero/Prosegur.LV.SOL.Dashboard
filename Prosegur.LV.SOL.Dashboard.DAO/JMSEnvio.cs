using System;
using System.Collections.Generic;
using Prosegur.LV.SOL.Dashboard.DAO.Base;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class JMSEnvio : ConexionJMS
    {
        #region "Metodos Publicos"
        
        public IList<Entities.JMSEnvioMensaje> GetMensajes(int desde_horas, string[] delegacion, string integracion, string estado)
        {
            CommandText = Consultas.JMSGetMensajePorHorasDelegacionIntegracionEstado;
            AddParam(":DESDE_HORAS", desde_horas);
            AddParam(":DELEGACION", delegacion);
            AddParam(":INTEGRACION", integracion);
            AddParam(":ESTADO", estado);

            return GetMany<Entities.JMSEnvioMensaje>();
        }

        public IList<Entities.JMSEnvioMensaje> GetUltimosMensajesError(int desde_horas, string[] delegacion, string integracion, string estado)
        {
            CommandText = Consultas.JMSGetUltimoMensajeErrorPorHorasDelegacionIntegracion;
            AddParam(":DESDE_HORAS", desde_horas);
            AddParam(":DELEGACION", delegacion);
            AddParam(":INTEGRACION", integracion);
            AddParam(":ESTADO", estado);

            return GetMany<Entities.JMSEnvioMensaje>();
        }

        public Entities.JMSEnvioMensaje GetUltimoMensaje(string integracion, string atributo2, string operacion, string delegacion)
        {
            CommandText = Consultas.JMSGetUltimoMensajePorAtributo2IntegracionOperacionDelegacion;
            AddParam(":INTEGRACION", integracion);
            AddParam(":ATRIBUTO2", atributo2);
            AddParam(":OPERACION", operacion);
            AddParam(":DELEGACION", delegacion);

            return GetOne<Entities.JMSEnvioMensaje>();
        }

        public Entities.JMSEnvioMensaje GetUltimoMensaje(string integracion, string id, string atributo1, string operacion, string delegacion)
        {
            CommandText = Consultas.JMSGetUltimoMensajePorAtributo1IntegracionIdOperacionDelegacion;
            AddParam(":INTEGRACION", integracion);
            AddParam(":ID", id);
            AddParam(":ATRIBUTO1", atributo1);
            AddParam(":OPERACION", operacion);
            AddParam(":DELEGACION", delegacion);

            return GetOne<Entities.JMSEnvioMensaje>();
        }


        public Entities.JMSEnvioMensaje GetUltimoMensaje(string integracion, string atributo2, string operacion, string delegacion, string[] estado)
        {
            CommandText = Consultas.JMSGetUltimoMensajePorAtributo2IntegracionOperacionDelegacionEstado;
            AddParam(":INTEGRACION", integracion);
            AddParam(":ATRIBUTO2", atributo2);
            AddParam(":OPERACION", operacion);
            AddParam(":DELEGACION", delegacion);
            AddParam(":ESTADO", estado);

            return GetOne<Entities.JMSEnvioMensaje>();
        }

        public Entities.JMSEnvioMensaje GetUltimoMensaje(string integracion, string atributo1, string atributo2, string operacion, string delegacion, string[] estado)
        {
            CommandText = Consultas.JMSGetUltimoMensajePorAtributoIntegracionOperacionDelegacionEstado;
            AddParam(":INTEGRACION", integracion);
            AddParam(":ATRIBUTO1", atributo1);
            AddParam(":ATRIBUTO2", atributo2);
            AddParam(":OPERACION", operacion);
            AddParam(":DELEGACION", delegacion);
            AddParam(":ESTADO", estado);

            return GetOne<Entities.JMSEnvioMensaje>();
        }

        public IList<Entities.JMSEnvioMensaje> GetMensajes(int[] jms_id)
        {
            CommandText = Consultas.JMSGetMensajePorJmsId;
            AddParam(":JMS_ID", jms_id);

            return GetMany<Entities.JMSEnvioMensaje>();
        }

        public IList<Entities.JMSEnvioMensaje> GetMensajes(string[] delegacion, string integracion, string[] estado, string[] id, string operacion)
        {
            CommandText = Consultas.JMSGetMensajePorDelegacionIntegracionEstadoIdOperacion;
            AddParam(":DELEGACION", delegacion);
            AddParam(":INTEGRACION", integracion);
            AddParam(":ESTADO", estado);
            AddParam(":ID", id);
            AddParam(":OPERACION", operacion);

            return GetMany<Entities.JMSEnvioMensaje>();
        }
        public IList<Entities.JMSEnvioMensaje> GetMensajesCierreFact(string[] delegacion, DateTime fec_alta)
        {
            CommandText = Consultas.JMSGetMensajeCierreFactPorDelegacion;
            AddParam(":DELEGACION", delegacion);
            AddParam(":FEC_ALTA", fec_alta);
          
            return GetMany<Entities.JMSEnvioMensaje>();
        }

        #endregion
    }
}
