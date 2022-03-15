using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SIGIIHojaDeRuta : ConexionSIGII
    {
        public IList<Entities.SIGIIHojaDeRuta> GetHojasDeRuta(int[] recorrido, DateTime f_servicio)
        {
            AbrirConexion();

            try
            {

                SetLockModeOn();

                CommandText = Consultas.SIGIIGetHojaDeRutaPorRecorridoFechaDeServicio;
                AddParam("@RECORRIDO", recorrido);
                AddParam("@F_SERVICIO", f_servicio.ToString("yyyy/MM/dd"));

                return GetMany<Entities.SIGIIHojaDeRuta>();
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
