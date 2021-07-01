using FRT.Web.AS400Models;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;

namespace FRT.Web.BO
{
    public class FastErrUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 處理快速付款失敗案件
        /// 1.回寫AS400 FRTBARM0失敗狀態
        /// 2.寄送匯款失敗通知MAIL
        /// 3.呼叫SRTB0008
        /// type-->S:系統;M:人工
        /// execType-->S:成功;F:失敗
        /// fastNo
        /// textType-->1、2:EACH;3:金資;622685:622685電文
        /// errorCode
        /// emsgTxt
        /// </summary>
        /// <param name="type"></param>S:系統;M:人工
        /// <param name="execType"></param>S:成功;F:失敗
        /// <param name="fastNo"></param>
        /// <param name="textType"></param>1、2:EACH;3:金資;4:622685電文
        /// <param name="errorCode"></param>
        /// <param name="emsgTxt"></param>
        /// <returns></returns>
        public FastErrModel procFailNotify(string type, string execType, string fastNo, string textType, string errorCode, string emsgTxt)
        {
            
            FastErrModel fastErrModel = new FastErrModel();
            fastErrModel.execType = execType;
            fastErrModel.fastNo = fastNo;
            fastErrModel.textType = textType;
            fastErrModel.errorCode = errorCode;
            fastErrModel.errorMsg = emsgTxt;

            logger.Info("type:" + type);
            logger.Info("execType:" + execType);
            logger.Info("fastNo:" + fastNo);
            logger.Info("textType:" + textType);
            logger.Info("errorCode:" + errorCode);
            logger.Info("emsgTxt:" + emsgTxt);

            using (EacConnection con = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                con.Open();


                FRTBARMModel fRTBARMModel = new FRTBARMModel();
                FRTBARMDao fRTBARMDao = new FRTBARMDao();

                fRTBARMModel = fRTBARMDao.qryByFastNo(fastNo, con);
                if ("".Equals(StringUtil.toString(fRTBARMModel.fastNo)))
                {
                    fastErrModel.errorMsg = "查無對應的快速付款編號!!";
                    notifySysErr(fastErrModel);
                }
                else
                {
                    if ("S".Equals(execType) && !"622685".Equals(fastErrModel.textType))
                        fastErrModel = doSCase(fastErrModel, fRTBARMModel, con);
                    else {
                        if("622685".Equals(fastErrModel.textType)) //FRQxml & ORTB011
                            fastErrModel = do622685Case(fastErrModel, fRTBARMModel, con, type, textType);
                        else 
                            //FRTXml
                            fastErrModel = doFCase(fastErrModel, fRTBARMModel, con, type, textType);
                    }
                }
            }

            return fastErrModel;

        }


        /// <summary>
        /// 執行成功案件
        /// 1.呼叫SRTB0008
        /// </summary>
        /// <param name="fastErrModel"></param>
        /// <param name="fRTBARMModel"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        private FastErrModel doSCase(FastErrModel fastErrModel, FRTBARMModel fRTBARMModel, EacConnection con)
        {
            bool execResult = callSRTB0008(con, fastErrModel);

            return fastErrModel;
        }



