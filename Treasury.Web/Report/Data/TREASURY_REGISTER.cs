using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class TREASURY_REGISTER : ReportTreasuryRegister
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();
            bool temp = false;
            string vTreaRegisterId = parms.Where(x => x.key == "vTreaRegisterId").FirstOrDefault()?.value ?? string.Empty;
            string vUser_Id = parms.Where(x => x.key == "vUser_Id").FirstOrDefault()?.value ?? string.Empty;
            string vTemp = parms.Where(x => x.key == "vTemp").FirstOrDefault()?.value ?? string.Empty;

            if (vTemp == "Y")
                temp = true;
            var Aply_Status = Ref.AccessProjectFormStatus.C02.ToString();
            SetDetail(vTreaRegisterId, vUser_Id);

            //報表資料
            List<Report_Treasury_Register> ReportDataList = new List<Report_Treasury_Register>();

            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }

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

                ReportDataList = GetReportModel(db.TREA_APLY_REC.AsNoTracking()
                    //.Where(x => x.CONFIRM_UID != null)
                    //.Where(x => x.APLY_STATUS == Aply_Status)
                    .Where(x => x.TREA_REGISTER_ID == vTreaRegisterId)
                    .AsEnumerable(), _Item_Desc, _Access_Type, _Seal_Desc, emps).ToList();

                if (temp)
                {
                    var _TREA_APLY_REC_ITEM_ID =
                        db.TREA_APLY_REC.AsNoTracking()
                        .Where(x => x.TREA_REGISTER_ID == vTreaRegisterId)
                        .Select(x => x.ITEM_ID).ToList();
                    List<string> confirmedItemId = new List<string>();
                    confirmedItemId.AddRange(_TREA_APLY_REC_ITEM_ID);
                    ReportDataList.AddRange(GetRoutineModel(
                        db.TREA_APLY_TEMP.AsNoTracking()
                        .Where(x => !confirmedItemId.Contains(x.ITEM_ID))
                        .AsEnumerable(), _Item_Desc).ToList());


                }

                ReportDataList = ReportDataList.OrderByDescending(x => x.ACCESS_NAME).ThenBy(x => x.APLY_NO).ToList();

                if (temp)
                    ReportDataList.AddRange(new List<Report_Treasury_Register>() {
                        new Report_Treasury_Register(),
                        new Report_Treasury_Register()
                    });
            }

            resultsTable.Tables.Add(ReportDataList.ToDataTable());

            SetExtensionParm();

            return resultsTable;
        }

        /// <summary>
        /// 申請單紀錄暫存檔資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Item_Desc"></param>
        /// <returns></returns>
        private IEnumerable<Report_Treasury_Register> GetRoutineModel(IEnumerable<TREA_APLY_TEMP> data, List<TREA_ITEM> _Item_Desc)
        {
            return data.Select(x => new Report_Treasury_Register()
            {
                ITEM_DESC = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC   //代碼.庫存狀態 
            });
        }

        /// <summary>
        /// 申請單紀錄檔資料轉報表資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Item_Desc"></param>
        /// <param name="_Access_Type"></param>
        /// <param name="_Seal_Desc"></param>
        /// <param name="_Confirm"></param>
        /// <returns></returns>
        private IEnumerable<Report_Treasury_Register> GetReportModel(IEnumerable<TREA_APLY_REC> data, List<TREA_ITEM> _Item_Desc, List<SYS_CODE> _Access_Type, List<BeforeOpenTreasurySeal> _Seal_Desc, List<V_EMPLY2> _Confirm)
        {
            return data.Select(x => new Report_Treasury_Register()
            {
                ITEM_DESC = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,   //存取項目
                SEAL_DESC = _Seal_Desc.FirstOrDefault(y => y.vAply_No == x.APLY_NO)?.vSeal_Desc,   //印章內容
                ACCESS_TYPE = _Access_Type.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE,   //代碼,作業別
                APLY_NO = _Item_Desc.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "3" ? x.APLY_NO : "",    //申請單號
                ACCESS_REASON = x.ACCESS_REASON,   //入庫原因
                ACCESS_NAME = x.CONFIRM_UID + "-" + _Confirm.FirstOrDefault(y => y.USR_ID == x.CONFIRM_UID)?.EMP_NAME,    //入庫人員
                ACTUAL_ACCESS_TYPE = _Access_Type.FirstOrDefault(y => y.CODE == x.ACTUAL_ACCESS_TYPE)?.CODE_VALUE,    //實際作業別
                ACTUAL_ACCESS_NAME = string.IsNullOrEmpty(x.ACTUAL_ACCESS_UID) ? null : x.ACTUAL_ACCESS_UID + "-" + _Confirm.FirstOrDefault(y => y.USR_ID == x.ACTUAL_ACCESS_UID)?.EMP_NAME    //實際入庫人員
            });
        }

    }
}