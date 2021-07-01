using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// 修改歷程：20190819 Mark
///           需求單號：201907160360 
/// </summary>

namespace FRT.Web.Daos
{
    public class FRTXmlT622821Dao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        public List<ORTB012Model> qryForORTB012(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            DateTime sB = Convert.ToDateTime(actDateB);
            DateTime sE = Convert.ToDateTime(actDateE);
            sE = sE.AddDays(1);
            var rows = new List<ORTB012Model>();
            var _data = db.FRT_XML_T_622823.AsNoTracking().Where(x => x.CRT_TIME >= sB && x.CRT_TIME < sE).AsEnumerable()
                .Where(x => x.FAST_NO.CompareTo(fastNoB) >= 0 && x.FAST_NO.CompareTo(fastNoE) <= 0)
                .Select(y => new ORTB012Model()
                {
                    elecType = "XM96_T",
                    fastNo = y.FAST_NO,
                    actDate = y.ACT_DATE,
                    bank = y.RCV_BANK,
                    actNo = y.RCV_ACNO,
                    rmtAmt = (y.TX_AMT != null ? (Convert.ToInt64(y.TX_AMT) / 100).ToString() : string.Empty),
                    rcvName = y.RCV_NAME,
                    rmtApx = y.APX,
                    crtTime = y.CRT_TIME?.ToString("yyyy/MM/dd HH:mm:ss"),
                });
            if (_data.Any())
                rows.AddRange(_data);
            else {
                rows.AddRange(
                    db.FRT_XML_T_622821.AsNoTracking().Where(x => x.CRT_TIME >= sB && x.CRT_TIME < sE).AsEnumerable()
                    .Where(x => x.FAST_NO.CompareTo(fastNoB) >= 0 && x.FAST_NO.CompareTo(fastNoE) <= 0)
                    .Select(y => new ORTB012Model()
                    {
                        elecType = "XM96_T",
                        fastNo = y.FAST_NO,
                        actDate = y.ACT_DATE,
                        bank = y.RCV_BANK,
                        actNo = y.RCV_ACNO,
                        rmtAmt = (y.TX_AMT != null ? (Convert.ToInt64(y.TX_AMT) / 100).ToString() : string.Empty),
                        rcvName = y.RCV_NAME,
                        rmtApx = y.APX,
                        crtTime = y.CRT_TIME?.ToString("yyyy/MM/dd HH:mm:ss"),
                    //SqlFunctions.DateName("year", y.CRT_TIME) + "/" +
                    //          SqlFunctions.DatePart("m", y.CRT_TIME) + "/" +
                    //          SqlFunctions.DateName("day", y.CRT_TIME).Trim() + " " +
                    //          SqlFunctions.DateName("hh", y.CRT_TIME).Trim() + ":" +
                    //          SqlFunctions.DateName("n", y.CRT_TIME).Trim() + ":" +
                    //          SqlFunctions.DateName("s", y.CRT_TIME).Trim()
                }));
            }
            return rows;

        }
    }
}