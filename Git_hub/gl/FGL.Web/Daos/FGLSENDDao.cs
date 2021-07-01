
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLSENDDao OMS報表發送對象維護檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLSENDDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        /// <summary>
        /// 查詢FGLSEND
        /// </summary>
        /// <param name="pgmId"></param>
        /// <param name="depFrom"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public List<FGLSEND0Model> qryByPgmDep(string pgmId, string depFrom, EacConnection con)
        {
            EacCommand cmd = new EacCommand();

            List<FGLSEND0Model> rows = new List<FGLSEND0Model>();

            string strSQL = "";
            try
            {
                cmd.Connection = con;
                strSQL += "SELECT PGM_ID, SEND_ID, DEP_FROM " +
                    " FROM FGLSEND0 " +
                    " WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(pgmId)))
                {
                    strSQL += " AND PGM_ID = :PGM_ID";
                    cmd.Parameters.Add("PGM_ID", pgmId);
                }

                if (!"".Equals(StringUtil.toString(depFrom)))
                {
                    strSQL += " AND DEP_FROM = :DEP_FROM";
                    cmd.Parameters.Add("DEP_FROM", depFrom);
                }


                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    FGLSEND0Model d = new FGLSEND0Model();
                    d.pgmId = result["PGM_ID"]?.ToString();
                    d.sendId = result["SEND_ID"]?.ToString();
                    d.depFrom = result["DEP_FROM"]?.ToString();

                    rows.Add(d);
                }


                cmd.Dispose();
                cmd = null;

                return rows;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


    }
}