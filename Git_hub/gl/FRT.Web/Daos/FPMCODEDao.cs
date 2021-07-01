using FRT.Web.AS400Models;
using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRT.Web.Daos
{
    public class FPMCODEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查詢FPMCODE(For jqgrid使用)
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <param name="refNo"></param>
        /// <returns></returns>
        public string jqGridList(string groupId, string srceFrom, string refNo, bool bPreCode)
        {
            var codeList = qryFPMCODE(groupId, srceFrom, refNo);
            string controlStr = "";
            foreach (var item in codeList)
            {
                if(bPreCode)
                    controlStr += item.refNo.Trim() + ":" + item.refNo.Trim() + "." +item.text.Trim() + ";";
                else
                    controlStr += item.refNo.Trim() + ":" + item.text.Trim() + ";";
            }
            controlStr = controlStr.Substring(0, controlStr.Length - 1) + "";
            return controlStr;
        }


        /// <summary>
        /// add by daiyu 20191210
        /// 將AS400 PMCODE的代碼資料取出來供畫面下拉選單使用
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <param name="refNo"></param>
        /// <param name="bPreCode"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string groupId, string srceFrom, string refNo, bool bPreCode)
        {
            var codeList = qryFPMCODE(groupId, srceFrom, refNo);
            var list = new List<listModel>();

            string controlStr = "";

            foreach (var item in codeList)
            {
                controlStr = "";
                if (bPreCode)
                    controlStr += item.refNo.Trim() + "." + item.text.Trim();
                else
                    controlStr += item.text.Trim();

                listModel d = new listModel();
                d.code_value = item.refNo;
                d.code_text = controlStr;
                list.Add(d);

            }

            var items = new SelectList
                (
                items: list,
                dataValueField: "code_value",
                dataTextField: "code_text",
                selectedValue: (object)null
                );

            return items;
        }


        /// <summary>
        /// 查詢FPMCODE
        /// </summary>
        /// <param name="bankNo"></param>
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


        public class listModel
        {
            public string code_value { get; set; }

            public string code_text { get; set; }

  
        }
    }
}