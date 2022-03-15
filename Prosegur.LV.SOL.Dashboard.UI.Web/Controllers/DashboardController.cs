using System.Web.Mvc;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public ActionResult Route()
        {
            return View();
        }

        public ActionResult Programmation()
        {
            return View();
        }

        public ActionResult ProgrammationBar()
        {
            return View();
        }

        public ActionResult CierresFacturacionBar()
        {
            return View();
        }
    }
}
