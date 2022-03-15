namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLIntegracionProgramacionDelegacion
    {
        public enum EstadosIntegracionProgramacionDelegacion
        {
            ACerrar = 1,
            Integrando = 2,
            Integrada = 3,
            Error = 4,
        }

        public EstadosIntegracionProgramacionDelegacion Estado { set; get; }

        public SOLDelegacion Delegacion { set; get; }
    }
}
