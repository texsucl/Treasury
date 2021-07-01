using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTXmlR622685Dao
    {
        public Tuple<FRT_XML_R_622685_NEW, FRT_XML_R_622685,int> qryLstByFastNo(string fastNo)
        {
            using (dbFGLEntities db = new dbFGLEntities()) {
                FRT_XML_R_622685_NEW data_new = db.FRT_XML_R_622685_NEW.AsNoTracking()
                    .Where(x => x.FAST_NO == fastNo)
                    .OrderByDescending(x=>x.CRT_TIME)
                    .FirstOrDefault();
                if (data_new == null)
                {
                    //FRT_XML_R_622685 data = (from m in db.FRT_XML_R_622685
                    //                         where m.FAST_NO == fastNo
                    //                         orderby m.CRT_TIME descending
                    //                         select m).FirstOrDefault();
                    //if (data == null)
                    return new Tuple<FRT_XML_R_622685_NEW, FRT_XML_R_622685, int>(null, null, 3);
                    //return new Tuple<FRT_XML_R_622685_NEW, FRT_XML_R_622685, int>(null, data, 2);
                }
                return new Tuple<FRT_XML_R_622685_NEW, FRT_XML_R_622685, int>(data_new,null,1);
            }
        }


        public List<ORTB012Model> qryForORTB012(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            DateTime sB = Convert.ToDateTime(actDateB);
            DateTime sE = Convert.ToDateTime(actDateE);
            sE = sE.AddDays(1);

            var rows = new List<ORTB012Model>();
            var _data = (from main in db.FRT_XML_R_622685_NEW
                         join memo in db.FRT_XML_522657 on main.FAST_NO equals memo.FAST_NO into psMemo
                         from xMemo in psMemo.DefaultIfEmpty()
                         where 1 == 1
                             & main.CRT_TIME >= sB & main.CRT_TIME < sE
                             & main.FAST_NO.CompareTo(fastNoB) >= 0 & main.FAST_NO.CompareTo(fastNoE) <= 0
                         orderby main.CRT_TIME descending
                         select new
                         {
                             fastNo = main.FAST_NO,
                             actNo = xMemo.RCV_ACT_NO,
                             rmtAmt = xMemo.RMT_AMT,
                             rcvName = xMemo.RCV_CUST_NAME,
                             funCode = main.FunCode,
                             status = main.Status,
                             errorCode = main.ucaErrorCod,
                             rmtApx = xMemo.RMT_APX,
                             crtTime = main.CRT_TIME,
                             updTime = main.UPD_TIME
                         }).AsEnumerable()
                            .Select(x => new ORTB012Model()
                            {
                                elecType = "622685",
                                fastNo = x.fastNo?.ToString(),
                                actNo = x.actNo,
                                rmtAmt = x.rmtAmt,
                                rcvName = x.rcvName,
                                funCode = x.funCode,
                                status = x.status,
                                errorCode = x.errorCode,
                                rmtApx = x.rmtApx,
                                crtTime = x.crtTime.ToString("yyyy/MM/dd HH:mm:ss"),
                                updTime = x.updTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                                _crtTime = x.crtTime
                            }).ToList();
            if (_data.Any())
            {
                foreach (var item in _data.GroupBy(x => x.fastNo))
                {
                    rows.Add(item.OrderByDescending(x=>x.funCode).ThenByDescending(x=>x._crtTime).First());
                }
            }               
            else
            {
                rows.AddRange((from main in db.FRT_XML_R_622685
                               join memo in db.FRT_XML_522657 on main.FAST_NO equals memo.FAST_NO into psMemo
                               from xMemo in psMemo.DefaultIfEmpty()
                               where 1 == 1
                                   & main.CRT_TIME >= sB & main.CRT_TIME < sE
                                   & main.FAST_NO.CompareTo(fastNoB) >= 0 & main.FAST_NO.CompareTo(fastNoE) <= 0
                               select new
                               {
                                   fastNo = main.FAST_NO,
                                   actNo = xMemo.RCV_ACT_NO,
                                   rmtAmt = xMemo.RMT_AMT,
                                   rcvName = xMemo.RCV_CUST_NAME,
                                   funCode = main.RMT_TYPE,
                                   status = main.STATUS,
                                   errorCode = main.ERROR_COD,
                                   rmtApx = xMemo.RMT_APX,
                                   crtTime = main.CRT_TIME,
                                   updTime = main.UPD_TIME
                               })
                .AsEnumerable()
                .Select(x => new ORTB012Model()
                {
                    elecType = "622685",
                    fastNo = x.fastNo,
                    actNo = x.actNo,
                    rmtAmt = x.rmtAmt,
                    rcvName = x.rcvName,
                    funCode = x.funCode,
                    status = x.status,
                    errorCode = x.errorCode,
                    rmtApx = x.rmtApx,
                    crtTime = x.crtTime.ToString("yyyyMMdd HH:mm:ss"),
                    updTime = x.updTime?.ToString("yyyyMMdd HH:mm:ss")
                }));
            }

            return rows;

        }


    }
}