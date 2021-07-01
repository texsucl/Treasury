
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLVRIDDao 新增人員設定檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLVRIDDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        /// <summary>
        /// 查詢FPMCODE
        /// </summary>
        /// <param name="bankNo"></param>
        /// <returns></returns>
        public List<FGLVRID0Model> qryByKey(string flowType, string sysType, string srceFrom, EacConnection con)
        {
            EacCommand cmd = new EacCommand();

            List<FGLVRID0Model> rows = new List<FGLVRID0Model>();

            string strSQL = "";
            try
            {
                cmd.Connection = con;
                strSQL += "SELECT FLOW_TYPE, SYS_TYPE, SRCE_FROM, ENTRY_ID " +
                    " FROM LGLVRID1 " +
                    " WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(flowType)))
                {
                    strSQL += " AND FLOW_TYPE = :FLOW_TYPE";
                    cmd.Parameters.Add("FLOW_TYPE", flowType);
                }

                if (!"".Equals(StringUtil.toString(sysType)))
                {
                    strSQL += " AND SYS_TYPE = :SYS_TYPE";
                    cmd.Parameters.Add("SYS_TYPE", sysType);
                }

                if (!"".Equals(StringUtil.toString(srceFrom)))
                {
                    strSQL += " AND SRCE_FROM = :SRCE_FROM";
                    cmd.Parameters.Add("SRCE_FROM", srceFrom);
                }

                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    FGLVRID0Model d = new FGLVRID0Model();
                    d.flowType = result["FLOW_TYPE"]?.ToString();
                    d.sysType = result["SYS_TYPE"]?.ToString();
                    d.srceFrom = result["SRCE_FROM"]?.ToString();
                    d.entryId = result["ENTRY_ID"]?.ToString();
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