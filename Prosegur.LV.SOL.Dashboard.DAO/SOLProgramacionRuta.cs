using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SOLProgramacionRuta : ConexionSOL
    {
        public IList<Entities.SOLProgramacionRuta> GetFechasCierre(DateTime fec_ruta, string[] cod_delegacion)
        {
            CommandText = Consultas.SOLGetEstadoProgramacionRutaPorFechaDelegacion;
            AddParam(":FEC_RUTA", fec_ruta);
            AddParam(":COD_DELEGACION", cod_delegacion);

            return GetMany<Entities.SOLProgramacionRuta>();
        }
    }
}
