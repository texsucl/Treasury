using ClosedXML.Excel;
using FAP.Web.AS400Models;
using FAP.Web.AS400PGM;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// 功能說明：電訪相關
/// 初版作者：
/// 修改歷程：
/// 需求單號：
/// 修改內容：
/// ----------------------------------------------------
/// 修改歷程：20210125 daiyu 
/// 需求單號：
/// 修改內容：1.電訪派件報表無法列印明細表(民國年月轉 DATE 問題)
///           2.電訪派件報表服務人員改抓保單主檔，再串業佣的API取得單位、姓名、電話
/// </summary>


namespace FAP.Web.BO
{
    public class VeTelUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void procCleanStageAply(string aply_no, FAP_TEL_INTERVIEW_HIS d, DateTime now, string user_id
            , SqlConnection conn, SqlTransaction transaction)
        {
            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
            FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();

            //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
            FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
            ObjectUtil.CopyPropertiesTo(d, _tel_check);
            _tel_check.tel_proc_no = d.tel_proc_no;
            _tel_check.tel_std_type = "tel_assign_case";
            _tel_check.update_id = user_id;
            _tel_check.update_datetime = now;
            _tel_check.data_status = "2";
            fAPTelCheckDao.updDataStatusByTelProcNo(_tel_check, conn, transaction);


            //新增【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】
            d.aply_no = aply_no;
            fAPTelInterviewHisDao.insert(d, conn, transaction);


            //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】資料狀態
            FAP_TEL_INTERVIEW _tel_interview = new FAP_TEL_INTERVIEW();
            _tel_interview.tel_proc_no = d.tel_proc_no;
            _tel_interview.data_status = "2";
            _tel_interview.update_datetime = now;
            _tel_interview.update_id = user_id;
            fAPTelInterviewDao.updDataStatus(_tel_interview, conn, transaction);

            //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
            FAP_TEL_PROC _tel_proc = new FAP_TEL_PROC();
            _tel_proc.tel_proc_no = d.tel_proc_no;
            _tel_proc.aply_no = aply_no;
            _tel_proc.data_type = "3";
            _tel_proc.proc_id = user_id;
            _tel_proc.proc_datetime = d.clean_f_date;
            _tel_proc.proc_status = d.clean_status;
            _tel_proc.expect_datetime = d.clean_f_date;
            _tel_proc.reason = d.remark;
            _tel_proc.appr_stat = "1";
            fAPTelProcDao.insert(_tel_proc, conn, transaction);
        }




        //將清理階段跳到下一個階段
        public void procCleanStageApprove(OAP0048Model d, DateTime now, string user_id
        , SqlConnection conn, SqlTransaction transaction)
        {
            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

            d.appr_id = user_id;

            //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
            FAP_TEL_INTERVIEW_HIS _his_o = fAPTelInterviewHisDao.qryByTelProcNo(d.tel_proc_no, "3", "1");
            ProcFAPTelProc(d.tel_proc_no, "2", _his_o, fAPTelProcDao, user_id, now, conn, transaction);


            //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
            FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
            ObjectUtil.CopyPropertiesTo(d, _tel_check);
            _tel_check.tel_proc_no = d.tel_proc_no;
            int upd_cnt = procFAPTelCheck(fAPTelCheckDao, _tel_check, user_id, now, conn, transaction);



            //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】資料狀態
            FAP_TEL_INTERVIEW _tel_interview = new FAP_TEL_INTERVIEW();
            _tel_interview.tel_proc_no = d.tel_proc_no;
            ProcFAPTelInterview("2", _his_o, fAPTelInterviewDao, _tel_interview, user_id, now, conn, transaction);


            //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
            fAPTelInterviewHisDao.updateApprStatus(user_id, "2", d.aply_no, d.tel_proc_no, "3", now, conn, transaction);


            //新增【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
            if (new string[] { "5", "8", "10" }.Contains(d.clean_status))
            {
                string practice = "";
                string cert_doc = "";

                switch (d.clean_status)
                {
                    case "5":
                        practice = "G17";
                        cert_doc = "F6";
                        break;
                    case "8":
                        practice = "G8";
                        cert_doc = "F4";
                        break;
                    case "10":
                        practice = "G4";
                        cert_doc = "F1";
                        break;
                }

                FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
                fAPVeTrackProcDao.insertForTelProc("tel_assign_case", d.tel_proc_no, practice, cert_doc, d.remark, now
                    , user_id, now, conn, transaction);
                FAPVeTraceDao faPVeTraceDao = new FAPVeTraceDao();
                faPVeTraceDao.updateForTelCheck("tel_assign_case", d.tel_proc_no, d.remark, now, conn, transaction);

            }


            writePiaLogCleanStage(user_id, 1, d.paid_id, "E", d.tel_proc_no);

        }

