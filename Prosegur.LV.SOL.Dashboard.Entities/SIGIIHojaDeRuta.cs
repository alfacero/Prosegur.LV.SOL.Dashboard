using System;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SIGIIHojaDeRuta
    {
        public int Recorrido { set; get; }

        public DateTime FechaProceso { set; get; }

        public DateTime HoraProceso { set; get; }

        public int Control { set; get; }

        public string OidOT { set; get; }
    }
}
