//using Treasury.WebBO;
//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Common.Logging;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.IO;
//using System.Linq;
//using System.Web;

///// <summary>
///// 功能說明：同步使用者指派單位檔中的單位名稱
///// 初版作者：20170926 黃黛鈺
///// 修改歷程：20170926 黃黛鈺 
/////           需求單號：201707240447-01 
/////           初版
///// </summary>
///// 

//namespace Treasury.WebScheduler
//{
    
//    public class UserMaintainUnitJob : IJob
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

//        public void Execute(IJobExecutionContext context)
//        {
            
//            logger.Info("[Execute]執行開始!!");


//            //查詢所有正在生效或將要生效的資料
//            String sysDate = DateUtil.getCurDate("yyyyMMdd");
//            CodeUserMaintainUnitDao maintainUnitDao = new CodeUserMaintainUnitDao();
//            List<CODEUSERMAINTAINUNIT> unitList = maintainUnitDao.qryEnableData(sysDate);

//            logger.Info("[Execute]本次作業需異動的筆數：" + unitList.Count);

//            String unitCodeO = "";
//            String unitSeqO = "";
//            String unitName = "";

//            string strConn = DbUtil.GetDBAccountConnStr();
//            using (SqlConnection conn = new SqlConnection(strConn))
//            {
//                conn.Open();

//                foreach (CODEUSERMAINTAINUNIT unit in unitList)
//                {

//                    //單位不同時，查詢SSSDB得到單位名稱
//                    if (!unitCodeO.Equals(unit.CUNITCODE.Trim()) || !unitSeqO.Equals(unit.CUNITSEQ.Trim()))
//                    {
//                        unitCodeO = unit.CUNITCODE.Trim();
//                        unitSeqO = unit.CUNITSEQ.Trim();
//                        unitName = qrySSSUnitName(unit.CUNITCODE.Trim(), unit.CUNITSEQ.Trim());
//                    }


//                    //更新單位名稱
//                    if ("".Equals(unitName))
//                    {
//                        logger.Error("[Execute]單位名稱異常，未更新： 身份證字號(" + unit.CAGENTID + ")、單位代碼("
//                            + unit.CUNITCODE + ")、單位序號(" + unit.CUNITSEQ + ")、生效日期(" + unit.CENABLEDATE + ")、終止日期(" + unit.CDISABLEDATE + ")");

//                    }
//                    else
//                    {
//                        unit.CUNITNAME = unitName;
//                        try
//                        {
//                            maintainUnitDao.updateUnitName(unit, conn);
//                        }
//                        catch (Exception e)
//                        {
//                            logger.Error("[Execute]更新UserMaintainUnitJob異常，未更新： 身份證字號(" + unit.CAGENTID + ")、單位代碼("
//                            + unit.CUNITCODE + ")、單位序號(" + unit.CUNITSEQ + ")、生效日期(" + unit.CENABLEDATE + ")、終止日期(" + unit.CDISABLEDATE + ")");

//                            logger.Error(e.ToString());

//                        }
//                    }
//                }
//            }

//            logger.Info("[Execute]執行結束!!");
//        }


//        /// <summary>
//        /// 自unitInfo取得單位名稱
//        /// </summary>
//        /// <param name="logger"></param>
//        /// <param name="unitCode"></param>
//        /// <param name="unitSeq"></param>
//        /// <returns></returns>
//        private String qrySSSUnitName(String unitCode, String unitSeq)
//        {
//            UnitInfoDao unitInfoDao = new UnitInfoDao();

//            String unitName = "";
//            UnitInfo unitInfo = unitInfoDao.qryByUnitCodeSeq(unitCode, unitSeq, true);

//            try
//            {
//                unitName = unitInfo.UnitName.Trim();
//            }
//            catch (Exception e)
//            {
//                unitName = "";

//                if (unitInfo == null)
//                    logger.Error("[qrySSSUnitName]查詢SSSDB 單位不存在：" + unitCode + "　" + unitSeq);
//                else
//                    logger.Error("[qrySSSUnitName]查詢SSSDB 其它錯誤：" + e.ToString());
//            }
//            return unitName;

//        }
//    }
//}