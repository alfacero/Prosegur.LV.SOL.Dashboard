using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using log4net;
using Prosegur.LV.SOL.Dashboard.Core;
using Prosegur.LV.SOL.Dashboard.Entities;
using Prosegur.LV.SOL.Dashboard.UI.Web.Models;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    [Authorize]
    public class RouteController : Controller
    {
        public ActionResult RouteBalancesPartial(RouteBalanceFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Balanceo de Rutas");

            if (!Request.IsAjaxRequest())
            {
                IEnumerable<Models.DelegationModel> modeloDelegaciones = new List<Models.DelegationModel>();

                Prosegur.LV.SOL.Dashboard.UI.Web.Models.UserData datosUsuario =
                    (Prosegur.LV.SOL.Dashboard.UI.Web.Models.UserData)HttpContext.User;

                IList<SOLDelegacion> solDelegaciones;
                Usuario coreUsuario = new Usuario();

                solDelegaciones = coreUsuario.GetUsuarioDelegaciones(datosUsuario.UserCode);

                if (solDelegaciones != null)
                {
                    modeloDelegaciones = from d in solDelegaciones
                                         select new DelegationModel()
                                         {
                                             Code = d.Codigo,
                                             Description = d.Descripcion
                                         };
                }

                model.Delegations = new SelectList(modeloDelegaciones, "Code", "Description");
            }
            else
            {
                try
                {
                    if (model.DelegationCodes != null && model.DelegationCodes.Count() > 0)
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<Entities.SOLIntegracionRuta>, IEnumerable<Entities.SOLRuta>> estadoIntegracionRutas = integracionSOLSIGII.EstadoDeRutas(model.RouteDate, model.DelegationCodes.ToArray());

                        model.StateRoutesSummary = from i in estadoIntegracionRutas.Item1
                                                   select new IntegrationStateRouteSummaryViewModel()
                                                   {
                                                       Quantity = i.Cantidad,
                                                       RouteState = IntegrationRouteStateModel.ConvertFrom(i.Estado)
                                                   };

                        if (estadoIntegracionRutas.Item2 != null)
                        {
                            model.RoutesWithError = estadoIntegracionRutas.Item2.Select(r => r.Codigo).ToList();
                        }
                    }

                    var jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(RouteController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "RoutesBalancesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView(model);
        }

        public ActionResult RouteIntegrationDetailsPartial(List<int> routeWithErrors, DateTime fecRuta)
        {
            ViewBag.OTDetailsTitle = "Integracion de OTs/Servicios";

            try
            {
                IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                IList<Tuple<Entities.SOLOrdenDeTrabajo, Entities.SIGIIHojaDeRuta>> detalleDeRutas = integracionSOLSIGII.DetalleDeRutas(routeWithErrors.ToArray(), fecRuta);

                var listadoDetalle = from d in detalleDeRutas
                                     let solOrdenDeTrabajo = d.Item1
                                     let sigIIHojaDeRuta = d.Item2
                                     select new RouteIntegrationDetails()
                                     {
                                         SolRouteCode = (solOrdenDeTrabajo != null ? (int?)solOrdenDeTrabajo.CodigoRuta : null),
                                         SolOTCode = (solOrdenDeTrabajo != null ? solOrdenDeTrabajo.OidOt : null),
                                         SolServiceCode = (solOrdenDeTrabajo != null ? solOrdenDeTrabajo.CodigoServicio : null),
                                         SolSequenceCode = (solOrdenDeTrabajo != null ? (int?)solOrdenDeTrabajo.CodigoSecuencia : null),
                                         SolOTProgrammedBeginDateTime = (solOrdenDeTrabajo != null ? (DateTime?)solOrdenDeTrabajo.FechaProgramadaInicio : null),
                                         SigRouteCode = (sigIIHojaDeRuta != null ? (int?)sigIIHojaDeRuta.Recorrido : null),
                                         SigControlCode = (sigIIHojaDeRuta != null ? (int?)sigIIHojaDeRuta.Control : null),
                                         SigServiceDate = (sigIIHojaDeRuta != null ? sigIIHojaDeRuta.FechaProceso.ToString("yyyy/MM/dd") : null),
                                         SigServiceDateTime = (sigIIHojaDeRuta != null ? (DateTime?)new DateTime(sigIIHojaDeRuta.FechaProceso.Year, sigIIHojaDeRuta.FechaProceso.Month, sigIIHojaDeRuta.FechaProceso.Day, sigIIHojaDeRuta.HoraProceso.Hour, sigIIHojaDeRuta.HoraProceso.Minute, sigIIHojaDeRuta.HoraProceso.Second) : null)
                                     };

                return Json(new { result = true, data = this.RenderPartialView(listadoDetalle) });
            }
            catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(RouteController));

                log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                throw new HttpException(ex.Message);
            }
        }
	}    
}