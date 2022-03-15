using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Models
{
    public class IntegrationRouteStateModel
    {
        public int Code { set; get; }

        public string Description { set; get; }

        private static List<IntegrationRouteStateModel> _routeStates;

        public static List<IntegrationRouteStateModel> GetAll()
        {
            if (_routeStates == null)
            {
                _routeStates = new List<IntegrationRouteStateModel>();

                _routeStates.Add(new IntegrationRouteStateModel
                {
                    Code = 0,
                    Description = "OK",
                });

                _routeStates.Add(new IntegrationRouteStateModel
                {
                    Code = 1,
                    Description = "ERRORES DE INTEGRACION",
                });
            }

            return _routeStates;
        }

        public static IntegrationRouteStateModel GetByCode(int code)
        {
            IntegrationRouteStateModel.GetAll();

            return _routeStates.Where(s => s.Code.Equals(code)).FirstOrDefault();
        }

        public static IntegrationRouteStateModel ConvertFrom(Entities.SOLIntegracionRuta.SOLIntegracionRutaEstado solIntegracionRutaEstado)
        {
            IntegrationRouteStateModel integrationRouteStateModel = GetByCode(0);

            switch (solIntegracionRutaEstado)
            {
                case Entities.SOLIntegracionRuta.SOLIntegracionRutaEstado.Integrada:
                    integrationRouteStateModel = GetByCode(0);
                    break;
                case Entities.SOLIntegracionRuta.SOLIntegracionRutaEstado.ConErroresDeIntegracion:
                    integrationRouteStateModel = GetByCode(1);
                    break;
            }

            return integrationRouteStateModel;
        }
    }
    
    public class RouteViewModel
    {
        [Display(Name = "Codigo")]
        public int Code { set; get; }

        [Display(Name = "Descripcion")]
        public string Description { set; get; }

        [Display(Name = "Fecha")]
        public DateTime RouteDate { set; get; }

        [Display(Name = "Programación Inicio")]
        public DateTime ProgramatedBeginDate { set; get; }

        [Display(Name = "Programación Fin")]
        public DateTime ProgramatedEndDate { set; get; }
    }

    public class IntegrationStateRouteSummaryViewModel
    {
        public int Quantity { set; get; }

        public IntegrationRouteStateModel RouteState { set; get; }
    }

    public class RouteBalanceFilterViewModel
    {
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Debe ingresar una fecha de programación")]
        public DateTime RouteDate { set; get; }

        [Required(ErrorMessage = "Debe seleccionar al menos una delegación")]
        public List<string> DelegationCodes { set; get; }

        [Display(Name = "Delegación")]
        public IEnumerable<SelectListItem> Delegations { set; get; }

        public IEnumerable<IntegrationStateRouteSummaryViewModel> StateRoutesSummary { set; get; }

        public IEnumerable<int> RoutesWithError { set; get; }
    }

    public class RouteIntegrationDetails
    {
        [Display(Name = "Código Servicio")]
        public string SolServiceCode { set; get; }

        public string SolOTCode { set; get; }

        [Display(Name = "Sección")]
        public int? SolSequenceCode { set; get; }

        [Display(Name = "Fecha y Hora")]
        public DateTime? SolOTProgrammedBeginDateTime { set; get; }

        [Display(Name = "Ruta")]
        public int? SolRouteCode { set; get; }

        [Display(Name = "Fecha servicio")]
        public string SigServiceDate { set; get; }

        [Display(Name = "Control")]
        public int? SigControlCode { set; get; }

        [Display(Name = "Fecha y Hora")]
        public DateTime? SigServiceDateTime { set; get; }

        [Display(Name = "Ruta")]
        public int? SigRouteCode { set; get; }
    }
}