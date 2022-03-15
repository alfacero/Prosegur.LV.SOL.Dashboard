using System;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLProgramacionSector
    {
        public string CodigoDelegacion { set; get; }

        public string DescripcionDelegacion { set; get; }

        public string CodigoSector { set; get; }

        public DateTime? FechaCierre { set; get; }

        public int CantidadRecursos { set; get; }
    }
}
