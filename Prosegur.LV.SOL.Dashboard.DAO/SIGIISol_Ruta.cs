using System;
using System.Collections.Generic;
using Prosegur.LV.SOL.Dashboard.DAO.Base;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SIGIISOL_Ruta : ConexionSIGII
    {
        public IList<Entities.SIGIISolRuta> GetRutas(DateTime f_servicio, String[] delegaciones)
        {
            AbrirConexion();

            try
            {

                SetLockModeOn();

                CommandText = Consultas.SIGIISol_Ruta;
                AddParam("@DELEGACION", delegaciones);
                AddParam("@F_SERVICIO", f_servicio.ToString("yyyy/MM/dd"));

                return GetMany<Entities.SIGIISolRuta>();
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