        /// <summary>
        /// 執行異常案件，依條件執行以下動作
        /// 1.異動BARM匯款狀態
        /// 2.寄送匯款失敗通知
        /// 3.呼叫SRTB0008
        /// </summary>
        /// <param name="fastErrModel"></param>
        /// <param name="fRTBARMModel"></param>
        /// <param name="con"></param>
        /// <param name="textType"></param>
        /// <param name="errorCode"></param>
        /// <param name="emsgTxt"></param>
        /// <returns></returns>
        private FastErrModel doFCase(FastErrModel fastErrModel, FRTBARMModel fRTBARMModel, EacConnection con, string type, string textType)
        {
            if ("".Equals(StringUtil.toString(fastErrModel.errorCode)))
            {
                fastErrModel.errorMsg = "未傳入ERROR_CODE";
                notifySysErr(fastErrModel);
                return fastErrModel;
            }

            bool execResult = true;
            FRTBERMDao fRTBERMDao = new FRTBERMDao();
            FRTBERMModel fRTBERMModel = fRTBERMDao.qryByErrCode(fastErrModel.errorCode);


            //錯誤代碼(ERROR_CODE)不存在於失敗原因對照檔，直接將ERROR_CODE英數字帶出且秀”ERROR_CODE不存在”，通知財務部窗口，暫存檔狀態停留原狀態”2 - 已匯款”
            //錯誤代碼(ERROR_CODE)對應失敗原因對照檔錯誤歸屬非客戶，通知財務部窗口，暫存檔狀態停留原狀態”2-已匯款”
            if ("".Equals(StringUtil.toString(fRTBERMModel.errCode)) || !"1".Equals(fRTBERMModel.errBelong))
            {
                fRTBARMModel.errBelong = "2";
                fRTBARMModel.errDesc = StringUtil.toString(fRTBERMModel.errCode) == "" ? "ERROR_CODE不存在" : fRTBERMModel.errDesc;
                fRTBARMModel.errCode = StringUtil.toString(fastErrModel.errorCode);
                fRTBARMModel.failCode = StringUtil.toString(fRTBERMModel.errCode) == "" ? "" : fRTBERMModel.transCode;
                execResult = procSendMail(type, fRTBARMModel, textType);

                if (!execResult)
                {
                    fastErrModel.errorMsg = "寄送匯款失敗通知MAIL失敗";
                    notifySysErr(fastErrModel);
                    
                }
                return fastErrModel;
            }
            else {
                //錯誤代碼(ERROR_CODE)對應失敗原因對照檔錯誤歸屬客戶，通知通知案件登錄人員，暫存檔狀態由” 2-已匯款”改為”4-匯款失敗”
                fRTBARMModel.errBelong = fRTBERMModel.errBelong;
                fRTBARMModel.errDesc = fRTBERMModel.errDesc;
                fRTBARMModel.errCode = fRTBERMModel.errCode;
                fRTBARMModel.failCode = StringUtil.toString(fRTBERMModel.transCode);
                execResult = updateBRAM(con, fRTBARMModel, fastErrModel);

                if (execResult)
                {
                    
                    execResult = procSendMail(type, fRTBARMModel, textType);

                    string strMsg = "";
                    if (!execResult)
                        strMsg = "寄送匯款失敗通知MAIL失敗；";

                    //呼叫SRTB0008
                    execResult = callSRTB0008(con, fastErrModel);
                    if (!execResult)
                        strMsg += "呼叫SRTB0008失敗；";

                    if (!"".Equals(strMsg))
                    {
                        fastErrModel.errorMsg = strMsg;
                        notifySysErr(fastErrModel);
                    }
                }
                else {
                    notifySysErr(fastErrModel);
                }
                return fastErrModel;

            }
            
        }


