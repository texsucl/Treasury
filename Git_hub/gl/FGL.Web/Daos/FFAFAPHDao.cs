
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FFAFAPHDao 人工申請主檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FFAFAPHDao
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

            string _CONT_DATE_B = _ENTRY_YY_B.ToString().PadLeft(3, '0') + _ENTRY_MM_B.ToString().PadLeft(2, '0') + "01";
            string _CONT_DATE_E = _ENTRY_YY_E.ToString().PadLeft(3, '0') + _ENTRY_MM_E.ToString().PadLeft(2, '0') + _ENTRY_DD_E.ToString().PadLeft(2, '0');

            //動態組USER自訂的條件
            string strWhere = "";
            foreach (FPMCODEModel condition in conList) {
                string refNo = condition.refNo.PadRight(2, ' ');
                if (!"".Equals(StringUtil.toString(refNo))) {
                    if (refNo.StartsWith("XX"))   //遇到"XX"不用判斷
                        continue;


                    if(!"".Equals(strWhere))
                        strWhere += " OR ( 1 = 1 ";
                    else
                        strWhere += " ( 1 = 1 ";


                    //選項
                    if (!"".Equals(StringUtil.toString(refNo.Substring(0, 1))))
                        strWhere += " AND APHK.KIND = '" + refNo.Substring(0, 1)  + "' ";

                    //給付方式
                    if (!"".Equals(StringUtil.toString(refNo.Substring(1, 1))))
                        strWhere += " AND APHK.PAID_TYPE = '" + refNo.Substring(1, 1) + "' ";


                    strWhere += " )";
                }
            }



            EacCommand cmd = new EacCommand();
            string strSQL = @"
SELECT APHK.SRCE_KIND, APHK.RECE_NO, APHK.CR_NO, APHK.PAID_TYPE, SUM(APHK.TOTAMT) TOTAMT, APHK.ENTRY_ID, KIND.CODE_NAME
  FROM LFAFAPHK6 APHK LEFT JOIN FFAKIND  KIND ON APHK.SRCE_KIND = KIND.CODE
WHERE APHK.CONT_STS <> '' 
  AND APHK.TRNS_STS = ''  
  AND (APHK.CONT_DATE >= :_CONT_DATE_B AND APHK.CONT_DATE <= :_CONT_DATE_E)
";

            if (!"".Equals(strWhere))
                strSQL += " AND ( " + strWhere + " )  ";

            strSQL += " GROUP BY APHK.SRCE_KIND, APHK.RECE_NO, APHK.CR_NO, APHK.PAID_TYPE, APHK.ENTRY_ID ,KIND.CODE_NAME  ";

            logger.Info("FFAFAPHDao strSQL:" + strSQL);


            try
            {
                cmd.Connection = conn;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("_CONT_DATE_B", _CONT_DATE_B);
                cmd.Parameters.Add("_CONT_DATE_E", _CONT_DATE_E);

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    BGL00009Model d = new BGL00009Model();
                    d.SYS_TYPE = "A";
                    d.RESOURCE = result["SRCE_KIND"]?.ToString();
                    d.RESOURCE = result["SRCE_KIND"]?.ToString();
                    d.RESOURCE_DESC = result["CODE_NAME"]?.ToString();
                    d.VHR_NO = result["RECE_NO"]?.ToString();
                    d.PRO_NO = result["CR_NO"]?.ToString();

                    switch (result["PAID_TYPE"]?.ToString()) {
                        case "A":
                            d.PAID_TYPE = "匯款";
                            break;
                        case "B":
                            d.PAID_TYPE = "開票";
                            break;
                    }

                    d.AMT = result["TOTAMT"]?.ToString();
                    d.ENTRY_ID = result["ENTRY_ID"]?.ToString();
                    d.COMP_TYPE = "FUBON";

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