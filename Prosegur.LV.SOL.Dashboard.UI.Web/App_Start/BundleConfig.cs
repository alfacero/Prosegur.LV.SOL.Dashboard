using System.Web.Optimization;

namespace Prosegur.LV.SOL.Dashboard.UI.Web
{
    public class BundleConfig
    {
        // Para obtener más información acerca de Bundling, consulte http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.2.1.js",
                        "~/Scripts/jquery-ui-1.12.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/popper.js" ,
                "~/Scripts/bootstrap.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.config.js"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información sobre los formularios. De este modo, estará
            // preparado para la producción y podrá utilizar la herramienta de creación disponible en http://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/customScrollBar").Include(
                "~/Scripts/jquery.mCustomScrollbar.concat.js"));

            //ATENCION: los archivos del Bundle fueron descargados de https://code.highcharts.com/#stylecss, de
            //la seccion de scripts donde el estilo es establecido por JS

            bundles.Add(new ScriptBundle("~/bundles/highCharts").Include(
                "~/Scripts/highcharts/highstock.js",
                "~/Scripts/highcharts/exporting.js",
                "~/Scripts/highcharts/accessibility.js"
                ));
            //                "~/Scripts/highcharts/highcharts.js",

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery-ui-1.12.1.css",
                      "~/Content/open-iconic-bootstrap.css",
                      "~/Content/site.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/jqPlot").Include(
                      "~/Content/jquery.jqplot.css"));

            bundles.Add(new StyleBundle("~/Content/signin").Include(
                      "~/Content/signin.css"));

            bundles.Add(new StyleBundle("~/Content/customScrollBar").Include(
                        "~/Content/jquery.mCustomScrollbar.css"));

            bundles.Add(new StyleBundle("~/Content/highCharts").Include(
                        "~/Content/highcharts.css"));

        }
    }
}