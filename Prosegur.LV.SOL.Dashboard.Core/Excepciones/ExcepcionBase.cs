using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.Core.Excepciones
{
    public class ExcepcionBase : Exception 
    {
        public string RequestGUID { set; get; }

        public ExcepcionBase(string requestGUID)
        {
            RequestGUID = requestGUID;
        }
    }
}
