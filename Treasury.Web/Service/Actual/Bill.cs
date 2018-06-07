using System;
using System.Collections.Generic;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{
    public class Bill : IBill
    {
        public Bill()
        {

        }

        #region GetData
        public IEnumerable<ITreaItem> GetTempData(string aplyNo)
        {
            var result = new List<BillViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
                var _Inventory_types = sys_codes
                    .Where(x => x.CODE_TYPE == SysCodeType.INVENTORY_TYPE.ToString()).ToList();
                result = db.BLANK_NOTE_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == aplyNo)
                    .OrderBy(x => x.ISSUING_BANK)
                    .ThenBy(x => x.CHECK_TYPE)
                    .ThenBy(x => x.CHECK_NO_TRACK)
                    .AsEnumerable()
                    .Select((x, y) => BlankNoteAplyToBillViewModel(x, y, _Inventory_types)).ToList();
            }

            return result;
        }

        public IEnumerable<ITreaItem> GetDayData(string aplyNo = null)
        {
            var result = new List<BillViewModel>();
            if (!aplyNo.IsNullOrWhiteSpace())
            {
                result.AddRange((List<BillViewModel>)GetTempData(aplyNo));
            }
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
                var _Inventory_types = sys_codes
                    .Where(x => x.CODE_TYPE == SysCodeType.INVENTORY_TYPE.ToString()).ToList();
                result.AddRange(db.ITEM_BLANK_NOTE.AsNoTracking()
                    .AsEnumerable()
                    .Select(x => ItemBlankNoteToBillViewModel(x, sys_codes)));
            }
            return result;
        }

        public List<SelectOption> GetCheckType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "CHECK_TYPE")
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

        #endregion

        #region SaveData
        public MSGReturnModel<ITreaItem> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData)
        {
            var result = new MSGReturnModel<ITreaItem>();
            result.RETURN_FLAG = false;

            var datas = (List<BillViewModel>)insertDatas;
            if (insertDatas != null)
            {

            }
            else
            {

            }
            return result;
        }
        #endregion

        #region privation function

        private BillViewModel BlankNoteAplyToBillViewModel(BLANK_NOTE_APLY data ,int num, List<SYS_CODE> Inventory_types)
        {
            return new BillViewModel()
            {
                vRowNum = (num +1).ToString(),
                vItemId = data.ITEM_ID,
                vAplyNo = data.APLY_NO,
                vDataSeq = data.DATA_SEQ.ToString(),
                vStatus = Inventory_types.FirstOrDefault(x => x.CODE == "3")?.CODE_VALUE,//預約存入
                vIssuingBank = data.ISSUING_BANK,
                vCheckType = data.CHECK_TYPE,
                vCheckNoTrack = data.CHECK_NO_TRACK,
                vCheckNoB = data.CHECK_NO_B,
                vCheckNoE = data.CHECK_NO_E,
                vCheckTotalNum = (TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString()
            };
        }

      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Inventory_types">庫存狀態</param>
        /// <returns></returns>
        private BillViewModel ItemBlankNoteToBillViewModel(ITEM_BLANK_NOTE data,List<SYS_CODE> Inventory_types)
        {
            return new BillViewModel()
            {
                vItemId = data.ITEM_ID,
                vAplyNo = null,
                vDataSeq = null,
                vStatus = Inventory_types.FirstOrDefault(x => x.CODE == data.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態
                vIssuingBank = data.ISSUING_BANK,
                vCheckType = data.CHECK_TYPE, 
                vCheckNoTrack = data.CHECK_NO_TRACK, 
                vCheckNoB = data.CHECK_NO_B,
                vCheckNoE = data.CHECK_NO_E,
                vCheckTotalNum = (TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString()
            };
        }

        #endregion
    }

}