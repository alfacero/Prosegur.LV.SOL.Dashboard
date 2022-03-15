using Prosegur.Framework.Dao;
using Prosegur.Framework.Dao.Odbc;
using Prosegur.LV.SOL.Dashboard.DAO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.DAO
{
    public class SIGIIPlanta : ConexionSIGII
    {
        #region "Metodos Publicos"

        public IList<Entities.SIGIIPlanta> GetPlantasConBasurero(string[] delegacion)
        {
            AbrirConexion();

            try
            {
                SetLockModeOn();

                CommandText = Consultas.SIGIIGetPlantaConRecorridoBasureroPorDelegacion;
                AddParam("@DELEGACION", delegacion);

                return GetMany<Entities.SIGIIPlanta, Entities.SIGIIRecorrido, int>
                    (
                        u => CastTo<int>(u["codigo"]),
                        p => p.Basureros,
                        u => !CastTo<int?>(u["recorrido"]).HasValue
                    );
            }
            catch
            {
                throw;
            }
            finally
            {
                CerrarConexion();
            }
        }

        #endregion
    }
}
