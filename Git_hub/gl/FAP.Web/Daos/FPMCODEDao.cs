

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Web.Mvc;

/// <summary>
/// FPMCODE   各類相關代碼檔
/// </summary>
namespace FAP.Web.Daos
{
    public class FPMCODEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<string, string> qryByTypeDic(string groupId, string srceFrom, bool bPreCode)
        {
            Dictionary<string, string> codeMap = new Dictionary<string, string>();
            var codeList = qryGrpList(groupId, srceFrom);

            foreach (var item in codeList)
            {
                codeMap.Add(item.Value.Trim(), item.Text.Trim());

            }


            return codeMap;
        }

        public SelectList qryGrpList( string groupId, string srceFrom)
        {
            logger.Info("qryGrpList begin!");

            var dataList = new List<SelectListItem>();
            EacConnection con = new EacConnection();
            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT GROUP_ID, REF_NO, TEXT
  FROM LPMCODE2 
    WHERE GROUP_ID = :GROUP_ID 
         AND SRCE_FROM = :SRCE_FROM";

            try
            {
                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();
                    cmdQ.Connection = conn400;
                    cmdQ.CommandText = strSQLQ;

                    cmdQ.Parameters.Clear();

                    cmdQ.Parameters.Add("GROUP_ID", groupId);
                    cmdQ.Parameters.Add("SRCE_FROM", srceFrom);

                    DbDataReader result = cmdQ.ExecuteReader();



                    while (result.Read())
                    {
                        dataList.Add(new SelectListItem()
                        {
                            Text = result["REF_NO"]?.ToString() + "." + result["TEXT"]?.ToString(),
                            Value = result["REF_NO"]?.ToString()
                        });


                    }

                    cmdQ.Dispose();
                    cmdQ = null;
                }


                

                logger.Info("qryGrpList end!");
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }



            return new SelectList(dataList, "Value", "Text");

        }

        /// <summary>
        /// for GRID下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public string jqGridList(string groupId, string srceFrom, bool bPreCode)
        {
            var codeList = qryGrpList(groupId, srceFrom);
            string controlStr = "";
            foreach (var item in codeList)
            {
                controlStr += item.Value.Trim() + ":" + item.Text.Trim() + ";";
            }
            controlStr = controlStr.Substring(0, controlStr.Length - 1) + "";
            return controlStr;
        }



        /// <summary>
        /// 查詢FPMCODE
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <param name="refNo"></param>
        /// <returns></returns>
        public List<FPMCODEModel> qryFPMCODE(string groupId, string srceFrom, string refNo)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FPMCODEModel> rows = new List<FPMCODEModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT GROUP_ID, SRCE_FROM, REF_NO, TEXT " +
                    " FROM FPMCODE0 " +
                    " WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(groupId)))
                {
                    strSQL += " AND GROUP_ID = :GROUP_ID";
                    cmd.Parameters.Add("GROUP_ID", groupId);
                }

                if (!"".Equals(StringUtil.toString(srceFrom)))
                {
                    strSQL += " AND SRCE_FROM = :SRCE_FROM";
                    cmd.Parameters.Add("SRCE_FROM", srceFrom);
                }

                if (!"".Equals(StringUtil.toString(refNo)))
                {
                    strSQL += " AND REF_NO = :REF_NO";
                    cmd.Parameters.Add("REF_NO", refNo);
                }

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int groupIdId = result.GetOrdinal("GROUP_ID");
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int refNoId = result.GetOrdinal("REF_NO");
                int textId = result.GetOrdinal("TEXT");

                while (result.Read())
                {
                    FPMCODEModel d = new FPMCODEModel();
                    d.groupId = StringUtil.toString(result.GetString(groupIdId));
                    d.srce_from = StringUtil.toString(result.GetString(srceFromId));
                    d.refNo = StringUtil.toString(result.GetString(refNoId));
                    d.text = StringUtil.toString(result.GetString(textId));
                    rows.Add(d);
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

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