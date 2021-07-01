using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTXmlR622821Dao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        public List<ORTB012Model> qryForORTB012(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            DateTime sB = Convert.ToDateTime(actDateB);
            DateTime sE = Convert.ToDateTime(actDateE);
            sE = sE.AddDays(1);
            var rows = new List<ORTB012Model>();
            var _data = db.FRT_XML_R_622823.AsNoTracking().Where(
                x => x.CRT_TIME >= sB && x.CRT_TIME < sE).AsEnumerable()
                .Where(x => x.FAST_NO.CompareTo(fastNoB) >= 0 && x.FAST_NO.CompareTo(fastNoE) <= 0)
                .Select(y => new ORTB012Model()
                {
                    elecType = "XM96_R",
                    fastNo = y.FAST_NO,
                    actDate = y.ACT_DATE,
                    rmtAmt = (y.TX_AMT != null ? (Convert.ToInt64(y.TX_AMT) / 100).ToString() : string.Empty),
                    errorCode = y.RC_CODE,
                    emsgId = y.EMSGID,
                    emsgTxt = y.EMSGTXT,
                    crtTime = y.CRT_TIME?.ToString("yyyy/MM/dd HH:mm:ss"),
                });
            if (_data.Any())
                rows.AddRange(_data);
            else
            {
                rows.AddRange(db.FRT_XML_R_622821.AsNoTracking().Where(
                x => x.CRT_TIME >= sB && x.CRT_TIME < sE).AsEnumerable()
                .Where(x => x.FAST_NO.CompareTo(fastNoB) >= 0 && x.FAST_NO.CompareTo(fastNoE) <= 0)
                .Select(y => new ORTB012Model()
                {
                    elecType = "XM96_R",
                    fastNo = y.FAST_NO,
                    actDate = y.ACT_DATE,
                    rmtAmt = (y.TX_AMT != null ? (Convert.ToInt64(y.TX_AMT) / 100).ToString() : string.Empty),
                    errorCode = y.RC_CODE,
                    emsgId = y.EMSGID,
                    emsgTxt = y.EMSGTXT,
                    crtTime = y.CRT_TIME?.ToString("yyyy/MM/dd HH:mm:ss"),
                     //SqlFunctions.DateName("year", y.CRT_TIME) + "/" +
                     //                  SqlFunctions.DatePart("m", y.CRT_TIME) + "/" +
                     //                  SqlFunctions.DateName("day", y.CRT_TIME).Trim() + " " +
                     //                  SqlFunctions.DateName("hh", y.CRT_TIME).Trim() + ":" +
                     //                  SqlFunctions.DateName("n", y.CRT_TIME).Trim() + ":" +
                     //                  SqlFunctions.DateName("s", y.CRT_TIME).Trim()
                 }));
            }
            return rows;
        }

        public ORTB012Model qryForORTB009(string fastNo)
        {
            ORTB012Model model = new ORTB012Model();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FRT_XML_R_622823 d3 = db.FRT_XML_R_622823.AsNoTracking().Where(x => x.FAST_NO == fastNo)
                    .OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                if (d3 != null)
                {
                    model.fastNo = d3.FAST_NO;
                    model.errorCode = d3.RC_CODE;
                    model.emsgId = d3.EMSGID;
                }
                else
                {
                    FRT_XML_R_622821 d = db.FRT_XML_R_622821.AsNoTracking().Where(x => x.FAST_NO == fastNo)
                        .OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                    if (d != null)
                    {
                        model.fastNo = d.FAST_NO;
                        model.errorCode = d.RC_CODE;
                        model.emsgId = d.EMSGID;
                    }
                }                
                return model;
            }            
        }

        public string qryForErrBelong(string fastNo)
        {
            var result = string.Empty;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _model = db.FRT_XML_R_622823.AsNoTracking().Where(x => x.FAST_NO == fastNo)
                    .OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                if (_model != null)
                {
                   result = new FRTBERMDao().qryByErrCode(_model.RC_CODE).errBelong;
                }
            }
            return result;
        }
    }
}