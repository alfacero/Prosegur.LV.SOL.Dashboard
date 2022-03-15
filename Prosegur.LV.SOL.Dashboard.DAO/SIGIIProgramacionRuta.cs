
using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SIGIIProgramacionRecorrido : ConexionSIGII
    {
        public IList<Entities.SIGIIProgramacionRecorrido> GetEstadoCierres(DateTime f_servicio, int[] recorrido)
        {
            AbrirConexion();

            try
            {
                SetLockModeOn();

                CommandText = Consultas.SIGIIGetEstadoProgramacionRecorridoPorFechaRecorrido;
                AddParam("@F_SERVICIO", f_servicio.ToString("yyyy/MM/dd"));
                AddParam("@RECORRIDO", recorrido);

                return GetMany<Entities.SIGIIProgramacionRecorrido>();
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
