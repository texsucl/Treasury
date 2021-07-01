using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Http;

namespace FRT.Web.Controllers
{
    [RoutePrefix("FastRemit")]
    public class FastRemitController : ApiController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 處理快速付款失敗案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("RemitOms")]
        [ValidateModel]
        public IHttpActionResult RemitOms(RemitOmsModel model)
        {
            string inputKey = model.mailGroup + "|" + model.sysType + "|" + model.srceFrom + "|" + model.srceKind;
            logger.Info("mailGroup:" + model.mailGroup);
            logger.Info("sysType:" + model.sysType);
            logger.Info("srceFrom:" + model.srceFrom);
            logger.Info("srceKind:" + model.srceKind);

            model.mailGroup = model.mailGroup.Trim();
            model.sysType = model.sysType.Trim();
            model.srceFrom = model.srceFrom.Trim();
            model.srceKind = model.srceKind.Trim();

            switch (StringUtil.toString(model.sysType)) {
                case "":
                    model.rtnCode = "1";
                    model.errorMsg = "系統代號未輸入!!";
                    break;
                case "A":
                    if ("".Equals(StringUtil.toString(model.srceKind))) {
                        model.rtnCode = "1";
                        model.errorMsg = "系統代號=A時，資料類別必需傳入!!";
                    }
                    break;
                case "F":
                    if ("".Equals(StringUtil.toString(model.srceFrom)))
                    {
                        model.rtnCode = "1";
                        model.errorMsg = "系統代號=A時，資料來源必需傳入!!";
                    }
                    break;
            }

            if (!"1".Equals(model.rtnCode)) {

                //查詢這筆資料對應要MAIL的群組
                SysCodeDao sysCodeDao = new SysCodeDao();
                string[] mailGrp = new string[] { "REMIT_OMS_BENE", "REMIT_OMS_CL" };
                SYS_CODE sysCode = new SYS_CODE();

                if ("A".Equals(model.sysType))
                    sysCode = sysCodeDao.qryByReserve("RT", "MAIL_GROUP", mailGrp, StringUtil.toString(model.srceKind), "", "", model.mailGroup);
                else
                    sysCode = sysCodeDao.qryByReserve("RT", "MAIL_GROUP", mailGrp, "", StringUtil.toString(model.srceFrom), "", model.mailGroup);


                if ("".Equals(StringUtil.toString(sysCode.CODE)))
                {
                    model.rtnCode = "1";
                    model.errorMsg = "查無建立人員及MAIL群組資訊!!";
                }
                else
                {
                    logger.Info("inputKey:" + inputKey + "==> Code:" + sysCode.CODE);
                    FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
                    List<MailNotifyModel> otherNotify = fRTMailNotify.qryNtyUsr(sysCode.CODE);

                    if (otherNotify.Count == 0)
                    {
                        model.rtnCode = "1";
                        model.errorMsg = "查無建立人員及MAIL群組資訊!!";
                    }
                    else
                    {
                        foreach (MailNotifyModel d in otherNotify)
                        {
                            model.mailTo += d.receiverEmpno + "|";
                        }
                    }
                }

            }


            logger.Info("inputKey:" + inputKey + "==> rtnCode:" + model.mailGroup);
            logger.Info("inputKey:" + inputKey + "==> rtnCode:" + model.rtnCode);
            logger.Info("inputKey:" + inputKey + "==> errorMsg:" + model.errorMsg);
            logger.Info("inputKey:" + inputKey + "==> mailTo:" + model.mailTo);

            return Ok(model);
        }


        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class RemitOmsModel
        {
            /// <summary>
            /// 寄信群組
            /// </summary>
            public string mailGroup { get; set; }

            /// <summary>
            ///系統別
            /// </summary>
            public string sysType { get; set; }

            /// <summary>
            /// 資料來源
            /// </summary>
            public string srceFrom { get; set; }

            /// <summary>
            /// 資料類別
            /// </summary>
            public string srceKind { get; set; }

            /// <summary>
            /// 寄信對象
            /// </summary>
            public string mailTo { get; set; }

            /// <summary>
            /// 0:成功;1失敗
            /// </summary>
            public string rtnCode { get; set; }


            /// <summary>
            /// 錯誤訊息
            /// </summary>
            public string errorMsg { get; set; }
        }
    }
}
