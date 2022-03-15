using Prosegur.LV.SOL.Dashboard.Core;
using Prosegur.LV.SOL.Dashboard.Entities;
using Prosegur.LV.SOL.Dashboard.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.Immutable;
using System.IO;
using System.IO.Log;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Usuario usuarioCore = new Usuario();
            IList<SOLUsuario> usuarios = usuarioCore.GetUsuarioConRoles("ar00012776");

            //IList<SOLDelegacion> delegaciones;
            //if (usuarios != null && usuarios.Count() >= 1)
            //{
            //    delegaciones = usuarioCore.GetUsuarioDelegaciones(usuarios[0].OidUsuario);
            //}

            //            ColaSOLJmsEnvio jms = new ColaSOLJmsEnvio();
            //            IList<SOLJmsEnvioMensaje> mensajes = jms.GetMensajes(40, new string[] { "03", "04", "05", "95", "19",
            //"31",
            //"23",
            //"32",
            //"21",
            //"22",
            //"20" }, "OT", "PE");

            //ColaSOLJmsEnvio jms = new ColaSOLJmsEnvio();
            //IList<SOLJmsEnvioMensaje> mensajes = jms.GetUltimosMensajesError(10, new string[] { "03", "04", "05", }, "OT", "ER");

            //IntegracionProgramacionSOL_SIGII integracion = new IntegracionProgramacionSOL_SIGII();
            //IList<SOLOrdenTrabajo> ots = integracion.GetDetalleOrdenDeTrabajo(mensajes.Select(m => m.Id).ToArray());

            //Prosegur.LV.SOL.Dashboard.Core.SOLOrdenDeTrabajo ot = new Prosegur.LV.SOL.Dashboard.Core.SOLOrdenDeTrabajo();

            //IList<Prosegur.LV.SOL.Dashboard.Entities.JMSEnvioMensaje> listadoOTs = ot.GetMensajesIntegracion(new string[] { "03", "04", "05", }, 10);

            //Prosegur.LV.SOL.Dashboard.DAO.SOLProgramacionSector solProgramacionSectorDao = new Prosegur.LV.SOL.Dashboard.DAO.SOLProgramacionSector();

            //IList<SOLProgramacionSector> solProgramacionSectores = solProgramacionSectorDao.GetEstadoCierre(DateTime.Now.AddDays(120), new string[] { "94", "07" });

            //Prosegur.LV.SOL.Dashboard.DAO.SIGIIPlanta sigIIPlantaDAO = new Prosegur.LV.SOL.Dashboard.DAO.SIGIIPlanta();
            //IList<Prosegur.LV.SOL.Dashboard.Entities.SIGIIPlanta> plantas = sigIIPlantaDAO.GetPlantaConBasurero(new string[] { "02" });

            //Prosegur.LV.SOL.Dashboard.DAO.SIGIIHojaDeRuta sigIIHojaDeRutaDAO = new Prosegur.LV.SOL.Dashboard.DAO.SIGIIHojaDeRuta();
            //IList<Prosegur.LV.SOL.Dashboard.Entities.SIGIIHojaDeRuta> hojasDeRuta = sigIIHojaDeRutaDAO.GetHojasDeRuta(new int[] { 1, 2, 3 }, DateTime.Now);

            //Prosegur.LV.SOL.Dashboard.DAO.SOLOrdenDeTrabajo solOrdenDeTrabajoDAO = new Prosegur.LV.SOL.Dashboard.DAO.SOLOrdenDeTrabajo();
            //IList<Prosegur.LV.SOL.Dashboard.Entities.SOLOrdenDeTrabajo> ordenesDeTrabajo = solOrdenDeTrabajoDAO.GetOrdenesDeTrabajoAIntegrar(new string[] { "01" }, DateTime.Now, new int[] { 0 });

            //Prosegur.LV.SOL.Dashboard.DAO.JMSEnvio jms = new Prosegur.LV.SOL.Dashboard.DAO.JMSEnvio();
            //IList<JMSEnvioMensaje> mensajes = jms.GetMensajes(new string[] { "01" }, "OT", new string[] { "ER" }, new string[] { "c51933d4-4a5c-11e8-a448-b36bf2b79120",
            //"600d7c4d-3f20-11e8-8ba8-c1cf58d15259" } , "A");

            //LogSample.Main2(null);

            //LogBackup.ArchiveToXML(new LogStore("Test2.log", FileMode.CreateNew), "Test3.log");


        }

        //static async Task SomeWork(string stackName)
        //{
        //    using (MyStack.Push(stackName))
        //    {
        //        Log("<SomeWork>");
        //        await MoreWork("A");
        //        await MoreWork("B");
        //        Log("</SomeWork>");
        //    }
        //}

        //static async Task MoreWork(string stackName)
        //{
        //    using (MyStack.Push(stackName))
        //    {
        //        Log("<MoreWork>");
        //        await Task.Delay(10);
        //        Log("</MoreWork>");
        //    }
        //}

        static void Log(string message)
        {
            //Console.WriteLine(MyStack.CurrentStack + ": " + message);
        }

        //public partial class MyStack
        //{
        //    private static readonly string name = Guid.NewGuid().ToString("N");

        //    private sealed class Wrapper : MarshalByRefObject
        //    {
        //        public ImmutableStack<string> Value { get; set; }
        //    }

        //    private static ImmutableStack<string> CurrentContext
        //    {
        //        get
        //        {
        //            var ret = CallContext.LogicalGetData(name) as Wrapper;
        //            return ret == null ? ImmutableStack.Create<string>() : ret.Value;
        //        }

        //        set
        //        {
        //            CallContext.LogicalSetData(name, new Wrapper { Value = value });
        //        }
        //    }

        //    public static IDisposable Push([CallerMemberName] string context = "")
        //    {
        //        CurrentContext = CurrentContext.Push(context);
        //        return new PopWhenDisposed();
        //    }

        //    private static void Pop()
        //    {
        //        CurrentContext = CurrentContext.Pop();
        //    }

        //    private sealed class PopWhenDisposed : IDisposable
        //    {
        //        private bool disposed;

        //        public void Dispose()
        //        {
        //            if (disposed)
        //                return;
        //            Pop();
        //            disposed = true;
        //        }
        //    }

        //    public static string CurrentStack
        //    {
        //        get
        //        {
        //            return string.Join(" ", CurrentContext.Reverse());
        //        }
        //    }
        //}
    }
}
