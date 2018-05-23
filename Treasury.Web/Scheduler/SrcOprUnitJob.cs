//using Treasury.WebBO;
//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Treasury.WebViewModels;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;

//namespace Treasury.WebScheduler
//{
//    public class SrcOprUnitJob : IJob
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

//        public void Execute(IJobExecutionContext context)
//        {
//            logger.Info("[Execute]執行開始!!");

//            bool bSynSuccess = true;
//            string strConn = DbUtil.GetDBAccountConnStr();


//            try
//            {
//                using (SqlConnection conn = new SqlConnection(strConn))
//                {
//                    conn.Open();

//                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

//                    //1.清除【CodeUserSrchUnitMapping使用者可查詢單位mapping檔】中原有的全部資料
//                    CodeUserSrchUnitMappingDao srchUnitDao = new CodeUserSrchUnitMappingDao();
//                    bSynSuccess = clearSrchUnit(srchUnitDao, conn, transaction);


//                    //2.清除【CodeUserOprUnitMapping使用者可作業單位mapping檔】中原有的全部資料
//                    CodeUserOprUnitMappingDao oprUnitDao = new CodeUserOprUnitMappingDao();
//                    if (bSynSuccess)
//                        bSynSuccess = clearOprUnit(oprUnitDao, conn, transaction);


//                    //3.查詢非總公司管理員角色的所有使用者的作業範圍和查詢範圍

//                    List<SrcOprUnitJobModel> userRoleList = new List<SrcOprUnitJobModel>();
//                    CodeUserRoleDao userRoleDao = new CodeUserRoleDao();
//                    if (bSynSuccess)
//                        userRoleList = qryValidUserRole(userRoleDao, userRoleList);




//                    //4.處理【CodeUserOprUnitMapping使用者可作業單位mapping檔】
//                    if (bSynSuccess)
//                        bSynSuccess = procUnit("opr", userRoleList, conn, transaction);


//                    //5.處理【CodeUserSrchUnitMapping使用者可查詢單位mapping檔】
//                    if (bSynSuccess)
//                        bSynSuccess = procUnit("srch", userRoleList, conn, transaction);


//                    if (bSynSuccess)
//                        transaction.Commit();
//                    else
//                        transaction.Rollback();

//                }
//            }
//            catch (Exception e) {
//                logger.Info("[Execute]其它錯誤：" + e.ToString());

//            }
            

//            logger.Info("[Execute]執行結束!!");
//        }



//        private bool procUnit(String procType, List<SrcOprUnitJobModel> userRoleList, SqlConnection conn, SqlTransaction transaction)
//        {
//            String[] area = new String[] { "2", "3" };
//            int sCnt = 0;
//            int fCnt = 0;

//            //列出agent清單
//            List<String> agentList = new List<String>();

//            if("opr".Equals(procType))
//                agentList = userRoleList.Where(x => area.Contains(x.cOperatorArea)).GroupBy(o => new { o.cAgentID })
//                    .Select(group => group.Key.cAgentID).ToList<String>();
//            else
//                agentList = userRoleList.Where(x => area.Contains(x.cSearchArea)).GroupBy(o => new { o.cAgentID })
//                .Select(group => group.Key.cAgentID).ToList<String>();



//            AgentInfoDao agentInfoDao = new AgentInfoDao();
//            UnitInfoDao unitInfoDao = new UnitInfoDao();
//            Dictionary<string, UnitInfo> unitMap = new Dictionary<string, UnitInfo>();
//            Dictionary<string, List<UnitInfo>> underUnitMap = new Dictionary<string, List<UnitInfo>>();

//            foreach (String cAgentId in agentList)
//            {
//                //查SSSDB，找出agent的單位
//                AgentInfo agentInfo = agentInfoDao.qryByAgentId(cAgentId);


//                //該agentId需存在於agentInfo
//                if (agentInfo != null)
//                {
//                    String uintCodeSeq = agentInfo.WorkUnitCode.Trim() + "-" + agentInfo.WorkUnitSeq.Trim();
//                    UnitInfo unitInfo = new UnitInfo();

//                    //依agent的單位，查詢對應的單位名稱
//                    if (unitMap.ContainsKey(uintCodeSeq))
//                        unitInfo = unitMap[uintCodeSeq];
//                    else
//                    {
//                        unitInfo = unitInfoDao.qryByUnitCodeSeq(agentInfo.WorkUnitCode.Trim(), agentInfo.WorkUnitSeq.Trim(), false);
//                        if (unitInfo != null)
//                            unitMap.Add(uintCodeSeq, unitInfo);
//                        else
//                            logger.Error("[procUnit]使用者單位不存在SSSDB，將不進行新增：cAgentID(" + cAgentId + ") uintCodeSeq(" + uintCodeSeq + ")");

//                    }


//                    //若單位已不存在SSSDB，將不進行新增
//                    if (unitInfo != null)
//                    {
//                        List<SrcOprUnitJobModel> procList = new List<SrcOprUnitJobModel>();

