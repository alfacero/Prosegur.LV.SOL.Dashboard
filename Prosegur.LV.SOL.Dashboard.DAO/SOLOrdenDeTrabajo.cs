using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SOLOrdenDeTrabajo : ConexionSOL 
    {
        public IList<Entities.SOLOrdenDeTrabajo> GetDetalleOrdenesDeTrabajo(string[] oid_ot)
        {
            CommandText = Consultas.SOLGetDetalleOrdenDeTrabajo;
            AddParam(":OID_OT", oid_ot.ToList());
            
            return GetMany<Entities.SOLOrdenDeTrabajo>();
        }

        public IList<Entities.SOLOrdenDeTrabajo> GetOrdenesDeTrabajoAIntegrar(string[] delegacion, DateTime fec_ruta, int[] cod_ruta_basurero)
        {
            CommandText = Consultas.SOLGetOrdenDeTrabajoAIntegrarPorFechaServicioDelegacion;
            AddParam(":DELEGACION", delegacion);
            AddParam(":FEC_RUTA", fec_ruta);
            AddParam(":COD_RUTA_BASURERO", cod_ruta_basurero);

            return GetMany<Entities.SOLOrdenDeTrabajo>();
        }

        public IList<Entities.SOLOrdenDeTrabajo> GetOrdenesDeTrabajoAIntegrar(int[] cod_ruta, DateTime fec_ruta)
        {
            CommandText = Consultas.SOLGetOrdenDeTrabajoAIntegrarPorFechaServicioRuta;
            AddParam(":FEC_RUTA", fec_ruta);
            AddParam(":COD_RUTA", cod_ruta);

            return GetMany<Entities.SOLOrdenDeTrabajo>();
        }
    }
}
