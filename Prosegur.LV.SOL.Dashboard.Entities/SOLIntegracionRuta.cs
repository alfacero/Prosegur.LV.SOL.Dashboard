namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLIntegracionRuta
    {
        public enum SOLIntegracionRutaEstado
        {
            Integrada,
            ConErroresDeIntegracion
        }

        public SOLIntegracionRutaEstado Estado { set; get; }

        public int Cantidad { set; get; }
    }
}