//                        if("opr".Equals(procType))
//                            procList = userRoleList.Where(
//                            x => x.cAgentID == cAgentId
//                            & x.cWorkUnitCode == agentInfo.WorkUnitCode
//                            & x.cWorkUnitSeq == agentInfo.WorkUnitSeq
//                            & area.Contains(x.cOperatorArea)).OrderBy(x => x.cOperatorArea).ToList<SrcOprUnitJobModel>();
//                        else
//                            procList = userRoleList.Where(
//                                x => x.cAgentID == cAgentId
//                                & x.cWorkUnitCode == agentInfo.WorkUnitCode
//                                & x.cWorkUnitSeq == agentInfo.WorkUnitSeq
//                                & area.Contains(x.cSearchArea)).OrderBy(x => x.cSearchArea).ToList<SrcOprUnitJobModel>();


//                        if (procList == null | procList.Count == 0) 
//                            logger.Error("[procUnit]AWTC與SSSDB單位資料不一致，將不進行新增：cAgentID(" + cAgentId + ")");
                        

//                        //依userRole的設定新增【CodeUserOprUnitMapping使用者可作業單位mapping檔】
//                        foreach (SrcOprUnitJobModel detail in procList)
//                        {
//                            String cArea = "";
//                            if ("opr".Equals(procType))
//                                cArea = detail.cOperatorArea;
//                            else
//                                cArea = detail.cSearchArea;

//                            //3：所屬單位
//                            if ("3".Equals(cArea))
//                            {
//                                if (insertUnit(procType, detail, unitInfo, "0", conn, transaction))
//                                    sCnt++;
//                                else
//                                    fCnt++;
//                            }
//                            //2：所屬單位及下轄單位
//                            else
//                            {
//                                //處理所屬單位
//                                if (insertUnit(procType, detail, unitInfo, "0", conn, transaction))
//                                    sCnt++;
//                                else
//                                    fCnt++;

//                                //處理下轄單位
//                                List<UnitInfo> upderUnitList = new List<UnitInfo>();
//                                if (underUnitMap.ContainsKey(uintCodeSeq))
//                                {
//                                    upderUnitList = underUnitMap[uintCodeSeq];
//                                }
//                                else {
//                                    upderUnitList = unitInfoDao.qryUnderUnit(upderUnitList, agentInfo.WorkUnitCode, agentInfo.WorkUnitSeq);
//                                    underUnitMap.Add(uintCodeSeq, upderUnitList);
//                                }

//                                foreach (UnitInfo underUnit in upderUnitList)
//                                {
//                                    if (insertUnit(procType, detail, underUnit, "1", conn, transaction))
//                                        sCnt++;
//                                    else
//                                        fCnt++;

//                                }

//                            }

//                        }

//                    }


//                } else
//                    logger.Error("[procUnit]使用者不存在SSSDB，將不進行新增：cAgentID(" + cAgentId + ")");
//            }

//            if (sCnt == 0 && fCnt > 0)
//            {
//                if("opr".Equals(procType))
//                    logger.Error("[procUnit]同步【CodeUserOprUnitMapping使用者可作業單位mapping檔】異常，將不進行同步");
//                else
//                    logger.Error("[procUnit]同步【CodeUserSrchUnitMapping使用者可查詢單位mapping檔】異常，將不進行同步");

//                return false;
//            }
//            else
//                return true;
                

//        }



//        /// <summary>
//        /// 將agent的所屬單位加入【CodeUserOprUnitMapping使用者可作業單位mapping檔】/【CodeUserSrchUnitMapping使用者可查詢單位mapping檔】
//        /// </summary>
//        /// <param name="procType"></param>
//        /// <param name="unit"></param>
//        /// <param name="unitInfo"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        /// <returns></returns>
//        private bool insertUnit(String procType, SrcOprUnitJobModel unit, UnitInfo unitInfo, String unitType, SqlConnection conn, SqlTransaction transaction) {

//            if ("opr".Equals(procType))
//                return insertOprUnit(unit, unitInfo, unitType, conn, transaction);
//            else
//                return insertSrchUnit(unit, unitInfo, unitType, conn, transaction);
            
//        }


//        private bool insertOprUnit(SrcOprUnitJobModel unit, UnitInfo unitInfo, String unitType, SqlConnection conn, SqlTransaction transaction) {
//            CodeUserOprUnitMappingDao oprUnitDao = new CodeUserOprUnitMappingDao();
//            CODEUSEROPRUNITMAPPING oprUnit = new CODEUSEROPRUNITMAPPING();
//            oprUnit.CAGENTID = unit.cAgentID.Trim();
//            oprUnit.CROLEID = unit.cRoleID.Trim();
//            oprUnit.CUNITCODE = unitInfo.UnitCode.Trim();
//            oprUnit.CUNITSEQ = unitInfo.UnitSeq.Trim();
//            oprUnit.CUNITTYPE = unitType;
//            oprUnit.CUNITNAME = unitInfo.UnitName.Trim();
//            oprUnit.CLEVELCODE = unitInfo.Levelcode.Trim();
//            oprUnit.REGIONCODE = unitInfo.RegionCode.Trim();
//            oprUnit.REGIONSEQ = unitInfo.RegionSeq.Trim();

