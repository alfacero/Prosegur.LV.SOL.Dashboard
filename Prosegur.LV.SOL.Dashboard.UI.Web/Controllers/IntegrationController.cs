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
    public class IntegrationController : Controller
    {
        public ActionResult OTIntegrationStatesPartial(Models.OTIntegrationFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Estado de Integración de OTs");

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

                model.PeriodFromHours = 0;
                model.Delegations = new SelectList(modeloDelegaciones, "Code", "Description");
            }
            else
            {
                if (model.DelegationCodes != null && model.DelegationCodes.Count() > 0)
                {
                    Prosegur.LV.SOL.Dashboard.Core.SOLOrdenDeTrabajo coreOrdenDeTrabajo = new Prosegur.LV.SOL.Dashboard.Core.SOLOrdenDeTrabajo();

                    IntegrationSummaryViewModel mensajesPendientes = new IntegrationSummaryViewModel()
                    {
                        Quantity = 0,
                        IntegrationState = IntegrationStateModel.GetByCode(Configuracion.CodigoEstadoPendiente)
                    };

                    IntegrationSummaryViewModel mensajesErroneos = new IntegrationSummaryViewModel()
                    {
                        Quantity = 0,
                        IntegrationState = IntegrationStateModel.GetByCode(Configuracion.CodigoEstadoError)
                    };

                    List<Models.IntegrationSummaryViewModel> list = new List<Models.IntegrationSummaryViewModel>() { mensajesPendientes, mensajesErroneos };

                    model.OTIntegrationStatesSummary = list;

                    try
                    {
                        IList<JMSEnvioMensaje> entSolJmsEnvioMensajes = coreOrdenDeTrabajo.GetMensajesIntegracion(model.DelegationCodes.ToArray(), model.PeriodFromHours);

                        mensajesPendientes.Quantity = entSolJmsEnvioMensajes.Where(m => m.Estado == Configuracion.CodigoEstadoPendiente).Count();
                        mensajesErroneos.Quantity = entSolJmsEnvioMensajes.Where(m => m.Estado == Configuracion.CodigoEstadoError).Count();
                    }
                    catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        ILog log = LogManager.GetLogger(typeof(IntegrationController));

                        log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                        throw new HttpException(ex.Message);
                    }
                }

                string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                return Json(new { result = true, data = jsonSerializedData });
            }

            ViewData.Add("ViewName", "OTIntegrationStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView(model);
        }

        public ActionResult OTErrorQueueMessages(Models.OTIntegrationFilterViewModel model)
        {
            ViewBag.OTDetailsTitle = "Detalles Mensajes OT";

            List<JmsQueueOTMessage> mensajes = new List<JmsQueueOTMessage>();

            try
            {
                Prosegur.LV.SOL.Dashboard.Core.SOLOrdenDeTrabajo coreOrdenDeTrabajo = new Prosegur.LV.SOL.Dashboard.Core.SOLOrdenDeTrabajo();

                IList<JMSEnvioMensaje> entSolJmsEnvioMensajes = coreOrdenDeTrabajo.GetMensajesIntegracionError(model.DelegationCodes.ToArray(), model.PeriodFromHours);

                if (entSolJmsEnvioMensajes != null && entSolJmsEnvioMensajes.Count() > 0)
                {
                    IList<Prosegur.LV.SOL.Dashboard.Entities.SOLOrdenDeTrabajo> entOrdenesDeTrabajo = coreOrdenDeTrabajo.GetListado(entSolJmsEnvioMensajes.Select(m => m.Id).ToArray());

                    if (entOrdenesDeTrabajo != null)
                    {
                        foreach (Prosegur.LV.SOL.Dashboard.Entities.JMSEnvioMensaje entSolJmsEnvioMensaje in entSolJmsEnvioMensajes)
                        {
                            Prosegur.LV.SOL.Dashboard.Entities.SOLOrdenDeTrabajo entOrdenDeTrabajo =
                                entOrdenesDeTrabajo.Where(o => o.OidOt == entSolJmsEnvioMensaje.Id).FirstOrDefault();

                            mensajes.Add(new JmsQueueOTMessage()
                            {
                                CodDelegacion = entSolJmsEnvioMensaje.Delegacion,
                                RouteCode = entOrdenDeTrabajo.CodigoRuta,
                                CodServicio = entOrdenDeTrabajo.CodigoServicio,
                                SequenceCode = entOrdenDeTrabajo.CodigoSecuencia,
                                Date = entOrdenDeTrabajo.FechaProgramadaInicio,
                                Observation = entSolJmsEnvioMensaje.Observacion
                            });
                        }
                    }
                }

                return Json(new { result = true, data = this.RenderPartialView("JmsQueueOTMessages", mensajes) });
            }
            catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegrationController));

                log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                throw new HttpException(ex.Message);
            }
        }

        public ActionResult ProgrammationServicesStatesPartial(ProgrammationFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Servicios General");

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
                    if (model.DelegationCodes != null && model.DelegationCodes.Count > 0)
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> estadoProgramacionDeServicios = integracionSOLSIGII.EstadoProgramacionDeServicios(model.ProgrammationDate, model.DelegationCodes.ToArray());

                        model.ProgrammationStatesSummary = from i in estadoProgramacionDeServicios.Item1
                                                           select new ProgrammationStateSummaryViewModel()
                                                           {
                                                               ProgrammationState = ProgrammationStateModel.ConvertFrom(i.Estado),
                                                               Quantity = i.Cantidad
                                                           };

                        if (estadoProgramacionDeServicios.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeServicios.Item2
                                                     select m.JmsId).ToList();
                        }

                        if (estadoProgramacionDeServicios.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from s in estadoProgramacionDeServicios.Item3
                                                                select s.Codigo).ToList();
                        }

                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });

                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationServiceStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationStatesPartial", model);
        }

        public ActionResult CierresFactPorDelegacionPartial(CierreFactFilterViewModel model)
        {
            

            ViewData.Add("ChartTitle", "Cierres Facturacion por Delegación");


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
                    if ((model.DelegationCodes != null && model.DelegationCodes.Count > 0) && (model.Days != null && model.Days.Count() > 0))
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> estadoCierresFacturacion = integracionSOLSIGII.EstadoCierresFacturacion(model.ProgrammationServiceOffset, model.DelegationCodes.ToArray());

                        model.ProgrammationStates = new ProgrammationBarStateViewModel
                        {
                            Delegations = (from p in estadoCierresFacturacion.Item1
                                           group p by new { p.Delegacion.Codigo, p.Delegacion.Descripcion } into grp
                                           select new DelegationModel()
                                           {
                                               Code = grp.Key.Codigo,
                                               Description = grp.Key.Descripcion
                                           }).OrderBy(d => d.Description)
                        };

                        List<IntegrationBarStateSummaryModel> integrationRouteResourcesByStateModels = new List<IntegrationBarStateSummaryModel>();

                        foreach (SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta state in Enum.GetValues(typeof(SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta)).Cast<SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta>())
                        {
                            IEnumerable<SOLIntegracionProgramacionDelegacionRuta> datosRecursosPorEstado = estadoCierresFacturacion.Item1.Where(e => e.Estado == state);

                            if (datosRecursosPorEstado != null)
                            {
                                IntegrationBarStateSummaryModel integrationRouteResourcesByStateModel = new IntegrationBarStateSummaryModel()
                                {
                                    State = IntegrationBarStateModel.ConvertFrom(state),
                                };

                                List<int[]> delegationsQty = new List<int[]>();
                                int idx = 0;
                                foreach (DelegationModel delegation in model.ProgrammationStates.Delegations.OrderBy(d => d.Description))
                                {
                                    int quantity = datosRecursosPorEstado.Where(s => s.Delegacion.Codigo == delegation.Code).Count();

                                    if (quantity > 0)
                                    {
                                        delegationsQty.Add(new int[2] { idx, quantity });
                                    }
                                    idx++;
                                }

                                integrationRouteResourcesByStateModel.DelegationsQuantity = delegationsQty;

                                integrationRouteResourcesByStateModels.Add(integrationRouteResourcesByStateModel);
                            }
                        }

                        model.ProgrammationStates.ProgrammationStates = integrationRouteResourcesByStateModels;

                        if (estadoCierresFacturacion.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoCierresFacturacion.Item2
                                                     select string.Format("{0}|{1}", m.Delegacion, m.JmsId)).ToList();
                        }

                        if (estadoCierresFacturacion.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoCierresFacturacion.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Recorrido)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }


            ViewData.Add("ViewName", "CierresFactPorDelegacionPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("CierresFactPorDelegacionPartial", model);


        }
            public ActionResult ProgrammationResourcesStatesPartial(ProgrammationFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Recursos General");

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
                    if (model.DelegationCodes != null && model.DelegationCodes.Count > 0)
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> estadoProgramacionDeRecursos = integracionSOLSIGII.EstadoProgramacionDeRecursos(model.ProgrammationDate, model.DelegationCodes.ToArray());

                        model.ProgrammationStatesSummary = from i in estadoProgramacionDeRecursos.Item1
                                                           select new ProgrammationStateSummaryViewModel()
                                                           {
                                                               ProgrammationState = ProgrammationStateModel.ConvertFrom(i.Estado),
                                                               Quantity = i.Cantidad
                                                           };

                        if (estadoProgramacionDeRecursos.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeRecursos.Item2
                                                     select m.JmsId).ToList();
                        }

                        if (estadoProgramacionDeRecursos.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from s in estadoProgramacionDeRecursos.Item3
                                                                select s.Codigo).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationResourcesStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationStatesPartial", model);
        }

        public ActionResult ProgrammationErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, DateTime programmationDate)
        {
            ViewBag.ProgrammingDetailsTitle = "Detalles de Integración de Programación por delegación " + programmationDate.ToString("dd/MM/yyyy");

            return ListaErrores(errorJmsIds, sigIIProgrammationPendings);
        }
        public ActionResult CierreFacErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, int programmationServiceOffset)
        {
            ViewBag.ProgrammingDetailsTitle = "Detalles de Cierres de Facturación " + DateTime.Now.AddDays(programmationServiceOffset).ToString("dd/MM/yyyy");
            


            return ListaErrores(errorJmsIds, sigIIProgrammationPendings);
        }

        private ActionResult ListaErrores(List<int> errorJmsIds, List<string> sigIIProgrammationPendings)
        {
            List<JmsQueueProgrammationPerRoute> queueMessages = new List<JmsQueueProgrammationPerRoute>();

            try
            {
                if (errorJmsIds != null && errorJmsIds.Count > 0)
                {
                    Core.ColaJMSEnvio colaJmsEnvio = new ColaJMSEnvio();

                    IList<Entities.JMSEnvioMensaje> jmsEnvioMensajesError = colaJmsEnvio.GetMensajes(errorJmsIds.ToArray());

                    queueMessages = (from m in jmsEnvioMensajesError
                                     select new JmsQueueProgrammationPerRoute()
                                     {
                                         Observation = m.Observacion,
                                         Delegation = m.Delegacion,
                                         Route = m.Cod_ruta
                                     }).ToList();
                }
            }
            catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegrationController));

                log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                throw new HttpException(ex.Message);
            }

            if (sigIIProgrammationPendings != null && sigIIProgrammationPendings.Count > 0)
            {

                foreach (string data in sigIIProgrammationPendings)
                {
                    queueMessages.Add(new JmsQueueProgrammationPerRoute()
                    {
                        Observation = "Ha ocurrido un error. Comuníquese con el CAU (Centro de atención de Usuarios)",
                        Delegation = data,
                        Route = String.Empty
                    });
                }
            }

            return Json(new { result = true, data = this.RenderPartialView("ProgrammationIntegrationDetailsPartial", queueMessages) });
        }

        public ActionResult ProgrammationResourcesPerRouteStatesPartial(ProgrammationFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Recursos por Ruta");

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
                    if (model.DelegationCodes != null && model.DelegationCodes.Count > 0)
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> estadoProgramacionDeServiciosPorRuta = integracionSOLSIGII.EstadoProgramacionDeRecursosPorRuta(model.ProgrammationDate, model.DelegationCodes.ToArray());

                        model.ProgrammationStatesSummary = from p in estadoProgramacionDeServiciosPorRuta.Item1
                                                           select new ProgrammationStateSummaryViewModel()
                                                           {
                                                               ProgrammationState = ProgrammationStateModel.ConvertFrom(p.Estado),
                                                               Quantity = p.Cantidad
                                                           };

                        if (estadoProgramacionDeServiciosPorRuta.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeServiciosPorRuta.Item2
                                                     select m.JmsId).ToList();
                        }

                        if (estadoProgramacionDeServiciosPorRuta.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeServiciosPorRuta.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Recorrido)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationResourcesPerRouteStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationPerRouteStatesPartial", model);
        }

        public ActionResult ProgrammationServicesPerRouteStatesPartial(ProgrammationFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Servicios por Ruta");

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
                    if (model.DelegationCodes != null && model.DelegationCodes.Count > 0)
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> estadoProgramacionDeServiciosPorRuta = integracionSOLSIGII.EstadoProgramacionDeServiciosPorRuta(model.ProgrammationDate, model.DelegationCodes.ToArray());

                        model.ProgrammationStatesSummary = from p in estadoProgramacionDeServiciosPorRuta.Item1
                                                           select new ProgrammationStateSummaryViewModel()
                                                           {
                                                               ProgrammationState = ProgrammationStateModel.ConvertFrom(p.Estado),
                                                               Quantity = p.Cantidad
                                                           };

                        if (estadoProgramacionDeServiciosPorRuta.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeServiciosPorRuta.Item2
                                                     select m.JmsId).ToList();
                        }

                        if (estadoProgramacionDeServiciosPorRuta.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeServiciosPorRuta.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Recorrido)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationServicesPerRouteStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationPerRouteStatesPartial", model);
        }

        public ActionResult ProgrammationPerRouteErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, DateTime programmationDate)
        {
            ViewBag.ProgrammingDetailsTitle = "Detalle de Integración de Programación por ruta " + programmationDate.ToString("dd/MM/yyyy");

            List<JmsQueueProgrammationPerRoute> queueMessages = new List<JmsQueueProgrammationPerRoute>();

            try
            {
                if (errorJmsIds != null && errorJmsIds.Count > 0)
                {
                    Core.ColaJMSEnvio colaJmsEnvio = new ColaJMSEnvio();

                    IList<Entities.JMSEnvioMensaje> jmsEnvioMensajesError = colaJmsEnvio.GetMensajes(errorJmsIds.ToArray());

                    queueMessages = (from m in jmsEnvioMensajesError
                                     let ruta = (m.Integracion == "PROGRAMACION_RECURSO" ?
                                        m.Atributo2.Split('|')[1] :
                                        m.Atributo1.Split('|')[0])
                                     select new JmsQueueProgrammationPerRoute()
                                     {
                                         Observation = m.Observacion,
                                         Delegation = m.Delegacion,
                                         FirstAttribute = m.Atributo1,
                                         SecondAttribute = m.Atributo2,
                                         Integration = m.Integracion,
                                         Route = ruta
                                     }).ToList();
                }
            }
            catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegrationController));

                log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                throw new HttpException(ex.Message);
            }

            if (sigIIProgrammationPendings != null && sigIIProgrammationPendings.Count > 0)
            {

                foreach (string data in sigIIProgrammationPendings)
                {
                    queueMessages.Add(new JmsQueueProgrammationPerRoute()
                    {
                        Observation = "Ha ocurrido un error. Comuníquese con el CAU (Centro de atención de Usuarios)",
                        Delegation = data.Split('|')[0],
                        Route = data.Split('|')[1],
                    });
                }
            }

            return Json(new { result = true, data = this.RenderPartialView("ProgrammationPerRouteIntegrationDetailsPartial", queueMessages) });
        }

        public ActionResult ProgrammationPerGroupStatesPartial(ProgrammationFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Recursos por Grupo");

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
                    if (model.DelegationCodes != null && model.DelegationCodes.Count > 0)
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIGrupo>> estadoProgramacionDeSectoresPorRuta = integracionSOLSIGII.EstadoProgramacionDeSectores(model.ProgrammationDate, model.DelegationCodes.ToArray());

                        model.ProgrammationStatesSummary = from p in estadoProgramacionDeSectoresPorRuta.Item1
                                                           select new ProgrammationStateSummaryViewModel()
                                                           {
                                                               ProgrammationState = ProgrammationStateModel.ConvertFrom(p.Estado),
                                                               Quantity = p.Cantidad
                                                           };

                        if (estadoProgramacionDeSectoresPorRuta.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeSectoresPorRuta.Item2
                                                     select m.JmsId).ToList();
                        }

                        if (estadoProgramacionDeSectoresPorRuta.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeSectoresPorRuta.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Codigo)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationPerGroupStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationPerGroupStatesPartial", model);
        }

        public ActionResult ProgrammationPerGroupErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, DateTime programmationDate)
        {
            ViewBag.ProgrammingDetailsTitle = "Detalle de Integración de Programación por grupo " + programmationDate.ToString("dd/MM/yyyy");

            List<JmsQueueProgrammationPerGroup> queueMessages = new List<JmsQueueProgrammationPerGroup>();

            try
            {
                if (errorJmsIds != null && errorJmsIds.Count > 0)
                {
                    Core.ColaJMSEnvio colaJmsEnvio = new ColaJMSEnvio();

                    IList<Entities.JMSEnvioMensaje> jmsEnvioMensajesError = colaJmsEnvio.GetMensajes(errorJmsIds.ToArray());

                    queueMessages = (from m in jmsEnvioMensajesError
                                     let grupo = m.Atributo2.Split('|')[1]
                                     select new JmsQueueProgrammationPerGroup()
                                     {
                                         Observation = m.Observacion,
                                         Delegation = m.Delegacion,
                                         FirstAttribute = m.Atributo1,
                                         SecondAttribute = m.Atributo2,
                                         Integration = m.Integracion,
                                         Group = grupo
                                     }).ToList();
                }
            }
            catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
            {
                throw;
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegrationController));

                log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                throw new HttpException(ex.Message);
            }

            if (sigIIProgrammationPendings != null && sigIIProgrammationPendings.Count > 0)
            {
                foreach (string data in sigIIProgrammationPendings)
                {
                    queueMessages.Add(new JmsQueueProgrammationPerGroup()
                    {
                        Observation = "Ha ocurrido un error. Comuníquese con el CAU (Centro de atención de Usuarios)",
                        Delegation = data.Split('|')[0],
                        Group = data.Split('|')[1],
                    });
                }
            }

            return Json(new { result = true, data = this.RenderPartialView("ProgrammationPerGroupIntegrationDetailsPartial", queueMessages) });
        }

        public ActionResult ProgrammationBarServicesStatesPartial(ProgrammationBarFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Cierre General Rutas");

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
                    if ((model.DelegationCodes != null && model.DelegationCodes.Count > 0) && (model.Days != null && model.Days.Count() > 0))
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacionDelegacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> estadoProgramacionDeServicios = integracionSOLSIGII.EstadoProgramacionDeServicios(model.ProgrammationServiceOffset, model.DelegationCodes.ToArray());

                        model.ProgrammationStates = new ProgrammationBarStateViewModel();

                        model.ProgrammationStates.Delegations = (from p in estadoProgramacionDeServicios.Item1
                                                                 group p by new { p.Delegacion.Codigo, p.Delegacion.Descripcion } into grp
                                                                 select new DelegationModel()
                                                                 {
                                                                     Code = grp.Key.Codigo,
                                                                     Description = grp.Key.Descripcion
                                                                 }).OrderBy(d => d.Description);

                        List<IntegrationBarStateSummaryModel> integrationServicesByStateModels = new List<IntegrationBarStateSummaryModel>();

                        foreach (SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion state in Enum.GetValues(typeof(SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion)).Cast<SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion>())
                        {
                            IEnumerable<SOLIntegracionProgramacionDelegacion> datosServiciosPorEstado = estadoProgramacionDeServicios.Item1.Where(e => e.Estado == state);

                            if (datosServiciosPorEstado != null && datosServiciosPorEstado.Count() > 0)
                            {
                                IntegrationBarStateSummaryModel integrationServicesByStateModel = new IntegrationBarStateSummaryModel()
                                {
                                    State = IntegrationBarStateModel.ConvertFrom(state),
                                };

                                List<int[]> delegationsQty = new List<int[]>();
                                int idx = 0;

                                foreach (DelegationModel delegation in model.ProgrammationStates.Delegations.OrderBy(d => d.Description))
                                {
                                    if (datosServiciosPorEstado.Any(s => s.Delegacion.Codigo == delegation.Code))
                                    {
                                        delegationsQty.Add(new int[2] { idx, 1 });
                                    }

                                    idx++;
                                }

                                integrationServicesByStateModel.DelegationsQuantity = delegationsQty;

                                integrationServicesByStateModels.Add(integrationServicesByStateModel);
                            }
                        }

                        model.ProgrammationStates.ProgrammationStates = integrationServicesByStateModels;

                        if (estadoProgramacionDeServicios.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeServicios.Item2
                                                     select string.Format("{0}|{1}", m.Delegacion, m.JmsId)).ToList();
                        }

                        if (estadoProgramacionDeServicios.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeServicios.Item3
                                                                select p.Codigo).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationBarServicesStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationBarStatesPartial", model);
        }

        public ActionResult ProgrammationBarResourcesStatesPartial(ProgrammationBarFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Recursos General");

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
                    if ((model.DelegationCodes != null && model.DelegationCodes.Count > 0) && (model.Days != null && model.Days.Count() > 0))
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacionDelegacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> estadoProgramacionDeRecursos = integracionSOLSIGII.EstadoProgramacionDeRecursos(model.ProgrammationServiceOffset, model.DelegationCodes.ToArray());

                        model.ProgrammationStates = new ProgrammationBarStateViewModel();

                        model.ProgrammationStates.Delegations = (from p in estadoProgramacionDeRecursos.Item1
                                                                 group p by new { p.Delegacion.Codigo, p.Delegacion.Descripcion } into grp
                                                                 select new DelegationModel()
                                                                 {
                                                                     Code = grp.Key.Codigo,
                                                                     Description = grp.Key.Descripcion
                                                                 }).OrderBy(d => d.Description);

                        List<IntegrationBarStateSummaryModel> integrationResourcesByStateModels = new List<IntegrationBarStateSummaryModel>();

                        foreach (SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion state in Enum.GetValues(typeof(SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion)).Cast<SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion>())
                        {
                            IEnumerable<SOLIntegracionProgramacionDelegacion> datosRecursosPorEstado = estadoProgramacionDeRecursos.Item1.Where(e => e.Estado == state);

                            if (datosRecursosPorEstado != null && datosRecursosPorEstado.Count() > 0)
                            {
                                IntegrationBarStateSummaryModel integrationResourcesByStateModel = new IntegrationBarStateSummaryModel()
                                {
                                    State = IntegrationBarStateModel.ConvertFrom(state),
                                };

                                List<int[]> delegationsQty = new List<int[]>();
                                int idx = 0;

                                foreach (DelegationModel delegation in model.ProgrammationStates.Delegations.OrderBy(d => d.Description))
                                {
                                    if (datosRecursosPorEstado.Where(s => s.Delegacion.Codigo == delegation.Code).Any())
                                    {
                                        delegationsQty.Add(new int[2] { idx, 1 });
                                    }

                                    idx++;
                                }

                                integrationResourcesByStateModel.DelegationsQuantity = delegationsQty;

                                integrationResourcesByStateModels.Add(integrationResourcesByStateModel);
                            }
                        }

                        model.ProgrammationStates.ProgrammationStates = integrationResourcesByStateModels;

                        if (estadoProgramacionDeRecursos.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeRecursos.Item2
                                                     select string.Format("{0}|{1}", m.Delegacion, m.JmsId)).ToList();
                        }

                        if (estadoProgramacionDeRecursos.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeRecursos.Item3
                                                                select p.Codigo).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationBarResourcesStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationBarStatesPartial", model);
        }

        public ActionResult ProgrammationBarPerRouteServicesStatesPartial(ProgrammationBarFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Cierre Rutas");

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
                    if ((model.DelegationCodes != null && model.DelegationCodes.Count > 0) && (model.Days != null && model.Days.Count() > 0))
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> estadoProgramacionDeServiciosPorRuta = integracionSOLSIGII.EstadoProgramacionDeServiciosPorRuta(model.ProgrammationServiceOffset, model.DelegationCodes.ToArray());

                        model.ProgrammationStates = new ProgrammationBarStateViewModel();

                        model.ProgrammationStates.Delegations = (from p in estadoProgramacionDeServiciosPorRuta.Item1
                                                                 group p by new { p.Delegacion.Codigo, p.Delegacion.Descripcion } into grp
                                                                 select new DelegationModel()
                                                                 {
                                                                     Code = grp.Key.Codigo,
                                                                     Description = grp.Key.Descripcion
                                                                 }).OrderBy(d => d.Description);
                        //cachitoooo
                        List<IntegrationBarStateSummaryModel> integrationRouteServicesByStateModels = new List<IntegrationBarStateSummaryModel>();

                        foreach (SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta state in Enum.GetValues(typeof(SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta)).Cast<SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta>())
                        {
                            IEnumerable<SOLIntegracionProgramacionDelegacionRuta> datosServiciosPorEstado = estadoProgramacionDeServiciosPorRuta.Item1.Where(e => e.Estado == state);

                            if (datosServiciosPorEstado != null)
                            {
                                IntegrationBarStateSummaryModel integrationRouteServicesByStateModel = new IntegrationBarStateSummaryModel()
                                {
                                    State = IntegrationBarStateModel.ConvertFrom(state),
                                };

                                List<int[]> delegationsQty = new List<int[]>();
                                int idx = 0;
                                foreach (DelegationModel delegation in model.ProgrammationStates.Delegations.OrderBy(d => d.Description))
                                {
                                    int quantity = datosServiciosPorEstado.Where(s => s.Delegacion.Codigo == delegation.Code).Count();

                                    if (quantity > 0)
                                    {
                                        delegationsQty.Add(new int[2] { idx, quantity });
                                    }
                                    idx++;
                                }

                                integrationRouteServicesByStateModel.DelegationsQuantity = delegationsQty;

                                integrationRouteServicesByStateModels.Add(integrationRouteServicesByStateModel);
                            }
                        }

                        model.ProgrammationStates.ProgrammationStates = integrationRouteServicesByStateModels;

                        if (estadoProgramacionDeServiciosPorRuta.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeServiciosPorRuta.Item2
                                                     select string.Format("{0}|{1}", m.Delegacion, m.JmsId)).ToList();
                        }

                        if (estadoProgramacionDeServiciosPorRuta.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeServiciosPorRuta.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Recorrido)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationBarPerRouteServicesStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationBarPerRouteStatesPartial", model);
        }

        public ActionResult ProgrammationBarPerRouteResourcesStatesPartial(ProgrammationBarFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Recursos por Ruta");

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
                    if ((model.DelegationCodes != null && model.DelegationCodes.Count > 0) && (model.Days != null && model.Days.Count() > 0))
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> estadoProgramacionDeRecursosPorRuta = integracionSOLSIGII.EstadoProgramacionDeRecursosPorRuta(model.ProgrammationServiceOffset, model.DelegationCodes.ToArray());

                        model.ProgrammationStates = new ProgrammationBarStateViewModel
                        {
                            Delegations = (from p in estadoProgramacionDeRecursosPorRuta.Item1
                                           group p by new { p.Delegacion.Codigo, p.Delegacion.Descripcion } into grp
                                           select new DelegationModel()
                                           {
                                               Code = grp.Key.Codigo,
                                               Description = grp.Key.Descripcion
                                           }).OrderBy(d => d.Description)
                        };

                        List<IntegrationBarStateSummaryModel> integrationRouteResourcesByStateModels = new List<IntegrationBarStateSummaryModel>();

                        foreach (SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta state in Enum.GetValues(typeof(SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta)).Cast<SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta>())
                        {
                            IEnumerable<SOLIntegracionProgramacionDelegacionRuta> datosRecursosPorEstado = estadoProgramacionDeRecursosPorRuta.Item1.Where(e => e.Estado == state);

                            if (datosRecursosPorEstado != null)
                            {
                                var integrationRouteResourcesByStateModel = new IntegrationBarStateSummaryModel()
                                {
                                    State = IntegrationBarStateModel.ConvertFrom(state),
                                };

                                List<int[]> delegationsQty = new List<int[]>();
                                int idx = 0;
                                foreach (DelegationModel delegation in model.ProgrammationStates.Delegations.OrderBy(d => d.Description))
                                {
                                    int quantity = datosRecursosPorEstado.Where(s => s.Delegacion.Codigo == delegation.Code).Count();

                                    if (quantity > 0)
                                    {
                                        delegationsQty.Add(new int[2] { idx, quantity });
                                    }
                                    idx++;
                                }

                                integrationRouteResourcesByStateModel.DelegationsQuantity = delegationsQty;

                                integrationRouteResourcesByStateModels.Add(integrationRouteResourcesByStateModel);
                            }
                        }

                        model.ProgrammationStates.ProgrammationStates = integrationRouteResourcesByStateModels;

                        if (estadoProgramacionDeRecursosPorRuta.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeRecursosPorRuta.Item2
                                                     select string.Format("{0}|{1}", m.Delegacion, m.JmsId)).ToList();
                        }

                        if (estadoProgramacionDeRecursosPorRuta.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeRecursosPorRuta.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Recorrido)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationBarPerRouteResourcesStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationBarPerRouteStatesPartial", model);
        }

        public ActionResult ProgrammationBarPerGroupStatesPartial(ProgrammationBarFilterViewModel model)
        {
            ViewData.Add("ChartTitle", "Recursos por Grupo");

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
                    if ((model.DelegationCodes != null && model.DelegationCodes.Count > 0) && (model.Days != null && model.Days.Count() > 0))
                    {
                        IntegracionSOLSIGII integracionSOLSIGII = new IntegracionSOLSIGII();

                        Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionSector>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIGrupo>> estadoProgramacionDeSectores = integracionSOLSIGII.EstadoProgramacionDeSectores(model.ProgrammationServiceOffset, model.DelegationCodes.ToArray());

                        model.ProgrammationStates = new ProgrammationBarStateViewModel();

                        model.ProgrammationStates.Delegations = (from p in estadoProgramacionDeSectores.Item1
                                                                 group p by new { p.Delegacion.Codigo, p.Delegacion.Descripcion } into grp
                                                                 select new DelegationModel()
                                                                 {
                                                                     Code = grp.Key.Codigo,
                                                                     Description = grp.Key.Descripcion
                                                                 }).OrderBy(d => d.Description);

                        var integrationGroupByStateModels = new List<IntegrationBarStateSummaryModel>();

                        foreach (SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo state in Enum.GetValues(typeof(SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo)).Cast<SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo>())
                        {
                            IEnumerable<SOLIntegracionProgramacionDelegacionSector> datosSectoresPorEstado = estadoProgramacionDeSectores.Item1.Where(e => e.Estado == state);

                            if (datosSectoresPorEstado != null && datosSectoresPorEstado.Count() > 0)
                            {
                                IntegrationBarStateSummaryModel integrationGroupByStateModel = new IntegrationBarStateSummaryModel()
                                {
                                    State = IntegrationBarStateModel.ConvertFrom(state),
                                };

                                List<int[]> delegationsQty = new List<int[]>();
                                int idx = 0;
                                foreach (DelegationModel delegation in model.ProgrammationStates.Delegations.OrderBy(d => d.Description))
                                {
                                    int quantity = datosSectoresPorEstado.Where(s => s.Delegacion.Codigo == delegation.Code).Count();

                                    if (quantity > 0)
                                    {
                                        delegationsQty.Add(new int[2] { idx, quantity });
                                    }
                                    idx++;
                                }

                                integrationGroupByStateModel.DelegationsQuantity = delegationsQty;

                                integrationGroupByStateModels.Add(integrationGroupByStateModel);
                            }
                        }

                        model.ProgrammationStates.ProgrammationStates = integrationGroupByStateModels;

                        if (estadoProgramacionDeSectores.Item2 != null)
                        {
                            model.ErrorMessagesId = (from m in estadoProgramacionDeSectores.Item2
                                                     select string.Format("{0}|{1}", m.Delegacion, m.JmsId)).ToList();
                        }

                        if (estadoProgramacionDeSectores.Item3 != null)
                        {
                            model.SigIIProgrammationPendings = (from p in estadoProgramacionDeSectores.Item3
                                                                select string.Format("{0}|{1}", p.CodigoDelegacion, p.Codigo)).ToList();
                        }
                    }

                    string jsonSerializedData = new JavaScriptSerializer().Serialize(model);

                    return Json(new { result = true, data = jsonSerializedData });
                }
                catch (Prosegur.LV.SOL.Dashboard.Core.Excepciones.ExcepcionBase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ILog log = LogManager.GetLogger(typeof(IntegrationController));

                    log.ErrorFormat("Message: {0} \n Stack: {1}", ex.Message, ex.StackTrace);

                    throw new HttpException(ex.Message);
                }
            }

            ViewData.Add("ViewName", "ProgrammationBarPerGroupStatesPartial");
            ViewData.TemplateInfo.HtmlFieldPrefix = ViewData["ViewName"].ToString();
            return PartialView("ProgrammationBarPerGroupStatesPartial", model);
        }

        public ActionResult ProgrammationBarPerRouteErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, int programmationServiceOffset)
        {
            return ProgrammationPerRouteErrorDetails(errorJmsIds, sigIIProgrammationPendings, DateTime.Now.AddDays(programmationServiceOffset));
        }

        public ActionResult ProgrammationBarErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, int programmationServiceOffset)
        {
            return ProgrammationErrorDetails(errorJmsIds, sigIIProgrammationPendings, DateTime.Now.AddDays(programmationServiceOffset));
        }

        public ActionResult ProgrammationBarPerGroupErrorDetails(List<int> errorJmsIds, List<string> sigIIProgrammationPendings, int programmationServiceOffset)
        {
            return ProgrammationPerGroupErrorDetails(errorJmsIds, sigIIProgrammationPendings, DateTime.Now.AddDays(programmationServiceOffset));
        }
    }
}

