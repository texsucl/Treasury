using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 電子憑證
/// 初版作者：20180723 張家華
/// 修改歷程：20180723 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Service.Actual
{
    public class Common
    {
        protected INTRA intra { private set; get; }

        public Common()
        {
            intra = new INTRA();
        }

        #region Get Date

        #endregion

        #region Save Data

        /// <summary>
        /// 共用存檔 申請單紀錄檔 回傳參數1 (APLY_NO) 回傳參數2 (log訊息)
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="taData">viewModel</param>
        /// <param name="logStr">紀錄訊息</param>
        /// <param name="dt">修改時間</param>
        /// <returns></returns>
        protected Tuple<string,string> SaveTREA_APLY_REC(TreasuryDBEntities db, TreasuryAccessViewModel taData, string logStr, DateTime dt)
        {
             
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

            if (logStr.IsNullOrWhiteSpace())
                logStr = string.Empty;

            #region 申請單紀錄檔
            var _TAR = new TREA_APLY_REC()
            {
                APLY_NO = $@"G6{qPreCode}{cId}", //申請單號 G6+系統日期YYYMMDD(民國年)+3碼流水號
                APLY_FROM = Ref.AccessProjectStartupType.M.ToString(), //人工
                ITEM_ID = taData.vItem, //申請項目
                ACCESS_TYPE = taData.vAccessType, //存入(P) or 取出(G)
                ACCESS_REASON = taData.vAccessReason, //申請原因
                APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(), //表單申請
                EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(taData.vExpectedAccessDate), //預計存取日期
                APLY_UNIT = taData.vAplyUnit, //申請單位
                APLY_UID = taData.vAplyUid, //申請人
                APLY_DT = dt,
                CREATE_UNIT = taData.vCreateUnit, //新增單位
                CREATE_UID = taData.vCreateUid, //新增人
                CREATE_DT = dt,
                LAST_UPDATE_UID = taData.vCreateUid,
                LAST_UPDATE_DT = dt
            };
            if (taData.vAplyUid != taData.vCreateUid) //當申請人不是新增人(代表為保管單位代申請)
            {
                _TAR.CUSTODY_UID = taData.vCreateUid; //保管單位直接帶 新增人
                _TAR.CONFIRM_DT = dt;
            }
            logStr += _TAR.modelToString(logStr);
            db.TREA_APLY_REC.Add(_TAR);
            #endregion

            #region 申請單歷程檔
            var _ARH = new APLY_REC_HIS()
            {
                APLY_NO = _TAR.APLY_NO,
                APLY_STATUS = _TAR.APLY_STATUS,
                PROC_DT = dt,
                PROC_UID = _TAR.CREATE_UID
            };
            logStr += _ARH.modelToString(logStr);
            db.APLY_REC_HIS.Add(_ARH);
            #endregion

            return new Tuple<string, string>(_TAR.APLY_NO , logStr); 
        }

        #endregion

    }
}