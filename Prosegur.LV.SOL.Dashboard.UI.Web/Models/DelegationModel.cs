using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Models
{
    public class DelegationModel
    {
        public string Code { set; get; }

        [Display(Name="Delegacion")]
        public string Description { set; get; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Code, Description);
        }
    }
}