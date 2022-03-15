using System.Web.Mvc;
using log4net;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            ILog log = LogManager.GetLogger(typeof(HomeController));

            string userName = ((Prosegur.LV.SOL.Dashboard.UI.Web.Models.UserData)User).UserName;

            log.Info(string.Format("El usuario \"{0}\" visito la pagina \"{1}\"", userName, Request.Url.ToString()));

            return View();
        }
    }
}
