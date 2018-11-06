using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Actual
{
    public class TreasuryRegisterSearchReport: Common, ITreasuryRegisterSearchReport
    {
        public TreasuryRegisterSearchReport()
        {

        }

        #region GetData
        /// <summary>
        /// 取得查詢結果資料
        /// </summary>
        /// <param name="vCreate_Date_From">入庫日期(起)</param>
        /// <param name="vCreate_Date_To">入庫日期(迄)</param>
        /// <param name="vTrea_Register_Id">金庫登記簿單號</param>
        /// <returns></returns>
        public List<TreasuryRegisterSearch> GetSearchList(string vCreate_Date_From, string vCreate_Date_To, string vTrea_Register_Id)
        {
            var result = new List<TreasuryRegisterSearch>();
            DateTime DateFrom, DateTo;

            DateTime.TryParse(vCreate_Date_From, out DateFrom);
            if (DateTime.TryParse(vCreate_Date_To, out DateTo))
                DateTo = DateTo.DateToLatestTime();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _Open_Trea_Type = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE").ToList();
                var _Regi_Status = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();

                result = GetSearchModel(db.TREA_OPEN_REC.AsNoTracking()
                    .Where(x => x.CREATE_DT >= DateFrom, !vCreate_Date_From.IsNullOrWhiteSpace())
                    .Where(x => x.CREATE_DT <= DateTo, !vCreate_Date_To.IsNullOrWhiteSpace())
                    .Where(x => x.TREA_REGISTER_ID == vTrea_Register_Id, !vTrea_Register_Id.IsNullOrWhiteSpace())
                    .AsEnumerable()
                    , _Open_Trea_Type, _Regi_Status).ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得明細資料
        /// </summary>
        /// <param name="vTrea_Register_Id">金庫登記簿單號</param>
        /// <returns></returns>
        public List<TreasuryRegisterDetail> GetDetailList(string vTrea_Register_Id)
        {
            var result = new List<TreasuryRegisterDetail>();

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
                result = GetDetailModel(db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.TREA_REGISTER_ID == vTrea_Register_Id)
                    .AsEnumerable(), _Item_Desc, _Access_Type, _Seal_Desc, _Confirm).ToList();
            }

            return result;
        }
        #endregion

        #region privation function
        /// <summary>
        /// 開庫紀錄檔資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Open_Trea_Type"></param>
        /// <param name="_Regi_Status"></param>
        /// <returns></returns>
        private IEnumerable<TreasuryRegisterSearch> GetSearchModel(IEnumerable<TREA_OPEN_REC> data, List<SYS_CODE> _Open_Trea_Type, List<SYS_CODE> _Regi_Status)
        {
            return data.Select(x => new TreasuryRegisterSearch()
            {
                vOpen_Trea_Type = _Open_Trea_Type.FirstOrDefault(y => y.CODE == x.OPEN_TREA_TYPE)?.CODE_VALUE,  //開庫類型
                vTrea_Register_Id = x.TREA_REGISTER_ID, //金庫登記簿單號
                vCreate_Dt = DateTime.Parse(x.CREATE_DT.ToString()).DateToTaiwanDate(9),    //入庫日期
                vOpen_Trea_Time = x.OPEN_TREA_TIME, //開庫時間
                vActual_Put_Time = string.IsNullOrEmpty(x.ACTUAL_PUT_TIME.ToString()) ? null : DateTime.Parse(x.ACTUAL_PUT_TIME.ToString()).ToString("HH:mm"),  //實際入庫時間
                vActual_Get_Time = string.IsNullOrEmpty(x.ACTUAL_GET_TIME.ToString()) ? null : DateTime.Parse(x.ACTUAL_GET_TIME.ToString()).ToString("HH:mm"),  //實際出庫時間
                vRegi_Status = _Regi_Status.FirstOrDefault(y => y.CODE == x.REGI_STATUS)?.CODE_VALUE    //登記簿狀態
            });
        }

        /// <summary>
        /// 申請單紀錄檔檔資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Item_Desc"></param>
        /// <param name="_Access_Type"></param>
        /// <param name="_Seal_Desc"></param>
        /// <param name="_Confirm"></param>
        /// <returns></returns>
        private IEnumerable<TreasuryRegisterDetail> GetDetailModel(IEnumerable<TREA_APLY_REC> data, List<TREA_ITEM> _Item_Desc, List<SYS_CODE> _Access_Type, List<BeforeOpenTreasurySeal> _Seal_Desc, List<V_EMPLY2> _Confirm)
        {
            return data.Select(x => new TreasuryRegisterDetail()
            {
                vItem_Op_Type= _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE,   //入庫作業類型
                vItem_Desc = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,   //存取項目
                vSeal_Desc = _Seal_Desc.FirstOrDefault(y => y.vAply_No == x.APLY_NO)?.vSeal_Desc,   //印章內容
                vAccess_Type = _Access_Type.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE,   //作業別
                vAply_No = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "3" ? x.APLY_NO : "",    //申請單號
                vAccess_Reason = x.ACCESS_REASON,   //入庫原因
                vConfirm = x.CONFIRM_UID + "-" + _Confirm.FirstOrDefault(y => y.USR_ID == x.CONFIRM_UID)?.EMP_NAME,    //入庫人員
                vActual_Access_Type=string.IsNullOrEmpty(x.ACTUAL_ACCESS_TYPE)?null: _Access_Type.FirstOrDefault(y => y.CODE == x.ACTUAL_ACCESS_TYPE)?.CODE_VALUE,   //實際作業別
                vActual_Access_Uid=string.IsNullOrEmpty(x.ACTUAL_ACCESS_UID)?null:x.ACTUAL_ACCESS_UID+"-"+_Confirm.FirstOrDefault(y=>y.USR_ID==x.ACTUAL_ACCESS_UID)?.EMP_NAME   //實際入庫人員
            });
        }
        #endregion
    }
}