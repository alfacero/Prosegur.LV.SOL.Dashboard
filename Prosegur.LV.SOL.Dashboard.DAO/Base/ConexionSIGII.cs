using Prosegur.Framework.Dao.Odbc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO.Base
{
    public class ConexionSIGII : OdbcDao
    {
        protected static OdbcDbConnection _odbcDbConnection = new OdbcDbConnection(ConfigurationManager.ConnectionStrings["InformixODBCSigIIConnectionString"].ConnectionString);

        #region "Constructores"

        public ConexionSIGII() : base(ConfigurationManager.ConnectionStrings["InformixODBCSigIIConnectionString"].ConnectionString) { }

        #endregion

        #region "Public Methods"

        public void SetLockModeOn()
        {
            CommandText = Consultas.SIGIILockMode;
            Execute();
        }

        #endregion
    }
}
