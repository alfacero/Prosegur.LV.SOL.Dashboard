using System;
using System.Diagnostics;

namespace Prosegur.LV.SOL.Dashboard.UI.Web
{
    public class ActivityIdHelper
    {        
        public override string ToString()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString();
        }
    }

}
