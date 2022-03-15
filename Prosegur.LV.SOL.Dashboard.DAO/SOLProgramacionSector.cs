using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SOLProgramacionSector : ConexionSOL
    {
        public IList<Entities.SOLProgramacionSector> GetEstadoCierre(DateTime fec_cierre, string[] cod_delegacion)
        {
            CommandText = Consultas.SOLGetEstadoProgramacionSectorPorFechaDelegacion;
            AddParam(":FEC_CIERRE", fec_cierre);
            AddParam(":COD_DELEGACION", cod_delegacion);

            return GetMany<Entities.SOLProgramacionSector>();
        }
    }
}
