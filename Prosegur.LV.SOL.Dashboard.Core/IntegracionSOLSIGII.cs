using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Prosegur.LV.SOL.Dashboard.Core.Excepciones;
using Prosegur.LV.SOL.Dashboard.Entities;

namespace Prosegur.LV.SOL.Dashboard.Core
{
    public class IntegracionSOLSIGII
    {
        public Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> EstadoProgramacionDeServicios(DateTime fechaProgramacion, string[] delegaciones)
        {
            IEnumerable<SOLIntegracionProgramacion> programacionesIntegracion = new List<SOLIntegracionProgramacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIPlanta> plantasSIGIINoIntegradas = null;

            SOLIntegracionProgramacion programacionPendienteSol = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.PendienteSOL
            };

            SOLIntegracionProgramacion pendienteIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLPendienteIntegracion
            };

            SOLIntegracionProgramacion errorIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLErrorIntegracion
            };

            SOLIntegracionProgramacion sigIIIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIIIntegrado
            };

            SOLIntegracionProgramacion sigIINoIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIINoIntegrado
            };

            programacionesIntegracion = new List<SOLIntegracionProgramacion>()
            {
                programacionPendienteSol,
                pendienteIntegracion,
                errorIntegracion,
                sigIIIntegrado,
                sigIINoIntegrado
            };

            try
            {
                DAO.SOLProgramacion daoSolProgramacion = new DAO.SOLProgramacion();

                IList<Entities.SOLProgramacion> solProgramaciones = daoSolProgramacion.GetFechasCierre(fechaProgramacion, delegaciones);

                var delegacionesPendientesSOL = from p in solProgramaciones
                                                where p.FechaCierreServicio == null ||
                                                        p.FechaCierreServicio == DateTime.MinValue
                                                select p.CodigoDelegacion;

                var delegacionesCerradasSOL = from p in solProgramaciones
                                              where p.FechaCierreServicio != null &&
                                                    p.FechaCierreServicio != DateTime.MinValue
                                              select p.CodigoDelegacion;

                programacionPendienteSol.Cantidad = delegacionesPendientesSOL.Count();

                if (delegacionesCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacion daoSIGIIProgramacion = new DAO.SIGIIProgramacion();

                    IList<Entities.SIGIIProgramacion> sigIIProgramaciones = daoSIGIIProgramacion.GetFechasCierreServicios(fechaProgramacion, delegacionesCerradasSOL.Select(d => d).ToArray());

                    sigIIIntegrado.Cantidad = sigIIProgramaciones.Where(d => d.FechaCierreServicio != DateTime.MinValue).Count();

                    var delegacionesNoIntegradas = from d in delegacionesCerradasSOL
                                                   where !sigIIProgramaciones.Where(s => s.CodigoDelegacion == d).Any() ||
                                                            (sigIIProgramaciones.Where(s => s.CodigoDelegacion == d && s.FechaCierreServicio == DateTime.MinValue).Any())
                                                   select d;

                    foreach (string delegacionNoIntegrada in delegacionesNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_SERVICIO_GENERAL", fechaProgramacion.ToString("dd/MM/yyyy"), "C", delegacionNoIntegrada, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                        {
                            errorIntegracion.Cantidad++;

                            if (mensajesIntegracionError == null)
                                mensajesIntegracionError = new List<JMSEnvioMensaje>();

                            mensajesIntegracionError.Add(jmsEnvioMensaje);
                            continue;
                        }

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                        {
                            sigIINoIntegrado.Cantidad++;

                            if (plantasSIGIINoIntegradas == null)
                                plantasSIGIINoIntegradas = new List<SIGIIPlanta>();

                            plantasSIGIINoIntegradas.Add(new SIGIIPlanta() { Codigo = delegacionNoIntegrada });
                            continue;
                        }

                        pendienteIntegracion.Cantidad++;
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "fechaProgramacion",  fechaProgramacion.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>>(programacionesIntegracion, mensajesIntegracionError, plantasSIGIINoIntegradas);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> EstadoProgramacionDeRecursos(DateTime fechaProgramacion, string[] delegaciones)
        {
            IEnumerable<SOLIntegracionProgramacion> programacionesIntegracion = new List<SOLIntegracionProgramacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIPlanta> plantasSIGIINoIntegradas = null;

            SOLIntegracionProgramacion programacionPendienteSol = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.PendienteSOL
            };

            SOLIntegracionProgramacion pendienteIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLPendienteIntegracion
            };

            SOLIntegracionProgramacion errorIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLErrorIntegracion
            };

            SOLIntegracionProgramacion sigIIIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIIIntegrado
            };

            SOLIntegracionProgramacion sigIINoIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIINoIntegrado
            };

            programacionesIntegracion = new List<SOLIntegracionProgramacion>()
            {
                programacionPendienteSol,
                pendienteIntegracion,
                errorIntegracion,
                sigIIIntegrado,
                sigIINoIntegrado
            };

            try
            {
                DAO.SOLProgramacion daoSolProgramacion = new DAO.SOLProgramacion();

                IList<Entities.SOLProgramacion> solProgramaciones = daoSolProgramacion.GetFechasCierre(fechaProgramacion, delegaciones);

                var delegacionesPendientesSOL = from p in solProgramaciones
                                                where p.FechaCierreRecurso == null ||
                                                        p.FechaCierreRecurso == DateTime.MinValue
                                                select p.CodigoDelegacion;

                var delegacionesCerradasSOL = from p in solProgramaciones
                                              where p.FechaCierreRecurso != null &&
                                                    p.FechaCierreRecurso != DateTime.MinValue
                                              select p.CodigoDelegacion;

                programacionPendienteSol.Cantidad = delegacionesPendientesSOL.Count();

                if (delegacionesCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacion daoSIGIIProgramacion = new DAO.SIGIIProgramacion();

                    IList<Entities.SIGIIProgramacion> sigIIProgramaciones = daoSIGIIProgramacion.GetFechasCierreRecursos(fechaProgramacion, delegacionesCerradasSOL.Select(d => d).ToArray());

                    sigIIIntegrado.Cantidad = sigIIProgramaciones.Count();

                    var delegacionesNoIntegradas = from d in delegacionesCerradasSOL
                                                   where !sigIIProgramaciones.Where(s => s.CodigoDelegacion == d).Any()
                                                   select d;

                    foreach (string delegacionNoIntegrada in delegacionesNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_RECURSO_GENERAL", fechaProgramacion.ToString("dd/MM/yyyy"), "C", delegacionNoIntegrada, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                        {
                            errorIntegracion.Cantidad++;

                            if (mensajesIntegracionError == null)
                                mensajesIntegracionError = new List<JMSEnvioMensaje>();

                            mensajesIntegracionError.Add(jmsEnvioMensaje);
                            continue;
                        }

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                        {
                            sigIINoIntegrado.Cantidad++;

                            if (plantasSIGIINoIntegradas == null)
                                plantasSIGIINoIntegradas = new List<SIGIIPlanta>();

                            plantasSIGIINoIntegradas.Add(new SIGIIPlanta() { Codigo = delegacionNoIntegrada });
                            continue;
                        }

                        pendienteIntegracion.Cantidad++;
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "fechaProgramacion",  fechaProgramacion.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>>(programacionesIntegracion, mensajesIntegracionError, plantasSIGIINoIntegradas);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> EstadoCierresFacturacion(int offsetDias, string[] lstDelegaciones)
        {
            var EstadosCierresFactxruta = new List<SOLIntegracionProgramacionDelegacionRuta>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIRecorrido> recorridosSIGIINoIntegrados = new List<SIGIIRecorrido>();

            var SRFC = new DAO.SolRutasFinalizadasCerradas();
            var RutasFinalizadasCerradas = SRFC.GetRutasFinalizadasCerradas(DateTime.Now.AddDays(offsetDias), lstDelegaciones);


            var JMS = new DAO.JMSEnvio();
            var Mensajes = JMS.GetMensajesCierreFact(lstDelegaciones, DateTime.Now.AddDays(offsetDias));

            //Agarro todos los mensajes de error con esta sencilla opereta

            mensajesIntegracionError = Mensajes.Where(s => s.Estado == "ER").ToList();


            //Bien ahora a trabajar en el primer punto....
            foreach (var ruta in RutasFinalizadasCerradas)
            {
                //La descripción trucha de delegacion desaparece luego gracias a una javascript oportuno que la asimila a una 
                var mydele = new SOLDelegacion { Codigo = ruta.CodigoDelegacion, Descripcion = "del" + ruta.CodigoDelegacion };
                var myruta = new SOLRuta { Codigo = ruta.CodigoRuta };

                if (ruta.CodigoEstado == "6")
                {
                    EstadosCierresFactxruta.Add(
                        new SOLIntegracionProgramacionDelegacionRuta
                        {
                            Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.ACerrar,
                            Delegacion = mydele,
                            Ruta = myruta
                        });
                    continue;
                }

                //A partir de acá todo es estado 7 ya que sólo pedimos este estado 
                //Chequeamos que exista al menos uno pendiente

                if (Mensajes.Any(x => x.Id == ruta.OidRuta && x.Estado == "PE"))
                {
                    EstadosCierresFactxruta.Add(
                                             new SOLIntegracionProgramacionDelegacionRuta
                                             {
                                                 Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrando,
                                                 Delegacion = mydele,
                                                 Ruta = myruta
                                             });

                    recorridosSIGIINoIntegrados.AddRange(from mensa in Mensajes
                                                         where mensa.Estado == "PE" && mensa.Id == ruta.OidRuta
                                                         select new SIGIIRecorrido()
                                                         {
                                                             CodigoDelegacion = mensa.Delegacion,
                                                             Observacion = mensa.Observacion,
                                                             Recorrido = ruta.CodigoRuta
                                                         });
                    continue;

                }

                //Si el último mensaje es un error...

                if (Mensajes.Where(x => x.Id == ruta.OidRuta).OrderByDescending ( x=> x.JmsId).First().Estado == "ER")
                    //if (Mensajes.Any(x => x.Id == ruta.OidRuta && x.Estado == "ER"))
                {
                    EstadosCierresFactxruta.Add(
                                            new SOLIntegracionProgramacionDelegacionRuta
                                            {
                                                Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Error,
                                                Delegacion = mydele,
                                                Ruta = myruta
                                            });
                    continue;

                }

                //Si el último mensaje de esta ruta es OK, entonces todo joya no?
                if (Mensajes.Where(x => x.Id == ruta.OidRuta).OrderByDescending(x => x.JmsId).First().Estado == "OK")
                 //   if (Mensajes.Any(x => x.Id == ruta.OidRuta && x.Estado == "OK"))
                {
                    EstadosCierresFactxruta.Add(
                                            new SOLIntegracionProgramacionDelegacionRuta
                                            {
                                                Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrado,
                                                Delegacion = mydele,
                                                Ruta = myruta
                                            });
                    continue;

                }

  
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>>(EstadosCierresFactxruta, mensajesIntegracionError, recorridosSIGIINoIntegrados);

        }

        public Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> EstadoProgramacionDeServiciosPorRuta(DateTime fechaProgramacion, string[] delegaciones)
        {
            IEnumerable<SOLIntegracionProgramacion> programacionesIntegracion = new List<SOLIntegracionProgramacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIRecorrido> recorridosSIGIINoIntegrados = null;

            SOLIntegracionProgramacion programacionPendienteSol = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.PendienteSOL
            };

            SOLIntegracionProgramacion pendienteIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLPendienteIntegracion
            };

            SOLIntegracionProgramacion errorIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLErrorIntegracion
            };

            SOLIntegracionProgramacion sigIIIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIIIntegrado
            };

            SOLIntegracionProgramacion sigIINoIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIINoIntegrado
            };

            programacionesIntegracion = new List<SOLIntegracionProgramacion>()
            {
                programacionPendienteSol,
                pendienteIntegracion,
                errorIntegracion,
                sigIIIntegrado,
                sigIINoIntegrado
            };

            try
            {
                DAO.SOLProgramacionRuta daoSolProgramacionRuta = new DAO.SOLProgramacionRuta();

                IList<Entities.SOLProgramacionRuta> solProgramacionesRuta = daoSolProgramacionRuta.GetFechasCierre(fechaProgramacion, delegaciones);

                var rutasPendientesSOL = from p in solProgramacionesRuta
                                         where p.FechaCierreServicio == DateTime.MinValue
                                         select p.CodigoDelegacion;

                var rutasCerradasSOL = from p in solProgramacionesRuta
                                       where p.FechaCierreServicio != DateTime.MinValue
                                       select p;

                programacionPendienteSol.Cantidad = rutasPendientesSOL.Count();

                if (rutasCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacionRecorrido daoSIGIIProgramacionRecorrido = new DAO.SIGIIProgramacionRecorrido();

                    IList<Entities.SIGIIProgramacionRecorrido> sigIIProgramacionRecorridos = daoSIGIIProgramacionRecorrido.GetEstadoCierres(fechaProgramacion, rutasCerradasSOL.Select(d => d.CodigoRuta).ToArray());

                    sigIIIntegrado.Cantidad = sigIIProgramacionRecorridos.Where(r => r.CierreServicios == 1).Count();

                    var rutasNoIntegradas = from r in rutasCerradasSOL
                                            where !sigIIProgramacionRecorridos.Where(s => s.CierreServicios == 1 && s.CodigoRecorrido == r.CodigoRuta).Any()
                                            select r;

                    foreach (Entities.SOLProgramacionRuta rutaNoIntegrada in rutasNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        //JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_SERVICIO", string.Format("{0}|{1}", rutaNoIntegrada.CodigoRuta, rutaNoIntegrada.CodigoDelegacion), fechaProgramacion.ToString("dd/MM/yyyy"), "C", rutaNoIntegrada.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_SERVICIO", rutaNoIntegrada.OidRuta, string.Format("{0}|{1}", rutaNoIntegrada.CodigoRuta, rutaNoIntegrada.CodigoDelegacion), "C", rutaNoIntegrada.CodigoDelegacion);

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                        {
                            errorIntegracion.Cantidad++;

                            if (mensajesIntegracionError == null)
                                mensajesIntegracionError = new List<JMSEnvioMensaje>();

                            mensajesIntegracionError.Add(jmsEnvioMensaje);
                            continue;
                        }

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                        {
                            sigIINoIntegrado.Cantidad++;

                            if (recorridosSIGIINoIntegrados == null)
                                recorridosSIGIINoIntegrados = new List<SIGIIRecorrido>();

                            recorridosSIGIINoIntegrados.Add(new SIGIIRecorrido() { Recorrido = rutaNoIntegrada.CodigoRuta, CodigoDelegacion = rutaNoIntegrada.CodigoDelegacion });
                            continue;
                        }

                        pendienteIntegracion.Cantidad++;
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "fechaProgramacion",  fechaProgramacion.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>>(programacionesIntegracion, mensajesIntegracionError, recorridosSIGIINoIntegrados);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> EstadoProgramacionDeRecursosPorRuta(DateTime fechaProgramacion, string[] delegaciones)
        {
            IEnumerable<SOLIntegracionProgramacion> programacionesIntegracion = new List<SOLIntegracionProgramacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIRecorrido> recorridosSIGIINoIntegrados = null;

            SOLIntegracionProgramacion programacionPendienteSol = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.PendienteSOL
            };

            SOLIntegracionProgramacion pendienteIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLPendienteIntegracion
            };

            SOLIntegracionProgramacion errorIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLErrorIntegracion
            };

            SOLIntegracionProgramacion sigIIIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIIIntegrado
            };

            SOLIntegracionProgramacion sigIINoIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIINoIntegrado
            };

            programacionesIntegracion = new List<SOLIntegracionProgramacion>()
            {
                programacionPendienteSol,
                pendienteIntegracion,
                errorIntegracion,
                sigIIIntegrado,
                sigIINoIntegrado
            };

            try
            {
                DAO.SOLProgramacionRuta daoSolProgramacionRuta = new DAO.SOLProgramacionRuta();

                IList<Entities.SOLProgramacionRuta> solProgramacionesRuta = daoSolProgramacionRuta.GetFechasCierre(fechaProgramacion, delegaciones);

                var rutasPendientesSOL = from p in solProgramacionesRuta
                                         where p.FechaCierreRecurso == DateTime.MinValue
                                         select p.CodigoDelegacion;

                var rutasCerradasSOL = from p in solProgramacionesRuta
                                       where p.FechaCierreRecurso != DateTime.MinValue
                                       select p;

                programacionPendienteSol.Cantidad = rutasPendientesSOL.Count();

                if (rutasCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacionRecorrido daoSIGIIProgramacionRecorrido = new DAO.SIGIIProgramacionRecorrido();

                    IList<Entities.SIGIIProgramacionRecorrido> sigIIProgramacionRecorridos = daoSIGIIProgramacionRecorrido.GetEstadoCierres(fechaProgramacion, rutasCerradasSOL.Select(d => d.CodigoRuta).ToArray());

                    sigIIIntegrado.Cantidad = sigIIProgramacionRecorridos.Where(r => r.CierreRecursos == 1).Count();

                    var rutasNoIntegradas = from r in rutasCerradasSOL
                                            where !sigIIProgramacionRecorridos.Where(s => s.CierreRecursos == 1 && s.CodigoRecorrido == r.CodigoRuta).Any()
                                            select r;

                    foreach (Entities.SOLProgramacionRuta rutaNoIntegrada in rutasNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_RECURSO", string.Format("{0}|{1}|{2}", rutaNoIntegrada.CodigoDelegacion, rutaNoIntegrada.CodigoRuta, fechaProgramacion.ToString("dd/MM/yyyy")), "C", rutaNoIntegrada.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                        {
                            errorIntegracion.Cantidad++;

                            if (mensajesIntegracionError == null)
                                mensajesIntegracionError = new List<JMSEnvioMensaje>();

                            mensajesIntegracionError.Add(jmsEnvioMensaje);
                            continue;
                        }

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                        {
                            sigIINoIntegrado.Cantidad++;

                            if (recorridosSIGIINoIntegrados == null)
                                recorridosSIGIINoIntegrados = new List<SIGIIRecorrido>();

                            recorridosSIGIINoIntegrados.Add(new SIGIIRecorrido() { Recorrido = rutaNoIntegrada.CodigoRuta, CodigoDelegacion = rutaNoIntegrada.CodigoDelegacion });
                            continue;
                        }

                        pendienteIntegracion.Cantidad++;
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "fechaProgramacion",  fechaProgramacion.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>>(programacionesIntegracion, mensajesIntegracionError, recorridosSIGIINoIntegrados);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIGrupo>> EstadoProgramacionDeSectores(DateTime fechaProgramacion, string[] delegaciones)
        {
            IEnumerable<SOLIntegracionProgramacion> programacionesIntegracion = new List<SOLIntegracionProgramacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIGrupo> gruposSIGIINoIntegrados = null;

            SOLIntegracionProgramacion programacionPendienteSol = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.PendienteSOL
            };

            SOLIntegracionProgramacion pendienteIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLPendienteIntegracion
            };

            SOLIntegracionProgramacion errorIntegracion = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLErrorIntegracion
            };

            SOLIntegracionProgramacion sigIIIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIIIntegrado
            };

            SOLIntegracionProgramacion sigIINoIntegrado = new SOLIntegracionProgramacion()
            {
                Estado = SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIINoIntegrado
            };

            programacionesIntegracion = new List<SOLIntegracionProgramacion>()
            {
                programacionPendienteSol,
                pendienteIntegracion,
                errorIntegracion,
                sigIIIntegrado,
                sigIINoIntegrado
            };

            try
            {
                DAO.SOLProgramacionSector daoSolProgramacionSector = new DAO.SOLProgramacionSector();

                IList<Entities.SOLProgramacionSector> solProgramacionesSector = daoSolProgramacionSector.GetEstadoCierre(fechaProgramacion, delegaciones);

                var sectoresPendientesSOL = from p in solProgramacionesSector
                                            where p.FechaCierre == null ||
                                                    p.FechaCierre == DateTime.MinValue
                                            select p.CodigoSector;

                var sectoresCerradosSOL = from p in solProgramacionesSector
                                          where p.FechaCierre != null &&
                                                p.FechaCierre != DateTime.MinValue
                                          select p;

                programacionPendienteSol.Cantidad = sectoresPendientesSOL.Count();

                if (sectoresCerradosSOL.Count() > 0 && sectoresCerradosSOL.Where(s => s.CantidadRecursos > 0).Any())
                {
                    DAO.SIGIIProgramacionGrupo daoSIGIIProgramacionGrupo = new DAO.SIGIIProgramacionGrupo();

                    var sectoresCerradosConRecursos = (from s in sectoresCerradosSOL
                                                       where s.CantidadRecursos > 0
                                                       select s.CodigoSector).ToArray();

                    IList<Entities.SIGIIProgramacionGrupo> sigIIProgramacionRecorridos = daoSIGIIProgramacionGrupo.GetEstadoCierres(fechaProgramacion, delegaciones, sectoresCerradosConRecursos);

                    sigIIIntegrado.Cantidad = sigIIProgramacionRecorridos.Count();

                    var gruposParaEvaluar = (from s in sectoresCerradosSOL
                                             where s.CantidadRecursos > 0 &&
                                                    !sigIIProgramacionRecorridos.Any(g => g.CodigoDelegacion == s.CodigoDelegacion && g.CodigoGrupo == s.CodigoSector)
                                             select s).ToList();

                    foreach (SOLProgramacionSector grupoParaEvaluar in gruposParaEvaluar)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_RECURSO", "S", string.Format("{0}|{1}|{2}", grupoParaEvaluar.CodigoDelegacion, grupoParaEvaluar.CodigoSector, fechaProgramacion.ToString("dd/MM/yyyy")), "C", grupoParaEvaluar.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                        {
                            errorIntegracion.Cantidad++;

                            if (mensajesIntegracionError == null)
                                mensajesIntegracionError = new List<JMSEnvioMensaje>();

                            mensajesIntegracionError.Add(jmsEnvioMensaje);
                            continue;
                        }

                        if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                        {
                            if (grupoParaEvaluar.CantidadRecursos > 0)
                            {
                                sigIINoIntegrado.Cantidad++;

                                if (gruposSIGIINoIntegrados == null)
                                    gruposSIGIINoIntegrados = new List<SIGIIGrupo>();

                                gruposSIGIINoIntegrados.Add(new SIGIIGrupo() { Codigo = grupoParaEvaluar.CodigoSector, CodigoDelegacion = grupoParaEvaluar.CodigoDelegacion });
                            }
                            else
                            {
                                sigIIIntegrado.Cantidad++;
                            }

                            continue;
                        }

                        pendienteIntegracion.Cantidad++;
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "fechaProgramacion",  fechaProgramacion.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIGrupo>>(programacionesIntegracion, mensajesIntegracionError, gruposSIGIINoIntegrados);
        }

        public Tuple<IEnumerable<SOLIntegracionRuta>, IEnumerable<SOLRuta>> EstadoDeRutas(DateTime fechaProgramacion, string[] delegaciones)
        {
            IEnumerable<SOLIntegracionRuta> estadoIntegracionesRuta = new List<SOLIntegracionRuta>();
            List<SOLRuta> rutasConErrorDeIntegracion = new List<SOLRuta>();

            SOLIntegracionRuta rutasIntegradas = new SOLIntegracionRuta()
            {
                Estado = SOLIntegracionRuta.SOLIntegracionRutaEstado.Integrada
            };

            SOLIntegracionRuta rutasNoItegradasPorError = new SOLIntegracionRuta()
            {
                Estado = SOLIntegracionRuta.SOLIntegracionRutaEstado.ConErroresDeIntegracion
            };

            estadoIntegracionesRuta = new List<SOLIntegracionRuta>()
            {
                rutasIntegradas,
                rutasNoItegradasPorError
            };

            try
            {
                IList<Entities.SIGIIPlanta> sigIIPlantas = new List<Entities.SIGIIPlanta>();

                var daoSIGIIPlanta = new DAO.SIGIIPlanta();

                sigIIPlantas = daoSIGIIPlanta.GetPlantasConBasurero(delegaciones);

                IEnumerable<SIGIIRecorrido> recorridosBasurerosPlanta = (from b in (from p in sigIIPlantas select p.Basureros).FirstOrDefault()
                                                                         select b);

                DAO.SOLOrdenDeTrabajo daoSOLOrdenDeTrabajo = new DAO.SOLOrdenDeTrabajo();

                IList<Entities.SOLOrdenDeTrabajo> solOrdenesDeTrabajo = daoSOLOrdenDeTrabajo.GetOrdenesDeTrabajoAIntegrar(delegaciones, fechaProgramacion, recorridosBasurerosPlanta.Select(b => b.Recorrido).ToArray());

                if (solOrdenesDeTrabajo.Count > 0)
                {
                    var rutasAEvaluar = from ot in solOrdenesDeTrabajo
                                        group ot by ot.CodigoRuta into g
                                        select g.Key;

                    DAO.SIGIIHojaDeRuta sigIIHojaDeRutaDao = new DAO.SIGIIHojaDeRuta();

                    IList<Entities.SIGIIHojaDeRuta> sigIIHojasDeRuta = sigIIHojaDeRutaDao.GetHojasDeRuta(rutasAEvaluar.ToArray(), fechaProgramacion);

                    foreach (int rutaAEvaluar in rutasAEvaluar)
                    {
                        var solOrdenesDeTrabajoRuta = from o in solOrdenesDeTrabajo
                                                      where o.CodigoRuta.Equals(rutaAEvaluar)
                                                      select o;

                        var sigIIHojasDeRutaOidOt = from h in sigIIHojasDeRuta
                                                    where solOrdenesDeTrabajoRuta.Any(s => s.OidOt == h.OidOT)
                                                    select h;

                        if (solOrdenesDeTrabajoRuta.Count().Equals(sigIIHojasDeRutaOidOt.Count()))
                        {
                            var comparacionFechasDeServicio = from o in solOrdenesDeTrabajoRuta
                                                              join s in sigIIHojasDeRutaOidOt on o.OidOt equals s.OidOT
                                                              let hojaDeRutaFechaServicio = new DateTime(s.FechaProceso.Year, s.FechaProceso.Month, s.FechaProceso.Day, s.HoraProceso.Hour, s.HoraProceso.Minute, s.HoraProceso.Second)
                                                              select o.FechaProgramadaInicio != hojaDeRutaFechaServicio;

                            bool existeDiferencia = comparacionFechasDeServicio.Where(d => d == true).Any();

                            if (existeDiferencia)
                            {
                                rutasNoItegradasPorError.Cantidad++;

                                rutasConErrorDeIntegracion.Add(new SOLRuta() { Codigo = rutaAEvaluar });
                            }
                            else
                            {
                                rutasIntegradas.Cantidad++;
                            }

                            continue;
                        }

                        if (solOrdenesDeTrabajoRuta.Count() > sigIIHojasDeRutaOidOt.Count())
                        {
                            DAO.JMSEnvio jmsEnvioDAO = new DAO.JMSEnvio();

                            Entities.JMSEnvioMensaje jmsEnvioMensaje = jmsEnvioDAO.GetUltimoMensaje("PROGRAMACION_SERVICIO", solOrdenesDeTrabajoRuta.FirstOrDefault().OidRuta, string.Format("{0}|{1}", rutaAEvaluar, solOrdenesDeTrabajo.FirstOrDefault().CodigoDelegacion), "C", solOrdenesDeTrabajo.FirstOrDefault().CodigoDelegacion);

                            //IList <Entities.JMSEnvioMensaje> jmsEnvioMensajes = jmsEnvioDAO.GetMensajes(solOrdenesDeTrabajoRuta.Select(s => s.CodigoDelegacion).ToArray(), "OT", new string[] { Configuracion.CodigoEstadoError }, solOrdenesDeTrabajoRuta.Select(s => s.OidOt).ToArray(), "A");

                            if (jmsEnvioMensaje != null && jmsEnvioMensaje.Estado.Equals(Configuracion.CodigoEstadoError))
                            {
                                rutasNoItegradasPorError.Cantidad++;

                                rutasConErrorDeIntegracion.Add(new SOLRuta() { Codigo = rutaAEvaluar });

                                continue;
                            }
                        }

                        var sigIIHojasDeRutaSinOidOt = from h in sigIIHojasDeRuta
                                                       where string.IsNullOrEmpty(h.OidOT)
                                                       select h;

                        if (sigIIHojasDeRutaSinOidOt.Any())
                        {
                            rutasNoItegradasPorError.Cantidad++;

                            rutasConErrorDeIntegracion.Add(new SOLRuta() { Codigo = rutaAEvaluar });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "fechaProgramacion",  fechaProgramacion.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionRuta>, IEnumerable<SOLRuta>>(estadoIntegracionesRuta, rutasConErrorDeIntegracion);
        }

        public IList<Tuple<Entities.SOLOrdenDeTrabajo, Entities.SIGIIHojaDeRuta>> DetalleDeRutas(int[] codigoRutas, DateTime fechaRuta)
        {
            DAO.SOLOrdenDeTrabajo solOrdenDeTrabajoDAO = new DAO.SOLOrdenDeTrabajo();
            DAO.SIGIIHojaDeRuta sigIIHojaDeRutaDAO = new DAO.SIGIIHojaDeRuta();

            IList<Entities.SOLOrdenDeTrabajo> solOrdenesDeTrabajo = null;

            IList<Entities.SIGIIHojaDeRuta> sigIIHojasDeRuta = null;

            try
            {
                solOrdenesDeTrabajo = solOrdenDeTrabajoDAO.GetOrdenesDeTrabajoAIntegrar(codigoRutas, fechaRuta);
                sigIIHojasDeRuta = sigIIHojaDeRutaDAO.GetHojasDeRuta(codigoRutas, fechaRuta);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "codigoRutas", string.Join(", ", codigoRutas) },
                    { "fechaRuta",  fechaRuta.ToString() }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            List<Tuple<Entities.SOLOrdenDeTrabajo, Entities.SIGIIHojaDeRuta>> detalle = (from o in solOrdenesDeTrabajo
                                                                                         join h in sigIIHojasDeRuta on new { r = o.CodigoRuta, ot = o.OidOt } equals new { r = h.Recorrido, ot = h.OidOT } into itemRuta
                                                                                         from sigIIHojaDeRuta in itemRuta.DefaultIfEmpty()
                                                                                         select new Tuple<Entities.SOLOrdenDeTrabajo, Entities.SIGIIHojaDeRuta>(o, (sigIIHojaDeRuta == null ? null : sigIIHojaDeRuta))).ToList();

            detalle.AddRange(from h in sigIIHojasDeRuta
                             where string.IsNullOrEmpty(h.OidOT)
                             select new Tuple<Entities.SOLOrdenDeTrabajo, Entities.SIGIIHojaDeRuta>(null, h));

            return detalle;
        }

        #region Metodos por Delegacion

        public Tuple<IEnumerable<SOLIntegracionProgramacionDelegacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> EstadoProgramacionDeServicios(int offsetDias, string[] delegaciones)
        {
            List<SOLIntegracionProgramacionDelegacion> estadoIntegracionServicios = new List<SOLIntegracionProgramacionDelegacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIPlanta> plantasSIGIINoIntegradas = null;

            try
            {
                DAO.SOLProgramacion daoSolProgramacion = new DAO.SOLProgramacion();

                IList<Entities.SOLProgramacion> solProgramaciones = daoSolProgramacion.GetFechasCierre(DateTime.Now.AddDays(offsetDias), delegaciones);

                var delegacionesPendientesSOL = from p in solProgramaciones
                                                where p.FechaCierreServicio == null || p.FechaCierreServicio == DateTime.MinValue
                                                select new { p.CodigoDelegacion, p.DescripcionDelegacion };

                // A Cerrar
                estadoIntegracionServicios.AddRange((from p in delegacionesPendientesSOL
                                                     select new SOLIntegracionProgramacionDelegacion()
                                                     {
                                                         Delegacion = new SOLDelegacion()
                                                         {
                                                             Codigo = p.CodigoDelegacion,
                                                             Descripcion = p.DescripcionDelegacion
                                                         },
                                                         Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.ACerrar,
                                                     }).ToList());

                var delegacionesCerradasSOL = from p in solProgramaciones
                                              where p.FechaCierreServicio != null && p.FechaCierreServicio != DateTime.MinValue
                                              select new { p.CodigoDelegacion, p.DescripcionDelegacion };

                if (delegacionesCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacion daoSIGIIProgramacion = new DAO.SIGIIProgramacion();

                    IList<Entities.SIGIIProgramacion> sigIIProgramaciones = daoSIGIIProgramacion.GetFechasCierreServicios(DateTime.Now.AddDays(offsetDias), delegacionesCerradasSOL.Select(d => d.CodigoDelegacion).ToArray());

                    // Delegaciones Integradas
                    estadoIntegracionServicios.AddRange((from p in delegacionesCerradasSOL
                                                         let sigII = sigIIProgramaciones.Where(s => s.CodigoDelegacion == p.CodigoDelegacion).FirstOrDefault()
                                                         where sigII != null && sigII.FechaCierreServicio != DateTime.MinValue
                                                         select new SOLIntegracionProgramacionDelegacion()
                                                         {
                                                             Delegacion = new SOLDelegacion()
                                                             {
                                                                 Codigo = p.CodigoDelegacion,
                                                                 Descripcion = p.DescripcionDelegacion
                                                             },
                                                             Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Integrada,
                                                         }).ToList());

                    var delegacionesNoIntegradas = from d in delegacionesCerradasSOL
                                                   let sigII = sigIIProgramaciones.Where(s => s.CodigoDelegacion == d.CodigoDelegacion).FirstOrDefault()
                                                   where sigII == null || (sigII != null && sigII.FechaCierreServicio == DateTime.MinValue)
                                                   select d;

                    foreach (var delegacionNoIntegrada in delegacionesNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_SERVICIO_GENERAL", DateTime.Now.AddDays(offsetDias).ToString("dd/MM/yyyy"), "C", delegacionNoIntegrada.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        // Con Error
                        if (jmsEnvioMensaje != null && (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError || jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado))
                        {
                            estadoIntegracionServicios.Add(new SOLIntegracionProgramacionDelegacion()
                            {
                                Delegacion = new SOLDelegacion()
                                {
                                    Codigo = delegacionNoIntegrada.CodigoDelegacion,
                                    Descripcion = delegacionNoIntegrada.DescripcionDelegacion
                                },
                                Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Error,
                            });

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                            {
                                if (mensajesIntegracionError == null)
                                    mensajesIntegracionError = new List<JMSEnvioMensaje>();

                                mensajesIntegracionError.Add(jmsEnvioMensaje);
                            }

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                            {
                                if (plantasSIGIINoIntegradas == null)
                                    plantasSIGIINoIntegradas = new List<SIGIIPlanta>();

                                plantasSIGIINoIntegradas.Add(new SIGIIPlanta() { Codigo = delegacionNoIntegrada.CodigoDelegacion });
                            }

                            continue;
                        }

                        // A Integrar
                        estadoIntegracionServicios.Add(new SOLIntegracionProgramacionDelegacion()
                        {
                            Delegacion = new SOLDelegacion()
                            {
                                Codigo = delegacionNoIntegrada.CodigoDelegacion,
                                Descripcion = delegacionNoIntegrada.DescripcionDelegacion
                            },
                            Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Integrando,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "offsetDias",  offsetDias.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacionDelegacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>>(estadoIntegracionServicios, mensajesIntegracionError, plantasSIGIINoIntegradas);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacionDelegacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>> EstadoProgramacionDeRecursos(int offsetDias, string[] delegaciones)
        {
            List<SOLIntegracionProgramacionDelegacion> estadoIntegracionRecursos = new List<SOLIntegracionProgramacionDelegacion>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIPlanta> plantasSIGIINoIntegradas = null;

            try
            {
                DAO.SOLProgramacion daoSolProgramacion = new DAO.SOLProgramacion();

                IList<Entities.SOLProgramacion> solProgramaciones = daoSolProgramacion.GetFechasCierre(DateTime.Now.AddDays(offsetDias), delegaciones);

                var delegacionesPendientesSOL = from p in solProgramaciones
                                                where p.FechaCierreRecurso == null || p.FechaCierreRecurso == DateTime.MinValue
                                                select new { p.CodigoDelegacion, p.DescripcionDelegacion };

                // A Cerrar
                estadoIntegracionRecursos.AddRange((from p in delegacionesPendientesSOL
                                                    select new SOLIntegracionProgramacionDelegacion()
                                                    {
                                                        Delegacion = new SOLDelegacion()
                                                        {
                                                            Codigo = p.CodigoDelegacion,
                                                            Descripcion = p.DescripcionDelegacion
                                                        },
                                                        Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.ACerrar,
                                                    }).ToList());

                var delegacionesCerradasSOL = from p in solProgramaciones
                                              where p.FechaCierreRecurso != null && p.FechaCierreRecurso != DateTime.MinValue
                                              select new { p.CodigoDelegacion, p.DescripcionDelegacion };

                if (delegacionesCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacion daoSIGIIProgramacion = new DAO.SIGIIProgramacion();

                    IList<Entities.SIGIIProgramacion> sigIIProgramaciones = daoSIGIIProgramacion.GetFechasCierreRecursos(DateTime.Now.AddDays(offsetDias), delegacionesCerradasSOL.Select(d => d.CodigoDelegacion).ToArray());

                    // Delegaciones Integradas
                    estadoIntegracionRecursos.AddRange((from p in delegacionesCerradasSOL
                                                        let sigII = sigIIProgramaciones.Where(s => s.CodigoDelegacion == p.CodigoDelegacion).FirstOrDefault()
                                                        where sigII != null && sigII.FechaCierreRecurso != DateTime.MinValue
                                                        select new SOLIntegracionProgramacionDelegacion()
                                                        {
                                                            Delegacion = new SOLDelegacion()
                                                            {
                                                                Codigo = p.CodigoDelegacion,
                                                                Descripcion = p.DescripcionDelegacion
                                                            },
                                                            Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Integrada,
                                                        }).ToList());

                    var delegacionesNoIntegradas = from d in delegacionesCerradasSOL
                                                   let sigII = sigIIProgramaciones.Where(s => s.CodigoDelegacion == d.CodigoDelegacion).FirstOrDefault()
                                                   where sigII == null || (sigII != null && sigII.FechaCierreRecurso == DateTime.MinValue)
                                                   select d;

                    foreach (var delegacionNoIntegrada in delegacionesNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_RECURSO_GENERAL", DateTime.Now.AddDays(offsetDias).ToString("dd/MM/yyyy"), "C", delegacionNoIntegrada.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        // Con Error
                        if (jmsEnvioMensaje != null && (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError || jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado))
                        {
                            estadoIntegracionRecursos.Add(new SOLIntegracionProgramacionDelegacion()
                            {
                                Delegacion = new SOLDelegacion()
                                {
                                    Codigo = delegacionNoIntegrada.CodigoDelegacion,
                                    Descripcion = delegacionNoIntegrada.DescripcionDelegacion
                                },
                                Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Error,
                            });

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                            {
                                if (mensajesIntegracionError == null)
                                    mensajesIntegracionError = new List<JMSEnvioMensaje>();

                                mensajesIntegracionError.Add(jmsEnvioMensaje);
                            }

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                            {
                                if (plantasSIGIINoIntegradas == null)
                                    plantasSIGIINoIntegradas = new List<SIGIIPlanta>();

                                plantasSIGIINoIntegradas.Add(new SIGIIPlanta() { Codigo = delegacionNoIntegrada.CodigoDelegacion });
                            }

                            continue;
                        }

                        // A Integrar
                        estadoIntegracionRecursos.Add(new SOLIntegracionProgramacionDelegacion()
                        {
                            Delegacion = new SOLDelegacion()
                            {
                                Codigo = delegacionNoIntegrada.CodigoDelegacion,
                                Descripcion = delegacionNoIntegrada.DescripcionDelegacion
                            },
                            Estado = SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Integrando,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "offsetDias",  offsetDias.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacionDelegacion>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIPlanta>>(estadoIntegracionRecursos, mensajesIntegracionError, plantasSIGIINoIntegradas);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> EstadoProgramacionDeServiciosPorRuta(int offsetDias, string[] delegaciones)
        {
            List<SOLIntegracionProgramacionDelegacionRuta> estadoIntegracionServiciosRuta = new List<SOLIntegracionProgramacionDelegacionRuta>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIRecorrido> recorridosSIGIINoIntegrados = null;

            try
            {
                DAO.SOLProgramacionRuta daoSolProgramacionRuta = new DAO.SOLProgramacionRuta();

                IList<Entities.SOLProgramacionRuta> solProgramacionesRuta = daoSolProgramacionRuta.GetFechasCierre(DateTime.Now.AddDays(offsetDias), delegaciones);

                var rutasPendientesSOL = from p in solProgramacionesRuta
                                         where p.FechaCierreServicio == DateTime.MinValue
                                         select new { p.CodigoDelegacion, p.DescripcionDelegacion, p.CodigoRuta };

                // A Cerrar
                estadoIntegracionServiciosRuta.AddRange((from p in rutasPendientesSOL
                                                         select new SOLIntegracionProgramacionDelegacionRuta()
                                                         {
                                                             Delegacion = new SOLDelegacion()
                                                             {
                                                                 Codigo = p.CodigoDelegacion,
                                                                 Descripcion = p.DescripcionDelegacion
                                                             },
                                                             Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.ACerrar,
                                                             Ruta = new SOLRuta()
                                                             {
                                                                 Codigo = p.CodigoRuta
                                                             }
                                                         }).ToList());

                var rutasCerradasSOL = from p in solProgramacionesRuta
                                       where p.FechaCierreServicio != DateTime.MinValue
                                       select p;

                if (rutasCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacionRecorrido daoSIGIIProgramacionRecorrido = new DAO.SIGIIProgramacionRecorrido();

                    IList<Entities.SIGIIProgramacionRecorrido> sigIIProgramacionRecorridos = daoSIGIIProgramacionRecorrido.GetEstadoCierres(DateTime.Now.AddDays(offsetDias), rutasCerradasSOL.Select(d => d.CodigoRuta).ToArray());

                    // Integradas
                    estadoIntegracionServiciosRuta.AddRange((from i in sigIIProgramacionRecorridos
                                                             let solRuta = rutasCerradasSOL.Where(r => r.CodigoRuta == i.CodigoRecorrido).First()
                                                             where i.CierreServicios == 1
                                                             select new SOLIntegracionProgramacionDelegacionRuta()
                                                             {
                                                                 Delegacion = new SOLDelegacion()
                                                                 {
                                                                     Codigo = solRuta.CodigoDelegacion,
                                                                     Descripcion = solRuta.DescripcionDelegacion
                                                                 },
                                                                 Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrado,
                                                                 Ruta = new SOLRuta()
                                                                 {
                                                                     Codigo = solRuta.CodigoRuta
                                                                 }
                                                             }).ToList());

                    var rutasNoIntegradas = from r in rutasCerradasSOL
                                            where !sigIIProgramacionRecorridos.Where(s => s.CierreServicios == 1 && s.CodigoRecorrido == r.CodigoRuta).Any()
                                            select r;

                    foreach (Entities.SOLProgramacionRuta rutaNoIntegrada in rutasNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_SERVICIO", rutaNoIntegrada.OidRuta, string.Format("{0}|{1}", rutaNoIntegrada.CodigoRuta, rutaNoIntegrada.CodigoDelegacion), "C", rutaNoIntegrada.CodigoDelegacion);

                        // Con Error
                        if (jmsEnvioMensaje != null && (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError || jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado))
                        {
                            estadoIntegracionServiciosRuta.Add(new SOLIntegracionProgramacionDelegacionRuta()
                            {
                                Delegacion = new SOLDelegacion()
                                {
                                    Codigo = rutaNoIntegrada.CodigoDelegacion,
                                    Descripcion = rutaNoIntegrada.DescripcionDelegacion
                                },
                                Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Error,
                                Ruta = new SOLRuta()
                                {
                                    Codigo = rutaNoIntegrada.CodigoRuta
                                }
                            });

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                            {
                                if (mensajesIntegracionError == null)
                                    mensajesIntegracionError = new List<JMSEnvioMensaje>();

                                mensajesIntegracionError.Add(jmsEnvioMensaje);
                            }

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                            {
                                if (recorridosSIGIINoIntegrados == null)
                                    recorridosSIGIINoIntegrados = new List<SIGIIRecorrido>();

                                recorridosSIGIINoIntegrados.Add(new SIGIIRecorrido() { Recorrido = rutaNoIntegrada.CodigoRuta, CodigoDelegacion = rutaNoIntegrada.CodigoDelegacion });
                            }

                            continue;
                        }

                        // A Integrar
                        estadoIntegracionServiciosRuta.Add(new SOLIntegracionProgramacionDelegacionRuta()
                        {
                            Delegacion = new SOLDelegacion()
                            {
                                Codigo = rutaNoIntegrada.CodigoDelegacion,
                                Descripcion = rutaNoIntegrada.DescripcionDelegacion
                            },
                            Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrando,
                            Ruta = new SOLRuta()
                            {
                                Codigo = rutaNoIntegrada.CodigoRuta
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "offsetDias",  offsetDias.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>>(estadoIntegracionServiciosRuta, mensajesIntegracionError, recorridosSIGIINoIntegrados);
        }

        public Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>> EstadoProgramacionDeRecursosPorRuta(int offsetDias, string[] delegaciones)
        {
            List<SOLIntegracionProgramacionDelegacionRuta> estadoIntegracionRecursosRuta = new List<SOLIntegracionProgramacionDelegacionRuta>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIRecorrido> recorridosSIGIINoIntegrados = null;

            try
            {
                DAO.SOLProgramacionRuta daoSolProgramacionRuta = new DAO.SOLProgramacionRuta();

                IList<Entities.SOLProgramacionRuta> solProgramacionesRuta = daoSolProgramacionRuta.GetFechasCierre(DateTime.Now.AddDays(offsetDias), delegaciones);

                var rutasPendientesSOL = from p in solProgramacionesRuta
                                         where p.FechaCierreRecurso == DateTime.MinValue
                                         select new { p.CodigoDelegacion, p.DescripcionDelegacion, p.CodigoRuta };

                // A Cerrar
                estadoIntegracionRecursosRuta.AddRange((from p in rutasPendientesSOL
                                                        select new SOLIntegracionProgramacionDelegacionRuta()
                                                        {
                                                            Delegacion = new SOLDelegacion()
                                                            {
                                                                Codigo = p.CodigoDelegacion,
                                                                Descripcion = p.DescripcionDelegacion
                                                            },
                                                            Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.ACerrar,
                                                            Ruta = new SOLRuta()
                                                            {
                                                                Codigo = p.CodigoRuta
                                                            }
                                                        }).ToList());

                var rutasCerradasSOL = from p in solProgramacionesRuta
                                       where p.FechaCierreRecurso != DateTime.MinValue
                                       select p;

                if (rutasCerradasSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacionRecorrido daoSIGIIProgramacionRecorrido = new DAO.SIGIIProgramacionRecorrido();

                    IList<Entities.SIGIIProgramacionRecorrido> sigIIProgramacionRecorridos = daoSIGIIProgramacionRecorrido.GetEstadoCierres(DateTime.Now.AddDays(offsetDias), rutasCerradasSOL.Select(d => d.CodigoRuta).ToArray());

                    // Integradas
                    estadoIntegracionRecursosRuta.AddRange((from i in sigIIProgramacionRecorridos
                                                            let solRuta = rutasCerradasSOL.Where(r => r.CodigoRuta == i.CodigoRecorrido).First()
                                                            where i.CierreRecursos == 1
                                                            select new SOLIntegracionProgramacionDelegacionRuta()
                                                            {
                                                                Delegacion = new SOLDelegacion()
                                                                {
                                                                    Codigo = solRuta.CodigoDelegacion,
                                                                    Descripcion = solRuta.DescripcionDelegacion
                                                                },
                                                                Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrado,
                                                                Ruta = new SOLRuta()
                                                                {
                                                                    Codigo = solRuta.CodigoRuta
                                                                }
                                                            }).ToList());

                    var rutasNoIntegradas = from r in rutasCerradasSOL
                                            where !sigIIProgramacionRecorridos.Where(s => s.CierreRecursos == 1 && s.CodigoRecorrido == r.CodigoRuta).Any()
                                            select r;

                    foreach (Entities.SOLProgramacionRuta rutaNoIntegrada in rutasNoIntegradas)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_RECURSO", string.Format("{0}|{1}|{2}", rutaNoIntegrada.CodigoDelegacion, rutaNoIntegrada.CodigoRuta, DateTime.Now.AddDays(offsetDias).ToString("dd/MM/yyyy")), "C", rutaNoIntegrada.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        // Con Error
                        if (jmsEnvioMensaje != null && (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError || jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado))
                        {
                            estadoIntegracionRecursosRuta.Add(new SOLIntegracionProgramacionDelegacionRuta()
                            {
                                Delegacion = new SOLDelegacion()
                                {
                                    Codigo = rutaNoIntegrada.CodigoDelegacion,
                                    Descripcion = rutaNoIntegrada.DescripcionDelegacion
                                },
                                Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Error,
                                Ruta = new SOLRuta()
                                {
                                    Codigo = rutaNoIntegrada.CodigoRuta
                                }
                            });

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                            {
                                if (mensajesIntegracionError == null)
                                    mensajesIntegracionError = new List<JMSEnvioMensaje>();

                                mensajesIntegracionError.Add(jmsEnvioMensaje);
                            }

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                            {
                                if (recorridosSIGIINoIntegrados == null)
                                    recorridosSIGIINoIntegrados = new List<SIGIIRecorrido>();

                                recorridosSIGIINoIntegrados.Add(new SIGIIRecorrido() { Recorrido = rutaNoIntegrada.CodigoRuta, CodigoDelegacion = rutaNoIntegrada.CodigoDelegacion });
                            }

                            continue;
                        }

                        // A Integrar
                        estadoIntegracionRecursosRuta.Add(new SOLIntegracionProgramacionDelegacionRuta()
                        {
                            Delegacion = new SOLDelegacion()
                            {
                                Codigo = rutaNoIntegrada.CodigoDelegacion,
                                Descripcion = rutaNoIntegrada.DescripcionDelegacion
                            },
                            Estado = SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrando,
                            Ruta = new SOLRuta()
                            {
                                Codigo = rutaNoIntegrada.CodigoRuta
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "offsetDias",  offsetDias.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionRuta>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIRecorrido>>(estadoIntegracionRecursosRuta, mensajesIntegracionError, recorridosSIGIINoIntegrados);

        }

        public Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionSector>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIGrupo>> EstadoProgramacionDeSectores(int offsetDias, string[] delegaciones)
        {
            List<SOLIntegracionProgramacionDelegacionSector> estadoIntegracionSectores = new List<SOLIntegracionProgramacionDelegacionSector>();
            List<JMSEnvioMensaje> mensajesIntegracionError = null;
            List<SIGIIGrupo> gruposSIGIINoIntegrados = null;

            try
            {
                DAO.SOLProgramacionSector daoSolProgramacionSector = new DAO.SOLProgramacionSector();

                IList<Entities.SOLProgramacionSector> solProgramacionesSector = daoSolProgramacionSector.GetEstadoCierre(DateTime.Now.AddDays(offsetDias), delegaciones);

                var sectoresPendientesSOL = from p in solProgramacionesSector
                                            where p.FechaCierre == null || p.FechaCierre == DateTime.MinValue
                                            select new { p.CodigoDelegacion, p.DescripcionDelegacion, p.CodigoSector };

                estadoIntegracionSectores.AddRange((from p in sectoresPendientesSOL
                                                    select new SOLIntegracionProgramacionDelegacionSector()
                                                    {
                                                        Delegacion = new SOLDelegacion()
                                                        {
                                                            Codigo = p.CodigoDelegacion,
                                                            Descripcion = p.DescripcionDelegacion
                                                        },
                                                        Estado = SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.ACerrar,
                                                        Sector = new SOLProgramacionSector()
                                                        {
                                                            CodigoSector = p.CodigoSector
                                                        }
                                                    }).ToList());

                var sectoresCerradosSOL = from p in solProgramacionesSector
                                          where p.FechaCierre != null && p.FechaCierre != DateTime.MinValue
                                          select p;

                if (sectoresCerradosSOL != null && sectoresCerradosSOL.Count() > 0)
                {
                    DAO.SIGIIProgramacionGrupo daoSIGIIProgramacionGrupo = new DAO.SIGIIProgramacionGrupo();

                    var sectoresCerradosConRecursos = (from s in sectoresCerradosSOL
                                                       where s.CantidadRecursos > 0
                                                       select s).ToArray();

                    IList<Entities.SIGIIProgramacionGrupo> sigIIProgramacionRecorridos = daoSIGIIProgramacionGrupo.GetEstadoCierres(DateTime.Now.AddDays(offsetDias), delegaciones, sectoresCerradosConRecursos.Select(s => s.CodigoSector).ToArray());

                    estadoIntegracionSectores.AddRange((from i in sigIIProgramacionRecorridos
                                                        let solSector = sectoresCerradosConRecursos.Where(r => r.CodigoDelegacion == i.CodigoDelegacion && r.CodigoSector == i.CodigoGrupo).First()
                                                        select new SOLIntegracionProgramacionDelegacionSector()
                                                        {
                                                            Delegacion = new SOLDelegacion()
                                                            {
                                                                Codigo = solSector.CodigoDelegacion,
                                                                Descripcion = solSector.DescripcionDelegacion
                                                            },
                                                            Estado = SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Integrado,
                                                            Sector = new SOLProgramacionSector()
                                                            {
                                                                CodigoSector = solSector.CodigoSector
                                                            }
                                                        }).ToList());

                    var gruposParaEvaluar = (from s in sectoresCerradosSOL
                                             let grupoSigII = sigIIProgramacionRecorridos.Where(g => g.CodigoDelegacion == s.CodigoDelegacion && g.CodigoGrupo == s.CodigoSector).FirstOrDefault()
                                             where s.CantidadRecursos == 0 || (s.CantidadRecursos != 0 && grupoSigII == null)
                                             select s).ToList();

                    foreach (SOLProgramacionSector grupoParaEvaluar in gruposParaEvaluar)
                    {
                        DAO.JMSEnvio daoJMSEnvio = new DAO.JMSEnvio();

                        JMSEnvioMensaje jmsEnvioMensaje = daoJMSEnvio.GetUltimoMensaje("PROGRAMACION_RECURSO", "S", string.Format("{0}|{1}|{2}", grupoParaEvaluar.CodigoDelegacion, grupoParaEvaluar.CodigoSector, DateTime.Now.AddDays(offsetDias).ToString("dd/MM/yyyy")), "C", grupoParaEvaluar.CodigoDelegacion, new string[] { Configuracion.CodigoEstadoError, Configuracion.CodigoEstadoProcesado });

                        if (jmsEnvioMensaje != null && (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError || jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado))
                        {
                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoProcesado)
                            {
                                if (grupoParaEvaluar.CantidadRecursos == 0)
                                {
                                    estadoIntegracionSectores.Add(new SOLIntegracionProgramacionDelegacionSector()
                                    {
                                        Delegacion = new SOLDelegacion()
                                        {
                                            Codigo = grupoParaEvaluar.CodigoDelegacion,
                                            Descripcion = grupoParaEvaluar.DescripcionDelegacion
                                        },
                                        Estado = SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Integrado,
                                        Sector = new SOLProgramacionSector()
                                        {
                                            CodigoSector = grupoParaEvaluar.CodigoSector
                                        }
                                    });
                                }
                                else
                                {
                                    estadoIntegracionSectores.Add(new SOLIntegracionProgramacionDelegacionSector()
                                    {
                                        Delegacion = new SOLDelegacion()
                                        {
                                            Codigo = grupoParaEvaluar.CodigoDelegacion,
                                            Descripcion = grupoParaEvaluar.DescripcionDelegacion
                                        },
                                        Estado = SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Error,
                                        Sector = new SOLProgramacionSector()
                                        {
                                            CodigoSector = grupoParaEvaluar.CodigoSector
                                        }
                                    });

                                    if (gruposSIGIINoIntegrados == null)
                                        gruposSIGIINoIntegrados = new List<SIGIIGrupo>();

                                    gruposSIGIINoIntegrados.Add(new SIGIIGrupo() { Codigo = grupoParaEvaluar.CodigoSector, CodigoDelegacion = grupoParaEvaluar.CodigoDelegacion });
                                }
                            }

                            if (jmsEnvioMensaje.Estado == Configuracion.CodigoEstadoError)
                            {
                                estadoIntegracionSectores.Add(new SOLIntegracionProgramacionDelegacionSector()
                                {
                                    Delegacion = new SOLDelegacion()
                                    {
                                        Codigo = grupoParaEvaluar.CodigoDelegacion,
                                        Descripcion = grupoParaEvaluar.DescripcionDelegacion
                                    },
                                    Estado = SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Error,
                                    Sector = new SOLProgramacionSector()
                                    {
                                        CodigoSector = grupoParaEvaluar.CodigoSector
                                    }
                                });

                                if (mensajesIntegracionError == null)
                                    mensajesIntegracionError = new List<JMSEnvioMensaje>();

                                mensajesIntegracionError.Add(jmsEnvioMensaje);
                            }

                            continue;
                        }

                        estadoIntegracionSectores.Add(new SOLIntegracionProgramacionDelegacionSector()
                        {
                            Delegacion = new SOLDelegacion()
                            {
                                Codigo = grupoParaEvaluar.CodigoDelegacion,
                                Descripcion = grupoParaEvaluar.DescripcionDelegacion
                            },
                            Estado = SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Integrando,
                            Sector = new SOLProgramacionSector()
                            {
                                CodigoSector = grupoParaEvaluar.CodigoSector
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(IntegracionSOLSIGII));

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "offsetDias",  offsetDias.ToString() },
                    { "delegaciones", string.Join(", ", delegaciones) }
                };

                string paramConcatenados = string.Join("\n", parameters.Select(p => string.Format(" \"{0}\" = {1} ", p.Key, p.Value)));
                log.ErrorFormat("Parametros: \n{0} \n Message: {1} \n Stack: {2}", paramConcatenados, ex.Message, ex.StackTrace);

                throw new ExcepcionBase(log4net.LogicalThreadContext.Properties["requestGUID"].ToString());
            }

            return new Tuple<IEnumerable<SOLIntegracionProgramacionDelegacionSector>, IEnumerable<JMSEnvioMensaje>, IEnumerable<SIGIIGrupo>>(estadoIntegracionSectores, mensajesIntegracionError, gruposSIGIINoIntegrados);
        }

        #endregion
    }
}
