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
    public class FRTXmlTEachDao
    {

        public List<ORTB012Model> qryForORTB012(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            DateTime sB = Convert.ToDateTime(actDateB);
            DateTime sE = Convert.ToDateTime(actDateE);
            sE = sE.AddDays(1);
            var rows = new List<ORTB012Model>();
            rows.AddRange(db.FRT_XML_T_eACH.AsNoTracking().Where(
                x => x.CRT_TIME >= sB && x.CRT_TIME < sE).AsEnumerable()
                .Where(x => x.FAST_NO.CompareTo(fastNoB) >= 0 && x.FAST_NO.CompareTo(fastNoE) <= 0)
                .Select(y => new ORTB012Model()
                {
                    elecType = "EACH_T",
                    fastNo = y.FAST_NO,
                    bank = y.TXNINBANK,
                    actNo = y.TRANSFERINACTNO,
                    rmtAmt = y.TXNAMT,
                    rmtApx = y.MEMO,
                    crtTime = y.CRT_TIME?.ToString("yyyy/MM/dd HH:mm:ss"),
                    //SqlFunctions.DateName("year", y.CRT_TIME) + "/" +
                    //                  SqlFunctions.DatePart("m", y.CRT_TIME) + "/" +
                    //                  SqlFunctions.DateName("day", y.CRT_TIME).Trim() + " " +
                    //                  SqlFunctions.DateName("hh", y.CRT_TIME).Trim() + ":" +
                    //                  SqlFunctions.DateName("n", y.CRT_TIME).Trim() + ":" +
                    //                  SqlFunctions.DateName("s", y.CRT_TIME).Trim()
                }));
            return rows;

        }
    }
}