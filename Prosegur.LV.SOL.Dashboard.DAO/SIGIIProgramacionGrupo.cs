using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prosegur.LV.SOL.Dashboard.DAO.Base;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SIGIIProgramacionGrupo : ConexionSIGII
    {
        public IList<Entities.SIGIIProgramacionGrupo> GetEstadoCierres(DateTime f_servicio, string[] delegacion, string[] grupo)
        {
            AbrirConexion();

            try
            {
                SetLockModeOn();

                CommandText = Consultas.SIGIIGetEstadoProgramacionGrupoPorFechaGrupo;
                AddParam("@F_SERVICIO", f_servicio.ToString("yyyy/MM/dd"));
                AddParam("@DELEGACION", delegacion);
                AddParam("@GRUPO", grupo);

                return GetMany<Entities.SIGIIProgramacionGrupo>();
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
