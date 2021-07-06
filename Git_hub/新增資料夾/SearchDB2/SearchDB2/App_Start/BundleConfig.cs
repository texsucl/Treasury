using System.Web;
using System.Web.Optimization;

namespace SearchDB2
{
    public class BundleConfig
    {
        // 如需「搭配」的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.blockUI.js",
                        "~/Scripts/message.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryajax").Include(
                        "~/Scripts/jquery.unobtrusive-ajax*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryjqgrid").Include(
                        "~/Scripts/jquery.jqGrid*",
                        "~/Scripts/i18n/grid.locale-tw*",
                        "~/Scripts/customJqgrid.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/toastr").Include(
                        "~/Scripts/toastr.js"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-filestyle.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                      "~/Scripts/jquery-ui-1.10.0.js"));

            bundles.Add(new ScriptBundle("~/bundles/Customer").Include(
                      "~/Scripts/i18n/datepicker-zh-TW.js",
                      "~/Scripts/jquery.validate.min.js",
                      "~/Scripts/verification.js",
                      "~/Scripts/customerUtility.js"));

            bundles.Add(new ScriptBundle("~/bundles/Sidebar").Include(
                      "~/Scripts/Sidebar.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/master.css"
                      ));
        }
    }
}
