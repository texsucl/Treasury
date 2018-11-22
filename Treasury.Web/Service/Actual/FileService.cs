using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebDaos;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;
/// <summary>
/// 功能說明：金庫上傳檔案
/// 初版作者：20180716 張家華
/// 修改歷程：20180716 張家華 
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
    public class FileService : IFileService
    {


        public FileService()
        {

        }

        #region Get Date

        public List<SelectOption> SearchUserID(string userName)
        {
            List<SelectOption> results = new List<SelectOption>();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                if (userName.Length >= 2)
                {
                    userName = userName.Remove(1, 1);
                    userName = userName.Insert(1, "Ｘ");
                }                  
                results = db.V_EMPLY2.AsNoTracking()
                  .Where(x => x.EMP_NAME != null &&
                  x.USR_ID != null &&
                  x.EMP_NAME.Trim() == userName )
                  .AsEnumerable()
                  .Select(x => new SelectOption()
                  {
                      Value = x.USR_ID,
                      Text = $@"{x.DPT_NAME}({x.USR_ID})"
                  }).ToList();
            }
            return results;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 存入 Excel上傳資料 ItemBook(Estate)
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public MSGReturnModel<string> saveItemBookEstate(List<FileItemBookEstateModel> datas)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            if (!datas.Any())
                return result;

            var item_id = TreaItemType.D1014.ToString();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var itemBooks = db.ITEM_BOOK.Where(x => x.ITEM_ID == item_id).ToList();
                datas.ForEach(x =>
                {
                    int? groupNp = TypeTransfer.stringToIntN(x.BOOK_NO);
                    if (groupNp != null)
                    {
                        var _itemBooks = itemBooks.Where(y => y.GROUP_NO == groupNp, groupNp != null).ToList();
                        saveItemBook(db, _itemBooks, "BOOK_NO", "冊號", x.BOOK_NO, item_id, groupNp.Value);
                        saveItemBook(db, _itemBooks, "BUILDING_NAME", "大樓名稱", x.BUILDING_NAME, item_id, groupNp.Value);
                        saveItemBook(db, _itemBooks, "LOCATED", "坐落", x.LOCATED, item_id, groupNp.Value);
                        saveItemBook(db, _itemBooks, "MEMO", "備註", x.MEMO, item_id, groupNp.Value);
                    }                
                });
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

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = MessageType.upload_Success.GetDescription();
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

            return result;
        }

        public MSGReturnModel<string> saveEstate(List<FileEstateModel> datas)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();

            if (!datas.Any())
                return result;

            string logStr = string.Empty; //log
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = Treasury.WebBO.DateUtil.getCurChtDateTime().Split(' ')[0];
            var item_Seq = "E3"; //不動產權狀流水號開頭編碼    

            List<ITEM_REAL_ESTATE> insertDatas = new List<ITEM_REAL_ESTATE>();

            List<VW_OA_DEPT> VOAs = new List<VW_OA_DEPT>();
            List<V_EMPLY2> VEs = new List<V_EMPLY2>();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                VOAs = db.VW_OA_DEPT.AsNoTracking().Where(x=>x.DPT_CD != null).ToList();
                VEs = db.V_EMPLY2.AsNoTracking().Where(x=>x.USR_ID != null).ToList();
            }
            int count = 0;
            datas.ForEach(x =>
            {
                var _V_EMPLY2 = VEs.FirstOrDefault(y => y.USR_ID == x.APLY_UID && y.DPT_CD != null);
                var _VW_OA_DEPT = VOAs.FirstOrDefault(z => z.DPT_CD.Trim() == _V_EMPLY2?.DPT_CD?.Trim());

                //insertDatas.Add(new ITEM_REAL_ESTATE() {

                //})
            });

            return result;
        }

        #endregion

        #region Excel

        #region Excel 資料轉成 A59ViewModel

        /// <summary>
        /// Excel 資料轉成 iewModel
        /// </summary>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="path">檔案路徑</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Tuple<string, IEnumerable<IFileModel>> getExcel(string pathType, string path, ExcelName excelName)
        {
            List<IFileModel> dataModel = new List<IFileModel>();
            IWorkbook wb = null;
            string msg = string.Empty;
            try
            {
                switch (pathType) //判斷型別
                {
                    case "xls":
                        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            wb = new HSSFWorkbook(stream);
                        }
                        break;

                    case "xlsx":
                        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            wb = new XSSFWorkbook(stream);
                        }
                        break;
                }
                ISheet sheet = wb.GetSheetAt(0);

                dataModel = (List<IFileModel>)(sheet.ISheetToFileModel(excelName));
                if (!dataModel.Any()) //判斷有無資料
                {
                    return new Tuple<string, IEnumerable<IFileModel>>(MessageType.not_Find_Any.GetDescription(), dataModel);
                }
            }
            catch (Exception ex)
            { }
            return new Tuple<string, IEnumerable<IFileModel>>(msg, dataModel);
        }

        #endregion Excel 資料轉成 A59ViewModel

        #endregion

        #region privateFunction

        /// <summary>
        /// 判斷 ITEMBOOK 修改或新增
        /// </summary>
        /// <param name="db"></param>
        /// <param name="datas"></param>
        /// <param name="COL"></param>
        /// <param name="COL_Name"></param>
        /// <param name="COL_Value"></param>
        /// <param name="itemId"></param>
        /// <param name="groupNo"></param>
        private void saveItemBook(TreasuryDBEntities db, List<ITEM_BOOK> datas, string COL,string COL_Name, string COL_Value ,string itemId,int groupNo)
        {
            var item = datas.FirstOrDefault(z => z.COL == COL);
            if (item != null)
                item.COL_VALUE = COL_Value;
            else
                db.ITEM_BOOK.Add(new ITEM_BOOK()
                {
                    ITEM_ID = itemId,
                    GROUP_NO = groupNo,
                    COL = COL,
                    COL_NAME = COL_Name,
                    COL_VALUE = COL_Value
                });
        }

        #endregion
    }
}