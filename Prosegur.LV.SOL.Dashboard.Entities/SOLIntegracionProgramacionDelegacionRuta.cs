namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLIntegracionProgramacionDelegacionRuta
    {
        public enum EstadosIntegracionProgramacionDelegacionRuta
        {
            ACerrar = 1,
            Integrando = 2,
            Integrado = 3,
            Error = 4,
        }

        public EstadosIntegracionProgramacionDelegacionRuta Estado { set; get; }

        public SOLRuta Ruta { set; get; }

        public SOLDelegacion Delegacion { set; get; }
    }

    
}
