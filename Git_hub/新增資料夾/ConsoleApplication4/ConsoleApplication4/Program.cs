using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.ServiceProcess;

namespace ConsoleApplication4
{
    class Program
    {
        static void Main(string[] args)
        {
            //set up domain context
            //PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            //Console.WriteLine(ctx.ConnectedServer);

            //string str = null;
            //str.Replace("", "");

            //var s = new ServiceController("FRTXmlServiceName", ".");
            //var status = string.Empty;
            //if (s.Status == ServiceControllerStatus.Stopped)
            //{

            //    status =  " 服務未啟動。";
            //}
            //else if (s.Status == ServiceControllerStatus.Running)
            //{
            //    status = " 服務正在執行。";
            //}
            //else
            //{
            //    status = " 服務未確認狀態。";
            //}
            //Console.WriteLine($@"FRTXmlServiceName{status}");

            //List<int> i = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };

            //foreach (var item in i)
            //{
            //    Console.WriteLine(item);
            //    if (item == 4)
            //        break;
            //}
            DateTime dtn = DateTime.Now;
            DateTime dtnd = dtn.AddDays(-1).Date;
            Console.WriteLine(dtnd);
            Console.ReadLine();

        }
    }
}