//            bool bSuccess = true;

//            try
//            {
//                oprUnitDao.insert(oprUnit, conn, transaction);

//            }
//            catch (SqlException sqlE)
//            {
//                bSuccess = false;
//                logger.Error("[insertOprUnit]資料庫錯誤，將不進行新增：" + sqlE.ErrorCode + " " + sqlE.ToString());
//            }
//            catch (Exception e)
//            {
//                bSuccess = false;
//                logger.Error("[insertOprUnit]其它錯誤，將不進行新增：" + e.ToString());
//            }

//            return bSuccess;
//        }


//        private bool insertSrchUnit(SrcOprUnitJobModel unit, UnitInfo unitInfo, String unitType, SqlConnection conn, SqlTransaction transaction)
//        {
//            CodeUserSrchUnitMappingDao srchUnitDao = new CodeUserSrchUnitMappingDao();
//            CODEUSERSRCHUNITMAPPING srchUnit = new CODEUSERSRCHUNITMAPPING();
//            srchUnit.CAGENTID = unit.cAgentID.Trim();
//            srchUnit.CROLEID = unit.cRoleID.Trim();
//            srchUnit.CUNITCODE = unitInfo.UnitCode.Trim();
//            srchUnit.CUNITSEQ = unitInfo.UnitSeq.Trim();
//            srchUnit.CUNITTYPE = unitType;
//            srchUnit.CUNITNAME = unitInfo.UnitName.Trim();
//            srchUnit.CLEVELCODE = unitInfo.Levelcode.Trim();
//            srchUnit.REGIONCODE = unitInfo.RegionCode.Trim();
//            srchUnit.REGIONSEQ = unitInfo.RegionSeq.Trim();

//            bool bSuccess = true;

//            try
//            {
//                srchUnitDao.insert(srchUnit, conn, transaction);

//            }
//            catch (SqlException sqlE)
//            {
//                bSuccess = false;
//                logger.Error("[insertSrchUnit]資料庫錯誤，將不進行新增：" + sqlE.ErrorCode + " " + sqlE.ToString());
//            }
//            catch (Exception e)
//            {
//                bSuccess = false;
//                logger.Error("[insertSrchUnit]其它錯誤，將不進行新增：" + e.ToString());
//            }

//            return bSuccess;
//        }



//        /// <summary>
//        /// CodeUserSrchUnitMapping清檔
//        /// </summary>
//        /// <param name="srchUnitDao"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        /// <returns></returns>
//        private bool clearSrchUnit(CodeUserSrchUnitMappingDao srchUnitDao, SqlConnection conn, SqlTransaction transaction) {
//            try
//            {
//                logger.Info("[clearSrchUnit]CodeUserSrchUnitMapping清檔筆數：" + srchUnitDao.delete(conn, transaction));
//                return true;
//            }
//            catch (Exception e)
//            {
//                transaction.Rollback();
//                logger.Info("[clearSrchUnit]CodeUserSrchUnitMapping清檔錯誤，將不進行後續同步：" + e.ToString());
//                return false;
//            }
//        }


//        /// <summary>
//        /// CodeUserOprUnitMapping清檔
//        /// </summary>
//        /// <param name="oprUnitDao"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        /// <returns></returns>
//        private bool clearOprUnit(CodeUserOprUnitMappingDao oprUnitDao, SqlConnection conn, SqlTransaction transaction)
//        {
//            try
//            {
//                logger.Info("[clearOprUnit]CodeUserOprUnitMapping清檔筆數：" + oprUnitDao.delete(conn, transaction));
//                return true;
//            }
//            catch (Exception e)
//            {
//                transaction.Rollback();
//                logger.Info("[clearOprUnit]CodeUserOprUnitMapping，將不進行後續同步：" + e.ToString());
//                return false;
//            }
//        }


//        /// <summary>
//        /// 查詢使用者角色檔
//        /// </summary>
//        /// <param name="userRoleDao"></param>
//        /// <param name="userRoleList"></param>
//        /// <returns></returns>
//        private List<SrcOprUnitJobModel> qryValidUserRole(CodeUserRoleDao userRoleDao, List<SrcOprUnitJobModel> userRoleList) {
//            try
//            {                
//                userRoleList = userRoleDao.qryValidUserRole(DateUtil.getCurDate("yyyyMMdd"));
//                //logger.Info("[qryValidUserRole]本次作業需異動的筆數：" + userRoleList.Count);
//            }
//            catch (Exception e)
//            {
//                logger.Info("[qryValidUserRole]查詢使用者角色檔異常，將不進行後續同步：" + e.ToString());
//            }

//            return userRoleList;
//        }

//    }
//}