using System.Web;
using System.Web.Optimization;

namespace NetStarter
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap-datepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/popper.min.js",
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/dataTables.min.js",
                      "~/Scripts/dataTables.bootstrap5.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/otherscripts").Include(
                      "~/Scripts/main.js"));

            bundles.Add(new Bundle("~/bundles/filetablechart").Include(
                      "~/Scripts/filevalidation.js",
                      "~/Scripts/chart.js/chart.min.js",
                      "~/Scripts/jquery.dataTables.min.js",
                      "~/Scripts/dataTables.buttons.min.js",
                      "~/Scripts/jszip.min.js",
                      "~/Scripts/pdfmake.min.js",
                      "~/Scripts/vfs_fonts.js",
                      "~/Scripts/buttons.html5.min.js",
                      "~/Scripts/buttons.print.min.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                 "~/Content/dataTables.min.css",
                      "~/Content/dataTables.bootstrap5.min.css",
                      "~/Content/bootstrap.min.css",
                      "~/Content/bootstrap-datepicker.min.css",
                      "~/Content/all.min.css",
                      "~/Content/style.css"));
        }
    }
}
