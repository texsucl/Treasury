
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FRTRMREDao 繳款資料明細檔
/// -----------------------------------------------
/// 修改歷程：20191205 Daiyu
/// 需求單號：201905230393-09
/// 修改內容：
///     1.設定檔設定
///        備用欄位10=Y-->抓”備用欄位10”不為空的
///        非上述-->不判斷”備用欄位10”
///     2.修改"公司別"的判斷方式
/// </summary>
namespace FGL.Web.Daos
{
    public class FRTRMREDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// For BGL0009 未接收報表(給付流程優化)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rows"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<BGL00009Model> qryForBGL0009(List<BGL00009Model> rows, List<FPMCODEModel> conList
            , EacConnection conn, DateTime end_date)
        {
            logger.Info("qryForBGL0009 begin!");
            int _ENTRY_YY_B, _ENTRY_MM_B, _ENTRY_YY_E, _ENTRY_MM_E, _ENTRY_DD_E = 0;

            _ENTRY_YY_B = end_date.AddMonths(-1).Year - 1911;
            _ENTRY_MM_B = end_date.AddMonths(-1).Month;

            _ENTRY_YY_E = end_date.Year - 1911;
            _ENTRY_MM_E = end_date.Month;
            _ENTRY_DD_E = end_date.Day;

            //動態組USER自訂的條件
            string strWhere = "";
            foreach (FPMCODEModel condition in conList) {
                string refNo = condition.refNo.PadRight(5, ' ');
                if (!"".Equals(StringUtil.toString(refNo))) {
                    if (refNo.StartsWith("XXXX"))   //遇到"XXXX"不用判斷
                        continue;


                    if(!"".Equals(strWhere))
                        strWhere += " OR ( 1 = 1 ";
                    else
                        strWhere += " ( 1 = 1 ";

                    //公司別
                    if ("1".Equals(refNo.Substring(0, 1)))
                        strWhere += " AND SUBSTR(RMRE.FILLER_20,2,1) <> 'O' ";
                    else
                        strWhere += " AND SUBSTR(RMRE.FILLER_20,2,1)= 'O' ";

                    //繳款原因
                    if (!"".Equals(StringUtil.toString(refNo.Substring(1, 1))))
                        strWhere += " AND RMRE.RM_RESN = '" + refNo.Substring(1, 1) + "' ";

                    //繳款類別
                    if (!"".Equals(StringUtil.toString(refNo.Substring(2, 1))))
                        strWhere += " AND RMRE.RM_TYPE = '" + refNo.Substring(2, 1)  + "' ";

                    //給付方式
                    if (!"".Equals(StringUtil.toString(refNo.Substring(3, 1))))
                        strWhere += " AND RMRE.REMIT_KIND = '" + refNo.Substring(3, 1) + "' ";

                    //銷帳註記  modify by daiyu 20191205
                    if ("Y".Equals(StringUtil.toString(refNo.Substring(4, 1))))
                        strWhere += " AND RMRE.FILLER_10 <> '' ";

                    strWhere += " )";
                }
            }



            EacCommand cmd = new EacCommand();
            string strSQL = @"
SELECT RMRE.RESOURCE, RMRE.PRO_NO, RMRE.REMIT_KIND, SUM(RMRE.REMIT_AMT) REMIT_AMT, RMRE.GEN_ID, CODE.TEXT, SUBSTR(RMRE.FILLER_20,2,1) COMP_TYPE
  FROM LRTRMRE3 RMRE  LEFT JOIN FPMCODE0 CODE ON RMRE.RESOURCE  = CODE.REF_NO AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'RESOURCE'
WHERE RMRE.VHR_NO2 = ''  
  AND RMRE.APPRV_ST = 'Y' 
  AND ((RMRE.GEN_YY = :_ENTRY_YY_B AND RMRE.GEN_MM = :_ENTRY_MM_B) 
     OR (RMRE.GEN_YY = :_ENTRY_YY_E AND RMRE.GEN_MM = :_ENTRY_MM_E AND RMRE.GEN_DD <= :_ENTRY_DD_E))  
";

            if (!"".Equals(strWhere))
                strSQL += " AND ( " + strWhere + " )  ";

            strSQL += " GROUP BY RMRE.RESOURCE, RMRE.PRO_NO, RMRE.REMIT_KIND, RMRE.GEN_ID, CODE.TEXT, SUBSTR(RMRE.FILLER_20,2,1) "; //modify by daiyu 20191210


            logger.Info("FRTRMREDao strSQL:" + strSQL);

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("_ENTRY_YY_B", _ENTRY_YY_B);
                cmd.Parameters.Add("_ENTRY_MM_B", _ENTRY_MM_B);
                cmd.Parameters.Add("_ENTRY_YY_E", _ENTRY_YY_E);
                cmd.Parameters.Add("_ENTRY_MM_E", _ENTRY_MM_E);
                cmd.Parameters.Add("_ENTRY_DD_E", _ENTRY_DD_E);

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    BGL00009Model d = new BGL00009Model();
                    d.SYS_TYPE = "F";
                    d.RESOURCE = result["RESOURCE"]?.ToString();
                    d.RESOURCE_DESC = result["TEXT"]?.ToString();
                    d.PRO_NO = result["PRO_NO"]?.ToString();

                    switch (result["REMIT_KIND"]?.ToString()) {
                        case "A":
                            d.PAID_TYPE = "匯款";
                            break;
                        case "B":
                            d.PAID_TYPE = "開票";
                            break;
                    }

                    d.AMT = result["REMIT_AMT"]?.ToString();
                    d.ENTRY_ID = result["GEN_ID"]?.ToString();

                    //modify by daiyu 20191210
                    string com_type = result["COMP_TYPE"]?.ToString();
                    if("O".Equals(StringUtil.toString(com_type)))
                        d.COMP_TYPE = "OIU";
                    else
                        d.COMP_TYPE = "FUBON";
                    //end modify 20191210


                    //20190813 add by daiyu 
                    if (Convert.ToDouble(d.AMT).CompareTo(0) != 0)
                        rows.Add(d);
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("qryForBGL0009 end!");

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