using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SOLProgramacion  : ConexionSOL
    {
        public IList<Entities.SOLProgramacion> GetFechasCierre(DateTime fec_programacion, string[] cod_delegacion)
        {
            CommandText = Consultas.SOLGetFechasProgramacionPorFechaDelegacion;
            AddParam(":FEC_PROGRAMACION", fec_programacion);
            AddParam(":COD_DELEGACION", cod_delegacion);

            return GetMany<Entities.SOLProgramacion>();
        }
    }
}
