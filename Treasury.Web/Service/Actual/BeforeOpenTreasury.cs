using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.WebUtility;
using Treasury.Web.ViewModels;
using System.Data.Entity.Infrastructure;
using Treasury.WebDaos;

namespace Treasury.Web.Service.Actual
{
    public class BeforeOpenTreasury : Common, IBeforeOpenTreasury
    {
        public BeforeOpenTreasury()
        {

        }

        #region GetData
        /// <summary>
        /// 金庫登記簿
        /// </summary>
        /// <returns></returns>
        public TreaOpenRec GetTreaOpenRec()
        {
            var result = new TreaOpenRec();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var Regi_Status = Ref.AccessProjectFormStatus.C02.ToString();
                var OTD = DateTime.Now.ToString("yyyy-MM-dd");
                var OTT = DateTime.Now.ToString("HH:mm");
                var _OpenTreaType = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE");

                result = db.TREA_OPEN_REC.AsNoTracking()
                    .Where(x => x.REGI_STATUS == Regi_Status && x.APPR_STATUS != "4")
                    .AsEnumerable()
                   .Select(x => new TreaOpenRec()
                   {
                       vTreaRegisterId = x.TREA_REGISTER_ID,
                       vOpenTreaTypeName = _OpenTreaType.FirstOrDefault(y => y.CODE == x.OPEN_TREA_TYPE)?.CODE_VALUE
                   }).FirstOrDefault();
            }

            //無符合開庫資料
            if (result == null)
            {
                result = new TreaOpenRec() { vTreaRegisterId = "", vOpenTreaTypeName = "" };
            }

            return result;
        }

