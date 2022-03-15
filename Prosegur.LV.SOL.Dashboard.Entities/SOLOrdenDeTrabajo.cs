using System;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLOrdenDeTrabajo
    {
        public string OidOt { set; get; }
        public string CodigoServicio { set; get; }
        public int CodigoSecuencia { set; get; }
        public int CodigoRuta { set; get; }
        public string OidRuta { set; get; }
        public DateTime FechaProgramadaInicio { set; get; }
        public string CodigoDelegacion { set; get; }
    }
}
