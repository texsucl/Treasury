using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.EasycomClient;
using System.Web.Http;


namespace FRT.Web.Controllers
{
    [RoutePrefix("FastMail")]
    public class FastMailController : ApiController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        [Route("MailNotify")]
        public object WaterNotify(FastMailModel model)
        {
            logger.Info("groupCode:" + StringUtil.toString(model.groupCode));
            logger.Info("reserve1:" + StringUtil.toString(model.reserve1));

            if ("".Equals(StringUtil.toString(model.groupCode))) {
                model.rtnCode = "F";
                model.errorMsg = "請輸入錯誤代碼";
                return model;
            }

            if ("REMIT_ERR".Equals(model.groupCode) && !("1".Equals(model.reserve1) || "2".Equals(model.reserve1))) {
                model.rtnCode = "F";
                model.errorMsg = "請輸入第N段水位警告";
                return model;
            }


            switch (model.groupCode) {
                case "REMIT_ERR":
                    procRemitErr(model);
                    if ("".Equals(model.rtnCode))
                        model.rtnCode = "S";
                    break;
            }

            return model;
        }

        /// <summary>
        /// 水位告警
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private FastMailModel procRemitErr(FastMailModel model)
        {

            SysParaDao sysParaDao = new SysParaDao();
            Dictionary<string, string> failMap = new Dictionary<string, string>();
            MailUtil mailUtil = new MailUtil();
            List<UserBossModel> notify = mailUtil.getMailGrpId("REMIT_ERR");
            

            string amt = sysParaDao.qryByKey("RT", "WaterLevel", model.reserve1 == "1" ? "alert1" : "alert2").PARA_VALUE;


            string mailContent = "可用餘額低於第";
            mailContent += model.reserve1 == "1" ? "一" : "二";
            mailContent += $@"段水位警示{amt}萬，請確認資金是否足夠支應";

            bool bSuccess = mailUtil.sendMailMulti(notify
            , "可用餘額低於水位，需資金調撥"
            , mailContent
            , true
           , ""
           , ""
           , null
           , true
           , true
           , model.reserve1 == "1" ? "alert1" : "alert2" + ":" + amt);

            if (!bSuccess)
            {
                model.rtnCode = "F";
                model.errorMsg += "MAIL寄送失敗;";
            }

            return model;
        }

        ///// <summary>
        ///// 水位告警
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //private FastMailModel procRemitErr(FastMailModel model) {
        //    List<MailNotifyModel> otherNotify = new List<MailNotifyModel>();
        //    Dictionary<string, UserBossModel> empMap = new Dictionary<string, UserBossModel>();
        //    OaEmpDao oaEmpDao = new OaEmpDao();
        //    SysParaDao sysParaDao = new SysParaDao();
        //    Dictionary<string, string> failMap = new Dictionary<string, string>();
        //    MailUtil mailUtil = new MailUtil();
        //    List<UserBossModel> notify = mailUtil.getMailGrpId("REMIT_ERR");

        //    EacConnection con = new EacConnection();
        //    EacCommand cmd = new EacCommand();
        //    con.ConnectionString = CommonUtil.GetEasycomConn();
        //    con.Open();
        //    cmd.Connection = con;

        //    FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
        //    otherNotify = fRTMailNotify.qryNtyUsr("REMIT_ERR");


        //    string amt = sysParaDao.qryByKey("RT", "WaterLevel", model.reserve1 == "1" ? "alert1" : "alert2").PARA_VALUE;

        //    foreach (MailNotifyModel dUser in otherNotify)
        //    {
        //        if (!empMap.ContainsKey(dUser.receiverEmpno))
        //        {
        //            UserBossModel userBossModel = oaEmpDao.getEmpBoss(dUser.receiverEmpno, con, cmd);
        //            if (userBossModel != null)
        //                empMap.Add(dUser.receiverEmpno, userBossModel);
        //            else
        //            {
        //                if (!failMap.ContainsKey(dUser.receiverEmpno)) {
        //                    model.rtnCode = "F";
        //                    model.errorMsg += "查無建立人員資訊:" + dUser.receiverEmpno + ";";
        //                    logger.Error("查無建立人員資訊:" + dUser.receiverEmpno);
        //                    failMap.Add(dUser.receiverEmpno, "查無建立人員資訊");
        //                }

        //                continue;
        //            }
        //        }

        //        UserBossModel userBoss = empMap[dUser.receiverEmpno];

        //        string mailContent = "可用餘額低於第";
        //        mailContent += model.reserve1 == "1" ? "一" : "二";
        //        mailContent += $@"段水位警示{amt}萬，請確認資金是否足夠支應";

        //        bool bSuccess = mailUtil.sendMail(userBoss
        //        , "可用餘額低於水位，需資金調撥"
        //        , mailContent
        //        , true
        //       , ""
        //       , ""
        //       , null
        //       , true
        //       , (dUser.isNotifyMgr == "Y" ? true : false)
        //       , (dUser.isNotifyDeptMgr == "Y" ? true : false)
        //       , true
        //       , dUser.receiverEmpno);

        //        if (!bSuccess) {
        //            model.rtnCode = "F";
        //            model.errorMsg += "MAIL寄送失敗;";
        //        }


        //    }
        //    return model;
        //}


        public partial class FastMailModel
        {

            [Required]
            public string groupCode { get; set; }

            [Required]
            public string reserve1 { get; set; }

            public string rtnCode { get; set; }

            public string errorMsg { get; set; }
        }
    }
}