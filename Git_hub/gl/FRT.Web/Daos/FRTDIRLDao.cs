using FRT.Web.AS400Models;
using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;


/// <summary>
/// 需求單號：201905310551-06
/// 修改日期：20191108
/// 修改人員：B0077
/// 修改項目：通知內容的"幣別"、"行庫代號"、"錯誤原因"從檔案的對應欄位取得
/// </summary>
namespace FRT.Web.Daos
{
    public class FRTDIRLDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// "保單匯款基本資料維護錯誤通知"查詢
        /// </summary>
        /// <param name="bankNo"></param>
        /// <returns></returns>
        public List<FRTDIRLModel> qryRemitInfoErr()
        {
            logger.Info("qryRemitInfoErr begin!!");
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FRTDIRLModel> dataList = new List<FRTDIRLModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT UPD_DATE, UPD_ID, SRCE_FROM, POLICY_NO, BANK_ACT, PAYMENT " +
                    " ,CURRENCY, BANK_NO, PAYF_ERRTX, RESOURCE" + //add by daiyu 20191108
                    " FROM LRTDIRL3 " +
                    " WHERE UPD_DATE = :UPD_DATE " +
                    " AND TYPE = '1' " +
                    " AND SRCEPGM = 'BRTNBCK' ";


                String strFormat = "yyyyMMdd";
                DateTime updDate = DateTime.Now.AddDays(-1);

                String strDateTime = updDate.ToString(strFormat);
                strDateTime = (updDate.Year - 1911).ToString().PadLeft(4, '0') + strDateTime.Substring(4, strDateTime.Length - 4);

                cmd.Parameters.Add("UPD_DATE", strDateTime);

                logger.Info("strSQ:" + strSQL);
                logger.Info("UPD_DATE:" + strDateTime);

                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    FRTDIRLModel d = new FRTDIRLModel();
                    d.updDate = result["UPD_DATE"]?.ToString();
                    d.updId = result["UPD_ID"]?.ToString();
                    d.resource = result["RESOURCE"]?.ToString();
                    d.policyNo = result["POLICY_NO"]?.ToString();
                    d.bankAct = result["BANK_ACT"]?.ToString();
                    d.payment = result["PAYMENT"]?.ToString();

                    /*---add by daiyu 20191108---*/
                    d.currency = result["CURRENCY"]?.ToString();    
                    d.bankNo = result["BANK_NO"]?.ToString();
                    d.payfErrtx = result["PAYF_ERRTX"]?.ToString();
                    /*---end add 20191108---*/

                    dataList.Add(d);
                }

                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return dataList;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

    }
}