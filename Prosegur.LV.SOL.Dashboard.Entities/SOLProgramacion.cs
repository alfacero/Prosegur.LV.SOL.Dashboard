using System;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLProgramacion
    {
        public string CodigoDelegacion { set; get; }

        public string DescripcionDelegacion { set; get; }

        public DateTime? FechaCierreServicio { set; get; }

        public DateTime? FechaCierreRecurso { set; get; }
    }
}
