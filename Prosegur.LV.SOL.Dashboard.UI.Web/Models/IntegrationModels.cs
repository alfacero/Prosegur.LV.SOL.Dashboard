using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Prosegur.LV.SOL.Dashboard.Core;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Models
{
    public class IntegrationStateModel
    {
        public string Code { set; get; }

        public string Description { set; get; }

        private static List<IntegrationStateModel> _integrationStates;

        public static List<IntegrationStateModel> GetAll()
        {
            if (_integrationStates == null)
            {
                _integrationStates = new List<IntegrationStateModel>();

                _integrationStates.Add(new IntegrationStateModel
                {
                    Code = Configuracion.CodigoEstadoPendiente,
                    Description = "PENDIENTE",
                });

                _integrationStates.Add(new IntegrationStateModel
                {
                    Code = Configuracion.CodigoEstadoError,
                    Description = "ERROR",
                });

                _integrationStates.Add(new IntegrationStateModel
                {
                    Code = Configuracion.CodigoEstadoDescartado,
                    Description = "DESCARTADO",
                });

                _integrationStates.Add(new IntegrationStateModel
                {
                    Code = Configuracion.CodigoEstadoProcesado,
                    Description = "PROCESADO",
                });
            }

            return _integrationStates;
        }

        public static IntegrationStateModel GetByCode(string code)
        {
            IntegrationStateModel.GetAll();

            return _integrationStates.Where(s => s.Code.Equals(code)).FirstOrDefault();
        }
    }

    public class IntegrationSummaryViewModel
    {
        public int Quantity { set; get; }

        public IntegrationStateModel IntegrationState { set; get; }
    }

    public class OTIntegrationFilterViewModel
    {
        [Display(Name = "Periodo (hrs)")]
        [Required(ErrorMessage = "Debe ingresar las horas desde")]
        public int PeriodFromHours { set; get; }

        [Required(ErrorMessage = "Debe seleccionar al menos una delegación")]
        public List<string> DelegationCodes { set; get; }

        [Display(Name = "Delegación")]
        public IEnumerable<SelectListItem> Delegations { set; get; }

        public IEnumerable<IntegrationSummaryViewModel> OTIntegrationStatesSummary { set; get; }
    }

    public class JmsQueueMessage
    {
        public string Id { set; get; }

        [Display(Name = "Mensaje")]
        public string Message { set; get; }

        [Display(Name = "Observacion")]
        public string Observation { set; get; }

        [Display(Name = "Delegacion")]
        public string Delegation { set; get; }

        [Display(Name = "Integracion")]
        public string Integration { set; get; }

        [Display(Name = "Atributo1")]
        public string FirstAttribute { set; get; }

        [Display(Name = "Atributo2")]
        public string SecondAttribute { set; get; }
    }

    public class JmsQueueProgrammation : JmsQueueMessage
    {
        
    }

    public class JmsQueueProgrammationPerRoute : JmsQueueMessage
    {
        [Display(Name = "Ruta")]
        public string Route { set; get; }
    }

    public class JmsQueueProgrammationPerGroup : JmsQueueMessage
    {
        [Display(Name = "Grupo")]
        public string Group { set; get; }
    }

    public class JmsQueueOTMessage : JmsQueueMessage
    {
        [Display(Name = "Servicio")]
        public string CodServicio { set; get; }

        [Display(Name = "Delegacion")]
        public string CodDelegacion { set; get; }

        [Display(Name = "Ruta")]
        public int RouteCode { set; get; }

        [Display(Name = "Sec.")]
        public int SequenceCode { set; get; }

        [Display(Name = "Fecha Programada Inicio")]
        public DateTime Date { set; get; }
    }
}