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

        protected List<string> TreasuryIn { private set; get; } //資料庫異動 金庫內查詢狀態
        protected string TreasuryOut { private set; get; }//資料庫異動 金庫外查詢狀態

        public Common()
        {
            intra = new INTRA();
            TreasuryIn = new List<string>()
            {
                "1", //在庫
                "8" //資料異動中
            };
            TreasuryOut = "2"; //已被取出
        }

        #region Get Date

        /// <summary>
        /// get SysCode by CodeType
        /// </summary>
        /// <param name="codeType"></param>
        /// <returns></returns>
        public List<SelectOption> GetSysCode(string codeType)
        {
            var result = new List<SelectOption>();
            if (codeType.IsNullOrWhiteSpace())
                return result;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == codeType)
                    .OrderBy(x=>x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Text = x.CODE_VALUE,
                        Value = x.CODE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 獲取 員工資料
        /// </summary>
        /// <returns></returns>
        public List<V_EMPLY2> GetEmps()
        {
            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
            return emps;
        }

        /// <summary>
        /// 獲取 部門資料
        /// </summary>
        /// <returns></returns>
        public List<VW_OA_DEPT> GetDepts()
        {
            var depts = new List<VW_OA_DEPT>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
            }
            return depts;
        }

        /// <summary>
        /// 取得新值
        /// </summary>
        /// <param name="Old">原值</param>
        /// <param name="Aft">異動值</param>
        /// <returns></returns>
        protected string GetNewValue(string Old, string Aft)
        {
            string NewValue = string.Empty;

            if (string.IsNullOrEmpty(Aft))
                NewValue = Old;
            else if (Aft.ToUpper() == "NULL")
                NewValue = null;
            else
                NewValue = Aft;

            return NewValue;
        }

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
                _TAR.CUSTODY_DT = dt;
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

        /// <summary>
        /// 共用存檔 資料庫異動申請單紀錄檔 回傳參數1 (APLY_NO) 回傳參數2 (log訊息)
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="data">viewModel</param>
        /// <param name="logStr">紀錄訊息</param>
        /// <param name="dt">時間</param>
        /// <param name="CHG_AUTH_UNIT">權責單位異動</param>
        /// <returns></returns>
        protected Tuple<string, string> SaveINVENTORY_CHG_APLY(TreasuryDBEntities db, CDCSearchViewModel data, string logStr, DateTime dt, string CHG_AUTH_UNIT = "N")
        {
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            var cId = sysSeqDao.qrySeqNo("G8", qPreCode).ToString().PadLeft(3, '0');

            #region 資料庫異動申請單紀錄檔  
            var ICA = new INVENTORY_CHG_APLY()
            {
                APLY_NO = $@"G8{qPreCode}{cId}", //申請單號 G8+系統日期YYYMMDD(民國年)+3碼流水號
                ITEM_ID = data.vJobProject,
                APPR_STATUS = "1", //表單申請
                CHG_AUTH_UNIT = CHG_AUTH_UNIT, //權責單位異動
                CREATE_UID = data.vCreate_Uid,
                CREATE_Date = dt.Date,
                CREATE_Time = dt.TimeOfDay
            };
            db.INVENTORY_CHG_APLY.Add(ICA);
            logStr += ICA.modelToString(logStr);
            #endregion
            return new Tuple<string, string>(ICA.APLY_NO, logStr);
        }

        #endregion

    }
}