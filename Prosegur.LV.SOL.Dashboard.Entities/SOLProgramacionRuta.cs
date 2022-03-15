using System;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLProgramacionRuta
    {
        public string DescripcionDelegacion { set; get; }

        public string CodigoDelegacion { set; get; }

        public int CodigoRuta { set; get; }

        public string OidRuta { set; get; }

        public DateTime FechaCierreServicio { set; get; }

        public DateTime FechaCierreRecurso { set; get; }
    }
}
