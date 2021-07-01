using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using FAP.Web.ViewModels;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Configuration;
using System.IO;
using FAP.Web.Daos;
using System.Reflection;
using System.Data.SqlClient;
using Dapper;
using FAP.Web.Service.Interface;
using ClosedXML.Excel;
using Ionic.Zip;

/// <summary>
/// 功能說明：BAP0004 逾期清理追蹤排程作業
/// 初版作者：20200921 張家華
/// 修改歷程：20200921 張家華 
/// 需求單號：202008120153-00
/// 修改內容：初版
/// ------------------------------------------
/// 需求單號：
/// 修改歷程：20210128 daiyu
/// 修改內容：1.「清理暨逾期處理清單」，只印有清理階段完成日的…及現在在哪一個階段的資料 
///           2.修改人員MAIL取得方式(改抓AD)
/// ------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class BAP0004 : Common , IBAP0004
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private string DefaultConn;

        public BAP0004()
        {
            DefaultConn = DbUtil.GetDBFglConnStr();
        }

        /// <summary>
        /// 比對出已達追蹤條件的資料，寄送追蹤報表給相關追蹤人員 
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <param name="type">執行類別 A:全部執行,M:僅執行寄送mail,E:僅執行產生Excel</param>
        /// <returns></returns>
        public Tuple<bool,string, string> VE_Clear_Scheduler(string userId = null, string type = "A")
        {
            DateTime dtn = DateTime.Now;
            var _dtn = DateForTWDate(dtn);
            bool _flag = true; //執行結果
            string _msg = string.Empty; //訊息
            List<VeClearSchedulerModel> _results = new List<VeClearSchedulerModel>();
            List<SYS_CODE> _SYS_CODEs = new List<SYS_CODE>();
            List<FAP_TEL_CODE> _FAP_TEL_CODEs = new List<FAP_TEL_CODE>();
            MemoryStream _stream = new MemoryStream();
            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            string fullPath = "";
            string guid = "";

            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //1.挑出【FAP_TEL_INTERVIEW 電訪及追蹤記錄檔】中"電訪覆核結果 = 11進入追踨、15轉行政單位" + "派件狀態 = 1.派件中"的資料
                    var _tel_appr_results = new List<string>() {
                    "11", //11進入追踨
                    "15"  //15轉行政單位
                };
                    var _dispatch_status = "1"; //1.派件中
                    var _FAP_TEL_INTERVIEWs = db.FAP_TEL_INTERVIEW.AsNoTracking()
                        .Where(x =>
                            _tel_appr_results.Contains(x.tel_appr_result) &&
                            x.dispatch_status == _dispatch_status).ToList();
                    //tel_result => 處理結果代碼
                    //tel_result_cnt => 處理結果已達次數
                    if (_FAP_TEL_INTERVIEWs.Any())
                    {
                        _SYS_CODEs = db.SYS_CODE.AsNoTracking()
                            .Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "tel_call").ToList();
                        var _code_type = "tel_call"; // tel_call：處理及追蹤結果暨追蹤標準
                        _FAP_TEL_CODEs = db.FAP_TEL_CODE.AsNoTracking()
                           .Where(x => x.code_type == _code_type).ToList();
                        //code_id => 處理結果代碼
                        _FAP_TEL_INTERVIEWs.ForEach(x =>
                        {
                            var _tel_result_cnt = x.tel_result_cnt >= 3 ? 3 : x.tel_result_cnt;
                            var _FAP_TEL_CODE = _FAP_TEL_CODEs.FirstOrDefault(y => y.code_id == x.tel_result);
                            if (_FAP_TEL_CODE != null && _tel_result_cnt != null)
                            {
                                var _std = 0;
                                switch (_tel_result_cnt)
                                {
                                    case 1:
                                        _std = _FAP_TEL_CODE.std_1 ?? 0;
                                        break;
                                    case 2:
                                        _std = _FAP_TEL_CODE.std_2 ?? 0;
                                        break;
                                    case 3:
                                        _std = _FAP_TEL_CODE.std_3 ?? 0;
                                        break;
                                }
                                if (_std != 0)
                                {
                                    var dates = GetWorkDate(DateForTWDate(x.tel_appr_datetime), _dtn);
                                    //已達追蹤標準 (時間範圍抓到的工作天數大於設定資料的天數
                                    var _VE_day = dates.Count - _std;
                                    if (_VE_day > 0)
                                    {
                                        try
                                        {
                                            List<OAP0046DModel> checkDataList = fAPTelCheckDao.qryForTelProcRpt(x.tel_proc_no);

                                            if (checkDataList.Count > 0)
                                            {
                                                OAP0046DModel check = checkDataList.Where(o => o.status != "1").FirstOrDefault();

                                                if (check != null)
                                                {
                                                    _results.Add(new VeClearSchedulerModel()
                                                    {
                                                        tel_proc_no = x.tel_proc_no, //電訪編號
                                                        proc_id = _FAP_TEL_CODE.proc_id, //追蹤人員
                                                        VE_day = _VE_day.ToString(), //逾期天數
                                                        VE_memo = $@"逾第{_tel_result_cnt}次追蹤標準", //逾期原因
                                                        tel_interview_date = x.tel_interview_f_datetime?.ToString("yyyy/MM/dd"), //最後一次電訪日期                                                                                                                   //tel_result_last = Get_tel_result(_SYS_CODEs, x.tel_result), //電訪處理結果_統計
                                                        tel_result_last = x.tel_result, //電訪處理結果_統計
                                                        tel_result_cnt = _tel_result_cnt ?? 0, //處理結果已達次數
                                                        paid_id = check.paid_id,
                                                        paid_name = check.paid_name
                                                    });
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Error(e.ToString());
                                        }

                                    }
                                }
                            }
                        });
                    }
                    if (_results.Any()) //寄送已達追蹤條件的資料
                    {
                        var _tel_proc_nos = _results.Select(x => x.tel_proc_no).ToList();
                        var _dataTypes = new List<string>() { "1", "2" };
                        var FAP_TEL_INTERVIEW_HISs = db.FAP_TEL_INTERVIEW_HIS.AsNoTracking()
                            .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                            (_dataTypes.Contains(x.data_type))).ToList();
                        var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                            .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                            x.tel_std_type == "tel_assign_case" && x.data_flag == "Y").ToList();
                        var systems = FAP_TEL_CHECKs.Select(x => x.system).Distinct().ToList();
                        var check_nos = FAP_TEL_CHECKs.Select(x => x.check_no).Distinct().ToList();
                        var check_acct_shorts = FAP_TEL_CHECKs.Select(x => x.check_acct_short).Distinct().ToList();
                        var FAP_VE_TRACEs = db.FAP_VE_TRACE.AsNoTracking()
                            .Where(x => systems.Contains(x.system) && x.status != "1" &&
                            check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();

                        foreach (var item in _results)
                        {
                            var _FAP_TEL_INTERVIEW_HIS_F = FAP_TEL_INTERVIEW_HISs
                                .FirstOrDefault(x => x.tel_proc_no == item.tel_proc_no && x.data_type == "1");
                            if (_FAP_TEL_INTERVIEW_HIS_F != null)
                            {
                                item.tel_result_first = Get_tel_result(_SYS_CODEs, _FAP_TEL_INTERVIEW_HIS_F.tel_result); //第一次電訪處理結果_統計
                            }
                            //var _FAP_TEL_CHECK = FAP_TEL_CHECKs.FirstOrDefault(x => x.tel_proc_no == item.tel_proc_no);
                            //if (_FAP_TEL_CHECK != null)
                            //{
                            //    var _FAP_VE_TRACE = FAP_VE_TRACEs.FirstOrDefault(x =>
                            //    x.system == _FAP_TEL_CHECK.system &&
                            //    x.check_no == _FAP_TEL_CHECK.check_no &&
                            //    x.check_acct_short == _FAP_TEL_CHECK.check_acct_short);
                            //    if (_FAP_VE_TRACE != null)
                            //    {
                            //        item.paid_id = _FAP_VE_TRACE.paid_id; //給付對象 ID
                            //        item.paid_name = _FAP_VE_TRACE.paid_name; //給付對象
                            //    }
                            //}
                            
                            Set_tel_interview("VE_Clear_Scheduler", item, FAP_TEL_INTERVIEW_HISs, _FAP_TEL_CODEs); //設定 追踨標準 & 追踨日期
                            item.tel_result_last = Get_tel_result(_SYS_CODEs, item.tel_result_last); //電訪處理結果_統計
                        }
                    }
                }
                //寄送追蹤報表給相關追蹤人員 
                if (_results.Any())
                {
                    var _blue = new XSSFColor(new byte[3] { 51, 102, 255 });
                    var userMemo = GetMemoByUserId(_results.Select(x => x.proc_id).Distinct(), true);
                    //var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                    //var mailAccount = ConfigurationManager.AppSettings["mailAccount"] ?? string.Empty;

                    guid = "T_" +Guid.NewGuid().ToString();

                    IWorkbook wb_e = new XSSFWorkbook();
                    NPOI.SS.UserModel.IFont whiteFont_e = wb_e.CreateFont();
                    whiteFont_e.Color = IndexedColors.White.Index; //White
                    var k = 0;
                    foreach (var _group in _results.GroupBy(x => x.proc_id))
                    {
                        fullPath = "";

                        k += 1;
                        var _userMemo = userMemo.FirstOrDefault(x => x.Item1 == _group.Key);
                        XLWorkbook Workbook = new XLWorkbook();

                        if ("E".Equals(type))
                        {
                            fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_" + guid, ".xlsx"));
                            
                            if (File.Exists(fullPath))
                                Workbook = new XLWorkbook(fullPath);
                        }
                        else {
                            fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_" +  guid + "_" + _group.Key, ".xlsx"));
                        }
                        var Worksheets = Workbook.Worksheets.Add($@"sheet1");
                        Workbook.SaveAs(fullPath);

                        using (XLWorkbook wb = new XLWorkbook(fullPath))
                        {
                            var ws = wb.Worksheet($@"sheet1");
                            ws.Name = $@"{_group.Key }";

                            ws.Cell("A1").Value = "電訪逾期追蹤未處理報表";
                            ws.Cell("A2").Value = $@"資料統計截止日：{dtn.ToString("yyyy-MM-dd")}";
                            ws.Range(1, 1, 1, 15).Merge();
                            ws.Range(2, 1, 2, 15).Merge();

                            ws.Cell(3, 1).Value = "電訪編號";
                            ws.Cell(3, 2).Value = "給付對象 ID";
                            ws.Cell(3, 3).Value = "給付對象";
                            ws.Cell(3, 4).Value = "追蹤人員";
                            ws.Cell(3, 5).Value = "逾期天數";
                            ws.Cell(3, 6).Value = "逾期原因";
                            ws.Cell(3, 7).Value = "第一次電訪處理結果_統計";
                            ws.Cell(3, 8).Value = "最後一次電訪日期";
                            ws.Cell(3, 9).Value = "電訪處理結果_統計";
                            ws.Cell(3, 10).Value = "第一次追踨標準";
                            ws.Cell(3, 11).Value = "第一次追蹤承辦回覆日";
                            ws.Cell(3, 12).Value = "第二次追踨標準";
                            ws.Cell(3, 13).Value = "第二次追蹤承辦回覆日";
                            ws.Cell(3, 14).Value = "第三次追踨標準";
                            ws.Cell(3, 15).Value = "第三次追蹤承辦回覆日";

                            int iRow = 3;
                            foreach (var _item in _group)
                            {
                                iRow++;
                                ws.Cell(iRow, 1).Value = _item.tel_proc_no;
                                ws.Cell(iRow, 2).Value = _item.paid_id;
                                ws.Cell(iRow, 3).Value = _item.paid_name;
                                ws.Cell(iRow, 4).Value = _userMemo.Item2;
                                ws.Cell(iRow, 5).Value = _item.VE_day;
                                ws.Cell(iRow, 6).Value = _item.VE_memo;
                                ws.Cell(iRow, 7).Value = _item.tel_result_first;
                                ws.Cell(iRow, 8).Value = _item.tel_interview_date;
                                ws.Cell(iRow, 9).Value = _item.tel_result_last;
                                ws.Cell(iRow, 10).Value = _item.tel_result_cnt_1;
                                ws.Cell(iRow, 11).Value = _item.tel_interview_date_1;
                                ws.Cell(iRow, 12).Value = _item.tel_result_cnt_2;
                                ws.Cell(iRow, 13).Value = _item.tel_interview_date_2;
                                ws.Cell(iRow, 14).Value = _item.tel_result_cnt_3;
                                ws.Cell(iRow, 15).Value = _item.tel_interview_date_3;

                            }

                            ws.Range(1, 1, 2, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Range(1, 1, iRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                            ws.Range(1, 1, iRow, 15).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            ws.Range(3, 1, 3, 15).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                            ws.Range(3, 1, 3, 15).Style.Font.FontColor = XLColor.White;

                            ws.Columns().AdjustToContents();  // Adjust column width
                            ws.Rows().AdjustToContents();     // Adjust row heights


                            wb.SaveAs(fullPath);
                        }



                        #region 後續動作

                        #region Mail
                        if (type == "A" || type == "M") //Mail
                        {
                            //modify by daiyu 20200926
                            try
                            {
                                MailUtil mailUtil = new MailUtil();

                                string chDt = BO.DateUtil.getCurChtDate(3);
                                string _fullPath = fullPath.Replace(".xlsx", ".zip");

                                using (var zip = new ZipFile())
                                {
                                    zip.Password = _userMemo.Item1 + chDt;
                                    zip.AddFile(fullPath, "");

                                    zip.Save(_fullPath);
                                }

                                File.Delete(fullPath);

                                //modify by daiyu 20210128
                                CommonUtil commonUtil = new CommonUtil();
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(_userMemo.Item1);
                                string[] mailToArr = new string[] { adModel.e_mail };
                                //string[] mailToArr = new string[] { _userMemo.Item5 };

                                bool bSucess = mailUtil.sendMail(mailToArr
                                    , "電訪逾期未處理報表"
                                    , "已達追蹤條件的資料"
                                    , true
                                   , ""
                                   , ""
                                   , new string[] { _fullPath }
                                   , true);
                            }
                            catch (Exception e)
                            {
                                _flag = false;
                                _msg = $@" SendMail Error : {e.exceptionMessage()}";
                                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
                            }

                        }
                        #endregion



                        #endregion

                    }


                    #region PIA
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = userId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "BAP0004";
                    piaLogMain.EXECUTION_CONTENT = "";
                    piaLogMain.AFFECT_ROWS = _results.Count;
                    piaLogMain.PIA_TYPE = "1100000000";
                    piaLogMain.EXECUTION_TYPE = "E";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
                    piaLogMainDao.Insert(piaLogMain);
                    #endregion

                    _msg = $@"總共查詢資料 : {_results.Count} 筆";
                }
                else
                {
                    _msg = "比對後無已達追蹤條件的資料";
                }
            }
            catch (Exception ex)
            {
                _flag = false;
                _msg = $@"Error : {ex.exceptionMessage()}";
                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
            }
            finally
            {
                _stream.Flush();
                _stream.Position = 0;
            }
            return new Tuple<bool, string, string>(_flag, _msg, guid);
        }

        /// <summary>
        /// 比對資料，查已達N個月需重新派件的資料
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        public Tuple<bool, string> VE_Clear_ReDispatch(string userId = null)
        {
            bool _flag = false; //執行結果
            string _msg = string.Empty; //訊息

            List<FAP_TEL_INTERVIEW> FAP_TEL_INTERVIEW_update = new List<FAP_TEL_INTERVIEW>();
            List<FAP_TEL_CHECK> FAP_TEL_CHECK_update = new List<FAP_TEL_CHECK>();
            List<FAP_TEL_INTERVIEW_HIS> FAP_TEL_INTERVIEW_HIS_insert = new List<FAP_TEL_INTERVIEW_HIS>();
            List<FAP_TEL_CHECK_HIS> FAP_TEL_CHECK_HIS_insert = new List<FAP_TEL_CHECK_HIS>();

            try
            {
                DateTime dtn = DateTime.Now;
                var _dtn = dtn.Date;
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //尚未派件月份(個月) =【SYS_PARA】中SYS_CD = 'AP' + GRP_ID = 'tel_assign_case' + PAPA_ID = 'assign_month'
                    var _assign_month = db.SYS_PARA.AsNoTracking()
                        .Where(x => x.SYS_CD == "AP" &&
                        x.GRP_ID == "tel_assign_case" &&
                        x.PARA_ID == "assign_month").FirstOrDefault()?.PARA_VALUE;
                    if (!_assign_month.IsNullOrWhiteSpace()) //已設定 尚未派件月份(個月)
                    {
                        var _assign_month_int = 0;
                        if (Int32.TryParse(_assign_month, out _assign_month_int))
                        {
                            var FAP_TEL_INTERVIEWs = db.FAP_TEL_INTERVIEW.Where(x =>
                            x.tel_result_cnt >= 3 && //處理結果已達次數 = 3
                            x.dispatch_status == "1" &&  //派件狀態 =  1 派件中
                            x.tel_appr_datetime != null)
                            .AsEnumerable()
                            .Where(x => x.tel_appr_datetime.Value.AddMonths(_assign_month_int) < _dtn).ToList();
                            //例：尚未派件月份(個月) = 3 個月、最後一次電訪覆核日期 = 2020/6/1
                            //-- > 則系統日 = 2020 / 9 / 1時，若派件狀態仍 <> "2 電訪結束"，
                            //財務部需至『OAP0043 電訪派件標準設定作業』重新設定人員，若重新派件，
                            //原有電訪編號的流程就算結束，若重派，則屬新的一個電訪編號。
                            var _tel_proc_nos = FAP_TEL_INTERVIEWs.Select(x => x.tel_proc_no).ToList();
                            var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                                .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                                x.tel_std_type == "tel_assign_case" && x.data_flag == "Y" & x.dispatch_status != "2").ToList();
                            var systems = FAP_TEL_CHECKs.Select(x => x.system).Distinct().ToList();
                            var check_nos = FAP_TEL_CHECKs.Select(x => x.check_no).Distinct().ToList();
                            var check_acct_shorts = FAP_TEL_CHECKs.Select(x => x.check_acct_short).Distinct().ToList();
                            var FAP_VE_TRACEs = db.FAP_VE_TRACE.AsNoTracking()
                                .Where(x => systems.Contains(x.system) && x.status != "1" &&
                                check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();
                            FAP_TEL_INTERVIEWs.ForEach(x =>
                            {
                                bool FAP_TEL_INTERVIEW_flag = false; //異動【FAP_TEL_INTERVIEW 電訪及追蹤記錄檔】註記
                                var _aply_no = string.Empty;
                                foreach (var _FAP_TEL_CHECK in FAP_TEL_CHECKs.Where(y => y.tel_proc_no == x.tel_proc_no ))
                                {
                                    if (_FAP_TEL_CHECK != null)
                                    {
                                        var _FAP_VE_TRACE = FAP_VE_TRACEs.FirstOrDefault(y => y.system == _FAP_TEL_CHECK.system &&
                                        y.check_no == _FAP_TEL_CHECK.check_no && y.check_acct_short == _FAP_TEL_CHECK.check_acct_short);
                                        if (_FAP_VE_TRACE != null) //符合條件
                                        {
                                            FAP_TEL_INTERVIEW_flag = true;
                                            // 異動【FAP_TEL_CHECK 電訪支票檔】
                                            FAP_TEL_CHECK_update.Add(new FAP_TEL_CHECK()
                                            {
                                                system = _FAP_TEL_CHECK.system,
                                                check_no = _FAP_TEL_CHECK.check_no,
                                                check_acct_short = _FAP_TEL_CHECK.check_acct_short,
                                                tel_std_aply_no = _FAP_TEL_CHECK.tel_std_aply_no,
                                                tel_std_type = _FAP_TEL_CHECK.tel_std_type,
                                                dispatch_status = "3", //派件狀態 = 3 重新派件
                                                dispatch_date = null, //派件日清空
                                                tel_proc_no = null, //電訪編號清空
                                                update_id = userId, //異動人員=userId
                                                update_datetime = dtn // 異動時間=系統時間
                                            });
                                            if(_aply_no.IsNullOrWhiteSpace())
                                                _aply_no = getaply_no("B0004");
                                            //var _FAP_TEL_CHECK_HIS_aplu_no = getaply_no("B0004");
                                            var _FAP_TEL_CHECK_HIS = ModelConvert<FAP_TEL_CHECK, FAP_TEL_CHECK_HIS>(_FAP_TEL_CHECK);
                                            _FAP_TEL_CHECK_HIS.aply_no = _aply_no;
                                            _FAP_TEL_CHECK_HIS.appr_stat = "2"; // 覆核狀態：2核可
                                            _FAP_TEL_CHECK_HIS.dispatch_status = "3"; //派件狀態 = 3 重新派件
                                            _FAP_TEL_CHECK_HIS.update_id = userId; //異動人員=userId
                                            _FAP_TEL_CHECK_HIS.update_datetime = dtn; // 異動時間=系統時間
                                            _FAP_TEL_CHECK_HIS.appr_id = userId; //覆核人員=userId
                                            _FAP_TEL_CHECK_HIS.approve_datetime = dtn; // 覆核時間=系統時間
                                            FAP_TEL_CHECK_HIS_insert.Add(_FAP_TEL_CHECK_HIS);
                                        }
                                    }
                                }
                                if (FAP_TEL_INTERVIEW_flag)
                                {
                                    //異動【FAP_TEL_INTERVIEW 電訪及追蹤記錄檔】
                                    FAP_TEL_INTERVIEW_update.Add(new FAP_TEL_INTERVIEW()
                                    {
                                        tel_proc_no = x.tel_proc_no,
                                        dispatch_status = "3", //派件狀態 = 3 重新派件
                                        update_id = userId, //異動人員=userId
                                        update_datetime = dtn // 異動時間=系統時間
                                    });
                                    var _FAP_TEL_INTERVIEW_HIS = ModelConvert<FAP_TEL_INTERVIEW, FAP_TEL_INTERVIEW_HIS>(x);
                                    _FAP_TEL_INTERVIEW_HIS.aply_no = _aply_no;
                                    _FAP_TEL_INTERVIEW_HIS.data_type = "1"; //資料類別：1 第一次電訪結果
                                    _FAP_TEL_INTERVIEW_HIS.appr_stat = "2"; // 覆核狀態：2核可
                                    _FAP_TEL_INTERVIEW_HIS.dispatch_status = "3"; //派件狀態 = 3 重新派件
                                    _FAP_TEL_INTERVIEW_HIS.update_id = userId; //異動人員=userId
                                    _FAP_TEL_INTERVIEW_HIS.update_datetime = dtn; // 異動時間=系統時間
                                    _FAP_TEL_INTERVIEW_HIS.appr_id = userId; //覆核人員=userId
                                    _FAP_TEL_INTERVIEW_HIS.approve_datetime = dtn; // 覆核時間=系統時間
                                    FAP_TEL_INTERVIEW_HIS_insert.Add(_FAP_TEL_INTERVIEW_HIS);
                                }
                            });
                            if (FAP_TEL_INTERVIEW_update.Any())
                            {
                                #region 異動資料
                                using (SqlConnection conn = new SqlConnection(DefaultConn))
                                {
                                    #region sql 語法
                                    //update FAP_TEL_INTERVIEW 
                                    string strSql1 = $@"
update FAP_TEL_INTERVIEW 
set dispatch_status = @dispatch_status,
    update_id = @update_id,
    update_datetime = @update_datetime
where tel_proc_no = @tel_proc_no ;
";
                                    //INSERT INTO [dbo].[FAP_TEL_INTERVIEW_HIS]
                                    string strSql2 = $@"
INSERT INTO [dbo].[FAP_TEL_INTERVIEW_HIS]
           ([aply_no]
           ,[tel_proc_no]
           ,[data_type]
           ,[tel_interview_id]
           ,[tel_interview_datetime]
           ,[tel_result]
           ,[tel_result_cnt]
           ,[tel_appr_result]
           ,[called_person]
           ,[record_no]
           ,[tel_addr]
           ,[tel_zip_code]
           ,[tel_mail]
           ,[cust_tel]
           ,[level_1]
           ,[level_2]
           ,[cust_counter]
           ,[counter_date]
           ,[reason]
           ,[remark]
           ,[dispatch_date]
           ,[dispatch_status]
           ,[clean_status]
           ,[clean_date]
           ,[clean_f_date]
           ,[exec_action]
           ,[update_id]
           ,[update_datetime]
           ,[appr_stat]
           ,[appr_id]
           ,[approve_datetime]
           ,[tel_interview_f_datetime])
     VALUES
           (@aply_no, 
            @tel_proc_no,
            @data_type, 
            @tel_interview_id,
            @tel_interview_datetime, 
            @tel_result,
            @tel_result_cnt, 
            @tel_appr_result, 
            @called_person,
            @record_no,
            @tel_addr, 
            @tel_zip_code, 
            @tel_mail, 
            @cust_tel,
            @level_1, 
            @level_2, 
            @cust_counter,
            @counter_date, 
            @reason,
            @remark, 
            @dispatch_date, 
            @dispatch_status, 
            @clean_status, 
            @clean_date,
            @clean_f_date,
            @exec_action, 
            @update_id, 
            @update_datetime,
            @appr_stat, 
            @appr_id, 
            @approve_datetime, 
            @tel_interview_f_datetime)
";
                                    //update FAP_TEL_CHECK 
                                    string strSql3 = $@"
update FAP_TEL_CHECK 
set dispatch_status = @dispatch_status,
    dispatch_date = @dispatch_date,
    tel_proc_no = @tel_proc_no,
    update_id = @update_id,
    update_datetime = @update_datetime
where system = @system 
and check_no = @check_no
and check_acct_short = @check_acct_short
and tel_std_aply_no = @tel_std_aply_no
and tel_std_type = @tel_std_type ;
";
                                    //INSERT INTO [dbo].[FAP_TEL_CHECK_HIS]
                                    string strSql4 = $@"
INSERT INTO [dbo].[FAP_TEL_CHECK_HIS]
           ([aply_no]
           ,[system]
           ,[check_no]
           ,[check_acct_short]
           ,[tel_std_aply_no]
           ,[tel_std_type]
           ,[fsc_range]
           ,[amt_range]
           ,[tel_proc_no]
           ,[tel_interview_id]
           ,[remark]
           ,[dispatch_date]
           ,[dispatch_status]
           ,[sms_date]
           ,[sms_status]
           ,[sec_stat]
           ,[appr_stat]
           ,[update_id]
           ,[update_datetime]
           ,[appr_id]
           ,[approve_datetime])
     VALUES
           (@aply_no,
            @system, 
            @check_no, 
            @check_acct_short, 
            @tel_std_aply_no,
            @tel_std_type,
            @fsc_range, 
            @amt_range,
            @tel_proc_no, 
            @tel_interview_id, 
            @remark,
            @dispatch_date, 
            @dispatch_status,
            @sms_date,
            @sms_status,
            @sec_stat, 
            @appr_stat,
            @update_id, 
            @update_datetime, 
            @appr_id, 
            @approve_datetime) 
";
                                    #endregion

                                    conn.Open();

                                    //交易
                                    using (SqlTransaction tran = conn.BeginTransaction())
                                    {
                                        int result1 = conn.Execute(strSql1, FAP_TEL_INTERVIEW_update, tran);
                                        int result2 = conn.Execute(strSql2, FAP_TEL_INTERVIEW_HIS_insert, tran);
                                        int result3 = conn.Execute(strSql3, FAP_TEL_CHECK_update, tran);
                                        int result4 = conn.Execute(strSql4, FAP_TEL_CHECK_HIS_insert, tran);
                                        if (result1 == FAP_TEL_INTERVIEW_update.Count &&
                                           result2 == FAP_TEL_INTERVIEW_HIS_insert.Count &&
                                           result3 == FAP_TEL_CHECK_update.Count &&
                                           result4 == FAP_TEL_CHECK_HIS_insert.Count)
                                        {
                                            _flag = true;
                                            tran.Commit();
                                        }
                                        else
                                        {
                                            _msg = "資料有異動,請從新執行!";
                                        }
                                    }
                                }
                                #endregion
                                #region PIA
                                if (_flag)
                                {
                                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_INTERVIEW_update.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "E";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
                                    piaLogMainDao.Insert(piaLogMain);

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_INTERVIEW_HIS_insert.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "A";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW_HIS";
                                    piaLogMainDao.Insert(piaLogMain);

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_CHECK_update.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "E";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
                                    piaLogMainDao.Insert(piaLogMain);

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_CHECK_HIS_insert.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "A";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK_HIS";
                                    piaLogMainDao.Insert(piaLogMain);
                                }
                                #endregion
                            }
                            else
                            {
                                _flag = true; //執行成功
                                _msg = "無符合資料";
                            }
                        }
                        else
                        {
                            _msg = "設定檔有誤";
                        }
                    }
                    else
                    {
                        _msg = "找不到設定檔";
                    }
                }


                //add by daiyu 20200926 MAIL重派清單
                if (FAP_TEL_CHECK_update.Count > 0) {

                    string guid = Guid.NewGuid().ToString();

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("BAP0004" + "TelReDispatch" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("電訪重新派件清單");

                        ws.Cell(1, 1).Value = "支票號碼";
                        ws.Cell(1, 2).Value = "支票帳號簡稱";
                        ws.Cell(1, 3).Value = "系統別";

                        int iRow = 1;
                        foreach (FAP_TEL_CHECK d in FAP_TEL_CHECK_update)
                        {
                            iRow++;
                            ws.Cell(iRow, 1).Value = d.check_no;
                            ws.Cell(iRow, 2).Value = d.check_acct_short;
                            ws.Cell(iRow, 3).Value = d.system;
                        }

                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights

                        wb.SaveAs(fullPath);
                    }

                    mailVeTel(fullPath, "電訪重新派件清單");    //E_MAIL通知電訪群組


                }


            }
            catch (Exception ex)
            {
                _msg = ex.exceptionMessage();
                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
            }
            return new Tuple<bool, string>(_flag, _msg);
        }

        /// <summary>
        ///  比對資料，查已達N個月未聯繫的資料
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        public Tuple<bool,string> VE_Clear_Cust(string userId = null)
        {
            bool _flag = false; //執行結果
            string _msg = string.Empty; //訊息
            List<FAP_TEL_CHECK> FAP_TEL_CHECK_update = new List<FAP_TEL_CHECK>();
            List<FAP_TEL_CHECK_HIS> FAP_TEL_CHECK_HIS_insert = new List<FAP_TEL_CHECK_HIS>();

            try
            {
                DateTime dtn = DateTime.Now;
                var _dtn = dtn.Date;
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //尚未派件月份(個月) =【SYS_PARA】中SYS_CD = 'AP' + GRP_ID = 'tel_assign_case' + PAPA_ID = 'assign_month'
                    var _assign_month = db.SYS_PARA.AsNoTracking()
                        .Where(x => x.SYS_CD == "AP" &&
                        x.GRP_ID == "tel_assign_case" &&
                        x.PARA_ID == "assign_month").FirstOrDefault()?.PARA_VALUE;
                    if (!_assign_month.IsNullOrWhiteSpace()) //已設定 尚未派件月份(個月)
                    {
                        var _assign_month_int = 0;
                        var _aply_no = string.Empty;
                        if (Int32.TryParse(_assign_month, out _assign_month_int))
                        {
                            var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                                .Where(x => 
                                x.data_flag == "Y" &&
                                x.dispatch_date != null &&
                                x.tel_std_type == "tel_assign_case" &&
                                //x.dispatch_status == "12" &&  
                                (x.tel_proc_no == "" || x.tel_proc_no == null))
                                .AsEnumerable()
                                .Where(x => x.dispatch_date.Value.AddMonths(_assign_month_int) < _dtn).ToList();
                            var systems = FAP_TEL_CHECKs.Select(x => x.system).Distinct().ToList();
                            var check_nos = FAP_TEL_CHECKs.Select(x => x.check_no).Distinct().ToList();
                            var check_acct_shorts = FAP_TEL_CHECKs.Select(x => x.check_acct_short).Distinct().ToList();
                            var FAP_VE_TRACEs = db.FAP_VE_TRACE.AsNoTracking()
                                .Where(x => systems.Contains(x.system) && x.status != "1" &&
                                check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();
                            FAP_TEL_CHECKs.ForEach(x =>
                            {
                                var _FAP_VE_TRACE = FAP_VE_TRACEs.FirstOrDefault(y => y.system == x.system &&
                                     y.check_no == x.check_no && y.check_acct_short == x.check_acct_short);
                                if (_FAP_VE_TRACE != null) //符合條件
                                {
                                    // 異動【FAP_TEL_CHECK 電訪支票檔】
                                    FAP_TEL_CHECK_update.Add(new FAP_TEL_CHECK()
                                    {
                                        system = x.system,
                                        check_no = x.check_no,
                                        check_acct_short = x.check_acct_short,
                                        tel_std_aply_no = x.tel_std_aply_no,
                                        tel_std_type = x.tel_std_type,
                                        dispatch_status = "3", //派件狀態 = 3 重新派件
                                        dispatch_date = null, //派件日清空
                                        tel_proc_no = null, //電訪編號清空
                                        update_id = userId, //異動人員=userId
                                        update_datetime = dtn // 異動時間=系統時間
                                    });
                                    //var _FAP_TEL_CHECK_HIS_aplu_no = getaply_no("B0004");
                                    var _FAP_TEL_CHECK_HIS = ModelConvert<FAP_TEL_CHECK, FAP_TEL_CHECK_HIS>(x);

                                    if (_aply_no.IsNullOrWhiteSpace())
                                        _aply_no = getaply_no("B0004");
                                    _FAP_TEL_CHECK_HIS.aply_no = _aply_no;

                                    _FAP_TEL_CHECK_HIS.appr_stat = "2"; // 覆核狀態：2核可
                                    _FAP_TEL_CHECK_HIS.dispatch_status = "3"; //派件狀態 = 3 重新派件
                                    _FAP_TEL_CHECK_HIS.update_id = userId; //異動人員=userId
                                    _FAP_TEL_CHECK_HIS.update_datetime = dtn; // 異動時間=系統時間
                                    _FAP_TEL_CHECK_HIS.appr_id = userId; //覆核人員=userId
                                    _FAP_TEL_CHECK_HIS.approve_datetime = dtn; // 覆核時間=系統時間
                                    FAP_TEL_CHECK_HIS_insert.Add(_FAP_TEL_CHECK_HIS);
                                }
                            });
                            if (FAP_TEL_CHECK_update.Any())
                            {
                                #region 異動資料
                                using (SqlConnection conn = new SqlConnection(DefaultConn))
                                {
                                    #region sql 語法
                                    //update FAP_TEL_CHECK 
                                    string strSql1 = $@"
update FAP_TEL_CHECK 
set dispatch_status = @dispatch_status,
    dispatch_date = @dispatch_date,
    tel_proc_no = @tel_proc_no,
    update_id = @update_id,
    update_datetime = @update_datetime
where system = @system 
and check_no = @check_no
and check_acct_short = @check_acct_short
and tel_std_aply_no = @tel_std_aply_no
and tel_std_type = @tel_std_type ;
";
                                    //INSERT INTO [dbo].[FAP_TEL_CHECK_HIS]
                                    string strSql2 = $@"
INSERT INTO [dbo].[FAP_TEL_CHECK_HIS]
           ([aply_no]
           ,[system]
           ,[check_no]
           ,[check_acct_short]
           ,[tel_std_aply_no]
           ,[tel_std_type]
           ,[fsc_range]
           ,[amt_range]
           ,[tel_proc_no]
           ,[tel_interview_id]
           ,[remark]
           ,[dispatch_date]
           ,[dispatch_status]
           ,[sms_date]
           ,[sms_status]
           ,[sec_stat]
           ,[appr_stat]
           ,[update_id]
           ,[update_datetime]
           ,[appr_id]
           ,[approve_datetime])
     VALUES
           (@aply_no,
            @system, 
            @check_no, 
            @check_acct_short, 
            @tel_std_aply_no,
            @tel_std_type,
            @fsc_range, 
            @amt_range,
            @tel_proc_no, 
            @tel_interview_id, 
            @remark,
            @dispatch_date, 
            @dispatch_status,
            @sms_date,
            @sms_status,
            @sec_stat, 
            @appr_stat,
            @update_id, 
            @update_datetime, 
            @appr_id, 
            @approve_datetime) 
";
                                    #endregion

                                    conn.Open();

                                    //交易
                                    using (SqlTransaction tran = conn.BeginTransaction())
                                    {
                                        int result3 = conn.Execute(strSql1, FAP_TEL_CHECK_update, tran);
                                        int result4 = conn.Execute(strSql2, FAP_TEL_CHECK_HIS_insert, tran);
                                        if (result3 == FAP_TEL_CHECK_update.Count &&
                                            result4 == FAP_TEL_CHECK_HIS_insert.Count)
                                        {
                                            _flag = true;
                                            tran.Commit();
                                        }
                                        else
                                        {
                                            _msg = "資料有異動,請從新執行!";
                                        }
                                    }
                                }
                                #endregion
                                #region PIA
                                if (_flag)
                                {
                                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_CHECK_update.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "E";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
                                    piaLogMainDao.Insert(piaLogMain);

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_CHECK_HIS_insert.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "A";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK_HIS";
                                    piaLogMainDao.Insert(piaLogMain);
                                }
                                #endregion
                            }
                            else
                            {
                                _flag = true; //執行成功
                                _msg = "無符合資料";
                            }
                        }
                        else
                        {
                            _msg = "設定檔有誤";
                        }
                    }
                    else
                    {
                        _msg = "找不到設定檔";
                    }

                }


                //add by daiyu 20200926 MAIL未聯繫清單
                if (FAP_TEL_CHECK_update.Count > 0)
                {

                    string guid = Guid.NewGuid().ToString();

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("BAP0004" + "TelNoProc" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("電訪已達指定時間未聯繫清單");

                        ws.Cell(1, 1).Value = "支票號碼";
                        ws.Cell(1, 2).Value = "支票帳號簡稱";
                        ws.Cell(1, 3).Value = "系統別";

                        int iRow = 1;
                        foreach (FAP_TEL_CHECK d in FAP_TEL_CHECK_update)
                        {
                            iRow++;
                            ws.Cell(iRow, 1).Value = d.check_no;
                            ws.Cell(iRow, 2).Value = d.check_acct_short;
                            ws.Cell(iRow, 3).Value = d.system;
                        }

                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights

                        wb.SaveAs(fullPath);
                    }

                    mailVeTel(fullPath, "電訪已達指定時間未聯繫清單");    //E_MAIL通知電訪群組

                }
            }
            catch (Exception ex)
            {
                _msg = ex.exceptionMessage();
                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
            }

            return new Tuple<bool, string>(_flag, _msg);
        }

        /// <summary>
        /// 比對資料，將符合設定期間的簡訊資訊清空
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        public Tuple<bool, string> SMS_Clear(string userId = null)
        {
            bool _flag = false; //執行結果
            string _msg = string.Empty; //訊息
            List<FAP_TEL_CHECK> FAP_TEL_CHECK_update = new List<FAP_TEL_CHECK>();
            List<FAP_TEL_CHECK_HIS> FAP_TEL_CHECK_HIS_insert = new List<FAP_TEL_CHECK_HIS>();

            try
            {
                DateTime dtn = DateTime.Now;
                var _dtn = dtn.Date;
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //3.2.1.2.1尚未派件月份(個月) =【SYS_PARA】中SYS_CD = 'AP' + GRP_ID = 'sms_assign_case' + PAPA_ID = 'assign_month'
                    var _assign_month = db.SYS_PARA.AsNoTracking()
                        .Where(x => x.SYS_CD == "AP" &&
                        x.GRP_ID == "sms_assign_case" &&
                        x.PARA_ID == "sms_clear_month").FirstOrDefault()?.PARA_VALUE;
                    if (!_assign_month.IsNullOrWhiteSpace()) //已設定 尚未派件月份(個月)
                    {
                        var _assign_month_int = 0;
                        var _aply_no = string.Empty;

                        if (Int32.TryParse(_assign_month, out _assign_month_int))
                        {
                            var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                               .Where(x => x.sms_status == "1" && //簡訊狀態 = 1 已發送
                               x.data_flag == "Y" &&  // 有效註記 = "Y"
                               x.tel_std_type == "sms_assign_case"
                               ) 
                               .AsEnumerable()
                               .Where(x => x.sms_date != null &&
                                x.sms_date.Value.AddMonths(_assign_month_int) < _dtn).ToList();

                            var systems = FAP_TEL_CHECKs.Select(x => x.system).Distinct().ToList();
                            var check_nos = FAP_TEL_CHECKs.Select(x => x.check_no).Distinct().ToList();
                            var check_acct_shorts = FAP_TEL_CHECKs.Select(x => x.check_acct_short).Distinct().ToList();
                            var FAP_VE_TRACEs = db.FAP_VE_TRACE.AsNoTracking()
                                .Where(x => systems.Contains(x.system) && x.status != "1" &&
                                check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();

                            FAP_TEL_CHECKs.ForEach(x =>
                            {
                                var _FAP_VE_TRACE = FAP_VE_TRACEs.FirstOrDefault(y => y.system == x.system &&
                                     y.check_no == x.check_no && y.check_acct_short == x.check_acct_short);

                                if (_FAP_VE_TRACE != null) //符合條件
                                {
                                    // 異動【FAP_TEL_CHECK 電訪支票檔】
                                    FAP_TEL_CHECK_update.Add(new FAP_TEL_CHECK()
                                    {
                                        system = x.system,
                                        check_no = x.check_no,
                                        check_acct_short = x.check_acct_short,
                                        tel_std_aply_no = x.tel_std_aply_no,
                                        tel_std_type = x.tel_std_type,
                                        sms_date = null, //簡訊發送日 = 空值
                                        sms_status = "0", //簡訊狀態 = 0
                                        update_id = userId, //異動人員=userId
                                        update_datetime = dtn // 異動時間=系統時間
                                    });
                                    //var _FAP_TEL_CHECK_HIS_aplu_no = getaply_no("B0004");
                                    var _FAP_TEL_CHECK_HIS = ModelConvert<FAP_TEL_CHECK, FAP_TEL_CHECK_HIS>(x);

                                    if (_aply_no.IsNullOrWhiteSpace())
                                        _aply_no = getaply_no("B0004");
                                    _FAP_TEL_CHECK_HIS.aply_no = _aply_no;
                                    //_FAP_TEL_CHECK_HIS.aply_no = getaply_no("B0004");

                                    _FAP_TEL_CHECK_HIS.appr_stat = "2"; // 覆核狀態：2核可
                                                                        // _FAP_TEL_CHECK_HIS.sms_date = null; //簡訊發送日 = 空值
                                    _FAP_TEL_CHECK_HIS.sms_status = "0"; //簡訊狀態 = 0
                                    _FAP_TEL_CHECK_HIS.update_id = userId; //異動人員=userId
                                    _FAP_TEL_CHECK_HIS.update_datetime = dtn; //異動時間=系統時間
                                    _FAP_TEL_CHECK_HIS.appr_id = userId; //覆核人員=userId
                                    _FAP_TEL_CHECK_HIS.approve_datetime = dtn; //覆核時間=系統時間
                                    FAP_TEL_CHECK_HIS_insert.Add(_FAP_TEL_CHECK_HIS);
                                }
                              
                            });
                            if (FAP_TEL_CHECK_update.Any())
                            {
                                #region 異動資料
                                using (SqlConnection conn = new SqlConnection(DefaultConn))
                                {
                                    #region sql 語法
                                    //update FAP_TEL_CHECK 
                                    string strSql1 = $@"
update FAP_TEL_CHECK 
set sms_date = @sms_date,
    sms_status = @sms_status,
    update_id = @update_id,
    update_datetime = @update_datetime
where system = @system 
and check_no = @check_no
and check_acct_short = @check_acct_short
and tel_std_aply_no = @tel_std_aply_no
and tel_std_type = @tel_std_type ;
";
                                    //INSERT INTO [dbo].[FAP_TEL_CHECK_HIS]
                                    string strSql2 = $@"
INSERT INTO [dbo].[FAP_TEL_CHECK_HIS]
           ([aply_no]
           ,[system]
           ,[check_no]
           ,[check_acct_short]
           ,[tel_std_aply_no]
           ,[tel_std_type]
           ,[fsc_range]
           ,[amt_range]
           ,[tel_proc_no]
           ,[tel_interview_id]
           ,[remark]
           ,[dispatch_date]
           ,[dispatch_status]
           ,[sms_date]
           ,[sms_status]
           ,[sec_stat]
           ,[appr_stat]
           ,[update_id]
           ,[update_datetime]
           ,[appr_id]
           ,[approve_datetime])
     VALUES
           (@aply_no,
            @system, 
            @check_no, 
            @check_acct_short, 
            @tel_std_aply_no,
            @tel_std_type,
            @fsc_range, 
            @amt_range,
            @tel_proc_no, 
            @tel_interview_id, 
            @remark,
            @dispatch_date, 
            @dispatch_status,
            @sms_date,
            @sms_status,
            @sec_stat, 
            @appr_stat,
            @update_id, 
            @update_datetime, 
            @appr_id, 
            @approve_datetime) 
";
                                    #endregion

                                    conn.Open();

                                    //交易
                                    using (SqlTransaction tran = conn.BeginTransaction())
                                    {
                                        int result3 = conn.Execute(strSql1, FAP_TEL_CHECK_update, tran);
                                        int result4 = conn.Execute(strSql2, FAP_TEL_CHECK_HIS_insert, tran);
                                        if (result3 == FAP_TEL_CHECK_update.Count &&
                                            result4 == FAP_TEL_CHECK_HIS_insert.Count)
                                        {
                                            _flag = true;
                                            tran.Commit();
                                        }
                                        else
                                        {
                                            _msg = "資料有異動,請從新執行!";
                                        }
                                    }
                                }
                                #endregion
                                #region PIA
                                if (_flag)
                                {
                                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_CHECK_update.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "E";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
                                    piaLogMainDao.Insert(piaLogMain);

                                    piaLogMain = new PIA_LOG_MAIN();
                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "BAP0004";
                                    piaLogMain.EXECUTION_CONTENT = "";
                                    piaLogMain.AFFECT_ROWS = FAP_TEL_CHECK_HIS_insert.Count;
                                    piaLogMain.PIA_TYPE = "1100000000";
                                    piaLogMain.EXECUTION_TYPE = "A";
                                    piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK_HIS";
                                    piaLogMainDao.Insert(piaLogMain);
                                }
                                #endregion
                            }
                            else
                            {
                                _flag = true; //執行成功
                                _msg = "無符合資料";
                            }
                        }
                        else
                        {
                            _msg = "設定檔有誤";
                        }
                    }
                    else
                    {
                        _msg = "找不到設定檔";
                    }
                }

                //add by daiyu 20200926 MAIL重派清單
                if (FAP_TEL_CHECK_update.Count > 0)
                {

                    string guid = Guid.NewGuid().ToString();

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("BAP0004" + "SmsReDispatch" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("簡訊重新派件清單");

                        ws.Cell(1, 1).Value = "支票號碼";
                        ws.Cell(1, 2).Value = "支票帳號簡稱";
                        ws.Cell(1, 3).Value = "系統別";

                        int iRow = 1;
                        foreach (FAP_TEL_CHECK d in FAP_TEL_CHECK_update)
                        {
                            iRow++;
                            ws.Cell(iRow, 1).Value = d.check_no;
                            ws.Cell(iRow, 2).Value = d.check_acct_short;
                            ws.Cell(iRow, 3).Value = d.system;
                        }

                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights

                        wb.SaveAs(fullPath);
                    }

                    mailVeTel(fullPath, "簡訊重新派件清單");    //E_MAIL通知電訪群組


                }



            }
            catch (Exception ex)
            {
                _msg = ex.exceptionMessage();
                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
            }
            return new Tuple<bool, string>(_flag, _msg);
        }

        /// <summary>
        /// E_MAIL通知電訪群組
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="content"></param>
        private void mailVeTel(string fullPath, string content) {
            MailUtil mailUtil = new MailUtil();
            List<UserBossModel> notify = mailUtil.getMailGrpId("VE_TEL");

            string chDt = BO.DateUtil.getCurChtDate(3);

            foreach (UserBossModel d in notify)
            {
                string _fullPath = fullPath.Replace(".xlsx", "_" + d.usrId + ".zip");

                using (var zip = new ZipFile())
                {
                    zip.Password = d.usrId + chDt;
                    zip.AddFile(fullPath, "");

                    zip.Save(_fullPath);
                }

                string[] mailToArr = new string[] { d.empMail };

                bool bSucess = mailUtil.sendMail(mailToArr
            , content
            , ""
            , true
           , ""
           , ""
           , new string[] { _fullPath }
           , true);
            }

            File.Delete(fullPath);
        }


        /// <summary>
        /// 產生"清理暨逾期處理清單"
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type">執行類別 A:全部執行,M:僅執行寄送mail,E:僅執行產生Excel</param>
        /// <returns></returns>
        public Tuple<bool, string, string> VE_Level_Detail(string userId = null, string type = "A")
        {
            DateTime dtn = DateTime.Now;
            var _dtn = DateForTWDate(dtn);
            bool _flag = true; //執行結果
            string _msg = string.Empty; //訊息
            List<Tuple<string, List<VeLevelDetailModel>>> _results = new List<Tuple<string, List<VeLevelDetailModel>>>();
            List<tempModel> models = new List<tempModel>();
            List<SYS_CODE> _SYS_CODEs_tel_call = new List<SYS_CODE>();
            List<SYS_CODE> _SYS_CODEs_O_PAID_CD = new List<SYS_CODE>();
            List<FAP_TEL_CODE> _FAP_TEL_CODEs = new List<FAP_TEL_CODE>();
            List<FAP_VE_CODE> _FAP_VE_CODEs = new List<FAP_VE_CODE>();
            MemoryStream _stream = new MemoryStream();

            string fullPath = "";
            string guid = "";


            try
            {

                SysCodeDao sysCodeDao = new SysCodeDao();
                //清理狀態
                Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var _FAP_TEL_INTERVIEWs = db.FAP_TEL_INTERVIEW.AsNoTracking()
                        .Where(x =>
                            x.tel_appr_result == "13" && //13進入清理
                            x.clean_status != "" &&
                            x.clean_f_date == null).ToList();


                    //tel_result => 處理結果代碼
                    //tel_result_cnt => 處理結果已達次數
                    if (_FAP_TEL_INTERVIEWs.Any())
                    {
                        var _tel_proc_nos = _FAP_TEL_INTERVIEWs.Select(x => x.tel_proc_no).ToList();
                        var _code_type = "tel_clean"; // tel_clean：清理階段追蹤標準
                        //資料類別 3：清理階段
                        var _FAP_TEL_INTERVIEW_HISs = db.FAP_TEL_INTERVIEW_HIS.AsNoTracking().Where(x =>
                        x.appr_stat == "2" && _tel_proc_nos.Contains(x.tel_proc_no) &&
                        x.clean_date != null).ToList();

                        //var _FAP_TEL_INTERVIEW_HISs = db.FAP_TEL_INTERVIEW_HIS.AsNoTracking().Where(x =>
                        //x.appr_stat == "2" && _tel_proc_nos.Contains(x.tel_proc_no) &&
                        //x.data_type == "3" &&  x.clean_date != null).ToList();
                        _FAP_TEL_CODEs = db.FAP_TEL_CODE.AsNoTracking()
                           .Where(x => x.code_type == _code_type && x.std_1 != null).ToList();
                        //code_id => 處理結果代碼
                        foreach (FAP_TEL_INTERVIEW x in _FAP_TEL_INTERVIEWs) {

                            var _FAP_TEL_CODE = _FAP_TEL_CODEs.FirstOrDefault(y => y.code_id == x.clean_status);
                            var _FAP_TEL_INTERVIEW_HISs_g = _FAP_TEL_INTERVIEW_HISs.Where(y => y.tel_proc_no == x.tel_proc_no).ToList();

                            //add by daiyu 20201217
                            if (_FAP_TEL_INTERVIEW_HISs_g.Count == 0) {
                                FAP_TEL_INTERVIEW_HIS f = new FAP_TEL_INTERVIEW_HIS();
                                ObjectUtil.CopyPropertiesTo(x, f);
                                _FAP_TEL_INTERVIEW_HISs_g.Add(f);
                            }


                            var _FAP_TEL_INTERVIEW_HIS = _FAP_TEL_INTERVIEW_HISs_g.OrderByDescending(y => y.clean_date).FirstOrDefault();
                            if (_FAP_TEL_CODE != null && _FAP_TEL_INTERVIEW_HIS != null)
                            {
                                var _std = _FAP_TEL_CODE.std_1;
                                // var _clean_date = DateForTWDate(_FAP_TEL_INTERVIEW_HIS.clean_date);
                                var _clean_date = DateForTWDate(x.clean_date); //modify by daiyu 20210302
                                var dates = GetWorkDate(_clean_date, DateForTWDate(dtn));
                                //已達追蹤標準 (時間範圍抓到的工作天數大於設定資料的天數
                                var _VE_day = dates.Count - _std;
                                if (_VE_day > 0)
                                {
                                    List<cleanDateModel> _cleanDates = new List<cleanDateModel>();
                                    _FAP_TEL_INTERVIEW_HISs_g.ForEach(z =>
                                    {
                                        _cleanDates.Add(new cleanDateModel()
                                        {
                                            clean_date = DateForTWDate(z.clean_date),
                                            clean_f_date = DateForTWDate(z.clean_f_date),
                                            clean_status = z.clean_status
                                        });
                                    });
                                    models.Add(new tempModel()
                                    {
                                        tel_proc_no = x.tel_proc_no, //電訪編號
                                        clean_status = x.clean_status, //清理階段
                                        proc_id = _FAP_TEL_CODE.proc_id, //承辦人員
                                        VE_day = _VE_day.ToString(), //逾期天數
                                        last_clean_status = x.clean_status, //最後完成的清理階段
                                        cleanDates = _cleanDates, //完成日資料
                                        level_1 = x.level_1, //清理大類
                                        level_2 = x.level_2 //清理小類
                                    });
                                }
                            }
                        }


                        //_FAP_TEL_INTERVIEWs.ForEach(x =>
                        //{
                        //    var _FAP_TEL_CODE = _FAP_TEL_CODEs.FirstOrDefault(y => y.code_id == x.clean_status);
                        //    var _FAP_TEL_INTERVIEW_HISs_g = _FAP_TEL_INTERVIEW_HISs.Where(y => y.tel_proc_no == x.tel_proc_no).ToList();
                        //    var _FAP_TEL_INTERVIEW_HIS = _FAP_TEL_INTERVIEW_HISs_g.OrderByDescending(y => y.clean_date).FirstOrDefault();
                        //    if (_FAP_TEL_CODE != null && _FAP_TEL_INTERVIEW_HIS != null)
                        //    {
                        //        var _std = _FAP_TEL_CODE.std_1;
                        //        var _clean_date = DateForTWDate(_FAP_TEL_INTERVIEW_HIS.clean_date);
                        //        var dates = GetWorkDate(_clean_date, DateForTWDate(dtn));
                        //        //已達追蹤標準 (時間範圍抓到的工作天數大於設定資料的天數
                        //        var _VE_day = dates.Count - _std;
                        //        if (_VE_day > 0)
                        //        {
                        //            List<cleanDateModel> _cleanDates = new List<cleanDateModel>();
                        //            _FAP_TEL_INTERVIEW_HISs_g.ForEach(z =>
                        //            {
                        //                _cleanDates.Add(new cleanDateModel()
                        //                {
                        //                    clean_date = DateForTWDate(z.clean_date),
                        //                    clean_f_date = DateForTWDate(z.clean_f_date),
                        //                    clean_status = z.clean_status
                        //                });
                        //            });
                        //            models.Add(new tempModel()
                        //            {
                        //                tel_proc_no = x.tel_proc_no, //電訪編號
                        //                clean_status = x.clean_status, //清理階段
                        //                proc_id = _FAP_TEL_CODE.proc_id, //承辦人員
                        //                VE_day = _VE_day.ToString(), //逾期天數
                        //                last_clean_status = x.clean_status, //最後完成的清理階段
                        //                cleanDates = _cleanDates, //完成日資料
                        //                level_1 = x.level_1, //清理大類
                        //                level_2 = x.level_2 //清理小類
                        //            });
                        //        }
                        //    }
                        //});
                    }
                    if (models.Any()) //寄送已達追蹤條件的資料
                    {
                        _SYS_CODEs_tel_call = db.SYS_CODE.AsNoTracking()
                            .Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "tel_clean")
                            .OrderBy(x => x.ISORTBY).ToList();
                        _SYS_CODEs_O_PAID_CD = db.SYS_CODE.AsNoTracking()
                            .Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "O_PAID_CD").ToList();
                        List<string> _code_types = new List<string>() { "CLR_LEVEL1", "CLR_LEVEL2" };
                        _FAP_VE_CODEs = db.FAP_VE_CODE.AsNoTracking()
                            .Where(x => _code_types.Contains(x.code_type)).ToList();
                        var _tel_proc_nos = models.Select(x => x.tel_proc_no).ToList();

                        var _dataTypes = new List<string>() { "1", "2" };
                        var FAP_TEL_INTERVIEW_HISs = db.FAP_TEL_INTERVIEW_HIS.AsNoTracking()
                            .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                            (_dataTypes.Contains(x.data_type))).ToList();

                        var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                            .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                            x.tel_std_type == "tel_assign_case" && x.data_flag == "Y").ToList();
                        var systems = FAP_TEL_CHECKs.Select(x => x.system).ToList().Distinct();
                        var check_nos = FAP_TEL_CHECKs.Select(x => x.check_no).ToList().Distinct();
                        var check_acct_shorts = FAP_TEL_CHECKs.Select(x => x.check_acct_short).ToList().Distinct();

                        var FAP_VE_TRACEs = db.FAP_VE_TRACE.AsNoTracking()
                            .Where(x => systems.Contains(x.system) && !_dataTypes.Contains(x.status) &&
                            check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();

                        var FAP_VE_TRACE_POLIs = db.FAP_VE_TRACE_POLI.AsNoTracking()
                            .Where(x => check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();

                        foreach (var item in models)
                        {
                            var _FAP_TEL_CHECK = FAP_TEL_CHECKs.FirstOrDefault(x => x.tel_proc_no == item.tel_proc_no);
                            if (_FAP_TEL_CHECK != null)
                            {
                                var _FAP_VE_TRACE = FAP_VE_TRACEs.FirstOrDefault(x =>
                                x.system == _FAP_TEL_CHECK.system && x.check_no == _FAP_TEL_CHECK.check_no &&
                                x.check_acct_short == _FAP_TEL_CHECK.check_acct_short);
                                if (_FAP_VE_TRACE != null)
                                {
                                    var _FAP_VE_TRACE_POLI = FAP_VE_TRACE_POLIs.FirstOrDefault(x =>
                                    x.check_no == _FAP_VE_TRACE.check_no && x.check_acct_short == _FAP_VE_TRACE.check_acct_short);
                                    List<VeLevelDetailModel> _VeLevelDetailMedols = new List<VeLevelDetailModel>();
                                    bool _firstFlag = true;
                                    foreach (var _SYS_CODE in _SYS_CODEs_tel_call)
                                    {
                                        VeLevelDetailModel _VeLevelDetailModel = new VeLevelDetailModel();
                                        var _FAP_TEL_CODE = _FAP_TEL_CODEs.FirstOrDefault(x => x.code_id == _SYS_CODE.CODE);
                                        if (_firstFlag)
                                        {
                                            _VeLevelDetailModel.tel_proc_no = item.tel_proc_no; //電訪編號
                                            _VeLevelDetailModel.paid_id = _FAP_VE_TRACE.paid_id; //給付對象 ID
                                            _VeLevelDetailModel.paid_name = _FAP_VE_TRACE.paid_name; //給付對象姓名
                                            _VeLevelDetailModel.check_no = _FAP_VE_TRACE.check_no; //支票號碼
                                            _VeLevelDetailModel.check_acct_short = _FAP_VE_TRACE.check_acct_short; //支票號碼簡稱
                                            _VeLevelDetailModel.check_date = DateForTWDate(_FAP_VE_TRACE.check_date); //支票到期日
                                            _VeLevelDetailModel.check_amt = _FAP_VE_TRACE.check_amt?.ToString(); //支票金額
                                            _VeLevelDetailModel.o_paid_cd = _SYS_CODEs_O_PAID_CD.FirstOrDefault(x =>
                                            x.CODE == (_FAP_VE_TRACE_POLI?.o_paid_cd))?.CODE_VALUE ?? _FAP_VE_TRACE_POLI?.o_paid_cd; //原給付性質
                                            _VeLevelDetailModel.level_1 = _FAP_VE_CODEs.FirstOrDefault(x => x.code_type == "CLR_LEVEL1" &&
                                            x.code_id == item.level_1)?.code_value ?? item.level_1; //清理大類
                                            _VeLevelDetailModel.level_2 = _FAP_VE_CODEs.FirstOrDefault(x => x.code_type == "CLR_LEVEL2" &&
                                            x.code_id == item.level_2)?.code_value ?? item.level_2; //清理小類

                                            _VeLevelDetailModel.status = _FAP_VE_TRACE.status;  //清理狀態

                                            if (_FAP_VE_TRACE.re_paid_date != null) {
                                                try
                                                {
                                                    _VeLevelDetailModel.re_paid_date = "'" + BO.DateUtil.ADDateToChtDate(Convert.ToDateTime(_FAP_VE_TRACE.re_paid_date), 3, "/");  //帳務日期
                                                }
                                                catch (Exception e)
                                                {
                                                    _VeLevelDetailModel.re_paid_date = _FAP_VE_TRACE.re_paid_date.ToString();
                                                }
                                            }
                                            
                                           

                                        }
                                        _VeLevelDetailModel.code_value = _SYS_CODE.CODE_VALUE; //清理階段
                                        _VeLevelDetailModel.std_1 = _FAP_TEL_CODE?.std_1?.ToString(); //清理標準
                                        _VeLevelDetailModel.clean_f_date = item.cleanDates.FirstOrDefault(y =>
                                        y.clean_status == _SYS_CODE.CODE)?.clean_f_date; //清理階段完成日期
                                        //_VeLevelDetailModel.showColor = item.last_clean_status == _SYS_CODE.CODE; //最後完成之進度
                                        if (_SYS_CODE.CODE == item.clean_status)
                                        {
                                            _VeLevelDetailModel.showColor = true;
                                            _VeLevelDetailModel.VE_day = item.VE_day; //逾期天數
                                            _VeLevelDetailModel.VE_memo = $@"逾{_SYS_CODE.CODE_VALUE}階段"; //逾期原因
                                        }
                                        _VeLevelDetailMedols.Add(_VeLevelDetailModel);
                                        _firstFlag = false;
                                    }
                                    _results.Add(new Tuple<string, List<VeLevelDetailModel>>(item.proc_id, _VeLevelDetailMedols));
                                }
                            }
                        }
                    }
                }


                //寄送追蹤報表給相關追蹤人員 
                if (_results.Any())
                {
                    var _yellow = new XSSFColor(new byte[3] { 255, 255, 0 });
                    var _green = new XSSFColor(new byte[3] { 5, 251, 5 });
                    //var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                    //var mailAccount = ConfigurationManager.AppSettings["mailAccount"] ?? string.Empty;
                    var userMemo = GetMemoByUserId(_results.Select(x => x.Item1).Distinct(), true);

                    guid = "C_" + Guid.NewGuid().ToString();

                    IWorkbook wb_e = new XSSFWorkbook();
                    var k = 0;

                    List<string> sendGrp = new List<string>();
                    List<string> sheetGrp = new List<string>();
                    if ("E".Equals(type))
                    {
                        sendGrp.Add(userId);
                    }
                    else {
                        sendGrp = _results.Select(x => x.Item1).Distinct().ToList();
                    }

                    

                    foreach (var _group in sendGrp)
                    {
                        if ("E".Equals(type))
                        {
                            sheetGrp = _results.Select(x => x.Item1).Distinct().ToList();
                        }
                        else {
                            sheetGrp = _results.Where(x => x.Item1 == _group).Select(x => x.Item1).Distinct().ToList();
                        }
                        k += 1;

                        var _userMemo = userMemo.FirstOrDefault(x => x.Item1 == _group);

                        foreach (var _sheet in sheetGrp) {
                            XLWorkbook Workbook = new XLWorkbook();
                            if ("E".Equals(type))
                            {
                                fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_" + _group + "_" + guid, ".xlsx"));

                                if (File.Exists(fullPath))
                                    Workbook = new XLWorkbook(fullPath);
                            }
                            else
                            {
                                fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_C_" + _userMemo.Item1 + "_" + guid, ".xlsx"));
                            }
                            var Worksheets = Workbook.Worksheets.Add($@"sheet1");
                            Workbook.SaveAs(fullPath);

                            using (XLWorkbook wb = new XLWorkbook(fullPath))
                            {
                                var ws = wb.Worksheet($@"sheet1");
                                ws.Name = $@"{_sheet}";

                                ws.Cell("A1").Value = "清理暨逾期處理清單";
                                ws.Cell("A2").Value = $@"資料統計截止日：{dtn.ToString("yyyy-MM-dd")}";
                                ws.Range(1, 1, 1, 17).Merge();
                                ws.Range(2, 1, 2, 17).Merge();

                                ws.Cell(3, 1).Value = "電訪編號";
                                ws.Cell(3, 2).Value = "給付對象 ID";
                                ws.Cell(3, 3).Value = "給付對象姓名";
                                ws.Cell(3, 4).Value = "支票號碼";
                                ws.Cell(3, 5).Value = "支票號碼簡稱";
                                ws.Cell(3, 6).Value = "支票到期日";
                                ws.Cell(3, 7).Value = "支票金額";
                                ws.Cell(3, 8).Value = "原給付性質";
                                ws.Cell(3, 9).Value = "清理大類";
                                ws.Cell(3, 10).Value = "清理小類";
                                ws.Cell(3, 11).Value = "清理狀態";
                                ws.Cell(3, 12).Value = "帳務日期";

                                ws.Cell(3, 13).Value = "清理階段";
                                ws.Cell(3, 14).Value = "清理標準";
                                ws.Cell(3, 15).Value = "清理階段完成日期";
                                ws.Cell(3, 16).Value = "逾期天數";
                                ws.Cell(3, 17).Value = "逾期原因";

                                int iRow = 3;
                                foreach (var _itemM in _results.Where(x => x.Item1 == _sheet))
                                {
                                    foreach (var _item in _itemM.Item2)
                                    {
                                        var _showColor = _item.showColor;


                                        //modify by daiyu 20210128
                                        if (!"".Equals(StringUtil.toString(_item.clean_f_date)) || _showColor) {
                                            iRow++;

                                            ws.Cell(iRow, 1).Value = "'" + _item.tel_proc_no;
                                            ws.Cell(iRow, 2).Value = _item.paid_id;
                                            ws.Cell(iRow, 3).Value = _item.paid_name;
                                            ws.Cell(iRow, 4).Value = _item.check_no;
                                            ws.Cell(iRow, 5).Value = _item.check_acct_short;
                                            ws.Cell(iRow, 6).Value = _item.check_date;
                                            ws.Cell(iRow, 7).Value = _item.check_amt;
                                            ws.Cell(iRow, 8).Value = _item.o_paid_cd;
                                            ws.Cell(iRow, 9).Value = _item.level_1;
                                            ws.Cell(iRow, 10).Value = _item.level_2;

                                            if (clrStatusMap.ContainsKey(StringUtil.toString(_item.status)))
                                                ws.Cell(iRow, 11).Value = clrStatusMap[_item.status];
                                            else
                                                ws.Cell(iRow, 11).Value = _item.status;


                                            ws.Cell(iRow, 12).Value = "'" + _item.re_paid_date;



                                            ws.Cell(iRow, 13).Value = _item.code_value;
                                            ws.Cell(iRow, 14).Value = _item.std_1;
                                            ws.Cell(iRow, 15).Value = _item.clean_f_date;
                                            ws.Cell(iRow, 16).Value = _item.VE_day;
                                            ws.Cell(iRow, 17).Value = _item.VE_memo;

                                            if (_showColor)
                                            {
                                                ws.Range(iRow, 13, iRow, 17).Style.Fill.BackgroundColor = XLColor.Green;
                                                ws.Range(iRow, 13, iRow, 17).Style.Font.FontColor = XLColor.White;
                                            }
                                        }

                                        
                                    }

                                }

                                ws.Range(1, 1, 2, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                ws.Range(1, 1, iRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                                ws.Range(1, 1, iRow, 17).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                                ws.Range(3, 1, 3, 12).Style.Fill.BackgroundColor = XLColor.Yellow;
                                // ws.Range(3, 1, 3, 15).Style.Font.FontColor = XLColor.White;

                                ws.Range(3, 13, 3, 17).Style.Fill.BackgroundColor = XLColor.Green;
                                ws.Range(3, 13, 3, 17).Style.Font.FontColor = XLColor.White;

                                ws.Columns().AdjustToContents();  // Adjust column width
                                ws.Rows().AdjustToContents();     // Adjust row heights


                                wb.SaveAs(fullPath);
                            }
                        }

                        

                        #region Mail
                        if (type == "A" || type == "M") //Mail
                        {

                            //MemoryStream stream = new MemoryStream();
                            //wb_m.Write(stream);

                            #region 寄信
                            try
                            {

                                MailUtil mailUtil = new MailUtil();

                                string chDt = BO.DateUtil.getCurChtDate(3);
                                string _fullPath = fullPath.Replace(".xlsx", ".zip");

                                using (var zip = new ZipFile())
                                {
                                    zip.Password = _userMemo.Item1 + chDt;
                                    zip.AddFile(fullPath, "");

                                    zip.Save(_fullPath);
                                }

                                File.Delete(fullPath);

                                //modify by daiyu 20210128
                                CommonUtil commonUtil = new CommonUtil();
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(_userMemo.Item1);
                                string[] mailToArr = new string[] { adModel.e_mail };
                                //string[] mailToArr = new string[] { _userMemo.Item5 };


                                bool bSucess = mailUtil.sendMail(mailToArr
                                   , "清理暨逾期處理清單報表"
                                   , "清理暨逾期處理清單報表"
                                   , true
                                  , ""
                                  , ""
                                  , new string[] { _fullPath }
                                  , true);

                            }
                            catch (Exception ex)
                            {
                                _flag = false;
                                _msg = ex.exceptionMessage();
                                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
                            }
                            finally
                            {
                                //stream.Flush();
                                //stream.Position = 0;
                            }
                            #endregion
                        }
                        #endregion

                    }
                

                    #region PIA
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = userId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "BAP0004";
                    piaLogMain.EXECUTION_CONTENT = "";
                    piaLogMain.AFFECT_ROWS = _results.Count;
                    piaLogMain.PIA_TYPE = "1100000000";
                    piaLogMain.EXECUTION_TYPE = "E";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
                    piaLogMainDao.Insert(piaLogMain);
                    #endregion

                    _msg = $@"總共查詢資料 : {_results.Count} 筆";
                }
                else
                {
                    _msg = "比對後無已達追蹤條件的資料";
                }
            }
            catch (Exception ex)
            {
                _flag = false;
                _msg = ex.exceptionMessage();
                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
            }
            finally
            {
                _stream.Flush();
                _stream.Position = 0;
            }
            if ("E".Equals(type))
                return new Tuple<bool, string, string>(_flag, _msg, userId + "_" + guid);
            else
                return new Tuple<bool, string, string>(_flag, _msg, guid);
        }







        /// <summary>
        /// 每日寄送追蹤報表給相關追蹤人員 
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <param name="type">執行類別 A:全部執行,M:僅執行寄送mail,E:僅執行產生Excel</param>
        /// <returns></returns>
        public Tuple<bool, string, string> Trace_Notify_Scheduler(string userId = null, string type = "A")
        {
            DateTime dtn = DateTime.Now;
            var _dtn = DateForTWDate(dtn);
            bool _flag = true; //執行結果
            string _msg = string.Empty; //訊息
            List<VeClearSchedulerModel> _results = new List<VeClearSchedulerModel>();
            List<SYS_CODE> _SYS_CODEs = new List<SYS_CODE>();
            List<FAP_TEL_CODE> _FAP_TEL_CODEs = new List<FAP_TEL_CODE>();
            MemoryStream _stream = new MemoryStream();
            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            string fullPath = "";
            string guid = "";

            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //1.挑出【FAP_TEL_INTERVIEW 電訪及追蹤記錄檔】中"電訪覆核結果 = 11進入追踨、15轉行政單位" + "派件狀態 = 1.派件中"的資料
                    var _tel_appr_results = new List<string>() {
                    "11", //11進入追踨
                    "15"  //15轉行政單位
                };
                    var _dispatch_status = "1"; //1.派件中
                    var _FAP_TEL_INTERVIEWs = db.FAP_TEL_INTERVIEW.AsNoTracking()
                        .Where(x =>
                            _tel_appr_results.Contains(x.tel_appr_result) &&
                            x.dispatch_status == _dispatch_status).ToList();
                    //tel_result => 處理結果代碼
                    //tel_result_cnt => 處理結果已達次數
                    if (_FAP_TEL_INTERVIEWs.Any())
                    {
                        _SYS_CODEs = db.SYS_CODE.AsNoTracking()
                            .Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "tel_call").ToList();
                        var _code_type = "tel_call"; // tel_call：處理及追蹤結果暨追蹤標準
                        _FAP_TEL_CODEs = db.FAP_TEL_CODE.AsNoTracking()
                           .Where(x => x.code_type == _code_type).ToList();
                        //code_id => 處理結果代碼
                        _FAP_TEL_INTERVIEWs.ForEach(x =>
                        {
                            var _tel_result_cnt = x.tel_result_cnt >= 3 ? 3 : x.tel_result_cnt;
                            var _FAP_TEL_CODE = _FAP_TEL_CODEs.FirstOrDefault(y => y.code_id == x.tel_result);
                            if (_FAP_TEL_CODE != null && _tel_result_cnt != null)
                            {
                                var _std = 0;
                                switch (_tel_result_cnt)
                                {
                                    case 1:
                                        _std = _FAP_TEL_CODE.std_1 ?? 0;
                                        break;
                                    case 2:
                                        _std = _FAP_TEL_CODE.std_2 ?? 0;
                                        break;
                                    case 3:
                                        _std = _FAP_TEL_CODE.std_3 ?? 0;
                                        break;
                                }
                                if (_std != 0)
                                {
                                    //         var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                                    //.Where(o => o.tel_proc_no == x.tel_proc_no &&
                                    //o.tel_std_type == "tel_assign_case" && o.data_flag == "Y").ToList();

                                    try
                                    {
                                        List<OAP0046DModel> checkDataList = fAPTelCheckDao.qryForTelProcRpt(x.tel_proc_no);

                                        if (checkDataList.Count > 0)
                                        {
                                            OAP0046DModel check = checkDataList.Where(o => o.status != "1").FirstOrDefault();

                                            if (check != null)
                                            {
                                                _results.Add(new VeClearSchedulerModel()
                                                {
                                                    tel_proc_no = x.tel_proc_no, //電訪編號
                                                    proc_id = _FAP_TEL_CODE.proc_id, //追蹤人員
                                                    VE_day = "", //逾期天數
                                                    VE_memo = $@"逾第{_tel_result_cnt}次追蹤標準", //逾期原因
                                                    tel_interview_date = x.tel_interview_f_datetime?.ToString("yyyy/MM/dd"), //最後一次電訪日期                                                                                                                   //tel_result_last = Get_tel_result(_SYS_CODEs, x.tel_result), //電訪處理結果_統計
                                                    tel_result_last = x.tel_result, //電訪處理結果_統計
                                                    tel_result_cnt = _tel_result_cnt ?? 0, //處理結果已達次數
                                                    paid_id = check.paid_id,
                                                    paid_name = check.paid_name
                                                });
                                            }
                                        }
                                    }
                                    catch (Exception e) {
                                        logger.Error(e.ToString());
                                    }

                                    
                                   
                                }
                            }
                        });
                    }
                    if (_results.Any()) //寄送已達追蹤條件的資料
                    {
                        var _tel_proc_nos = _results.Select(x => x.tel_proc_no).ToList();
                        var _dataTypes = new List<string>() { "1", "2" };
                        var FAP_TEL_INTERVIEW_HISs = db.FAP_TEL_INTERVIEW_HIS.AsNoTracking()
                            .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                            (_dataTypes.Contains(x.data_type))).ToList();
                        var FAP_TEL_CHECKs = db.FAP_TEL_CHECK.AsNoTracking()
                            .Where(x => _tel_proc_nos.Contains(x.tel_proc_no) &&
                            x.tel_std_type == "tel_assign_case" && x.data_flag == "Y").ToList();
                        var systems = FAP_TEL_CHECKs.Select(x => x.system).Distinct().ToList();
                        var check_nos = FAP_TEL_CHECKs.Select(x => x.check_no).Distinct().ToList();
                        var check_acct_shorts = FAP_TEL_CHECKs.Select(x => x.check_acct_short).Distinct().ToList();
                        var FAP_VE_TRACEs = db.FAP_VE_TRACE.AsNoTracking()
                            .Where(x => systems.Contains(x.system) && x.status != "1" &&
                            check_nos.Contains(x.check_no) && check_acct_shorts.Contains(x.check_acct_short)).ToList();


                        
                        foreach (var item in _results)
                        {
                            var _FAP_TEL_INTERVIEW_HIS_F = FAP_TEL_INTERVIEW_HISs
                                .FirstOrDefault(x => x.tel_proc_no == item.tel_proc_no && x.data_type == "1");
                            if (_FAP_TEL_INTERVIEW_HIS_F != null)
                            {
                                item.tel_result_first = Get_tel_result(_SYS_CODEs, _FAP_TEL_INTERVIEW_HIS_F.tel_result); //第一次電訪處理結果_統計
                            }
                            

                            
                            Set_tel_interview("Trace_Notify_Scheduler", item, FAP_TEL_INTERVIEW_HISs, _FAP_TEL_CODEs); //設定 追踨標準 & 追踨日期
                            item.tel_result_last = Get_tel_result(_SYS_CODEs, item.tel_result_last); //電訪處理結果
                        }
                    }
                }
                //寄送追蹤報表給相關追蹤人員 
                if (_results.Any())
                {
                    var _blue = new XSSFColor(new byte[3] { 51, 102, 255 });
                    var userMemo = GetMemoByUserId(_results.Select(x => x.proc_id).Distinct(), true);
                    //var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                    //var mailAccount = ConfigurationManager.AppSettings["mailAccount"] ?? string.Empty;

                    guid = "T_" + Guid.NewGuid().ToString();

                    IWorkbook wb_e = new XSSFWorkbook();
                    NPOI.SS.UserModel.IFont whiteFont_e = wb_e.CreateFont();
                    whiteFont_e.Color = IndexedColors.White.Index; //White
                    var k = 0;
                    foreach (var _group in _results.GroupBy(x => x.proc_id))
                    {
                        fullPath = "";

                        k += 1;
                        var _userMemo = userMemo.FirstOrDefault(x => x.Item1 == _group.Key);
                        XLWorkbook Workbook = new XLWorkbook();

                        if ("E".Equals(type))
                        {
                            fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_" + guid, ".xlsx"));

                            if (File.Exists(fullPath))
                                Workbook = new XLWorkbook(fullPath);
                        }
                        else
                        {
                            fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_" + guid + "_" + _group.Key, ".xlsx"));
                        }
                        var Worksheets = Workbook.Worksheets.Add($@"sheet1");
                        Workbook.SaveAs(fullPath);

                        using (XLWorkbook wb = new XLWorkbook(fullPath))
                        {
                            var ws = wb.Worksheet($@"sheet1");
                            ws.Name = $@"{_group.Key }";

                            ws.Cell("A1").Value = "電訪追蹤報表";
                            ws.Cell("A2").Value = $@"資料統計截止日：{dtn.ToString("yyyy-MM-dd")}";
                            ws.Range(1, 1, 1, 15).Merge();
                            ws.Range(2, 1, 2, 15).Merge();

                            ws.Cell(3, 1).Value = "電訪編號";
                            ws.Cell(3, 2).Value = "給付對象 ID";
                            ws.Cell(3, 3).Value = "給付對象";
                            ws.Cell(3, 4).Value = "追蹤人員";
                            ws.Cell(3, 5).Value = "電訪日期";
                            ws.Cell(3, 6).Value = "電訪處理結果";
                            ws.Cell(3, 7).Value = "第一次追踨標準";
                            ws.Cell(3, 8).Value = "第一次追踨日期";
                            ws.Cell(3, 9).Value = "第二次追踨標準";
                            ws.Cell(3, 10).Value = "第二次追踨日期";
                            ws.Cell(3, 11).Value = "第三次追踨標準";
                            ws.Cell(3, 12).Value = "第三次追踨日期";

                            int iRow = 3;
                            foreach (var _item in _group.OrderBy(x => x.tel_result_last).ThenBy(x => Convert.ToDateTime(x.tel_interview_date)))
                            {
                                iRow++;
                                ws.Cell(iRow, 1).Value = "'" + _item.tel_proc_no;
                                ws.Cell(iRow, 2).Value = _item.paid_id;
                                ws.Cell(iRow, 3).Value = _item.paid_name;
                                ws.Cell(iRow, 4).Value = _userMemo.Item2;
                                ws.Cell(iRow, 5).Value = _item.tel_interview_date;
                                ws.Cell(iRow, 6).Value = _item.tel_result_last;
                                ws.Cell(iRow, 7).Value = _item.tel_result_cnt_1;
                                ws.Cell(iRow, 8).Value = _item.tel_interview_date_1;
                                ws.Cell(iRow, 9).Value = _item.tel_result_cnt_2;
                                ws.Cell(iRow, 10).Value = _item.tel_interview_date_2;
                                ws.Cell(iRow, 11).Value = _item.tel_result_cnt_3;
                                ws.Cell(iRow, 12).Value = _item.tel_interview_date_3;

                            }

                            ws.Range(1, 1, 2, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(4, 5, iRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(4, 1, iRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            ws.Range(1, 1, iRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                            ws.Range(1, 1, iRow, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            ws.Range(3, 1, 3, 12).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                            ws.Range(3, 1, 3, 12).Style.Font.FontColor = XLColor.White;

                            ws.Columns().AdjustToContents();  // Adjust column width
                            ws.Rows().AdjustToContents();     // Adjust row heights


                            wb.SaveAs(fullPath);
                        }



                        #region 後續動作

                        #region Mail
                        if (type == "A" || type == "M") //Mail
                        {
                            //modify by daiyu 20200926
                            try
                            {
                                MailUtil mailUtil = new MailUtil();

                                string chDt = BO.DateUtil.getCurChtDate(3);
                                string _fullPath = fullPath.Replace(".xlsx", ".zip");

                                using (var zip = new ZipFile())
                                {
                                    zip.Password = _userMemo.Item1 + chDt;
                                    zip.AddFile(fullPath, "");

                                    zip.Save(_fullPath);
                                }

                                File.Delete(fullPath);

                                //modify by daiyu 20210128
                                CommonUtil commonUtil = new CommonUtil();
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(_userMemo.Item1);
                                string[] mailToArr = new string[] { adModel.e_mail };
                                //string[] mailToArr = new string[] { _userMemo.Item5 };

                                bool bSucess = mailUtil.sendMail(mailToArr
                                    , "電訪追踨報表"
                                    , ""
                                    , true
                                   , ""
                                   , ""
                                   , new string[] { _fullPath }
                                   , true);
                            }
                            catch (Exception e)
                            {
                                _flag = false;
                                _msg = $@" SendMail Error : {e.exceptionMessage()}";
                                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
                            }

                        }
                        #endregion



                        #endregion

                    }


                    #region PIA
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = userId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "BAP0004";
                    piaLogMain.EXECUTION_CONTENT = "";
                    piaLogMain.AFFECT_ROWS = _results.Count;
                    piaLogMain.PIA_TYPE = "1100000000";
                    piaLogMain.EXECUTION_TYPE = "E";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
                    piaLogMainDao.Insert(piaLogMain);
                    #endregion

                    _msg = $@"總共查詢資料 : {_results.Count} 筆";
                }
                else
                {
                    _msg = "比對後無已達追蹤條件的資料";
                }
            }
            catch (Exception ex)
            {
                _flag = false;
                _msg = $@"Error : {ex.exceptionMessage()}";
                NLog.LogManager.GetCurrentClassLogger().Error(_msg);
            }
            finally
            {
                _stream.Flush();
                _stream.Position = 0;
            }
            return new Tuple<bool, string, string>(_flag, _msg, guid);
        }
















        private string getaply_no(string type = "")
        {
            DateTime now = DateTime.Now;
            string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

            SysSeqDao sysSeqDao = new SysSeqDao();
            string qPreCode = curDateTime[0].Substring(0, 3);
            var cId = sysSeqDao.qrySeqNo("AP", "B0004", qPreCode).ToString();
            string aply_no = "B0004" + qPreCode + cId.ToString().PadLeft(4, '0');



            //String qPreCode = DateTime.Now.ToString("yyyyMM");
            ////String qPreCode = curDateTime[0].Substring(0, 6);
            //var cId = sysSeqDao.qrySeqNo("AP", type, qPreCode).ToString();
            //int seqLen = 12 - (type + qPreCode).Length;
            //var aply_no = "B0004" + qPreCode + cId.ToString().PadLeft(seqLen, '0');
            return aply_no;
        }

        /// <summary>
        /// 設定 追踨標準 & 追踨日期
        /// </summary>
        /// <param name="model"></param>
        /// <param name="FAP_TEL_INTERVIEW_HISs"></param>
        /// <param name="FAP_TEL_CODEs"></param>
        private void Set_tel_interview(string type,
            VeClearSchedulerModel model,
            List<FAP_TEL_INTERVIEW_HIS> FAP_TEL_INTERVIEW_HISs,
            List<FAP_TEL_CODE> FAP_TEL_CODEs)
        {

            List<FAP_TEL_INTERVIEW_HIS> traceList = new List<FAP_TEL_INTERVIEW_HIS>();

            foreach (FAP_TEL_INTERVIEW_HIS _his in FAP_TEL_INTERVIEW_HISs.Where(x => x.tel_proc_no == model.tel_proc_no & x.data_type == "2" & x.appr_stat != "3")
                .OrderByDescending(x => x.update_datetime)) {
                if (model.tel_result_last.Equals(_his.tel_result))
                    traceList.Add(_his);
                else
                    break;
            }

            Dictionary<int, string> interview_date_dic = new Dictionary<int, string>();
            //int j = 0;
            foreach (FAP_TEL_INTERVIEW_HIS _his in traceList) {

                if (interview_date_dic.ContainsKey(Convert.ToInt32(_his.tel_result_cnt)))
                    continue;
                else
                    interview_date_dic.Add(Convert.ToInt32(_his.tel_result_cnt), BO.DateUtil.DatetimeToString(_his.tel_interview_datetime, "yyyy/MM/dd"));
                
            }



            for (var i = model.tel_result_cnt; i > 0; i--)
            {
                switch (i)
                {
                    case 1:
                        model.tel_result_cnt_1 = FAP_TEL_CODEs.FirstOrDefault(
                            x => x.code_id == model.tel_result_last)?.std_1?.ToString(); //第一次追踨標準

                        if ("Trace_Notify_Scheduler".Equals(type)) {
                            if (interview_date_dic.ContainsKey(1))  //若追踨人員有改處理結果...要改抓追蹤人員的覆核日期
                                model.tel_interview_date = interview_date_dic[1];
                        }
                        


                        if (interview_date_dic.ContainsKey(2))  //因為第一次的處理日期不能算..所以取日期時要取第N+1次的資料
                            model.tel_interview_date_1 = interview_date_dic[2];
                        //model.tel_interview_date_1 = Get_tel_interview_datetime(
                        //    FAP_TEL_INTERVIEW_HISs, model.tel_result_last, i); //第一次追踨日期
                        break;
                    case 2:
                        model.tel_result_cnt_2 = FAP_TEL_CODEs.FirstOrDefault(
                            x => x.code_id == model.tel_result_last)?.std_2?.ToString(); //第二次追踨標準

                        if (interview_date_dic.ContainsKey(3))
                            model.tel_interview_date_2 = interview_date_dic[3];
                        //model.tel_interview_date_2 = Get_tel_interview_datetime(
                        //    FAP_TEL_INTERVIEW_HISs, model.tel_result_last, i); //第二次追踨日期
                        break;
                    case 3:
                        model.tel_result_cnt_3 = FAP_TEL_CODEs.FirstOrDefault(
                            x => x.code_id == model.tel_result_last)?.std_3?.ToString(); //第三次追踨標準

                        if (interview_date_dic.ContainsKey(4))
                            model.tel_interview_date_3 = interview_date_dic[4];

                        //model.tel_interview_date_3 = Get_tel_interview_datetime(
                        //    FAP_TEL_INTERVIEW_HISs, model.tel_result_last, i); //第三次追踨日期
                        break;
                }
            }
        }

        /// <summary>
        /// 獲得第N次追踨日期
        /// </summary>
        /// <param name="FAP_TEL_INTERVIEW_HISs"></param>
        /// <param name="tel_result"></param>
        /// <param name="tel_result_cnt"></param>
        /// <returns></returns>
        private string Get_tel_interview_datetime(List<FAP_TEL_INTERVIEW_HIS> FAP_TEL_INTERVIEW_HISs,string tel_result,int tel_result_cnt)
        { 
            return FAP_TEL_INTERVIEW_HISs
                   .Where(x => x.tel_result_cnt == tel_result_cnt && x.tel_result == tel_result)
                   .OrderByDescending(x => x.update_datetime).FirstOrDefault()?.tel_interview_datetime?.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// 電訪結果 中文
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="tel_result"></param>
        /// <returns></returns>
        private string Get_tel_result(List<SYS_CODE> datas, string tel_result)
        { 
            return datas.FirstOrDefault(x => 
            x.CODE == tel_result)?.CODE_VALUE ?? tel_result;
        }

        /// <summary>
        /// 判斷 日期2 是否大於等於 日期1加指定工作日天數
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="std"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        private bool compareDate(DateTime? date1, int? std, DateTime? date2 = null)
        {
            bool _flag = false; //是否符合條件
            if (date1 != null && std != null && std > 0)
            {
                if (date2 == null)
                    date2 = DateTime.Now.Date;
                var _date1 = 0;
                var _date2 = 0;
                if (Int32.TryParse(GetSTDDate(DateForTWDate(date1), std.Value), out _date1) &&
                    Int32.TryParse(DateForTWDate(date2), out _date2) &&
                    _date1 > 0 && _date2 > 0)
                {
                    _flag = _date2 >= _date1;
                }
            }
            return _flag;
        }

        /// <summary>
        /// 取得指定日加設定工作日的日期(不包含指定日)
        /// </summary>
        /// <param name="TWDate">指定日(民國年月日)</param>
        /// <param name="std">工作日</param>
        /// <returns></returns>
        private string GetSTDDate(string TWDate, int std)
        {
            string result = string.Empty;
            if (TWDate.IsNullOrWhiteSpace() || std < 1)
                return result;
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
                    select ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) STDDate from (
                    select YEAR,MONTH,DAY from LGLCALE1 
                    where ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) > @Date
                    and corp_rest <> 'Y'
                    order by year,month,day
                    FETCH FIRST @STD ROWS ONLY)
                    order by year desc,month desc , day desc
                    FETCH FIRST 1 ROWS ONLY; ";
                        com.Parameters.Add($@"Date", TWDate);
                        com.Parameters.Add($@"STD", std);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            result = dbresult["STDDate"]?.ToString()?.Trim();                       
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 抓取開始日到結束日所有的工作日 (不包含開始日)
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">結束日</param>
        /// <returns></returns>
        private List<string> GetWorkDate(string startDate, string endDate)
        {
            FGLCALEDao fGLCALEDao = new FGLCALEDao();
            return fGLCALEDao.GetWorkDate(startDate, endDate);

        //    var results = new List<string>();

        //    if (startDate.IsNullOrWhiteSpace() || endDate.IsNullOrWhiteSpace())
        //        return results;
        //    try
        //    {
        //        using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
        //        {
        //            conn.Open();
        //            string sql = string.Empty;
        //            using (EacCommand com = new EacCommand(conn))
        //            {
        //                sql = $@"
        //select ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) STDDate  from LGLCALE1 
        //where  ((LPAD(year,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) > @startDate
        //and  ((LPAD(year,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) <= @endDate
        //and corp_rest <> 'Y'
        //order by YEAR,MONTH,DAY; ";
        //                com.Parameters.Add($@"startDate", startDate);
        //                com.Parameters.Add($@"endDate", endDate);
        //                com.CommandText = sql;
        //                com.Prepare();
        //                DbDataReader dbresult = com.ExecuteReader();
        //                while (dbresult.Read())
        //                {
        //                    results.Add(dbresult["STDDate"]?.ToString()?.Trim()); //工作日                      
        //                }
        //                com.Dispose();
        //            }
        //            conn.Dispose();
        //            conn.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
        //    }

        //    return results;
        }

        /// <summary>
        /// 日期轉民國日期
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string DateForTWDate(DateTime? dt)
        {
            return dt == null ? string.Empty : $@"{(dt.Value.Year - 1911)}{dt.Value.Month.ToString().PadLeft(2, '0')}{dt.Value.Day.ToString().PadLeft(2, '0')}";
        }

        public static T2 ModelConvert<T1, T2>(T1 model) where T2 : new()
        {
            T2 newModel = new T2();
            if (model != null)
            {
                foreach (PropertyInfo itemInfo in model.GetType().GetProperties())
                {
                    PropertyInfo propInfoT2 = typeof(T2).GetProperty(itemInfo.Name);
                    if (propInfoT2 != null)
                    {
                        // 型別相同才可轉換
                        if (propInfoT2.PropertyType == itemInfo.PropertyType)
                        {
                            propInfoT2.SetValue(newModel, itemInfo.GetValue(model, null), null);
                        }
                    }
                }
            }
            return newModel;
        }

        protected class tempModel
        {
            public string tel_proc_no { get; set; }

            public string clean_status { get; set; }

            public string proc_id { get; set; }

            public string VE_day { get; set; }

            public List<cleanDateModel> cleanDates { get; set; } = new List<cleanDateModel>();

            public string last_clean_status { get; set; }

            public string level_1 { get; set; }

            public string level_2 { get; set; }
        }

        protected class cleanDateModel
        { 
            public string clean_status { get; set; }

            public string clean_date { get; set; }

            public string clean_f_date { get; set; }

            public string level_1 { get; set; }

            public string level_2 { get; set; }
        }
    }
}