using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Transactions;
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
                  x.EMP_NAME.Trim() == userName)
                  .AsEnumerable()
                  .Select(x => new SelectOption()
                  {
                      Value = x.USR_ID,
                      Text = $@"{x.DPT_NAME}({x.USR_ID})"
                  }).ToList();
            }
            return results;
        }

        #endregion Get Date

        #region Save Data

        /// <summary>
        /// Excel資料存到DB
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MSGReturnModel<string> saveData(IEnumerable<IFileModel> datas, ExcelName type)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            if (datas == null || !datas.Any())
            {
                result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                return result;
            }

            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            DateTime dtn = DateTime.Now;

            bool _flag = true;
            int count = 0;

            switch (type)
            {
                #region 存入保證金
                case ExcelName.Marginp:
                    List<ITEM_DEP_RECEIVED> MarginpModels = new List<ITEM_DEP_RECEIVED>();
                    foreach (FileMarginpModel item in datas)
                    {
                        string item_id = string.Empty;

                        if (item.ITEM_ID.IsNullOrWhiteSpace())
                        {
                            switch (item.MARGIN_TAKE_OF_TYPE_CODE)
                            {
                                case "1":
                                    item_id = sysSeqDao.qrySeqNo("X", string.Empty).ToString().PadLeft(8, '0');
                                    item_id = $@"X{item_id}";
                                    break;
                                case "2":
                                    item_id = sysSeqDao.qrySeqNo("Y", string.Empty).ToString().PadLeft(8, '0');
                                    item_id = $@"Y{item_id}";
                                    break;
                                case "3":
                                    item_id = sysSeqDao.qrySeqNo("Z", string.Empty).ToString().PadLeft(8, '0');
                                    item_id = $@"Z{item_id}";
                                    break;
                            }

                        }
                        else
                        {
                            item_id = item.ITEM_ID?.Trim();
                        }
                       
                        var _AMOUNT = TypeTransfer.stringToDecimal(item.AMOUNT);
                        var _EFFECTIVE_DATE_B = TypeTransfer.stringToDateTimeN(item.EFFECTIVE_DATE_B);
                        var _EFFECTIVE_DATE_E = TypeTransfer.stringToDateTimeN(item.EFFECTIVE_DATE_E);
                        MarginpModels.Add(new ITEM_DEP_RECEIVED()
                        {
                            ITEM_ID = item_id,
                            INVENTORY_STATUS = "1",
                            TRAD_PARTNERS = item.TRAD_PARTNERS,
                            TRAD_PARTNERS_ACCESS = item.TRAD_PARTNERS,
                            MARGIN_TAKE_OF_TYPE = item.MARGIN_TAKE_OF_TYPE_CODE,
                            MARGIN_TAKE_OF_TYPE_ACCESS = item.MARGIN_TAKE_OF_TYPE_CODE,
                            MARGIN_ITEM = item.MARGIN_ITEM_CODE,
                            MARGIN_ITEM_ACCESS = item.MARGIN_ITEM_CODE,
                            AMOUNT = _AMOUNT,
                            AMOUNT_ACCESS = _AMOUNT,
                            MARGIN_ITEM_ISSUER = item.MARGIN_ITEM_ISSUER,
                            MARGIN_ITEM_ISSUER_ACCESS = item.MARGIN_ITEM_ISSUER,
                            PLEDGE_ITEM_NO = item.PLEDGE_ITEM_NO,
                            PLEDGE_ITEM_NO_ACCESS = item.PLEDGE_ITEM_NO,
                            EFFECTIVE_DATE_B = _EFFECTIVE_DATE_B,
                            EFFECTIVE_DATE_B_ACCESS = _EFFECTIVE_DATE_B,
                            EFFECTIVE_DATE_E = _EFFECTIVE_DATE_E,
                            EFFECTIVE_DATE_E_ACCESS = _EFFECTIVE_DATE_E,
                            DESCRIPTION = item.DESCRIPTION,
                            DESCRIPTION_ACCESS = item.DESCRIPTION,
                            MEMO = item.MEMO,
                            MEMO_ACCESS = item.MEMO,
                            BOOK_NO = item.BOOK_NO,
                            BOOK_NO_ACCESS = item.BOOK_NO,
                            APLY_DEPT = item.APLY_DEPT,
                            APLY_SECT = item.APLY_SECT,
                            APLY_UID = item.APLY_UID,
                            CHARGE_DEPT = item.APLY_DEPT,
                            CHARGE_SECT = item.APLY_SECT,
                            PUT_DATE = TypeTransfer.stringToDateTimeN(item.PUT_DATE),
                            LAST_UPDATE_DT = dtn
                        });
                    }
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
                    {
                        TreasuryDBEntities db = null;
                        try
                        {
                            db = new TreasuryDBEntities();
                            db.Configuration.AutoDetectChangesEnabled = false;                           
                            foreach (var model in MarginpModels)
                            {
                                ++count;
                                db = Common.AddToContext(db, model, count, 100, true);
                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _flag = false;
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                        finally
                        {
                            if (_flag)
                            {
                                scope.Complete();
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = $@"新增存入保證金明細成功,新增資料筆數:{count}.";
                            }                          
                            db.Dispose();
                        }
                    }
                    break;
                #endregion
                #region 存出保證金
                case ExcelName.Marging:
                    List<ITEM_REFUNDABLE_DEP> MargingModels = new List<ITEM_REFUNDABLE_DEP>();
                    foreach (FileMargingModel item in datas)
                    {
                        string item_id = string.Empty;

                        if (item.ITEM_ID.IsNullOrWhiteSpace())
                        {
                            switch (item.MARGIN_DEP_TYPE_CODE)
                            {
                                case "1":
                                    item_id = sysSeqDao.qrySeqNo("A", string.Empty).ToString().PadLeft(8, '0');
                                    item_id = $@"A{item_id}";
                                    break;
                                case "2":
                                    item_id = sysSeqDao.qrySeqNo("B", string.Empty).ToString().PadLeft(8, '0');
                                    item_id = $@"B{item_id}";
                                    break;
                                case "3":
                                    item_id = sysSeqDao.qrySeqNo("C", string.Empty).ToString().PadLeft(8, '0');
                                    item_id = $@"C{item_id}";
                                    break;
                            }
                        }
                        else
                        {
                            item_id = item.ITEM_ID?.Trim();
                        }
                      
                        var _AMOUNT = TypeTransfer.stringToDecimal(item.AMOUNT);
                        MargingModels.Add(new ITEM_REFUNDABLE_DEP()
                        {
                            ITEM_ID = item_id,
                            INVENTORY_STATUS = "1",
                            TRAD_PARTNERS = item.TRAD_PARTNERS,
                            TRAD_PARTNERS_ACCESS = item.TRAD_PARTNERS,
                            MARGIN_DEP_TYPE = item.MARGIN_DEP_TYPE_CODE,
                            MARGIN_DEP_TYPE_ACCESS = item.MARGIN_DEP_TYPE_CODE,
                            AMOUNT = _AMOUNT,
                            AMOUNT_ACCESS = _AMOUNT,
                            WORKPLACE_CODE = item.WORKPLACE_CODE,
                            WORKPLACE_CODE_ACCESS = item.WORKPLACE_CODE,
                            DESCRIPTION = item.DESCRIPTION,
                            DESCRIPTION_ACCESS = item.DESCRIPTION,
                            MEMO = item.MEMO,
                            MEMO_ACCESS = item.MEMO,
                            BOOK_NO = item.BOOK_NO,
                            BOOK_NO_ACCESS = item.BOOK_NO,
                            APLY_DEPT = item.APLY_DEPT,
                            APLY_SECT = item.APLY_SECT,
                            APLY_UID = item.APLY_UID,
                            CHARGE_DEPT = item.APLY_DEPT,
                            CHARGE_SECT = item.APLY_SECT,
                            PUT_DATE = TypeTransfer.stringToDateTimeN(item.PUT_DATE),                           
                            LAST_UPDATE_DT = dtn
                        });
                    }
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
                    {
                        TreasuryDBEntities db = null;
                        try
                        {
                            db = new TreasuryDBEntities();
                            db.Configuration.AutoDetectChangesEnabled = false;
                            foreach (var model in MargingModels)
                            {
                                ++count;
                                db = Common.AddToContext(db, model, count, 100, true);
                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _flag = false;
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                        finally
                        {
                            if (_flag)
                            {
                                scope.Complete();
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = $@"新增存出保證金明細成功,新增資料筆數:{count}.";
                            }
                            db.Dispose();
                        }
                    }
                    break;
                #endregion
                #region 重要物品
                case ExcelName.Itemimp:
                    List<ITEM_IMPO> ItemImpModels = new List<ITEM_IMPO>();
                    var Itemimp_Seq = "E8"; //重要物品流水號開頭編碼
                    foreach (FileItemImpModel item in datas)
                    {

                        var item_id = string.Empty;
                        if (item.ITEM_ID.IsNullOrWhiteSpace())
                        {
                            item_id = $@"{Itemimp_Seq}{sysSeqDao.qrySeqNo(Itemimp_Seq, string.Empty).ToString().PadLeft(8, '0')}";
                        }
                        else
                        {
                            item_id = item.ITEM_ID?.Trim();
                        }                      

                        var _AMOUNT = TypeTransfer.stringToDecimal(item.AMOUNT);
                        var _QUANTITY = TypeTransfer.stringToInt(item.QUANTITY);
                        var _EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.EXPECTED_ACCESS_DATE);
                        ItemImpModels.Add(new ITEM_IMPO()
                        {
                            ITEM_ID = $@"{item_id}", //物品編號
                            INVENTORY_STATUS = "1", //在庫
                            ITEM_NAME = item.ITEM_NAME, //重要物品名稱
                            ITEM_NAME_ACCESS = item.ITEM_NAME,
                            QUANTITY = _QUANTITY,
                            QUANTITY_ACCESS = _QUANTITY,
                            REMAINING = _QUANTITY,                           
                            AMOUNT = _AMOUNT,
                            AMOUNT_ACCESS = _AMOUNT,
                            EXPECTED_ACCESS_DATE = _EXPECTED_ACCESS_DATE,
                            EXPECTED_ACCESS_DATE_ACCESS = _EXPECTED_ACCESS_DATE,
                            DESCRIPTION = item.DESCRIPTION,
                            DESCRIPTION_ACCESS = item.DESCRIPTION,
                            MEMO = item.MEMO,
                            MEMO_ACCESS = item.MEMO,
                            APLY_DEPT = item.APLY_DEPT,
                            APLY_SECT = item.APLY_SECT,
                            APLY_UID = item.APLY_UID,
                            CHARGE_DEPT = item.APLY_DEPT,
                            CHARGE_SECT = item.APLY_SECT,
                            PUT_DATE = TypeTransfer.stringToDateTimeN(item.PUT_DATE),
                            LAST_UPDATE_DT = dtn
                        });
                    }
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
                    {
                        TreasuryDBEntities db = null;
                        try
                        {
                            db = new TreasuryDBEntities();
                            db.Configuration.AutoDetectChangesEnabled = false;
                            foreach (var model in ItemImpModels)
                            {
                                ++count;
                                db = Common.AddToContext(db, model, count, 100, true);
                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _flag = false;
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                        finally
                        {
                            if (_flag)
                            {
                                scope.Complete();
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = $@"新增重要物品明細成功,新增資料筆數:{count}.";
                            }
                            db.Dispose();
                        }
                    }
                    break;
                #endregion
                #region 股票
                case ExcelName.Stock:
                    List<ITEM_STOCK> StockModels = new List<ITEM_STOCK>();
                    foreach (FileStockModel item in datas)
                    {
                        string item_id = string.Empty;

                        if (item.ITEM_ID.IsNullOrWhiteSpace())
                        {
                            item_id = $@"E7{sysSeqDao.qrySeqNo("E7", string.Empty).ToString().PadLeft(8, '0')}";
                        }
                        else
                        {
                            item_id = item.ITEM_ID?.Trim();
                        }
                        var _STOCK_CNT = TypeTransfer.stringToIntN(item.STOCK_CNT);
                        var _AMOUNT_PER_SHARE = TypeTransfer.stringToDecimalN(item.AMOUNT_PER_SHARE);
                        var _SINGLE_NUMBER_OF_SHARES = TypeTransfer.stringToDecimalN(item.SINGLE_NUMBER_OF_SHARES);
                        var _DENOMINATION = TypeTransfer.stringToDecimalN(item.DENOMINATION);
                        var _NUMBER_OF_SHARES = TypeTransfer.stringToDecimalN(item.NUMBER_OF_SHARES);
                        StockModels.Add(new ITEM_STOCK()
                        {
                            ITEM_ID = item_id,
                            INVENTORY_STATUS = "1",
                            GROUP_NO = TypeTransfer.stringToInt(item.GROUP_NO),
                            TREA_BATCH_NO = TypeTransfer.stringToInt(item.TREA_BATCH_NO),
                            STOCK_TYPE = item.STOCK_TYPE_CODE,
                            STOCK_TYPE_ACCESS = item.STOCK_TYPE_CODE,
                            STOCK_NO_PREAMBLE = item.STOCK_NO_PREAMBLE,
                            STOCK_NO_PREAMBLE_ACCESS = item.STOCK_NO_PREAMBLE,
                            STOCK_NO_B = item.STOCK_NO_B,
                            STOCK_NO_B_ACCESS = item.STOCK_NO_B,
                            STOCK_NO_E = item.STOCK_NO_E,
                            STOCK_NO_E_ACCESS = item.STOCK_NO_E,
                            STOCK_CNT = _STOCK_CNT,
                            STOCK_CNT_ACCESS = _STOCK_CNT,
                            AMOUNT_PER_SHARE = _AMOUNT_PER_SHARE,
                            AMOUNT_PER_SHARE_ACCESS = _AMOUNT_PER_SHARE,
                            SINGLE_NUMBER_OF_SHARES = _SINGLE_NUMBER_OF_SHARES,
                            SINGLE_NUMBER_OF_SHARES_ACCESS = _SINGLE_NUMBER_OF_SHARES,
                            DENOMINATION = _DENOMINATION,
                            DENOMINATION_ACCESS = _DENOMINATION,
                            NUMBER_OF_SHARES = _NUMBER_OF_SHARES,
                            NUMBER_OF_SHARES_ACCESS = _NUMBER_OF_SHARES,
                            MEMO = item.MEMO,
                            MEMO_ACCESS = item.MEMO,
                            APLY_DEPT = item.APLY_DEPT,
                            APLY_SECT = item.APLY_SECT,
                            APLY_UID = item.APLY_UID,
                            CHARGE_DEPT = item.APLY_DEPT,
                            CHARGE_SECT = item.APLY_SECT,
                            PUT_DATE = TypeTransfer.stringToDateTimeN(item.PUT_DATE),
                            LAST_UPDATE_DT = dtn
                        });
                    }
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
                    {
                        TreasuryDBEntities db = null;
                        try
                        {
                            db = new TreasuryDBEntities();
                            db.Configuration.AutoDetectChangesEnabled = false;
                            foreach (var model in StockModels)
                            {
                                ++count;
                                db = Common.AddToContext(db, model, count, 100, true);
                            }
                            db.SaveChanges();

                            ////更新下一次入庫批號
                            //using (TreasuryDBEntities dbs = new TreasuryDBEntities())
                            //{
                            //    var item_books = dbs.ITEM_BOOK.Where(x => x.ITEM_ID == "D1015" &&
                            //    x.COL == "NEXT_BATCH_NO").ToList();
                            //    foreach (var item in StockModels.GroupBy(x => x.GROUP_NO))
                            //    {
                            //        int GROUP_NO = item.Key; //股票群組
                            //        int max_TREA_BATCH_NO = item.Max(x => x.TREA_BATCH_NO) + 1; //下一次的入庫批號
                            //        var item_book = item_books.FirstOrDefault(x => x.GROUP_NO == GROUP_NO);
                            //        if (item_book != null)
                            //            item_book.COL_VALUE = max_TREA_BATCH_NO.ToString();
                            //    }
                            //    dbs.SaveChanges();
                            //}
                        }
                        catch (Exception ex)
                        {
                            _flag = false;
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                        finally
                        {
                            if (_flag)
                            {
                                scope.Complete();
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = $@"新增{type.GetDescription()}明細成功,新增資料筆數:{count}.";
                            }
                            db.Dispose();
                        }
                    }
                    break;
                    #endregion
            }
            return result;
        }

        #endregion Save Data

        #region Excel

        #region Excel 資料轉成  ViewModel

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
                List<string> errors = new List<string>();
                var pros = FactoryRegistry.GetInstance(excelName).GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                foreach (var item in dataModel)
                {
                    var context = new ValidationContext(item, null, null);
                    var result = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(item, context, result, true))
                    {
                        result.ForEach(x =>
                        {
                            var m = x.MemberNames.FirstOrDefault()?.ToString();
                            var p = pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                            var val = (p == null) ? null : (p.GetValue(item))?.ToString();
                            if (!val.IsNullOrWhiteSpace())
                                errors.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                            else
                                errors.Add(x.ErrorMessage);
                        });
                    }
                }
                if (errors.Any())
                {
                    msg = string.Join(",", errors.Distinct());
                }
                else
                {
                    switch (excelName)
                    {
                        case ExcelName.Marginp:
                            var Marginpdata = setMarginpModel(dataModel);
                            msg = Marginpdata.Item1;
                            dataModel = Marginpdata.Item2.ToList();
                            break;
                        case ExcelName.Marging:
                            var Margingdata = setMargingModel(dataModel);
                            msg = Margingdata.Item1;
                            dataModel = Margingdata.Item2.ToList();
                            break;
                        case ExcelName.Itemimp:
                            var Itemimpdata = setItemImpModel(dataModel);
                            msg = Itemimpdata.Item1;
                            dataModel = Itemimpdata.Item2.ToList();
                            break;
                        case ExcelName.Stock:
                            var Stockdata = getStockModel(dataModel);
                            msg = Stockdata.Item1;
                            dataModel = Stockdata.Item2.ToList();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.exceptionMessage();
            }
            return new Tuple<string, IEnumerable<IFileModel>>(msg, dataModel);
        }

        #endregion Excel 資料轉成  ViewModel

        #endregion Excel

        #region privateFunction

        /// <summary>
        /// 存入保證金 檢核
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private Tuple<string, IEnumerable<IFileModel>> setMarginpModel(IEnumerable<IFileModel> models)
        {
            List<FileMarginpModel> result = new List<FileMarginpModel>();
            string msg = string.Empty;
            List<string> aplyIds = new List<string>(); //申請人ID
            List<string> MTOTs = new List<string>(); //存入保證金類別
            List<string> MIs = new List<string>(); //保證物品

            foreach (FileMarginpModel model in models)
            {
                if(model.APLY_UID != null)
                    aplyIds.Add(model.APLY_UID);
                if(model.MARGIN_TAKE_OF_TYPE != null)
                    MTOTs.Add(model.MARGIN_TAKE_OF_TYPE);
                if(model.MARGIN_ITEM != null)
                    MIs.Add(model.MARGIN_ITEM);
            }

            aplyIds = aplyIds.Distinct().ToList();
            MTOTs = MTOTs.Distinct().ToList();
            MIs = MIs.Distinct().ToList();


            List<string> codeTypes = new List<string>() { "MARGIN_TAKE_OF_TYPE", "MARGIN_ITEM" };
            List<SYS_CODE> sysCodes = new List<SYS_CODE>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                sysCodes = db.SYS_CODE.AsNoTracking()
                    .Where(x => codeTypes.Contains(x.CODE_TYPE)).ToList();

            }

            List<V_EMPLY2> emps = new List<V_EMPLY2>();
            List<VW_OA_DEPT> depts = new List<VW_OA_DEPT>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emps = db.V_EMPLY2.AsNoTracking()
                    .Where(x => x.USR_ID != null && aplyIds.Contains(x.USR_ID)).ToList();

                List<string> dpts = emps.Where(x=>x.DPT_CD != null).Select(x => x.DPT_CD).Distinct().ToList();

                depts = db.VW_OA_DEPT.AsNoTracking()
                    .Where(x=> dpts.Contains(x.DPT_CD)).ToList();
            }

            bool flag = true;
            List<string> msgs = new List<string>();
            var dtn = DateTime.Now.Date.ToString("yyyy/MM/dd");
            foreach (FileMarginpModel model in models)
            {
                var MTOT = sysCodes.Where(x => x.CODE_TYPE == "MARGIN_TAKE_OF_TYPE" &&
                x.CODE_VALUE?.Trim() == model.MARGIN_TAKE_OF_TYPE?.Trim())
                .FirstOrDefault();
                if (MTOT == null)
                {
                    flag = false;
                    msgs.Add($@"存入保證金類別 : {model.MARGIN_TAKE_OF_TYPE} 找不到指定的代碼");
                }
                else
                {
                    model.MARGIN_TAKE_OF_TYPE_CODE = MTOT.CODE; //存入保證金類別
                }
                var MI = sysCodes.Where(x => x.CODE_TYPE == "MARGIN_ITEM" &&
                x.CODE_VALUE?.Trim() == model.MARGIN_ITEM?.Trim())
                .FirstOrDefault();
                if (MI == null)
                {
                    flag = false;
                    msgs.Add($@"保證物品 : {model.MARGIN_ITEM} 找不到指定的代碼");
                }
                else
                {
                    model.MARGIN_ITEM_CODE = MI.CODE; //保證物品
                }
                var emp = emps.FirstOrDefault(x => x.USR_ID == model.APLY_UID);
                if (emp == null)
                {
                    flag = false;
                    msgs.Add($@"申請人 : {model.APLY_UID} 比對不到指定的代碼");
                }
                else
                {
                    var dept = depts.FirstOrDefault(x => x.DPT_CD == emp.DPT_CD);
                    if (dept == null)
                    {
                        flag = false;
                        msgs.Add($@"申請人 : {model.APLY_UID} 比對不到部門");
                    }
                    else
                    {
                        if (dept.Dpt_type == "03")
                        {
                            model.APLY_SECT = string.Empty;
                            model.APLY_DEPT = dept.DPT_CD?.Trim();
                            model.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                        }
                        if (dept.Dpt_type == "04")
                        {
                            model.APLY_SECT = dept.DPT_CD?.Trim();
                            model.APLY_DEPT = dept.UP_DPT_CD?.Trim();
                            model.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                        }
                    }
                }
                model.EFFECTIVE_DATE_B = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(model.EFFECTIVE_DATE_B));
                model.EFFECTIVE_DATE_E = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(model.EFFECTIVE_DATE_E));
                if (!model.EFFECTIVE_DATE_B.IsNullOrWhiteSpace() && !model.EFFECTIVE_DATE_E.IsNullOrWhiteSpace() && 
                    (TypeTransfer.stringToDateTime(model.EFFECTIVE_DATE_B) > TypeTransfer.stringToDateTime(model.EFFECTIVE_DATE_E)))
                {
                    flag = false;
                    msgs.Add($@"有效區間(起) : {model.EFFECTIVE_DATE_B} 不能大於 有效區間(迄) : {model.EFFECTIVE_DATE_E}");
                }
                model.PUT_DATE = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(model.PUT_DATE));
                if (model.PUT_DATE.IsNullOrWhiteSpace())
                    model.PUT_DATE = dtn;              
            }
            if (flag)
                result = models.Cast<FileMarginpModel>().ToList();
            else
                msg = string.Join(",", msgs);
            return new Tuple<string, IEnumerable<IFileModel>>(msg, result);
        }

        /// <summary>
        /// 存出保證金檢核
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private Tuple<string, IEnumerable<IFileModel>> setMargingModel(IEnumerable<IFileModel> models)
        {
            List<FileMargingModel> result = new List<FileMargingModel>();
            string msg = string.Empty;
            List<string> aplyIds = new List<string>(); //申請人ID
            List<string> MDTs = new List<string>(); //存出保證金類別

            foreach (FileMargingModel model in models)
            {
                if (model.APLY_UID != null)
                    aplyIds.Add(model.APLY_UID);
                if (model.MARGIN_DEP_TYPE != null)
                    MDTs.Add(model.MARGIN_DEP_TYPE);
            }

            aplyIds = aplyIds.Distinct().ToList();
            MDTs = MDTs.Distinct().ToList();

            List<string> codeTypes = new List<string>() { "MARGING_TYPE" };
            List<SYS_CODE> sysCodes = new List<SYS_CODE>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                sysCodes = db.SYS_CODE.AsNoTracking()
                    .Where(x => codeTypes.Contains(x.CODE_TYPE)).ToList();

            }

            List<V_EMPLY2> emps = new List<V_EMPLY2>();
            List<VW_OA_DEPT> depts = new List<VW_OA_DEPT>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emps = db.V_EMPLY2.AsNoTracking()
                    .Where(x => x.USR_ID != null && aplyIds.Contains(x.USR_ID)).ToList();

                List<string> dpts = emps.Where(x => x.DPT_CD != null).Select(x => x.DPT_CD).Distinct().ToList();

                depts = db.VW_OA_DEPT.AsNoTracking()
                    .Where(x => dpts.Contains(x.DPT_CD)).ToList();
            }

            bool flag = true;
            List<string> msgs = new List<string>();
            var dtn = DateTime.Now.Date.ToString("yyyy/MM/dd");
            foreach (FileMargingModel model in models)
            {
                var MDT = sysCodes.Where(x => x.CODE_TYPE == "MARGING_TYPE" &&
                x.CODE_VALUE?.Trim() == model.MARGIN_DEP_TYPE?.Trim())
                .FirstOrDefault();
                if (MDT == null)
                {
                    flag = false;
                    msgs.Add($@"存出保證金類別 : {model.MARGIN_DEP_TYPE} 找不到指定的代碼");
                }
                else
                {
                    model.MARGIN_DEP_TYPE_CODE = MDT.CODE; //存出保證金類別
                }
                var emp = emps.FirstOrDefault(x => x.USR_ID == model.APLY_UID);
                if (emp == null)
                {
                    flag = false;
                    msgs.Add($@"申請人 : {model.APLY_UID} 比對不到指定的代碼");
                }
                else
                {
                    var dept = depts.FirstOrDefault(x => x.DPT_CD == emp.DPT_CD);
                    if (dept == null)
                    {
                        flag = false;
                        msgs.Add($@"申請人 : {model.APLY_UID} 比對不到部門");
                    }
                    else
                    {
                        if (dept.Dpt_type == "03")
                        {
                            model.APLY_SECT = string.Empty;
                            model.APLY_DEPT = dept.DPT_CD?.Trim();
                            model.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                        }
                        if (dept.Dpt_type == "04")
                        {
                            model.APLY_SECT = dept.DPT_CD?.Trim();
                            model.APLY_DEPT = dept.UP_DPT_CD?.Trim();
                            model.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                        }
                    }
                }
                model.PUT_DATE = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(model.PUT_DATE));
                if (model.PUT_DATE.IsNullOrWhiteSpace())
                    model.PUT_DATE = dtn;
            }
            if (flag)
                result = models.Cast<FileMargingModel>().ToList();
            else
                msg = string.Join(",", msgs);
            return new Tuple<string, IEnumerable<IFileModel>>(msg, result);
        }

        /// <summary>
        /// 重要物品檢核
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private Tuple<string, IEnumerable<IFileModel>> setItemImpModel(IEnumerable<IFileModel> models)
        {
            List<FileItemImpModel> result = new List<FileItemImpModel>();
            string msg = string.Empty;
            List<string> aplyIds = new List<string>(); //申請人ID

            foreach (FileItemImpModel model in models)
            {
                if (model.APLY_UID != null)
                    aplyIds.Add(model.APLY_UID);
            }

            aplyIds = aplyIds.Distinct().ToList();

            List<V_EMPLY2> emps = new List<V_EMPLY2>();
            List<VW_OA_DEPT> depts = new List<VW_OA_DEPT>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emps = db.V_EMPLY2.AsNoTracking()
                    .Where(x => x.USR_ID != null && aplyIds.Contains(x.USR_ID)).ToList();

                List<string> dpts = emps.Where(x => x.DPT_CD != null).Select(x => x.DPT_CD).Distinct().ToList();

                depts = db.VW_OA_DEPT.AsNoTracking()
                    .Where(x => dpts.Contains(x.DPT_CD)).ToList();
            }

            bool flag = true;
            List<string> msgs = new List<string>();
            var dtn = DateTime.Now.Date.ToString("yyyy/MM/dd");
            foreach (FileItemImpModel model in models)
            {
                var emp = emps.FirstOrDefault(x => x.USR_ID == model.APLY_UID);
                if (emp == null)
                {
                    flag = false;
                    msgs.Add($@"申請人 : {model.APLY_UID} 比對不到指定的代碼");
                }
                else
                {
                    var dept = depts.FirstOrDefault(x => x.DPT_CD == emp.DPT_CD);
                    if (dept == null)
                    {
                        flag = false;
                        msgs.Add($@"申請人 : {model.APLY_UID} 比對不到部門");
                    }
                    else
                    {
                        if (dept.Dpt_type == "03")
                        {
                            model.APLY_SECT = string.Empty;
                            model.APLY_DEPT = dept.DPT_CD?.Trim();
                            model.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                        }
                        if (dept.Dpt_type == "04")
                        {
                            model.APLY_SECT = dept.DPT_CD?.Trim();
                            model.APLY_DEPT = dept.UP_DPT_CD?.Trim();
                            model.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                        }
                    }
                }
                model.EXPECTED_ACCESS_DATE = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(model.EXPECTED_ACCESS_DATE));
                model.PUT_DATE = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(model.PUT_DATE));
                if (model.PUT_DATE.IsNullOrWhiteSpace())
                    model.PUT_DATE = dtn;
            }
            if (flag)
                result = models.Cast<FileItemImpModel>().ToList();
            else
                msg = string.Join(",", msgs);
            return new Tuple<string, IEnumerable<IFileModel>>(msg, result);
        }

        /// <summary>
        /// 股票檢核
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private Tuple<string, IEnumerable<IFileModel>> getStockModel(IEnumerable<IFileModel> models)
        {
            List<FileStockModel> result = new List<FileStockModel>();
            string msg = string.Empty;
            List<string> aplyIds = new List<string>(); //申請人ID
            List<string> SNs = new List<string>(); //股票名稱

            foreach (FileStockModel model in models)
            {
                if (model.APLY_UID != null)
                    aplyIds.Add(model.APLY_UID);
                SNs.Add(model.STOCK_NAME);
                model.TREA_BATCH_NO = TypeTransfer.stringToInt(model.TREA_BATCH_NO).ToString();
            }

            aplyIds = aplyIds.Distinct().ToList();
            SNs = SNs.Distinct().ToList();

            List<string> codeTypes = new List<string>() { "STOCK_TYPE" }; //股票類型
            List<SYS_CODE> sysCodes = new List<SYS_CODE>();
            List<ITEM_BOOK> itemBooks = new List<ITEM_BOOK>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                sysCodes = db.SYS_CODE.AsNoTracking()
                    .Where(x => codeTypes.Contains(x.CODE_TYPE)).ToList();
                itemBooks = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == "D1015").ToList();
            }

            List<V_EMPLY2> emps = new List<V_EMPLY2>();
            List<VW_OA_DEPT> depts = new List<VW_OA_DEPT>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emps = db.V_EMPLY2.AsNoTracking()
                    .Where(x => x.USR_ID != null && aplyIds.Contains(x.USR_ID)).ToList();

                List<string> dpts = emps.Where(x => x.DPT_CD != null).Select(x => x.DPT_CD).Distinct().ToList();

                depts = db.VW_OA_DEPT.AsNoTracking()
                    .Where(x => dpts.Contains(x.DPT_CD)).ToList();
            }

            bool flag = true;
            List<string> msgs = new List<string>();
            var dtn = DateTime.Now.Date.ToString("yyyy/MM/dd");
            foreach (var model in models.Cast<FileStockModel>().ToList().GroupBy(x=>x.STOCK_NAME))
            {
                var _itemBook = itemBooks.FirstOrDefault(x => x.COL == "NAME" && x.COL_VALUE == model.Key?.Trim());
                if (_itemBook != null)
                {
                    var _groupNo = _itemBook.GROUP_NO; //群組編號
                    var _area = itemBooks.FirstOrDefault(x => x.GROUP_NO == _groupNo && x.COL == "AREA")?.COL_VALUE; //區域
                    var _NEXT_BATCH_NO = TypeTransfer.stringToInt(itemBooks.FirstOrDefault(x => x.GROUP_NO == _groupNo && x.COL == "NEXT_BATCH_NO")?.COL_VALUE); //下一次入庫批號

                    foreach (var _model in model.GroupBy(y => y.TREA_BATCH_NO).OrderBy(y => y.Key))
                    {

                        foreach (var item in _model)
                        {
                            var ST = sysCodes.Where(x => x.CODE_TYPE == "STOCK_TYPE" &&
                                            x.CODE_VALUE?.Trim() == item.STOCK_TYPE?.Trim())
                                            .FirstOrDefault();
                            if (ST == null)
                            {
                                flag = false;
                                msgs.Add($@"股票類型 : {item.STOCK_TYPE} 找不到指定的代碼");
                            }
                            else if (_NEXT_BATCH_NO <= TypeTransfer.stringToInt(item.TREA_BATCH_NO))
                            {
                                flag = false;
                                msgs.Add($@"股票類型 : {item.STOCK_TYPE} ,股票名稱 : {item.STOCK_NAME}, 找不到入庫批號 : {item.TREA_BATCH_NO}");                              
                            }
                            else
                            {
                                item.GROUP_NO = _groupNo.ToString();
                                item.TREA_BATCH_NO = item.TREA_BATCH_NO;
                                if (_area == "D")//國內
                                {
                                    item.STOCK_TYPE_CODE = ST.CODE; //股票類型代碼
                                    switch (ST.CODE)
                                    {
                                        case "S": //股票
                                            if (!item.STOCK_NO_B.IsNullOrWhiteSpace() && !item.STOCK_NO_E.IsNullOrWhiteSpace())
                                            {
                                                if (TypeTransfer.stringToInt(item.STOCK_NO_B) > TypeTransfer.stringToInt(item.STOCK_NO_E))
                                                {
                                                    flag = false;
                                                    msgs.Add($@"股票名稱 : {model.Key}, 迄號({item.STOCK_NO_E})不可小於起號({item.STOCK_NO_B})");
                                                }
                                                else
                                                {
                                                    //張數 = 迄 - 起 + 1
                                                    item.STOCK_CNT = (TypeTransfer.stringToInt(item.STOCK_NO_E) - TypeTransfer.stringToInt(item.STOCK_NO_B) + 1).ToString();
                                                }
                                            }
                                            if (item.STOCK_NO_PREAMBLE.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有股票序號前置代碼");
                                            }
                                            if (item.STOCK_NO_B.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有股票序號(起)");
                                            }
                                            if (item.STOCK_NO_E.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有股票序號(迄)");
                                            }
                                            if (item.STOCK_CNT.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有張數");
                                            }
                                            if (item.AMOUNT_PER_SHARE.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有每股金額");
                                            }
                                            if (item.AMOUNT_PER_SHARE.IsNullOrWhiteSpace() || TypeTransfer.stringToInt(item.AMOUNT_PER_SHARE) <= 0)
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有每股金額或每股金額需大於1");
                                            }
                                            if (item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股票, 需要有單張股數");
                                            }
                                            if (!item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace() && !item.AMOUNT_PER_SHARE.IsNullOrWhiteSpace())
                                            {
                                                //單張面額 = 每股金額 * 單張股數
                                                item.DENOMINATION = (TypeTransfer.stringToInt(item.AMOUNT_PER_SHARE) * TypeTransfer.stringToInt(item.SINGLE_NUMBER_OF_SHARES)).ToString();
                                            }
                                            if (!item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace() && !item.STOCK_CNT.IsNullOrWhiteSpace())
                                            {
                                                //股數 = 張數 * 單張股數
                                                item.NUMBER_OF_SHARES = (TypeTransfer.stringToInt(item.STOCK_CNT) * TypeTransfer.stringToInt(item.SINGLE_NUMBER_OF_SHARES)).ToString();
                                            }
                                            break;
                                        case "P": //股權(持股)憑證
                                            if (item.STOCK_CNT.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股權(持股)憑證, 需要有張數");
                                            }
                                            if (item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股權(持股)憑證, 需要有單張股數");
                                            }
                                            if (!item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace() && !item.STOCK_CNT.IsNullOrWhiteSpace())
                                            {
                                                //股數 = 張數 * 單張股數
                                                item.NUMBER_OF_SHARES = (TypeTransfer.stringToInt(item.STOCK_CNT) * TypeTransfer.stringToInt(item.SINGLE_NUMBER_OF_SHARES)).ToString();
                                            }
                                            if (item.MEMO.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 股權(持股)憑證, 需要有備註");
                                            }
                                            break;
                                        case "C": //其他
                                            if (item.STOCK_CNT.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 其他, 需要有張數");
                                            }
                                            if (item.MEMO.IsNullOrWhiteSpace())
                                            {
                                                flag = false;
                                                msgs.Add($@"股票名稱 : {model.Key}, 區域為國內, 股票類型 : 其他, 需要有備註");
                                            }
                                            break;
                                    }
                                }
                                else if (_area == "F")//國外
                                {
                                    if (!item.STOCK_NO_B.IsNullOrWhiteSpace() && !item.STOCK_NO_E.IsNullOrWhiteSpace())
                                    {
                                        if (TypeTransfer.stringToInt(item.STOCK_NO_B) > TypeTransfer.stringToInt(item.STOCK_NO_E))
                                        {
                                            flag = false;
                                            msgs.Add($@"股票名稱 : {model.Key}, 迄號({item.STOCK_NO_E})不可小於起號({item.STOCK_NO_B})");
                                        }
                                        else
                                        {
                                            //張數 = 迄 - 起 + 1
                                            item.SINGLE_NUMBER_OF_SHARES = (TypeTransfer.stringToInt(item.STOCK_NO_E) - TypeTransfer.stringToInt(item.STOCK_NO_B) + 1).ToString();
                                        }
                                    }
                                    if (!item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace() && !item.AMOUNT_PER_SHARE.IsNullOrWhiteSpace())
                                    {
                                        //單張面額 = 每股金額 * 單張股數
                                        item.DENOMINATION = (TypeTransfer.stringToInt(item.AMOUNT_PER_SHARE) * TypeTransfer.stringToInt(item.SINGLE_NUMBER_OF_SHARES)).ToString();
                                    }
                                    if (!item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace() && !item.STOCK_CNT.IsNullOrWhiteSpace())
                                    {
                                        //股數 = 張數 * 單張股數
                                        item.NUMBER_OF_SHARES = (TypeTransfer.stringToInt(item.STOCK_CNT) * TypeTransfer.stringToInt(item.SINGLE_NUMBER_OF_SHARES)).ToString();
                                    }
                                    if (!(!item.STOCK_NO_PREAMBLE.IsNullOrWhiteSpace() ||
                                        !item.STOCK_NO_B.IsNullOrWhiteSpace() ||
                                        !item.STOCK_NO_E.IsNullOrWhiteSpace() ||
                                        !item.AMOUNT_PER_SHARE.IsNullOrWhiteSpace() ||
                                        !item.SINGLE_NUMBER_OF_SHARES.IsNullOrWhiteSpace() ||
                                        !item.MEMO.IsNullOrWhiteSpace()))
                                    {
                                        flag = false;
                                        msgs.Add($@"股票名稱 : {model.Key}, 區域為國外, 不符合驗證規則, 需要有一個欄位有參數");
                                    }
                                }
                            }
                            var emp = emps.FirstOrDefault(x => x.USR_ID == item.APLY_UID);
                            if (emp == null)
                            {
                                flag = false;
                                msgs.Add($@"申請人 : {item.APLY_UID} 比對不到指定的代碼");
                            }
                            else
                            {
                                var dept = depts.FirstOrDefault(x => x.DPT_CD == emp.DPT_CD);
                                if (dept == null)
                                {
                                    flag = false;
                                    msgs.Add($@"申請人 : {item.APLY_UID} 比對不到部門");
                                }
                                else
                                {
                                    if (dept.Dpt_type == "03")
                                    {
                                        item.APLY_SECT = string.Empty;
                                        item.APLY_DEPT = dept.DPT_CD?.Trim();
                                        item.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                                    }
                                    if (dept.Dpt_type == "04")
                                    {
                                        item.APLY_SECT = dept.DPT_CD?.Trim();
                                        item.APLY_DEPT = dept.UP_DPT_CD?.Trim();
                                        item.APLY_SHOW = $@"{dept.DPT_NAME?.Trim()} {emp.EMP_NAME?.Trim()}";
                                    }
                                }
                            }
                            item.PUT_DATE = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(item.PUT_DATE));
                            if (item.PUT_DATE.IsNullOrWhiteSpace())
                                item.PUT_DATE = dtn;
                        }                     
                        //_NEXT_BATCH_NO += 1;
                    }               
                }
                else
                {
                    flag = false;
                    msgs.Add($@"股票名稱 : {model.Key} 找不到對應的冊號");
                }
            }
            if (flag)
                result = models.Cast<FileStockModel>().ToList();
            else
                msg = string.Join(",", msgs);
            return new Tuple<string, IEnumerable<IFileModel>>(msg, result);
        }
        
        #endregion privateFunction
    }
}