using System.Collections.Generic;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SIGIIPlanta
    {
        public SIGIIPlanta()
        {
            Basureros = new List<SIGIIRecorrido>();
        }

        public string Codigo { set; get; }

        public string Descripcion { set; get; }

        public IList<SIGIIRecorrido> Basureros { set; get; }
    }
}