        private void writePiaLogCleanStage(string user_id, int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = user_id;
            piaLogMain.ACCOUNT_NAME ="";
            piaLogMain.PROGFUN_NAME = "VeTelUtil";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        private int procFAPTelCheck(FAPTelCheckDao fAPTelCheckDao, FAP_TEL_CHECK _tel_check
            , string user_id, DateTime now, SqlConnection conn, SqlTransaction transaction)
        {

            _tel_check.tel_std_type = "tel_assign_case";
            _tel_check.update_id = user_id;
            _tel_check.update_datetime = now;
            _tel_check.data_status = "1";
            return fAPTelCheckDao.updDataStatusByTelProcNo(_tel_check, conn, transaction);

        }

        //【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
        private void ProcFAPTelProc(string tel_proc_no, string appr_stat, FAP_TEL_INTERVIEW_HIS _his_o, FAPTelProcDao fAPTelProcDao
            ,string user_id, DateTime now, SqlConnection conn, SqlTransaction transaction)
        {


            FAP_TEL_PROC _tel_proc = new FAP_TEL_PROC();
            ObjectUtil.CopyPropertiesTo(_his_o, _tel_proc);
            _tel_proc.proc_id = _his_o.update_id;
            _tel_proc.proc_datetime = _his_o.clean_f_date;
            _tel_proc.proc_status = _his_o.clean_status;
            _tel_proc.reason = _his_o.remark;
            _tel_proc.appr_status = "";
            _tel_proc.appr_stat = appr_stat;
            _tel_proc.appr_datetime = now;
            _tel_proc.appr_id = user_id;
            fAPTelProcDao.insert(_tel_proc, conn, transaction);

        }


        //【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】
        private void ProcFAPTelInterview(string appr_stat, FAP_TEL_INTERVIEW_HIS _his_o, FAPTelInterviewDao fAPTelInterviewDao
            , FAP_TEL_INTERVIEW _tel_interview, string user_id, DateTime now, SqlConnection conn, SqlTransaction transaction)
        {
            _tel_interview.data_status = "1";
            _tel_interview.update_datetime = now;
            _tel_interview.update_id = user_id;

            if ("3".Equals(appr_stat))
                fAPTelInterviewDao.updDataStatus(_tel_interview, conn, transaction);
            else
            {
                _tel_interview.remark = _his_o.remark;
                _tel_interview.clean_status = _his_o.clean_status;
                _tel_interview.clean_date = now;
                _tel_interview.remark = _his_o.remark;


                //判斷下一個清理階段
                switch (_his_o.clean_status)
                {
                    case "1":   //1 檢核寄信記錄
                        _tel_interview.clean_status = "6";

                        if ("A8".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "2";

                        if ("B1".Equals(_his_o.level_2) || "B2".Equals(_his_o.level_2) || "B3".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "3";

                        if ("B4".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "4";

                        if ("A7".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "5";

                        break;

                    case "2":   //2 金額小於5000元
                        _tel_interview.clean_status = "9";  //依淑美 2020.11.30 MAIL 改成順序為 1-> 2 -> 9 -> 10 -> 11 -> 12
                        break;

                    case "3":   //3 放棄領取
                        _tel_interview.clean_status = "11";
                        break;

                    case "4":   //4 準備法扣資料
                        _tel_interview.clean_status = "11";
                        break;

                    case "5":   //5 準備法人網站資料
                        _tel_interview.clean_status = "11";
                        break;

                    case "6":   //6 準備調閱資料
                        _tel_interview.clean_status = "7";
                        break;

                    case "7":   //7 用印送件
                        _tel_interview.clean_status = "8";
                        break;

                    case "8":   //8 調閱完成
                        _tel_interview.clean_status = "9";
                        break;

                    case "9":   //9 要保書地址是否相符
                        _tel_interview.clean_status = "10";
                        break;

                    case "10":   //10 再次寄信
                        _tel_interview.clean_status = "11";
                        break;

                    case "11":   //11 結案登錄
                        _tel_interview.clean_status = "12";
                        break;
                    case "12": // 主管覆核
                        _tel_interview.clean_date = _his_o.clean_date;
                        _tel_interview.clean_f_date = _his_o.clean_f_date;
                        break;
                    case "13": // 給付結案
                        _tel_interview.clean_date = _his_o.clean_date;
                        _tel_interview.clean_f_date = _his_o.clean_f_date;
                        break;
                }


                fAPTelInterviewDao.updForOAP0048A(_tel_interview, conn, transaction);
            }
        }



        public string getSmsMobile(FAP_TEL_SMS_TEMP d, EacConnection conn400) {
            //B.手機號碼規則：
            //先支票抬頭比對『被保人姓名』（請記得先剔除前後空格再比對）
            //  若相等，Ａ系統案件讀LECPTELP / LIBAETNA檔中，該保單之類別41被保人手機，並判斷電話之合理(例如: 09開頭且碼數有10位者)
            //        F系統案件先讀FPMADDR0中的類別23被保人行動電話，讀不到再讀FNBADDN0檔中MOBILE_I被保人行動電話
            //  若不相等，再接以下比對
            //支票抬頭比對『要保人姓名』（請記得先剔除前後空格再比對）
            //  相等，Ａ系統案件讀LECPTELP / LIBAETNA檔中，該保單之類別32要保人手機，並判斷電話之合理(例如: 09開頭且碼數有10位者)
            //      F系統案件先讀FPMADDR0中的類別01收費地址 或 03要保人住所／戶籍地址 上的行動電話，讀不到再讀FNBADDN0檔中MOBILE行動電話
            // 抬頭只比對要、被保人即可，以上需有抓到手機資料的部份者，再依一般及密戶件產出

            string mobile = "";

            
            if ("A".Equals(StringUtil.toString(d.system)))
            {
                FECPTELDao fECPTELDao = new FECPTELDao();
                
                if (StringUtil.toString(d.paid_name).Equals(StringUtil.toString(d.ins_name)))
                    mobile = fECPTELDao.getTel("41", d.policy_no, d.policy_seq, conn400);
                
                if ("".Equals(StringUtil.toString(mobile)) & StringUtil.toString(d.paid_name).Equals(StringUtil.toString(d.appl_name)))
                    mobile = fECPTELDao.getTel("32", d.policy_no, d.policy_seq, conn400);
            }
            else {
                FPMADDRDao fPMADDRDao = new FPMADDRDao();
                if (StringUtil.toString(d.paid_name).Equals(StringUtil.toString(d.ins_name))) 
                    mobile = fPMADDRDao.getTel("23", d.policy_no, d.policy_seq, d.id_dup, conn400);

                if ("".Equals(StringUtil.toString(mobile)) & StringUtil.toString(d.paid_name).Equals(StringUtil.toString(d.appl_name)))
                {
                    mobile = fPMADDRDao.getTel("01", d.policy_no, d.policy_seq, d.id_dup, conn400);

                    if("".Equals(StringUtil.toString(mobile)))
                        mobile = fPMADDRDao.getTel("03", d.policy_no, d.policy_seq, d.id_dup, conn400);
                }

                if ("".Equals(StringUtil.toString(mobile))) {
                    FNBADDNDao fNBADDNDao = new FNBADDNDao();

                    if (StringUtil.toString(d.paid_name).Equals(StringUtil.toString(d.ins_name)))
                        mobile = fNBADDNDao.getTel("mobile_i", d.policy_no, d.policy_seq, d.id_dup, conn400);
                    if ("".Equals(StringUtil.toString(mobile)) & StringUtil.toString(d.paid_name).Equals(StringUtil.toString(d.appl_name)))
                        mobile = fNBADDNDao.getTel("mobile", d.policy_no, d.policy_seq, d.id_dup, conn400);
                }
            }

            


            return mobile;
        }


        /// <summary>
        /// 一年以下簡訊通知報表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="model"></param>
        /// <param name="fullPath"></param>
        /// <param name="aply_no"></param>
        /// <returns></returns>
        public int genSmsNotifyRpt(string type, OAP0042Model model, string fullPath, string aply_no)
        {
            int cnt = 0;
            
            
            try
            {
                FAPTelSmsTempDao fAPTelSmsTempDao = new FAPTelSmsTempDao();
                List<TelDispatchRptModel> dataList = fAPTelSmsTempDao.qrySmsNotifyRpt(model.rpt_cnt_tp, aply_no);

                foreach (TelDispatchRptModel d in dataList)
                {
                    DateTime check_ym = Convert.ToDateTime(d.check_date);
                    d.fsc_range = (check_ym.Year - 1911) + "/" + check_ym.Month.ToString().PadLeft(2, '0');
                }


                List <TelDispatchRptModel> idCheckList = dataList.GroupBy(o => new { o.temp_id, o.check_no, o.check_acct_short, o.check_amt, o.fsc_range })
              .Select(group => new TelDispatchRptModel
              {
                  temp_id = group.Key.temp_id,
                  check_no = group.Key.check_no,
                  check_acct_short = group.Key.check_acct_short,
                  fsc_range = group.Key.fsc_range,
                  check_amt = group.Key.check_amt
              }).ToList<TelDispatchRptModel>();


                List<TelDispatchRptModel> idList = idCheckList.GroupBy(o => new { o.temp_id }).Select(group => new TelDispatchRptModel
                {
                    temp_id = group.Key.temp_id,
                    amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                }).ToList<TelDispatchRptModel>();

                string[] idArr = idList.Where(x => x.amt >= Convert.ToInt64(model.stat_amt_b) & x.amt <= Convert.ToInt64(model.stat_amt_e))
                .ToList<TelDispatchRptModel>().Select(x => x.temp_id).ToArray();


                if (idArr.Count() > 0) {
                    //紀錄稽核軌跡
                    //writePiaLog(pgm_id, user_id, user_name, dataList.Count(), dataList[0].paid_id, "X");
                    if ("S".Equals(type))
                        genSmsNotifyRptS(idCheckList.Where(x => idArr.Contains(x.temp_id)).ToList(), fullPath);
                    else
                        genSmsNotifyRptD(dataList.Where(x => idArr.Contains(x.temp_id)).ToList(), fullPath);
                }


                return idCheckList.Count();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


        /// <summary>
        /// 一年以下簡訊通知-統計表
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="fullPath"></param>
        public void genSmsNotifyRptS(List<TelDispatchRptModel> dataList, string fullPath)
        {

            try
            {

                string[] fsc_range_arr = dataList.OrderBy(x => x.fsc_range).Select(x => x.fsc_range).Distinct().ToArray();

                List<TelDispatchRptModel> tmpList = dataList.GroupBy(o => new { o.temp_id, o.fsc_range })
              .Select(group => new TelDispatchRptModel
              {
                  temp_id = group.Key.temp_id,
                  fsc_range = group.Key.fsc_range,
                  amt = group.Sum(x => Convert.ToInt64(x.check_amt))
              }).ToList<TelDispatchRptModel>();


                List<string> amt_range_list = new List<string>();
                //查詢級距
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");

                foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                {
                    decimal amt_range = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);
                    amt_range_list.Add(amt_range_desc);
                    long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);

                    tmpList.Where(x => x.amt >= range_l & x.amt <= range_h).Select(x => { x.amt_range = amt_range.ToString(); return x; }).ToList();
                    tmpList.Where(x => x.amt >= range_l & x.amt <= range_h).Select(x => { x.amt_range_desc = amt_range_desc; return x; }).ToList();
                }

                List<TelDispatchRptModel> rptStatRow = tmpList.GroupBy(o => new { o.fsc_range, o.amt_range, o.amt_range_desc })
          .Select(group => new TelDispatchRptModel
          {
              fsc_range = group.Key.fsc_range,
              amt_range = group.Key.amt_range,
              amt_range_desc = group.Key.amt_range_desc,
              cnt = group.Count(),
              amt = group.Sum(x => Convert.ToInt64(x.amt))
          }).ToList<TelDispatchRptModel>();

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("一年以下簡訊通知統計表");

                    int iCol = 0;
                    int iRow = 0;
                    long fsc_range_tot_cnt = 0;
                    long fsc_range_tot_amt = 0;

                    ws.Cell(1, 1).Value = "支票到期月份";
                    ws.Cell(2, 1).Value = "級距";

                    //支票到期月份
                    foreach (string fsc in fsc_range_arr)
                    {
                        fsc_range_tot_cnt = 0;
                        fsc_range_tot_amt = 0;

                        iRow = 3;
                        iCol += 2;
                        ws.Cell(1, iCol).Value = "'" + fsc;


                        ws.Range(1, iCol, 1, iCol + 1).Merge();


                        ws.Cell(2, iCol).Value = "件數";
                        ws.Cell(2, iCol + 1).Value = "金額";


                        //級距迴圈
                        foreach (string amt_range_desc in amt_range_list)
                        {
                            if (iCol == 2)
                                ws.Cell(iRow, 1).Value = amt_range_desc;


                            TelDispatchRptModel rptItem = rptStatRow.Where(x => x.fsc_range == fsc && x.amt_range_desc == amt_range_desc).FirstOrDefault();



                            if (rptItem == null)
                            {
                                ws.Cell(iRow, iCol).Value = 0;
                                ws.Cell(iRow, iCol + 1).Value =  0;
                            }
                            else
                            {
                                ws.Cell(iRow, iCol).Value = rptItem.cnt;
                                ws.Cell(iRow, iCol + 1).Value = rptItem.amt;

                                fsc_range_tot_cnt += rptItem.cnt;
                                fsc_range_tot_amt += rptItem.amt;
                            }


                            //處理"級距"統計資料
                            ws.Cell(1, iCol + 2).Value = "合計";
                            ws.Range(1, iCol + 2, 1, iCol + 3).Merge();
                            ws.Cell(2, iCol + 2).Value = "件數";
                            ws.Cell(2, iCol + 3).Value = "金額";
                            TelDispatchRptModel rptItem_range_tot = rptStatRow.Where(x => x.amt_range_desc == amt_range_desc).GroupBy(o => new { o.amt_range })
               .Select(group => new TelDispatchRptModel
               {
                   amt_range = group.Key.amt_range,
                   cnt = group.Sum(x => Convert.ToInt64(x.cnt)),
                   amt = group.Sum(x => Convert.ToInt64(x.amt))
               }).FirstOrDefault();

                            if (rptItem_range_tot == null)
                            {
                                ws.Cell(iRow, iCol + 2).Value = 0;
                                ws.Cell(iRow, iCol + 3).Value = 0;
                            }
                            else
                            {
                                ws.Cell(iRow, iCol + 2).Value = rptItem_range_tot.cnt;
                                ws.Cell(iRow, iCol + 3).Value = rptItem_range_tot.amt;
                            }

                            iRow++;
                        }


                        //處理"保局範圍"統計資料
                        if (iCol == 2)
                        {
                            ws.Cell(iRow, 1).Value = "合計";
                            //ws.Cell(iRow + 1, 1).Value = "總計";
                            //ws.Cell(iRow + 2, 1).Value = "佔比";
                        }


                        //符合畫面條件的保局範圍統計資料
                        TelDispatchRptModel rptItem_fac_tot = rptStatRow.Where(x => x.fsc_range == fsc).GroupBy(o => new { o.fsc_range })
               .Select(group => new TelDispatchRptModel
               {
                   fsc_range = group.Key.fsc_range,
                   cnt = group.Sum(x => Convert.ToInt64(x.cnt)),
                   amt = group.Sum(x => Convert.ToInt64(x.amt))
               }).FirstOrDefault();

                        if (rptItem_fac_tot == null)
                        {
                            ws.Cell(iRow, iCol).Value = 0;
                            ws.Cell(iRow, iCol + 1).Value = 0;
                        }
                        else
                        {
                            ws.Cell(iRow, iCol).Value =  rptItem_fac_tot.cnt;
                            ws.Cell(iRow, iCol + 1).Value = rptItem_fac_tot.amt;
                        }

                    }


                    //處理統計資料
                    ws.Cell(iRow, iCol + 2).Value = rptStatRow.Sum(x => x.cnt);
                    ws.Cell(iRow, iCol + 3).Value = rptStatRow.Sum(x => x.amt);


                    ws.Range(2, 1, 2, iCol + 3).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                    ws.Range(2, 1, 2, iCol + 3).Style.Font.FontColor = XLColor.White;
                    ws.Range(3, 1, iRow, iCol + 3).Style.NumberFormat.Format ="#,##0";


                    ws.Columns().AdjustToContents();  // Adjust column width
                    ws.Rows().AdjustToContents();     // Adjust row heights
                    
                    workbook.SaveAs(fullPath);
                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }



        /// <summary>
        /// 一年以下簡訊通知-明細表
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="fullPath"></param>
        public void genSmsNotifyRptD(List<TelDispatchRptModel> dataList, string fullPath)
        {

            try
            {
                //若屬一年以下簡訊通知，一張支票以一張保單號碼為代表
                List<TelDispatchRptModel> tmpList = new List<TelDispatchRptModel>();

                CommonUtil commonUtil = new CommonUtil();
                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                string _check_acct_short = "";
                string _check_no = "";
                foreach (TelDispatchRptModel d in dataList.OrderBy(x => x.check_acct_short).ThenBy(x => x.check_no))
                {
                    if (!_check_acct_short.Equals(d.check_acct_short) || !_check_no.Equals(d.check_no))
                    {
                        tmpList.Add(d);
                        _check_acct_short = d.check_acct_short;
                        _check_no = d.check_no;
                    }
                }

                dataList = tmpList;

                //#region 查詢AS400資料
                //EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn());
                //conn400.Open();
                //SAP7018Util sAP7018Util = new SAP7018Util();
                //foreach (TelDispatchRptModel d in dataList) {
                //    try
                //    {
                //        //保戶電話
                        
                //        SAP7018Model sAP7018Model = new SAP7018Model();

                //        ObjectUtil.CopyPropertiesTo(d, sAP7018Model);

                //        List<SAP7018TelModel> telList = sAP7018Util.callSAP7018(conn400, sAP7018Model);

                //        foreach (SAP7018TelModel telD in telList)
                //        {
                //            if ("M".Equals(StringUtil.toString(telD.tel_type)))
                //                d.policy_mobile += StringUtil.toString(telD.tel) + ";";
                //            else
                //                d.policy_tel += StringUtil.toString(telD.tel) + ";";

                //        }
                //    }
                //    catch (Exception exTel)
                //    {
                //        logger.Error(exTel.ToString());
                //    }
                //}
                
                //conn400.Close();
                //conn400 = null;
                //#endregion


                FAPTelPoliDao fAPTelPoliDao = new FAPTelPoliDao();
                SysCodeDao sysCodeDao = new SysCodeDao();

                //SysCodeDao sysCodeDao = new SysCodeDao();
                FPMCODEDao fPMCODEDao = new FPMCODEDao();
                //給付類型
                //Dictionary<string, string> oPaidCdMap = sysCodeDao.qryByTypeDic("AP", "O_PAID_CD");
                Dictionary<string, string> apPaidMap = fPMCODEDao.qryByTypeDic("AP_PAID", "AP", true);

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    //int iCol = 0;
                    int iRow = 0;

                    for (int i = 1; i <= 2; i++)
                    {
                        string _sheet_name = "一年以下簡訊通知明細表_";

                        string _sec_stat = "";
                        iRow = 2;

                        if (i == 1)
                        {
                            _sheet_name += "一般件";
                            _sec_stat = "N";
                        }
                        else
                        {
                            _sheet_name += "密戶件";
                            _sec_stat = "Y";
                        }


                        var ws = workbook.Worksheets.Add(_sheet_name);

                        ws.Cell(1, 1).Value = "dispatch_date";
                        ws.Cell(1, 2).Value = "tel_interview_id";
                        ws.Cell(1, 3).Value = "tel_interview_name";
                        ws.Cell(1, 4).Value = "fsc_range";
                        ws.Cell(1, 5).Value = "check_no";
                        ws.Cell(1, 6).Value = "check_acct_short";
                        ws.Cell(1, 7).Value = "check_date";
                        ws.Cell(1, 8).Value = "check_amt";
                        ws.Cell(1, 9).Value = "o_paid_cd";
                        ws.Cell(1, 10).Value = "o_paid_cd_nm";
                        ws.Cell(1, 11).Value = "paid_id";
                        ws.Cell(1, 12).Value = "paid_name";
                        ws.Cell(1, 13).Value = "system";
                        ws.Cell(1, 14).Value = "policy_no";
                        ws.Cell(1, 15).Value = "policy_seq";
                        ws.Cell(1, 16).Value = "id_dup";
                        ws.Cell(1, 17).Value = "appl_id";
                        ws.Cell(1, 18).Value = "appl_name";
                        ws.Cell(1, 19).Value = "ins_id";
                        ws.Cell(1, 20).Value = "ins_name";
                        ws.Cell(1, 21).Value = "policy_mobile";
                        ws.Cell(1, 22).Value = "policy_tel";

                        ws.Cell(2, 1).Value = "派件日";
                        ws.Cell(2, 2).Value = "簡訊人員(帳號)";
                        ws.Cell(2, 3).Value = "簡訊人員";
                        ws.Cell(2, 4).Value = "保局範圍";
                        ws.Cell(2, 5).Value = "支票號碼";
                        ws.Cell(2, 6).Value = "支票帳號簡稱";
                        ws.Cell(2, 7).Value = "支票到期日";
                        ws.Cell(2, 8).Value = "支票金額";
                        ws.Cell(2, 9).Value = "給付類型";
                        ws.Cell(2, 10).Value = "給付類型中文名稱";
                        ws.Cell(2, 11).Value = "給付對象 ID";
                        ws.Cell(2, 12).Value = "給付對象姓名";
                        ws.Cell(2, 13).Value = "大系統別";
                        ws.Cell(2, 14).Value = "保單號碼";
                        ws.Cell(2, 15).Value = "保單序號";
                        ws.Cell(2, 16).Value = "重覆碼";
                        ws.Cell(2, 17).Value = "要保人 ID";
                        ws.Cell(2, 18).Value = "要保人姓名";
                        ws.Cell(2, 19).Value = "被保人 ID";
                        ws.Cell(2, 20).Value = "被保人姓名";
                        ws.Cell(2, 21).Value = "保戶電話(1)";
                        ws.Cell(2, 22).Value = "保戶電話(2)";

                        foreach (TelDispatchRptModel d in dataList.Where(x => x.sec_stat == _sec_stat)
                          .OrderBy(x => x.paid_id).ThenBy(x => Convert.ToDateTime(x.check_date)))
                        {
                            try
                            {
                                iRow++;

                                //派件日
                                ws.Cell(iRow, 1).Value = "";

                                //簡訊人員
                                ws.Cell(iRow, 2).Value =StringUtil.toString(d.tel_interview_id);

                                if (!empMap.ContainsKey(d.tel_interview_id))
                                {
                                    ADModel adModel = new ADModel();
                                    adModel = commonUtil.qryEmp(d.tel_interview_id);
                                    empMap.Add(d.tel_interview_id, adModel);
                                }
                                d.tel_interview_name = empMap[d.tel_interview_id].name;
                                ws.Cell(iRow, 3).Value = d.tel_interview_name;
                                

                                //保局範圍
                                ws.Cell(iRow, 4).Value = "";

                                //支票號碼
                                ws.Cell(iRow, 5).Value = d.check_no;

                                //支票帳號簡稱
                                ws.Cell(iRow, 6).Value = d.check_acct_short;

                                //支票到期日
                                ws.Cell(iRow, 7).Value = "'" + DateUtil.ADDateToChtDate(d.check_date, 3, "/");

                                //支票金額
                                ws.Cell(iRow, 8).Value = d.check_amt;

                                //給付類型
                                ws.Cell(iRow, 9).Value = d.o_paid_cd;

                                if (apPaidMap.ContainsKey(d.o_paid_cd))
                                    ws.Cell(iRow, 10).Value = apPaidMap[d.o_paid_cd];

                                //給付對象 ID
                                ws.Cell(iRow, 11).Value = d.paid_id;
                                ws.Cell(iRow, 12).Value = d.paid_name;

                                //保單
                                ws.Cell(iRow, 13).Value = d.system;
                                ws.Cell(iRow, 14).Value = d.policy_no;
                                ws.Cell(iRow, 15).Value = d.policy_seq;
                                ws.Cell(iRow, 16).Value = d.id_dup;

                                //要保人
                                ws.Cell(iRow, 17).Value = d.appl_id;
                                ws.Cell(iRow, 18).Value = d.appl_name;

                                //被保人
                                ws.Cell(iRow, 19).Value = d.ins_id;
                                ws.Cell(iRow, 20).Value = d.ins_name;


                                //保戶電話
                                ws.Cell(iRow, 21).Value = d.policy_mobile;
                                ws.Cell(iRow, 22).Value = "";

                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                        }


                        ws.Range(1, 1, 1, 22).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(1, 1, 1, 22).Style.Font.FontColor = XLColor.White;



                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights

                    }

                    workbook.SaveAs(fullPath);
                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }



        public void procAs400TelAddr(FAPPPAWModel ppaw, List<FAPPPAWModel> ppawList ,EacConnection conn400, EacTransaction transaction400) {
            FAPPPAWDao fAPPPAWDao = new FAPPPAWDao();
            //fAPPPAWDao.delete("X0007", "2", conn400, transaction400);
            fAPPPAWDao.delByCheckNo(ppaw.report_tp, ppaw.dept_group, ppaw.check_no, conn400, transaction400);
            fAPPPAWDao.insert(ppawList, conn400, transaction400);

        }

        /// <summary>
        /// 依電訪覆核狀態更新相關檔案
        /// </summary>
        /// <param name="_usr_id"></param>
        /// <param name="exec_action"></param>
        /// <param name="aply_no"></param>
        /// <param name="_tel"></param>
        /// <param name="_paid_id"></param>
        /// <param name="_paid_name"></param>
        /// <param name="now"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public FAPPPAWModel procTelInterview(string _usr_id, string exec_action, string aply_no, FAP_TEL_INTERVIEW _tel, string _paid_id, string _paid_name
            , DateTime now, SqlConnection conn, SqlTransaction transaction)
        {
            FAPPPAWModel _tel_addr = new FAPPPAWModel();

            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();

            string _dispatch_status = "";
            string _clean_status = "";

            switch (_tel.tel_appr_result)
            {
                case "11":  //進入追蹤
                    _dispatch_status = "1"; //派件中
                    break;
                case "12":  //PENDING
                    _dispatch_status = "2"; //電訪結束
                    break;
                case "13":  //進入清理
                    _dispatch_status = "2"; //電訪結束
                    _clean_status = "1";
                    _tel.clean_date = now;
                    break;
                case "15":  //轉行政單位
                    _dispatch_status = "2"; //電訪結束
                    break;
                case "16":  //給付結案
                    _dispatch_status = "2"; //電訪結束
                    break;
                case "14":  //重新派案
                    _dispatch_status = "3"; //重新派件
                    break;
            }
            _tel.dispatch_status = _dispatch_status;
            _tel.clean_status = _clean_status;


            //新增【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
            FAPVeTraceDao faPVeTraceDao = new FAPVeTraceDao();

            if ("A".Equals(exec_action))
            {
                fAPTelInterviewDao.insert(_tel, conn, transaction);

                fAPVeTrackProcDao.insertForTelProc("tel_assign_case", _tel.tel_proc_no, "G6", "F2", _tel.reason, (DateTime)_tel.tel_interview_datetime
                , _usr_id, now, conn, transaction);

                faPVeTraceDao.updateForTelCheck("tel_assign_case", aply_no, _tel.reason, (DateTime)_tel.tel_interview_datetime, conn, transaction);
            }
            else if ("U".Equals(exec_action)) {
                fAPTelInterviewDao.updForOAP0047A(_tel, conn, transaction);

                switch (_tel.tel_result) {
                    case "3":
                        //處理結果為寄信,則追踨時則須寫入踐行程序
                        //証明文件:“F1郵寄通知記錄” 
                        //執行日期: 追踨日期
                        //踐行程序: G15電訪寄發信函
                        //過程說明: 原因說明
                        fAPVeTrackProcDao.insertForTelProc("tel_assign_case", _tel.tel_proc_no, "G15", "F1", _tel.reason, (DateTime)_tel.tel_interview_datetime
                , _usr_id, now, conn, transaction);

                        faPVeTraceDao.updateForTelCheck("tel_assign_case", aply_no, _tel.reason, (DateTime)_tel.tel_interview_datetime, conn, transaction);
                        break;

                    case "4":
                        //處理結果為E-MAIL,則追踨時則須寫入踐行程序
                        //証明文件:“F5 emai或簡訊通知紀錄” 
                        //執行日期: 追踨日期
                        //踐行程序: G16 email/簡訊
                        //過程說明: email/簡訊通知
                        fAPVeTrackProcDao.insertForTelProc("tel_assign_case", _tel.tel_proc_no, "G16", "F5", "email/簡訊通知", (DateTime)_tel.tel_interview_datetime
                , _usr_id, now, conn, transaction);

                        faPVeTraceDao.updateForTelCheck("tel_assign_case", aply_no, _tel.reason, (DateTime)_tel.tel_interview_datetime, conn, transaction);
                        break;
                }

            }

            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
            _tel_check.dispatch_status = _dispatch_status;
            _tel_check.update_id = _tel.update_id;
            _tel_check.update_datetime = _tel.update_datetime;
            _tel_check.tel_std_type = "tel_assign_case";
            _tel_check.tel_proc_no = _tel.tel_proc_no;

            if ("3".Equals(_dispatch_status))
                fAPTelCheckDao.reAssignByTelProcNo(_tel_check, conn, transaction);
            else
                fAPTelCheckDao.updDispatchStatusByTelProcNo(_tel_check, conn, transaction);

            //電訪處理結果="寄信"時，處理地址
            //1.異動【FAP_HOUSEHOLD_ADDR 戶政查詢地址檔】
            //2.回寫地址到AS400
            if ("3".Equals(_tel.tel_result) & !"".Equals(StringUtil.toString(_paid_id)))
                _tel_addr = procAddr(_usr_id, _paid_id, _paid_name, _tel, now, conn, transaction);

            return _tel_addr;

        }


        /// <summary>
        /// 電訪處理結果="寄信"：若"給付對象ID"<>空白，則將地址轉入清理記錄檔
        /// </summary>
        /// <param name="_usr_id"></param>
        /// <param name="_paid_id"></param>
        /// <param name="_paid_name"></param>
        /// <param name="_tel"></param>
        /// <param name="now"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private FAPPPAWModel procAddr(string _usr_id, string _paid_id, string _paid_name, FAP_TEL_INTERVIEW _tel, DateTime now
            , SqlConnection conn, SqlTransaction transaction)
        {
            FAPPPAWModel _tel_addr = new FAPPPAWModel();

            FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();
            FAP_HOUSEHOLD_ADDR oAddr = addrDao.qryByKey(_paid_id, "TEL");
            OAP0009Model addr = new OAP0009Model();
            addr.paid_id = _paid_id;
            addr.addr_type = "TEL";
            addr.paid_name = _paid_name;
            addr.zip_code = _tel.tel_zip_code;
            addr.address = _tel.tel_addr;

            if (!"".Equals(StringUtil.toString(oAddr.paid_id)))
            {
                addrDao.update(now, _usr_id, addr, conn, transaction);
            }
            else
            {
                addrDao.insert(now, _usr_id, addr, conn, transaction);
            }

            string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');
            _tel_addr.report_tp = "X0007";
            _tel_addr.dept_group = "2";
            _tel_addr.paid_id = _paid_id;
            _tel_addr.check_no = _paid_id;
            _tel_addr.r_zip_code = _tel.tel_zip_code;
            _tel_addr.r_addr = _tel.tel_addr;
            _tel_addr.entry_id = _usr_id;
            _tel_addr.entry_date = chtDt[0];
            _tel_addr.entry_time = chtDt[1];


            return _tel_addr;

        }



        /// <summary>
        /// 派件報表
        /// </summary>
        /// <param name="pgm_id"></param>
        /// <param name="user_id"></param>
        /// <param name="user_name"></param>
        /// <param name="type"></param>
        /// <param name="fullPath"></param>
        /// <param name="dataList"></param>
        public async Task genDispatchRpt(string pgm_id, string user_id, string user_name, string type, string fullPath,
            List<TelDispatchRptModel> dataList) {

            //若屬一年以下簡訊通知，一張支票以一張保單號碼為代表
            if ("sms_notify_case".Equals(type))
            {
                List<TelDispatchRptModel> tmpList = new List<TelDispatchRptModel>();

                string _check_acct_short = "";
                string _check_no = "";
                foreach (TelDispatchRptModel d in dataList.OrderBy(x => x.check_acct_short).ThenBy(x => x.check_no))
                {
                    if (!_check_acct_short.Equals(d.check_acct_short) || !_check_no.Equals(d.check_no))
                    {
                        tmpList.Add(d);
                        _check_acct_short = d.check_acct_short;
                        _check_no = d.check_no;
                    }
                }

                dataList = tmpList;
            }



            //紀錄稽核軌跡
            writePiaLog(pgm_id, user_id, user_name, dataList.Count(), dataList[0].paid_id, "X");
            FAPTelPoliDao fAPTelPoliDao = new FAPTelPoliDao();
            FMNPPAADao fMNPPAADao = new FMNPPAADao();


            try
            {
                #region 查詢AS400資料
                EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn());
                conn400.Open();
                try
                {

                    dataList = fMNPPAADao.qryForTelDispatchRpt(conn400, dataList);

                    SAP7018Util sAP7018Util = new SAP7018Util();

                    int i = 0;

                    foreach (TelDispatchRptModel d in dataList)
                    {
                        i++;

                        if (i % 500 == 0)
                            logger.Info("i:" + i);


                        try
                        {
                            //取得服務人員手機
                            if (!"".Equals(StringUtil.toString(d.send_id)))
                            {
                                logger.Info("send_id" + d.send_id);

                                if ("F".Equals(StringUtil.toString(d.system)) && "3".Equals(StringUtil.toString(d.sysmark)))
                                {
                                    LydiaUtil lydiaUtil = new LydiaUtil();
                                    Lydia004Model lydia004Model = new Lydia004Model();
                                    lydia004Model.agentId = d.send_id;
                                    lydia004Model = await lydiaUtil.callLydia004Async(lydia004Model);

                                    d.send_tel = StringUtil.toString(lydia004Model.mobilePhone);
                                    d.send_name = StringUtil.toString(lydia004Model.agentName);     //add by daiyu 20210225
                                    d.send_unit = StringUtil.toString(lydia004Model.unitCode);  //add by daiyu 20210225

                                    logger.Info("lydia004Model.returnCode:" + lydia004Model.returnCode);
                                }
                                else
                                {
                                    LydiaUtil lydiaUtil = new LydiaUtil();
                                    Lydia001Model lydia001Model = new Lydia001Model();
                                    lydia001Model.agentId = d.send_id;
                                    lydia001Model = await lydiaUtil.callLydia001Async(lydia001Model);

                                    d.send_tel = StringUtil.toString(lydia001Model.mobileNo);
                                    d.send_name = StringUtil.toString(lydia001Model.agentName);     //add by daiyu 20210225
                                    d.send_unit = StringUtil.toString(lydia001Model.unitCode) + StringUtil.toString(lydia001Model.unitSeq);  //add by daiyu 20210225
                                    
                                    logger.Info("lydia001Model.returnCode:" + lydia001Model.returnCode);
                                }

                                
                            }
                        }
                        catch (Exception exSendTel)
                        {
                            logger.Error(exSendTel.ToString());
                        }

                        try
                        {

                            //保單聯絡電話
                            SAP7018Model sAP7018Model = new SAP7018Model();

                            ObjectUtil.CopyPropertiesTo(d, sAP7018Model);

                            List<SAP7018TelModel> telList = sAP7018Util.callSAP7018(conn400, sAP7018Model);

                            foreach (SAP7018TelModel telD in telList)
                            {
                                if ("M".Equals(StringUtil.toString(telD.tel_type)))
                                    d.policy_mobile += StringUtil.toString(telD.tel) + ";";
                                else
                                    d.policy_tel += StringUtil.toString(telD.tel) + ";";

                                d.address = telD.address;
                            }
                        }
                        catch (Exception exTel)
                        {
                            logger.Error(exTel.ToString());
                        }
                    }

                }
                catch (Exception ex400)
                {
                    logger.Error(ex400.ToString());
                    throw ex400;
                }
                conn400.Close();
                conn400 = null;
                #endregion

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    CommonUtil commonUtil = new CommonUtil();
                    Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();



                    FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                    SysCodeDao sysCodeDao = new SysCodeDao();

                    //設定項目
                    Dictionary<string, string> typeMap = sysCodeDao.qryByTypeDic("AP", "OAP0042_TYPE");

                    //保局範圍
                    Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

                    //SysCodeDao sysCodeDao = new SysCodeDao();
                    FPMCODEDao fPMCODEDao = new FPMCODEDao();
                    //原給付性質
                    //Dictionary<string, string> oPaidCdMap = sysCodeDao.qryByTypeDic("AP", "O_PAID_CD");
                    Dictionary<string, string> oPaidCdMap = fPMCODEDao.qryByTypeDic("PAID_CDTXT", "AP", true);



                    //查詢級距
                    List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");
                    foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                    {
                        string amt_range = StringUtil.toString(range.code_id) == "" ? "0" : range.code_id;
                        string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);

                        dataList.Where(x => x.amt_range == amt_range).Select(x => { x.range_l = range.code_id; return x; }).ToList();
                        dataList.Where(x => x.amt_range == amt_range).Select(x => { x.range_u = range.code_value; return x; }).ToList();
                    }

                    int iRow = 2;

                    for (int i = 1; i <= 2; i++)
                    {

                        string _sheet_name = "";
                        if (typeMap.ContainsKey(type))
                            _sheet_name = typeMap[type] + "_";

                        string _sec_stat = "";
                        iRow = 2;

                        if (i == 1)
                        {
                            _sheet_name += "一般件";
                            _sec_stat = "N";
                        }
                        else
                        {
                            _sheet_name += "密戶件";
                            _sec_stat = "Y";
                        }


                        var ws = workbook.Worksheets.Add(_sheet_name);
                        ws.Cell(1, 1).Value = "dispatch_date";
                        ws.Cell(1, 2).Value = "tel_interview_id";
                        ws.Cell(1, 3).Value = "tel_interview_name";
                        ws.Cell(1, 4).Value = "fsc_range";
                        ws.Cell(1, 5).Value = "check_no";
                        ws.Cell(1, 6).Value = "check_acct_short";
                        ws.Cell(1, 7).Value = "range_u";
                        ws.Cell(1, 8).Value = "range_l";
                        ws.Cell(1, 9).Value = "main_amt";
                        ws.Cell(1, 10).Value = "check_date";
                        ws.Cell(1, 11).Value = "check_amt";
                        ws.Cell(1, 12).Value = "o_paid_cd";
                        ws.Cell(1, 13).Value = "o_paid_cd_nm";
                        ws.Cell(1, 14).Value = "paid_id";
                        ws.Cell(1, 15).Value = "paid_name";
                        ws.Cell(1, 16).Value = "system";
                        ws.Cell(1, 17).Value = "policy_no";
                        ws.Cell(1, 18).Value = "policy_seq";
                        ws.Cell(1, 19).Value = "id_dup";
                        ws.Cell(1, 20).Value = "change_id";
                        ws.Cell(1, 21).Value = "appl_id";
                        ws.Cell(1, 22).Value = "appl_name";
                        ws.Cell(1, 23).Value = "appl_birth";
                        ws.Cell(1, 24).Value = "ins_id";
                        ws.Cell(1, 25).Value = "ins_name";
                        ws.Cell(1, 26).Value = "ins_birth";
                        ws.Cell(1, 27).Value = "sysmark";
                        ws.Cell(1, 28).Value = "send_id";
                        ws.Cell(1, 29).Value = "send_unit";
                        ws.Cell(1, 30).Value = "send_name";
                        ws.Cell(1, 31).Value = "send_tel";
                        ws.Cell(1, 32).Value = "policy_mobile";
                        ws.Cell(1, 33).Value = "policy_tel";
                        ws.Cell(1, 34).Value = "address";


                        ws.Cell(2, 1).Value = "派件日";
                        ws.Cell(2, 2).Value = "第一次電訪人員(帳號)";
                        ws.Cell(2, 3).Value = "第一次電訪人員";
                        ws.Cell(2, 4).Value = "保局範圍";
                        ws.Cell(2, 5).Value = "支票號碼";
                        ws.Cell(2, 6).Value = "支票帳號簡稱";
                        ws.Cell(2, 7).Value = "級距上限";
                        ws.Cell(2, 8).Value = "級距下限";
                        ws.Cell(2, 9).Value = "回存金額";
                        ws.Cell(2, 10).Value = "支票到期日";
                        ws.Cell(2, 11).Value = "支票金額";
                        ws.Cell(2, 12).Value = "原給付性質";
                        ws.Cell(2, 13).Value = "給付性質";
                        ws.Cell(2, 14).Value = "給付對象 ID";
                        ws.Cell(2, 15).Value = "給付對象姓名";
                        ws.Cell(2, 16).Value = "大系統別";
                        ws.Cell(2, 17).Value = "保單號碼";
                        ws.Cell(2, 18).Value = "保單序號";
                        ws.Cell(2, 19).Value = "重覆碼";
                        ws.Cell(2, 20).Value = "案號";
                        ws.Cell(2, 21).Value = "要保人 ID";
                        ws.Cell(2, 22).Value = "要保人姓名";
                        ws.Cell(2, 23).Value = "要保人生日";
                        ws.Cell(2, 24).Value = "被保人 ID";
                        ws.Cell(2, 25).Value = "被保人姓名";
                        ws.Cell(2, 26).Value = "被保人生日";
                        ws.Cell(2, 27).Value = "通路別";
                        ws.Cell(2, 28).Value = "服務人員代碼";
                        ws.Cell(2, 29).Value = "服務人員單位";
                        ws.Cell(2, 30).Value = "服務人員姓名";
                        ws.Cell(2, 31).Value = "服務人員電話";
                        ws.Cell(2, 32).Value = "保戶電話(手機)";
                        ws.Cell(2, 33).Value = "保戶電話(市話)";
                        ws.Cell(2, 34).Value = "收費地址";


                        foreach (TelDispatchRptModel d in dataList.Where(x => x.sec_stat == _sec_stat)
                            .OrderBy(x => x.paid_id).ThenBy(x => DateUtil.ChtDateToADDate(x.check_date, '/')))
                        {
                            try
                            {
                                iRow++;

                                //派件日
                                ws.Cell(iRow, 1).Value = d.dispatch_date;

                                //第一次電訪人員
                                ws.Cell(iRow, 2).Value = d.tel_interview_id;

                                if (!empMap.ContainsKey(d.tel_interview_id))
                                {
                                    ADModel adModel = new ADModel();
                                    adModel = commonUtil.qryEmp(d.tel_interview_id);
                                    empMap.Add(d.tel_interview_id, adModel);
                                }
                                d.tel_interview_name = empMap[d.tel_interview_id].name;
                                ws.Cell(iRow, 3).Value = d.tel_interview_name;


                                //保局範圍
                                if (fscRangeMap.ContainsKey(d.fsc_range))
                                    ws.Cell(iRow, 4).Value = fscRangeMap[d.fsc_range];
                                else
                                    ws.Cell(iRow, 4).Value = d.fsc_range;

                                //支票號碼
                                ws.Cell(iRow, 5).Value = d.check_no;

                                //支票帳號簡稱
                                ws.Cell(iRow, 6).Value = d.check_acct_short;

                                //級距上限
                                ws.Cell(iRow, 7).Value = d.range_u;

                                //級距下限
                                ws.Cell(iRow, 8).Value = d.range_l;

                                //回存金額
                                ws.Cell(iRow, 9).Value = d.main_amt;

                                //支票到期日
                                ws.Cell(iRow, 10).Value = "'" + d.check_date;


                                //支票金額
                                ws.Cell(iRow, 11).Value = d.check_amt;

                                //原給付性質
                                ws.Cell(iRow, 12).Value = d.o_paid_cd;

                                if (oPaidCdMap.ContainsKey(d.o_paid_cd))
                                    ws.Cell(iRow, 13).Value = oPaidCdMap[d.o_paid_cd];

                                //給付對象 ID
                                ws.Cell(iRow, 14).Value = d.paid_id;
                                ws.Cell(iRow, 15).Value = d.paid_name;

                                //保單
                                ws.Cell(iRow, 16).Value = d.system;
                                ws.Cell(iRow, 17).Value = d.policy_no;
                                ws.Cell(iRow, 18).Value = d.policy_seq;
                                ws.Cell(iRow, 19).Value = d.id_dup;
                                ws.Cell(iRow, 20).Value = d.change_id;

                                //要保人
                                ws.Cell(iRow, 21).Value = d.appl_id;
                                ws.Cell(iRow, 22).Value = d.appl_name;
                                ws.Cell(iRow, 23).Value = d.appl_birth;

                                //被保人
                                ws.Cell(iRow, 24).Value = d.ins_id;
                                ws.Cell(iRow, 25).Value = d.ins_name;
                                ws.Cell(iRow, 26).Value = d.ins_birth;

                                //通路別
                                ws.Cell(iRow, 27).Value = d.sysmark;

                                //服務人員
                                ws.Cell(iRow, 28).Value = d.send_id;
                                ws.Cell(iRow, 29).Value = d.send_unit;
                                ws.Cell(iRow, 30).Value = d.send_name;
                                ws.Cell(iRow, 31).Value = "'" + d.send_tel;


                                //保戶電話
                                ws.Cell(iRow, 32).Value = "'" + d.policy_mobile;
                                ws.Cell(iRow, 33).Value = "'" + d.policy_tel;


                                //收費地址
                                ws.Cell(iRow, 34).Value = d.address;

                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                        }

                        ws.Range(1, 1, 1, 33).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(1, 1, 1, 33).Style.Font.FontColor = XLColor.White;


                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights

                    }


                    workbook.SaveAs(fullPath);

                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


        private void writePiaLog(string pgm_id, string usr_id, string user_name, int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = usr_id;
            piaLogMain.ACCOUNT_NAME = user_name;
            piaLogMain.PROGFUN_NAME = pgm_id;
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }



        /// <summary>
        /// 取得金額級距
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public List<OAP0043Model> getAmtRangeDesc(List<OAP0043Model>  dataList)
        {
            List<string> amt_range_list = new List<string>();
            //查詢級距
            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");

            foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
            {
                decimal amt_range = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);
                amt_range_list.Add(amt_range_desc);
                long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);

                dataList.Where(x => Convert.ToInt64(x.amt_range) >= range_l & Convert.ToInt64(x.amt_range) <= range_h)
                    .Select(x => { x.amt_range_desc = amt_range_desc; return x; }).ToList();
            }

            return dataList;
        }


       

    }
}