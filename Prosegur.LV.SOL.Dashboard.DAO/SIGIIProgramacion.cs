
using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SIGIIProgramacion : ConexionSIGII
    {
        public IList<Entities.SIGIIProgramacion> GetFechasCierreServicios(DateTime f_servicio, string[] delegacion)
        {
            AbrirConexion();

            try
            {
                SetLockModeOn();

                CommandText = Consultas.SIGIIGetFechaProgramacionServicioPorFechaDelegacion;
                AddParam("@F_SERVICIO", f_servicio.ToString("yyyy/MM/dd"));
                AddParam("@DELEGACION", delegacion);

                return GetMany<Entities.SIGIIProgramacion>();
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

        public IList<Entities.SIGIIProgramacion> GetFechasCierreRecursos(DateTime f_servicio, string[] delegacion)
        {
            AbrirConexion();

            try
            {
                SetLockModeOn();

                CommandText = Consultas.SIGIIGetFechaProgramacionRecursoPorFechaDelegacion;
                AddParam("@F_SERVICIO", f_servicio.ToString("yyyy/MM/dd"));
                AddParam("@DELEGACION", delegacion);

                return GetMany<Entities.SIGIIProgramacion>();
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