        /// <summary>
        /// 處理622685回傳電文結果的後續處理
        /// </summary>
        /// <param name="fastErrModel"></param>
        /// <param name="fRTBARMModel"></param>
        /// <param name="con"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private FastErrModel do622685Case(FastErrModel fastErrModel, FRTBARMModel fRTBARMModel, EacConnection con, string type, string textType)
        {
            bool execResult = true;

            FRTBERMDao fRTBERMDao = new FRTBERMDao();
            FRTBERMModel fRTBERMModel = fRTBERMDao.qryByErrCode(fastErrModel.errorCode);

            #region new update by mark in 20190816

            if (fastErrModel.execType.Split(';').Count() > 1)
            {
                //execType:借用欄位，為622685電文的 FunCode + Status (FunCode;Status)
                var FunCode = fastErrModel.execType.Split(';')[0];
                var Status = fastErrModel.execType.Split(';')[1];

                //1.3.3.1增加先判斷(FunCode)選項為1-匯出異常時，(STATUS)類別為1-匯出異常，通知財務部窗口增加先判斷(FunCode)選項為1-匯出異常時，
                //(STATUS)類別為2--人工退還客戶，錯誤代碼(ERROR_CODE) = 99，
                //通知財務部窗口,其餘錯誤代碼(ERROR_CODE)對應失敗原因對照檔錯誤歸屬客戶，通知案件登錄人員
                //暫存檔狀態停留原狀態2-已匯款
                if ((FunCode == "1" && Status == "1") || (FunCode == "1" && Status == "2"))
                {
                    fRTBARMModel.errBelong = "2";
                    fRTBARMModel.errDesc = StringUtil.toString(fRTBERMModel.errCode) == "" ? "ERROR_CODE不存在" : fRTBERMModel.errDesc;
                    fRTBARMModel.errCode = fastErrModel.errorCode;
                    fRTBARMModel.failCode = "";
                    execResult = procSendMail(type, fRTBARMModel, textType, Status);

                    if (!execResult)
                    {
                        fastErrModel.errorMsg = "寄送匯款失敗通知MAIL失敗";
                        notifySysErr(fastErrModel);

                    }
                    return fastErrModel;
                }
                //1.3.3.2增加先判斷(FunCode)選項為2-匯出被退匯時，(STATUS)類別為3-人工退還客戶，錯誤代碼(ERROR_CODE) = 99，通知財務部窗口,其餘錯誤代碼(ERROR_CODE)對應失敗原因對照檔錯誤歸屬客戶，通知案件登錄人員
                else if ((FunCode == "2" && Status == "3"))
                {
                    switch (fRTBERMModel.errCode?.Trim() ?? string.Empty)
                    {
                        case "":
                            fRTBARMModel.errBelong = "2";
                            fRTBARMModel.errDesc = "ERROR_CODE不存在";
                            fRTBARMModel.errCode = StringUtil.toString(fastErrModel.errorCode);
                            fRTBARMModel.failCode = "";
                            execResult = procSendMail(type, fRTBARMModel, textType, Status);

                            if (!execResult)
                            {
                                fastErrModel.errorMsg = "寄送匯款失敗通知MAIL失敗";
                                notifySysErr(fastErrModel);
                            }
                            return fastErrModel;
                        case "99":
                            fRTBARMModel.errBelong = "2";
                            break;
                        default:
                            fRTBARMModel.errBelong = fRTBERMModel.errBelong;
                            break;
                    }


                    fRTBARMModel.errDesc = fRTBERMModel.errDesc;
                    fRTBARMModel.errCode = fRTBERMModel.errCode;
                    fRTBARMModel.failCode = fRTBERMModel.transCode;
                    fRTBARMModel.failCode = StringUtil.toString(fRTBERMModel.transCode);
                    execResult = updateBRAM(con, fRTBARMModel, fastErrModel);

                    if (execResult)
                    {

                        execResult = procSendMail(type, fRTBARMModel, textType, Status);

                        string strMsg = "";
                        if (!execResult)
                            strMsg = "寄送匯款失敗通知MAIL失敗；";

                        //呼叫SRTB0008
                        execResult = callSRTB0008(con, fastErrModel);
                        if (!execResult)
                            strMsg += "呼叫SRTB0008失敗；";

                        if (!"".Equals(strMsg))
                        {
                            fastErrModel.errorMsg = strMsg;
                            notifySysErr(fastErrModel);
                        }
                    }
                    else
                    {
                        notifySysErr(fastErrModel);
                    }

                    return fastErrModel;
                }
            }
            else
            {
                #region old
                //execType:借用欄位，為622685電文的STATUS
                //622685電文，STATUS為1-匯出異常，通知財務部窗口，暫存檔狀態停留原狀態2-已匯款
                if ("1".Equals(fastErrModel.execType))
                {
                    fRTBARMModel.errBelong = "2";
                    fRTBARMModel.errDesc = StringUtil.toString(fRTBERMModel.errCode) == "" ? "ERROR_CODE不存在" : fRTBERMModel.errDesc;
                    fRTBARMModel.errCode = fastErrModel.errorCode;
                    fRTBARMModel.failCode = "";
                    execResult = procSendMail(type, fRTBARMModel, textType, fastErrModel.execType);

                    if (!execResult)
                    {
                        fastErrModel.errorMsg = "寄送匯款失敗通知MAIL失敗";
                        notifySysErr(fastErrModel);

                    }
                    return fastErrModel;
                }
                else if ("2".Equals(fastErrModel.execType))
                {
                    //錯誤代碼(ERROR_CODE) = 99，將對應的轉碼代號帶到人工修改匯款失敗原因作業畫面，通知財務部窗口(接AS400的9.人工修改匯款失敗原因作業)，暫存檔狀態改為”4 - 匯款失敗”
                    //其餘錯誤代碼(ERROR_CODE)對應失敗原因對照檔錯誤歸屬客戶，通知案件登錄人員，暫存檔狀態改為”4-匯款失敗”，失敗原因要寫入暫存檔的”失敗原因”欄位
                    switch (StringUtil.toString(fRTBERMModel.errCode))
                    {
                        case "":
                            fRTBARMModel.errBelong = "2";
                            fRTBARMModel.errDesc = "ERROR_CODE不存在";
                            fRTBARMModel.errCode = StringUtil.toString(fastErrModel.errorCode);
                            fRTBARMModel.failCode = "";
                            execResult = procSendMail(type, fRTBARMModel, textType, fastErrModel.execType);

                            if (!execResult)
                            {
                                fastErrModel.errorMsg = "寄送匯款失敗通知MAIL失敗";
                                notifySysErr(fastErrModel);
                            }
                            return fastErrModel;
                        case "99":
                            fRTBARMModel.errBelong = "2";
                            break;
                        default:
                            fRTBARMModel.errBelong = fRTBERMModel.errBelong;
                            break;
                    }


                    fRTBARMModel.errDesc = fRTBERMModel.errDesc;
                    fRTBARMModel.errCode = fRTBERMModel.errCode;
                    fRTBARMModel.failCode = fRTBERMModel.transCode;
                    fRTBARMModel.failCode = StringUtil.toString(fRTBERMModel.transCode);
                    execResult = updateBRAM(con, fRTBARMModel, fastErrModel);

                    if (execResult)
                    {

                        execResult = procSendMail(type, fRTBARMModel, textType, fastErrModel.execType);

                        string strMsg = "";
                        if (!execResult)
                            strMsg = "寄送匯款失敗通知MAIL失敗；";

                        //呼叫SRTB0008
                        execResult = callSRTB0008(con, fastErrModel);
                        if (!execResult)
                            strMsg += "呼叫SRTB0008失敗；";

                        if (!"".Equals(strMsg))
                        {
                            fastErrModel.errorMsg = strMsg;
                            notifySysErr(fastErrModel);
                        }
                    }
                    else
                    {
                        notifySysErr(fastErrModel);
                    }

                    return fastErrModel;
                }
                #endregion
            }




            #endregion





            return fastErrModel;

        }


