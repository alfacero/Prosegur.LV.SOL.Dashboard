namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLIntegracionProgramacion
    {
        public enum SOLIntegracionProgramacionEstado
        { 
            PendienteSOL,
            CerradoSOLPendienteIntegracion,
            CerradoSOLErrorIntegracion,
            CerradoSOLIntegracionCorrectaSIGIIIntegrado,
            CerradoSOLIntegracionCorrectaSIGIINoIntegrado,
        }

        public SOLIntegracionProgramacionEstado Estado { set; get; }

        public int Cantidad { set; get; }
    }
}
