using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.WebUtility;
using Treasury.Web.ViewModels;

namespace Treasury.Web.Service.Actual
{
    public class BeforeOpenTreasury: Common, IBeforeOpenTreasury
    {
        public BeforeOpenTreasury()
        {

        }

        #region GetData
        /// <summary>
        /// 開庫類型
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetOpenTreaType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 取得每日例行進出未確認項目
        /// </summary>
        /// <returns></returns>
        public List<BeforeOpenTreasuryViewModel> GetRoutineList()
        {
            var result = new List<BeforeOpenTreasuryViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _Item_Desc = db.TREA_ITEM.AsNoTracking().ToList();
                result = GetRoutineModel(db.TREA_APLY_TEMP.AsNoTracking().AsEnumerable(), _Item_Desc).ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得已入庫確認資料
        /// </summary>
        /// <returns></returns>
        public List<BeforeOpenTreasuryViewModel> GetStorageList()
        {
            var result = new List<BeforeOpenTreasuryViewModel>();
            var Aply_Status = Ref.AccessProjectFormStatus.C02.ToString();
            var EAD = DateTime.Now.ToString("yyyy-MM-dd");
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _Item_Desc = db.TREA_ITEM.AsNoTracking().ToList();
                var _Access_Type = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();
                var _Seal_Desc = db.OTHER_ITEM_APLY.AsNoTracking().Join(db.ITEM_SEAL.AsNoTracking(),
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
                    .Where(x => x.TREA_REGISTER_ID == null)
                    .Where(x => x.CONFIRM_UID != null)
                    .Where(x => x.APLY_STATUS == Aply_Status)
                    .Where(x => x.EXPECTED_ACCESS_DATE.ToString() == EAD)
                    .AsEnumerable(), _Item_Desc, _Access_Type, _Seal_Desc, _Confirm).ToList();

                //設定印章內容

            }

            return result;
        }

        #endregion

        #region SaveData
        /// <summary>
        /// 產生工作底稿
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITreaItem>> DraftData()
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
                    //查詢【開庫紀錄檔】，是否有尚待執行開庫的申請資料
                    var _TOR_List = db.TREA_OPEN_REC.AsNoTracking()
                        .Where(x => x.REGI_STATUS == Regi_Status)
                        .Where(x => x.OPEN_TREA_DATE.ToString() == OTD)
                        .AsEnumerable().ToList();

                    if(_TOR_List.Any())
                    {
                        foreach(var item in _TOR_List)
                        {
                            //異動【開庫紀錄檔】
                            var _TOR_Data = new TREA_OPEN_REC();

                        }
                    }
                    else
                    {
                        result.DESCRIPTION = "無可供開庫作業的申請資料，若需開庫，請執行指定時間開庫申請!!";
                    }
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