        /// <summary>
        /// 發送匯款失敗通知
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fRTBARMModel"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private bool procSendMail(string type, FRTBARMModel fRTBARMModel, string textType,string status = null)
        {
            bool rtn = true;

            List<FRTBARMModel> recData = new List<FRTBARMModel>();
            FastErrMailUtil fastErrMailUtil = new FastErrMailUtil();

            recData.Add(fRTBARMModel);

            Dictionary<string, string> errMap = fastErrMailUtil.procSendMail(type, recData, textType, status);
            if (errMap.Count > 0)
                rtn = false;

            return rtn;
        }


        /// <summary>
        /// 回寫AS400主檔錯誤
        /// </summary>
        /// <param name="con"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool updateBRAM(EacConnection con, FRTBARMModel fRTBARMModel, FastErrModel fastErrModel)
        {
            bool rtn = true;
            

            //回壓LRTBARM1錯誤
            try
            {
                using (EacCommand ec = new EacCommand(con))
                {
                    string sql = @"
update LRTBARM1
set FAIL_CODE = :FAIL_CODE,
REMIT_STAT = :REMIT_STAT
where FAST_NO = :FAST_NO
and REMIT_STAT <> :REMIT_STAT
";
                    ec.CommandText = sql;

                    ec.Parameters.Add("FAIL_CODE", fRTBARMModel.failCode);
                    ec.Parameters.Add("REMIT_STAT", "4");
                    ec.Parameters.Add("FAST_NO", fRTBARMModel.fastNo);

                    var updateNum = ec.ExecuteNonQuery();

                    ec.Dispose();
                }
            }
            catch (Exception e)
            {
                fastErrModel.errorMsg = "Update LRTBARM1:" + e.Message;
                return false;
            }

            return rtn;
        }


