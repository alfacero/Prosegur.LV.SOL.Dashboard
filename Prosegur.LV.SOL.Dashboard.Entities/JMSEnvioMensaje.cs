using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class JMSEnvioMensaje
    {
        public int JmsId { set; get; }
        
        public string Id { set; get; }
        
        public string Mensaje { set; get; }
        
        public string Observacion { set; get; }
        
        public string Estado { set; get; }
        
        public string Delegacion { set; get; }

        public string Atributo1 { set; get; }

        public string Atributo2 { set; get; }

        public string Integracion { set; get; }
    }
}
