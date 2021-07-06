using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using System.Net.NetworkInformation;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                var a = Request.UserHostAddress;
                //Request.
                //string computer_name = System.Net.Dns.GetHostEntry(a).HostName;
                //var n = new IPAddress();
                //IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                //string computer_name = properties.HostName + "." + properties.DomainName;
                //var q = Dns.GetHostByAddress(a);
                var w = Dns.GetHostByName(a);
                var e = Dns.Resolve(a);
                string computer_name = System.Net.Dns.GetHostEntry(IPAddress.Parse(a)).HostName;
                //string computer_name = Dns.GetHostByName(a).HostName;
                //IPAddress ipAddress = IPAddress.Parse(a);
                //var server = new TcpClient();
                //server.Client.Connect(a, Request.Url.Port);
                //var ns = server.GetStream();

                ViewBag.Message = "HostName : " + computer_name;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}