        /// <summary>
        /// //呼叫SRTB0008
        /// </summary>
        /// <param name="con"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool callSRTB0008(EacConnection con, FastErrModel model)
        {
            bool rtn = true;

            try
            {
                using (EacCommand ec = new EacCommand(con))
                {
                    ec.CommandType = CommandType.StoredProcedure;
                    ec.CommandText = "*PGM/SRTB0008";
                    ec.Parameters.Clear();

                    EacParameter fastNo = new EacParameter();
                    fastNo.ParameterName = "LK-FAST-NO";
                    fastNo.DbType = DbType.String;
                    fastNo.Size = 10;
                    fastNo.Direction = ParameterDirection.InputOutput;
                    fastNo.Value = model.fastNo;

                    EacParameter rtnCode = new EacParameter();
                    rtnCode.ParameterName = "LK-RTNCODE";
                    rtnCode.DbType = DbType.String;
                    rtnCode.Size = 1;
                    rtnCode.Direction = ParameterDirection.InputOutput;
                    rtnCode.Value = "";

                    ec.Parameters.Add(fastNo);
                    ec.Parameters.Add(rtnCode);

                    ec.Prepare();

                    logger.Info("Call SRTB0008 begin");
                    ec.ExecuteNonQuery();

                    logger.Info("Call SRTB0008 end");

                    if (!"Y".Equals(rtnCode.Value))
                    {
                        model.errorMsg = "Call SRTB0008:失敗";
                        return false;

                    }

                    ec.Dispose();
                }
            }
            catch (Exception e)
            {
                model.errorMsg = "Call SRTB0008:" + e.Message;
                return false;
            }

            return rtn;
        }

        /// <summary>
        /// 處理快速付款失敗案件...發生系統錯誤時，通知相關人員
        /// </summary>
        /// <param name="fastErrModel"></param>
        private void notifySysErr(FastErrModel fastErrModel)
        {
            MailUtil mailUtil = new MailUtil();
            List<UserBossModel> notify = mailUtil.getMailGrpId("FAST_ERR");


            string mailContent = "";
            mailContent =
                "   快速付款編號 = " + fastErrModel.fastNo + "<br/>" +
                "   失敗說明 = " + fastErrModel.errorMsg + "<br/>";

            bool bSucess = mailUtil.sendMailMulti(notify
                , "快速付款匯款失敗回壓AS400通知表"
                , mailContent
                , true
               , ""
               , ""
               , null
               , true
               , true
               , fastErrModel.fastNo);

            //return bSucess;


            //List<MailNotifyModel> otherNotify = new List<MailNotifyModel>();
            //Dictionary<string, UserBossModel> empMap = new Dictionary<string, UserBossModel>();
            //Dictionary<string, string> failMap = new Dictionary<string, string>();

            //OaEmpDao oaEmpDao = new OaEmpDao();
            //FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();

            //EacConnection con = new EacConnection();
            //EacCommand cmd = new EacCommand();

            //otherNotify = fRTMailNotify.qryNtyUsr("FAST_ERR");

            //foreach (MailNotifyModel dUser in otherNotify)
            //{
            //    if (!empMap.ContainsKey(dUser.receiverEmpno))
            //    {
            //        UserBossModel userBossModel = oaEmpDao.getEmpBoss(dUser.receiverEmpno, con, cmd);
            //        if (userBossModel != null)
            //            empMap.Add(dUser.receiverEmpno, userBossModel);
            //        else
            //        {
            //            if(!failMap.ContainsKey(fastErrModel.fastNo))
            //                failMap.Add(fastErrModel.fastNo, "查無建立人員資訊");
            //            continue;
            //        }
            //    }

            //    UserBossModel userBoss = empMap[dUser.receiverEmpno];
            //    bool bSendSucess = genMail(userBoss, fastErrModel
            //        , (dUser.isNotifyMgr == "Y" ? true : false)
            //        , (dUser.isNotifyDeptMgr == "Y" ? true : false));
            //    if (!bSendSucess)
            //    {
            //        if (!failMap.ContainsKey(fastErrModel.fastNo))
            //            failMap.Add(fastErrModel.fastNo, "寄送MAIL失敗");
            //        continue;
            //    }
            //}

        }


        //private bool genMail(UserBossModel userBoss, FastErrModel d, bool bMailMgr, bool bMailDeptMgr)
        //{
        //    bool bSucess = true;
        //    MailUtil mailUtil = new MailUtil();


        //    string mailContent = "";
        //    mailContent =
        //        "   快速付款編號 = " + d.fastNo + "<br/>" +
        //        "   失敗說明 = " + d.errorMsg + "<br/>";

        //    bSucess = mailUtil.sendMail(userBoss
        //        , "快速付款匯款失敗回壓AS400通知表"
        //        , mailContent
        //        , true
        //       , ""
        //       , ""
        //       , null
        //       , true
        //       , bMailMgr
        //       , bMailDeptMgr
        //       , true
        //       , d.fastNo);

        //    return bSucess;

        //}

    }
}