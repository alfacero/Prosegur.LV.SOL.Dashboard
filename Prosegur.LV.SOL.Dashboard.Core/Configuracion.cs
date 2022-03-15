using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prosegur.LV.SOL.Dashboard.Core
{
    public static class Configuracion
    {
        #region Métodos Privados
        private static string GetValorAppSettingsCaracteres(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        private static int GetValorAppSettingsEntero(string key)
        {
            int retValue;
            Int32.TryParse(System.Configuration.ConfigurationManager.AppSettings[key], out retValue);
            return retValue;
        }

        #endregion

        #region Métodos Públicos
        public static string CodigoEstadoPendiente
        {
            get
            {
                return GetValorAppSettingsCaracteres("CodEstadoPendiente");
            }
        }        

        public static string CodigoEstadoError
        {
            get
            {
                return GetValorAppSettingsCaracteres("CodEstadoError");
            }
        }

        public static string CodigoEstadoProcesado
        {
            get
            {
                return GetValorAppSettingsCaracteres("CodEstadoProcesado");
            }
        }

        public static string CodigoEstadoDescartado
        {
            get
            {
                return GetValorAppSettingsCaracteres("CodEstadoDescartado");
            }
        }

        public static string DominioProsegur
        {
            get
            {
                return GetValorAppSettingsCaracteres("DominioProsegur");
            }
        }

        public static int RefrescoAutomaticoSegundos
        {
            get
            {
                int retValue = GetValorAppSettingsEntero("RefrescoAutomaticoSegundos");
                return (retValue == 0 ? 30 : retValue);
            }
        }

        public static string UsuarioAdmin
        {
            get
            {
                return GetValorAppSettingsCaracteres("UsuarioAdmin");
            }
        }

        public static string PasswordAdmin
        {
            get
            {
                return GetValorAppSettingsCaracteres("PasswordAdmin");
            }
        }

        public static string UsuarioSolSuplantacionAdmin
        {
            get
            {
                return GetValorAppSettingsCaracteres("UsuarioSolSuplantacionAdmin");
            }
        }

        #endregion
    }
}
