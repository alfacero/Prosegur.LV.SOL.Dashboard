using System.Collections.Generic;

namespace Prosegur.LV.SOL.Dashboard.Entities
{
    public class SOLUsuario
    {
        public SOLUsuario() 
        { 
            Roles = new List<SOLRolUsuario>(); 
        }

        public string Oid { set; get; }

        public string Nombre { set; get; }

        public IList<SOLRolUsuario> Roles { set; get; }
    }
}
