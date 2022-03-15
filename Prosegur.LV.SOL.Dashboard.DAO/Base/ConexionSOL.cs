using Prosegur.Framework.Dao.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO.Base
{
    public class ConexionSOL : OracleDao
    {
        #region "Constructores"
        public ConexionSOL() : base(ConfigurationManager.ConnectionStrings["OracleODPNetSOLConnectionString"].ConnectionString) { }

        #endregion
    }
}