        /// <summary>
        /// 取得每日例行進出未確認項目
        /// </summary>
        /// <returns></returns>
        public List<BeforeOpenTreasuryViewModel> GetRoutineList(string TreaRegisterId)
        {
            var result = new List<BeforeOpenTreasuryViewModel>();
            List<string> confirmedItemId = new List<string>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _Item_Desc = db.TREA_ITEM.AsNoTracking().ToList();
                var _TREA_APLY_REC_ITEM_ID = db.TREA_APLY_REC.AsNoTracking().Where(x => x.TREA_REGISTER_ID == TreaRegisterId).Where(x => x.APLY_STATUS == "C02")
                    .Select(x => x.ITEM_ID)
                    .ToList();
                confirmedItemId.AddRange(_TREA_APLY_REC_ITEM_ID);

                result = GetRoutineModel(db.TREA_APLY_TEMP.AsNoTracking().Where(x => !confirmedItemId.Contains(x.ITEM_ID)).AsEnumerable(), _Item_Desc).ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得已入庫確認資料
        /// </summary>
        /// <param name="TreaRegisterId">金庫登記簿單號</param>
        /// <returns></returns>
        public List<BeforeOpenTreasuryViewModel> GetStorageList(string TreaRegisterId)
        {
            var result = new List<BeforeOpenTreasuryViewModel>();
            var Aply_Status = Ref.AccessProjectFormStatus.C02.ToString();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _Item_Desc = db.TREA_ITEM.AsNoTracking().ToList();
                var _Access_Type = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();
                //取得入庫類型為2的印章內容
                var _Trea_Item = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_OP_TYPE == "2").Select(x => x.ITEM_ID).ToList();
                var _Trea_Aply_Rec = db.TREA_APLY_REC.AsNoTracking().Where(x => _Trea_Item.Contains(x.ITEM_ID)).Select(x => x.APLY_NO).ToList();
                var _Other_Item_Aply = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => _Trea_Aply_Rec.Contains(x.APLY_NO)).ToList();
                var _Seal_Desc = _Other_Item_Aply.Join(db.ITEM_SEAL.AsNoTracking(),
                    OIA => OIA.ITEM_ID,
                    IS => IS.ITEM_ID,
                    (OIA, IS) => new BeforeOpenTreasurySeal
                    {
                        vAply_No = OIA.APLY_NO,
                        vItem_Id = OIA.ITEM_ID,
                        vSeal_Desc = IS.SEAL_DESC
                    }).ToList();
                var _Confirm = GetEmps();
                result = GetStorageModel(db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.CONFIRM_UID != null)
                    .Where(x => x.APLY_STATUS == Aply_Status)
                    .Where(x => x.TREA_REGISTER_ID == TreaRegisterId)
                    .AsEnumerable(), _Item_Desc, _Access_Type, _Seal_Desc, _Confirm).ToList();
                result.AddRange(GetRoutineList(TreaRegisterId));
            }

            return result;
        }

        #endregion

        #region SaveData
        /// <summary>
        /// 產生工作底稿
        /// </summary>
        /// <param name="currentUserId">目前使用者ID</param>
        /// <param name="Trea_Register_Id">金庫開庫單號</param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITreaItem>> DraftData(string currentUserId,string Trea_Register_Id)
        {
            var result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;
            var Regi_Status = Ref.AccessProjectFormStatus.C02.ToString();
            var OTD = DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    string logStr = string.Empty; //log

                    //查詢【開庫紀錄檔】，是否有尚待執行開庫的申請資料
                    var _TOR_List = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == Trea_Register_Id);

                    var Update_Regi_Status = Ref.AccessProjectFormStatus.D01.ToString();

                    #region 開庫紀錄檔
                    if (_TOR_List != null)
                    {
                        //異動【開庫紀錄檔】
                        _TOR_List.REGI_STATUS = Update_Regi_Status;
                        //開庫類型=1
                        if (_TOR_List.OPEN_TREA_TYPE == "1")
                        {
                            _TOR_List.CREATE_UID = currentUserId;
                        }
                        _TOR_List.LAST_UPDATE_UID = currentUserId;
                        _TOR_List.LAST_UPDATE_DT = dt;

                        logStr += "|";
                        logStr += _TOR_List.modelToString();

                        //畫面上「已入庫確認資料」
                        var _TAR_List = db.TREA_APLY_REC
                            .Where(x => x.CONFIRM_UID != null)
                            .Where(x => x.APLY_STATUS == Regi_Status)
                            .Where(x => x.TREA_REGISTER_ID == _TOR_List.TREA_REGISTER_ID)
                            .ToList();


                            foreach (var item in _TAR_List)
                            {
                                #region 申請單紀錄檔
                                item.APLY_STATUS = Update_Regi_Status;
                                item.LAST_UPDATE_UID = currentUserId;
                                item.LAST_UPDATE_DT = dt;

                                logStr += "|";
                                logStr += item.modelToString();
                                #endregion

                                #region 申請單歷程檔
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = item.APLY_NO,
                                    APLY_STATUS = Update_Regi_Status,
                                    PROC_UID = currentUserId,
                                    PROC_DT = dt
                                });
                                #endregion
                            }
                        

                        #region Save Db
                        var validateMessage = db.GetValidationErrors().getValidateString();
                        if (validateMessage.Any())
                        {
                            result.DESCRIPTION = validateMessage;
                        }
                        else
                        {
                            try
                            {
                                db.SaveChanges();

                                #region LOG
                                //新增LOG
                                Log log = new Log();
                                log.CFUNCTION = "金庫登記簿執行作業(開庫前)";
                                log.CACTION = "A";
                                log.CCONTENT = logStr;
                                LogDao.Insert(log, currentUserId);
                                #endregion

                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = "金庫登記簿執行作業(開庫前)，執行成功";
                            }
                            catch (DbUpdateException ex)
                            {
                                result.DESCRIPTION = ex.exceptionMessage();
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        result.DESCRIPTION = "無可供開庫作業的申請資料，若需開庫，請執行指定時間開庫申請!!";
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }

            return result;
        }

        #endregion

        #region privation function
        /// <summary>
        /// 申請單紀錄暫存檔資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Item_Desc"></param>
        /// <returns></returns>
        private IEnumerable<BeforeOpenTreasuryViewModel> GetRoutineModel(IEnumerable<TREA_APLY_TEMP> data, List<TREA_ITEM> _Item_Desc)
        {
            return data.Select(x => new BeforeOpenTreasuryViewModel()
            {
                vItem_Desc = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC   //代碼.庫存狀態 
            });
        }

        /// <summary>
        /// 申請單紀錄檔資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Item_Desc"></param>
        /// <param name="_Access_Type"></param>
        /// <param name="_Seal_Desc"></param>
        /// <param name="_Confirm"></param>
        /// <returns></returns>
        private IEnumerable<BeforeOpenTreasuryViewModel> GetStorageModel(IEnumerable<TREA_APLY_REC> data, List<TREA_ITEM> _Item_Desc, List<SYS_CODE> _Access_Type, List<BeforeOpenTreasurySeal> _Seal_Desc, List<V_EMPLY2> _Confirm)
        {
            return data.Select(x => new BeforeOpenTreasuryViewModel()
            {
                vItem_Desc = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,   //存取項目
                vSeal_Desc = _Seal_Desc.FirstOrDefault(y => y.vAply_No == x.APLY_NO)?.vSeal_Desc,   //印章內容
                vAccess_Type = _Access_Type.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE,   //代碼,作業別
                vAply_No = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "3" ? x.APLY_NO : "",    //申請單號
                vAccess_Reason = x.ACCESS_REASON,   //入庫原因
                vConfirm = x.CONFIRM_UID + "-" + _Confirm.FirstOrDefault(y => y.USR_ID == x.CONFIRM_UID)?.EMP_NAME    //確認人員
            });
        }
        #endregion
    }
}