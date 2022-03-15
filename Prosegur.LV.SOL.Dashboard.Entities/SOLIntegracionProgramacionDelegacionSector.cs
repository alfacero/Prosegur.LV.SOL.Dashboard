namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLIntegracionProgramacionDelegacionSector
    {
        public enum EstadosIntegracionProgramacionDelegacionGrupo
        {
            ACerrar = 1,
            Integrando = 2,
            Integrado = 3,
            Error = 4,
        }

        public EstadosIntegracionProgramacionDelegacionGrupo Estado { set; get; }

        public SOLProgramacionSector Sector { set; get; }

        public SOLDelegacion Delegacion { set; get; }
    }

    
}
