using System;
using System.Collections.Generic;
using Prosegur.LV.SOL.Dashboard.DAO.Base;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public  class SolRutasFinalizadasCerradas : ConexionSOL
    {
        public IList<Entities.SolRFinalizadasCerradas> GetRutasFinalizadasCerradas(DateTime f_servicio, string[] delegaciones)
        {
            AbrirConexion();

            try
            {
                CommandText = Consultas.SOLRutasFinalizadasCerradas;
                AddParam(":FEC_RUTA", f_servicio);
                AddParam(":DELEGACIONES", delegaciones);
  
                return GetMany<Entities.SolRFinalizadasCerradas>();
            }
            catch
            {
                throw;
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}
