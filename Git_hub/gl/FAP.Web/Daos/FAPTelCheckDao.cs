
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System.Web.Mvc;

/// <summary>
/// 功能說明：FAP_TEL_CHECK 電訪支票檔
/// 初版作者：20200824 Daiyu
/// 修改歷程：20200824 Daiyu
/// 需求單號：
/// 修改內容：初版
/// -------------------------------------------
/// 修改歷程：20210126 Daiyu
/// 需求單號：
/// 修改內容：OAP0053
///           1.修改"給付細項"誤帶"原給付性質"問題
///           2.增加「結案日期」
///           OAP0051
///           1.多讀"派件日期"
/// -------------------------------------------
/// 修改歷程：20210421 Daiyu
/// 需求單號：202103250638-00
/// 修改內容：重新派案時，未將派件狀態="2電訪結束"的更新成"3重新派案"
/// --------------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelCheckDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 戶政調閱清單查詢
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public List<OAP0053Model> qryOAP0053(OAP0053Model d)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                DateTime proc_date_b = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.proc_date_b));
                DateTime proc_date_e = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.proc_date_e));


                var rows = (from telM in db.FAP_TEL_CHECK
                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }

                            join telInterview in db.FAP_TEL_INTERVIEW on telM.tel_proc_no equals telInterview.tel_proc_no

                            join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                            from xPoli in psPoli.DefaultIfEmpty()

                            join proc in db.FAP_TEL_PROC.Where(x => x.appr_stat == "2" & x.proc_status == "8" & x.data_type == "3") on new { telM.tel_proc_no } equals new { proc.tel_proc_no }

                            join code_fsc_range in db.FAP_VE_CODE.Where(x => x.code_type == "FSC_RANGE") on  m.fsc_range  equals code_fsc_range.code_id  into psCodeFscRange
                            from xCodeFscRange in psCodeFscRange.DefaultIfEmpty()

                            join code_clr_status in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "CLR_STATUS") on m.status equals code_clr_status.CODE into psCodeClrStatus
                            from xCodeClrStatus in psCodeClrStatus.DefaultIfEmpty()

                            join code_o_paid_cd in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "O_PAID_CD") on xPoli.o_paid_cd equals code_o_paid_cd.CODE into psCodeOPaidCd
                            from xCodeOPaidCd in psCodeOPaidCd.DefaultIfEmpty()

                            join code_paid_code in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "PAID_CODE") on m.paid_code equals code_paid_code.CODE into psCodePaidCode
                            from xCodePaidCode in psCodePaidCode.DefaultIfEmpty()

                            where 1 == 1
                            & telM.tel_std_type == "tel_assign_case"
                            & telM.data_flag == "Y"
                            & (proc.proc_datetime >= proc_date_b & proc.proc_datetime <= proc_date_e)

                            select new OAP0053Model
                            {
                                fsc_range = m.fsc_range,
                                fsc_range_desc = xCodeFscRange.code_value == null ? m.fsc_range : xCodeFscRange.code_value,
                                paid_id = m.paid_id,
                                paid_name = m.paid_name,
                                check_no = m.check_no,
                                check_acct_short = m.check_acct_short,
                                check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                closed_date = m.closed_date == null ? "" : (SqlFunctions.DatePart("year", m.closed_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.closed_date) + "/" +
                                                  SqlFunctions.DateName("day", m.closed_date).Trim(),

                                check_amt = (Decimal)m.check_amt,
                                o_paid_cd = xPoli.o_paid_cd,
                                o_paid_cd_desc = xCodeOPaidCd.CODE_VALUE == null ? xPoli.o_paid_cd : xCodeOPaidCd.CODE_VALUE,
                                proc_date = proc.proc_datetime == null ? "" : (SqlFunctions.DatePart("year", proc.proc_datetime) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", proc.proc_datetime) + "/" +
                                                  SqlFunctions.DateName("day", proc.proc_datetime).Trim(),
                                re_paid_date = m.re_paid_date == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date).Trim(),
                                status = m.status,
                                status_desc = xCodeClrStatus.CODE_VALUE == null ? m.status : xCodeClrStatus.CODE_VALUE,
                                paid_code = m.paid_code,
                                paid_code_desc = xCodePaidCode.CODE_VALUE == null ? m.paid_code : xCodePaidCode.CODE_VALUE,
                                remark = proc.reason

                            }).Distinct().ToList<OAP0053Model>();

                return rows;
            }
        }


        /// <summary>
        /// OAP0051 - 電訪人員成效報表
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public List<OAP0051Model> qryOAP0051_id(OAP0051Model d)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {

                DateTime tel_interview_f_datetime_b = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.tel_interview_f_datetime_b));
                DateTime tel_interview_f_datetime_e = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.tel_interview_f_datetime_e));


                DateTime dispatch_date_b = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.dispatch_date_b));
                DateTime dispatch_date_e = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.dispatch_date_e));


                var rows = (from telM in db.FAP_TEL_CHECK
                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }

                            join telInterview in db.FAP_TEL_INTERVIEW.Where(x => x.tel_interview_f_datetime >= tel_interview_f_datetime_b & x.tel_interview_f_datetime <= tel_interview_f_datetime_e)
                            on telM.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                            from xTelInterview in psTelInterview.DefaultIfEmpty()

                            where 1 == 1
                            & telM.tel_std_type == "tel_assign_case"
                            & telM.data_flag == "Y"
                            & (telM.dispatch_date >= dispatch_date_b & telM.dispatch_date <= dispatch_date_e)

                            select new OAP0051Model
                            {
                                temp_id = d.cnt_type == "P" ? (m.paid_id == "" ? m.check_no : m.paid_id) : m.check_no,
                                tel_interview_id = telM.tel_interview_id,
                                tel_interview_f_datetime = xTelInterview.tel_interview_f_datetime == null ? "" : SqlFunctions.DateName("year", xTelInterview.tel_interview_f_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", xTelInterview.tel_interview_f_datetime) + "/" +
                                                 SqlFunctions.DateName("day", xTelInterview.tel_interview_f_datetime).Trim(),
                                dispatch_date = telM.dispatch_date == null ? "" : SqlFunctions.DateName("year", telM.dispatch_date) + "/" +
                                                 SqlFunctions.DatePart("m", telM.dispatch_date) + "/" +
                                                 SqlFunctions.DateName("day", telM.dispatch_date).Trim(),
                            }).Distinct().ToList<OAP0051Model>();

                return rows;
            }
        }


        /// <summary>
        /// OAP0051 - 電訪成效統計表
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public List<OAP0051Model> qryOAP0051_result(OAP0051Model d)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                DateTime tel_std_appr_date_b = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.tel_std_appr_date_b));
                DateTime tel_std_appr_date_e = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.tel_std_appr_date_e));

                DateTime tel_interview_f_datetime_b = DateTime.Now;
                DateTime tel_interview_f_datetime_e = DateTime.Now;

                DateTime dispatch_date_b = DateTime.Now;
                DateTime dispatch_date_e = DateTime.Now;

                bool bInterviewDatetime = true;
                bool bDispatchDate = true;

                if (!"".Equals(StringUtil.toString(d.tel_interview_f_datetime_b))) {
                    bInterviewDatetime = false;
                    tel_interview_f_datetime_b = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.tel_interview_f_datetime_b));
                    tel_interview_f_datetime_e = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.tel_interview_f_datetime_e));
                }


                if (!"".Equals(StringUtil.toString(d.dispatch_date_b))) {
                    bDispatchDate = false;
                    dispatch_date_b = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.dispatch_date_b));
                    dispatch_date_e = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.dispatch_date_e));
                }
                    


                var rows = (from telM in db.FAP_TEL_CHECK
                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }

                            join para in db.SYS_PARA_HIS
                            .Where(x => x.SYS_CD == "AP" & x.GRP_ID == "tel_assign_case" & x.APPR_STATUS == "2" & x.PARA_ID == "rpt_cnt_tp")
                            on telM.tel_std_aply_no equals para.APLY_NO

                            join telInterview in db.FAP_TEL_INTERVIEW
                            on telM.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                            from xTelInterview in psTelInterview.DefaultIfEmpty()

                            where 1 == 1
                            & telM.tel_std_type == "tel_assign_case"
                            & telM.data_flag == "Y"
                            & (para.APPR_DT >= tel_std_appr_date_b & para.APPR_DT <= tel_std_appr_date_e)
                            & (bInterviewDatetime || (!bInterviewDatetime & (xTelInterview.tel_interview_f_datetime >= tel_interview_f_datetime_b & xTelInterview.tel_interview_f_datetime <= tel_interview_f_datetime_e)))
                            & (bDispatchDate || (!bDispatchDate & (telM.dispatch_date >= dispatch_date_b & telM.dispatch_date <= dispatch_date_e)))
                            

                            select new OAP0051Model
                            {
                                temp_id = d.cnt_type == "P" ? (m.paid_id == "" ? m.check_no : m.paid_id) : m.check_no,
                                system = m.system,
                                check_acct_short = m.check_acct_short,
                                check_no = m.check_no,
                                tel_result = xTelInterview.tel_result,
                                paid_code = m.paid_code,
                                check_amt = (decimal)m.check_amt,
                                amt_range = telM.amt_range
                            }).Distinct().ToList<OAP0051Model>();


                return rows;
            }
        }



        public List<OAP0049DModel> qryOAP0049CheckList(string tel_std_type, string tel_proc_no)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from telM in db.FAP_TEL_CHECK
                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }

                            join code in db.FAP_VE_CODE.Where(x => x.code_type == "FSC_RANGE") on telM.fsc_range equals code.code_id  into psCode
                            from xCode in psCode.DefaultIfEmpty()


                            where 1 == 1
                            & telM.tel_std_type == tel_std_type
                            & telM.tel_proc_no.Equals(tel_proc_no)

                            select new OAP0049DModel
                            {
                                fsc_range = xCode.code_value == null ? telM.fsc_range : xCode.code_value,
                                check_no = telM.check_no,
                                check_acct_short = telM.check_acct_short,
                                check_amt = (decimal)m.check_amt,
                                check_date = m.check_date == null ? "" : SqlFunctions.DateName("year", m.check_date) + "/" +
                                                 SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                 SqlFunctions.DateName("day", m.check_date).Trim()

                            }).Distinct().ToList<OAP0049DModel>();

                return rows;
            }
        }


        public List<OAP0049Model> qryOAP0049Summary(string tel_std_type, string paid_id, string tel_proc_no)
        {
            bool bPaidId = false;
            if ("".Equals(StringUtil.toString(paid_id)))
                bPaidId = true;

            bool bTelProcNo = false;
            if ("".Equals(StringUtil.toString(tel_proc_no)))
                bTelProcNo = true;


            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from telM in db.FAP_TEL_CHECK

                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                            
                            join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                            from xPoli in psPoli.DefaultIfEmpty()

                            join proc in db.FAP_TEL_PROC.Where(x => x.appr_stat == "2") on new { telM.tel_proc_no } equals new { proc.tel_proc_no } into psProc
                            from xProc in psProc.DefaultIfEmpty()

                            where 1 == 1
                            & telM.tel_std_type == tel_std_type
                            & telM.tel_proc_no != ""
                            & (bPaidId || (!bPaidId & m.paid_id.Equals(paid_id)))
                            & (bTelProcNo || (!bTelProcNo & telM.tel_proc_no.Equals(tel_proc_no)))

                            select new OAP0049Model
                            {
                                seq_no = xProc == null ? 0 :xProc.seq_no,
                                tel_proc_no = telM.tel_proc_no,
                                data_type = xProc.data_type,
                                proc_id = xProc.proc_id,
                                proc_status = xProc.proc_status,
                                proc_datetime = xProc.proc_datetime == null ? "" : (SqlFunctions.DatePart("year", xProc.proc_datetime) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", xProc.proc_datetime) + "/" +
                                                  SqlFunctions.DateName("day", xProc.proc_datetime).Trim(),
                                reason = xProc.reason,
                                aply_no = xProc.aply_no,
                                appr_id = xProc.appr_id,
                                appr_datetime = xProc.appr_datetime == null ? "" : SqlFunctions.DateName("year", xProc.appr_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", xProc.appr_datetime) + "/" +
                                                 SqlFunctions.DateName("day", xProc.appr_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", xProc.appr_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", xProc.appr_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", xProc.appr_datetime).Trim(),
                                appr_status = xProc.appr_status
                            }).OrderBy(x => x.seq_no).Distinct().ToList<OAP0049Model>();

                return rows;
            }
        }



        public List<TelDispatchRptModel> qryOAP0044Rpt(string tel_std_type, string tel_interview_id_o, string check_no, string paid_id)
        {
            bool bInterviewId = false;
            if ("".Equals(StringUtil.toString(tel_interview_id_o)))
                bInterviewId = true;

            bool bCheckNo = false;
            if ("".Equals(StringUtil.toString(check_no)))
                bCheckNo = true;

            bool bPaidId = false;
            if ("".Equals(StringUtil.toString(paid_id)))
                bPaidId = true;


            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from telM in db.FAP_TEL_CHECK

                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                            join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                            from xPoli in psPoli.DefaultIfEmpty()

                            join telInterview in db.FAP_TEL_INTERVIEW on telM.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                            from xTelInterview in psTelInterview.DefaultIfEmpty()

                            where 1 == 1
                            & telM.tel_std_type == tel_std_type
                            & telM.data_flag == "Y"
                            & (telM.dispatch_status == "3" || (telM.dispatch_status == "1" & xTelInterview == null))
                            & m.status != "1"
                            & (bInterviewId || (!bInterviewId & telM.tel_interview_id.Equals(tel_interview_id_o)))
                            & (bCheckNo || (!bCheckNo & telM.check_no.Equals(check_no)))
                            & (bPaidId || (!bPaidId & m.paid_id.Equals(paid_id)))

                            select new TelDispatchRptModel
                            {
                                temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                tel_interview_id = telM.tel_interview_id,
                                fsc_range = telM.fsc_range,
                                check_no = telM.check_no,
                                check_acct_short = telM.check_acct_short,
                                amt_range = telM.amt_range,
                                main_amt = xPoli == null ? 0:(Decimal)xPoli.main_amt,
                                check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                check_amt = (Decimal)m.check_amt,
                                o_paid_cd = xPoli == null ? "" : xPoli.o_paid_cd,
                                paid_id = m.paid_id,
                                paid_name = m.paid_name,
                                system = m.system,
                                policy_no = xPoli == null ? "" : xPoli.policy_no,
                                policy_seq = xPoli == null ? 0 : xPoli.policy_seq,
                                id_dup = xPoli == null ? "" : xPoli.id_dup,
                                change_id = xPoli == null ? "" : xPoli.change_id,
                                sec_stat = telM.sec_stat,
                                data_status = telM.data_status
                            }).OrderBy(x => x.temp_id).Distinct().ToList<TelDispatchRptModel>();

                return rows;
            }
        }

        public List<OAP0046DModel> qryForTelProcRpt(string tel_proc_no)
        {
            

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()

                                join telInterview in db.FAP_TEL_INTERVIEW on telM.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                                from xTelInterview in psTelInterview.DefaultIfEmpty()

                                where 1 == 1
                                   & telM.tel_std_type == "tel_assign_case"
                                   & telM.data_flag == "Y"
                                   & telM.tel_proc_no == tel_proc_no

                                select new OAP0046DModel
                                {

                                    tel_proc_no = telM.tel_proc_no,
                                    tel_interview_id = telM.tel_interview_id,
                                    status = m.status,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    main_amt = xPoli == null ? (Decimal)m.check_amt : (Decimal)xPoli.main_amt,
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd == null ? "" : xPoli.o_paid_cd,
                                    system = m.system,
                                    policy_no = xPoli.policy_no == null ? "" : xPoli.policy_no,
                                    policy_seq = xPoli == null ? 0 : xPoli.policy_seq,
                                    id_dup = xPoli.id_dup == null ? "" : xPoli.id_dup,
                                    dispatch_status = telM.dispatch_status,
                                    tel_result = xTelInterview.tel_result == null ? "" : xTelInterview.tel_result,
                                    data_status = telM.data_status,
                                    tel_appr_result = xTelInterview.tel_appr_result == null ? "" : xTelInterview.tel_appr_result

                                }).Distinct().ToList<OAP0046DModel>();

                    return rows;

                }
            }

        }


        public List<OAP0046DModel> qryForOAP0046(string paid_id, string policy_no, string policy_seq, string id_dup
            , string paid_name, string check_no, string srce_pgm)
        {
            bool bPaidId = true;
            bool bPolicyNo = true;
            bool bPolicySeq = true;
            bool bIdDup = true;
            bool bPaidName = true;
            bool bCheckNo = true;

            if (!"".Equals(StringUtil.toString(paid_id)))
                bPaidId = false;

            if (!"".Equals(StringUtil.toString(policy_no)))
                bPolicyNo = false;

            int _i_policy_seq = 0;
            if (!"".Equals(StringUtil.toString(policy_seq))) {
                bPolicySeq = false;
                _i_policy_seq = Convert.ToInt32(policy_seq);
            }
                

            if (!"".Equals(StringUtil.toString(id_dup)))
                bIdDup = false;

            if (!"".Equals(StringUtil.toString(paid_name)))
                bPaidName = false;

            if (!"".Equals(StringUtil.toString(check_no)))
                bCheckNo = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()

                                join telInterview in db.FAP_TEL_INTERVIEW on telM.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                                from xTelInterview in psTelInterview.DefaultIfEmpty()

                                join telInterview_h in db.FAP_TEL_INTERVIEW_HIS.Where(x => x.appr_stat == "1" & ("0047" == srce_pgm & "2".Equals(x.data_type) || ("0046" == srce_pgm & "1".Equals(x.data_type))))
                                on telM.tel_proc_no equals telInterview_h.tel_proc_no into psTelInterview_h
                                from xTelInterview_h in psTelInterview_h.DefaultIfEmpty()

                                where 1 == 1
                                   & telM.tel_std_type == "tel_assign_case"
                                   & telM.data_flag == "Y"
                                   & new string[] { "1", "2" }.Contains(telM.dispatch_status)
                                   & (bPaidId || (!bPaidId && m.paid_id == paid_id))
                                   & (bPolicyNo || (!bPolicyNo && xPoli.policy_no == policy_no))
                                   & (bPolicySeq || (!bPolicySeq && _i_policy_seq.CompareTo(xPoli.policy_seq) == 0))
                                   & (bIdDup || (!bIdDup && xPoli.id_dup == id_dup))
                                   & (bPaidName || (!bPaidName && m.paid_name == paid_name))
                                   & (bCheckNo || (!bCheckNo && xPoli.check_no == check_no))
                                   & (("0047" == srce_pgm & !telM.tel_proc_no.Equals("")) || ("0046" == srce_pgm))

                                select new OAP0046DModel
                                {
                                    tel_proc_no = telM.tel_proc_no == null ? "" :telM.tel_proc_no,
                                    tel_interview_id = telM.tel_interview_id,
                                    status = m.status,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    main_amt = xPoli == null ? (Decimal)m.check_amt:(Decimal)xPoli.main_amt,
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd == null ? "" : xPoli.o_paid_cd,
                                    system = m.system,
                                    policy_no = xPoli.policy_no == null ? "" : xPoli.policy_no,
                                    policy_seq = xPoli == null ? 0 : xPoli.policy_seq,
                                    id_dup = xPoli.id_dup == null ? "" : xPoli.id_dup,
                                    dispatch_status = telM.dispatch_status,
                                    tel_result = xTelInterview.tel_result == null ? "" : xTelInterview.tel_result,
                                    data_status = "0046" == srce_pgm ? telM.data_status : xTelInterview.data_status,
                                    appr_stat = xTelInterview_h.appr_stat == null ? "" : xTelInterview_h.appr_stat,
                                    aply_no = xTelInterview_h.aply_no == null ? "" : xTelInterview_h.aply_no,
                                    tel_appr_result = xTelInterview.tel_appr_result == null ? "" : xTelInterview.tel_appr_result

                                }).Distinct().ToList<OAP0046DModel>();

                    return rows;

                }
            }

        }

        public List<TelDispatchRptModel> qryForBAP0003ByPaidId(string paid_id, string type, string dispatch_status, string sms_status)
        {
            bool bDisPatch = true;
            bool bSmsStatus = true;

            if (!"".Equals(StringUtil.toString(dispatch_status)))
                bDisPatch = false;

            if (!"".Equals(StringUtil.toString(sms_status)))
                bSmsStatus = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                where 1 == 1
                                   & telM.tel_std_type.Equals(type)
                                   & telM.data_flag == "Y"
                                   & telM.dispatch_status != "2"
                                   & m.paid_id == paid_id
                                   & m.status != "1"
                                   & (bDisPatch || (!bDisPatch && telM.dispatch_status == dispatch_status))
                                   & (bSmsStatus || (!bSmsStatus && telM.sms_status == sms_status))

                                select new TelDispatchRptModel
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    tel_interview_id = telM.tel_interview_id,
                                    fsc_range = telM.fsc_range,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    amt_range = telM.amt_range,
                                    main_amt = (Decimal)xPoli.main_amt,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    system = m.system,
                                    policy_no = xPoli.policy_no,
                                    policy_seq = xPoli.policy_seq,
                                    id_dup = xPoli.id_dup,
                                    change_id = xPoli.change_id,
                                    sec_stat = telM.sec_stat
                                }).Distinct().ToList<TelDispatchRptModel>();

                    return rows;

                }
            }

        }



        public List<TelDispatchRptModel> qryForBAP0003(string tel_interview_id, string type, string dispatch_status, string sms_status)
        {
            bool bDisPatch = true;
            bool bSmsStatus = true;

            if (!"".Equals(StringUtil.toString(dispatch_status)))
                bDisPatch = false;

            if (!"".Equals(StringUtil.toString(sms_status)))
                bSmsStatus = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                where 1 == 1
                                   & telM.tel_std_type.Equals(type)
                                   & telM.data_flag == "Y"
                                   & telM.dispatch_status != "2"
                                   & telM.tel_interview_id == tel_interview_id
                                   & m.status != "1"
                                   & (bDisPatch || (!bDisPatch && telM.dispatch_status == dispatch_status))
                                   & (bSmsStatus || (!bSmsStatus && telM.sms_status == sms_status))

                                select new TelDispatchRptModel
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    tel_interview_id = telM.tel_interview_id,
                                    fsc_range = telM.fsc_range,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    amt_range = telM.amt_range,
                                    main_amt = xPoli == null ? 0 : (Decimal)xPoli.main_amt,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli == null ? "" : xPoli.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    system = m.system,
                                    policy_no = xPoli == null ? "" :xPoli.policy_no,
                                    policy_seq = xPoli == null ? 0 : xPoli.policy_seq,
                                    id_dup = xPoli == null ? "" : xPoli.id_dup,
                                    change_id = xPoli == null ? "" : xPoli.change_id,
                                    sec_stat = telM.sec_stat
                                }).Distinct().ToList<TelDispatchRptModel>();

                    return rows;

                }
            }

        }

        public List<OAP0043Model> qryForOAP0043SmsNotify(string type, string fsc_range, string amt_range)
        {

            bool bFscRange = false;
            if ("".Equals(StringUtil.toString(fsc_range)))
                bFscRange = true;

            bool bAmtRange = false;
            if ("".Equals(StringUtil.toString(amt_range)))
                bAmtRange = true;


            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_TEL_SMS_TEMP
                                join tel in db.FAP_TEL_CHECK.Where(x => x.data_flag == "Y" & x.tel_interview_id == "") 
                                on new { m.check_no, m.check_acct_short, m.tel_std_aply_no } equals new { tel.check_no, tel.check_acct_short, tel.tel_std_aply_no }

                                where 1 == 1
                                   & tel.tel_std_type.Equals(type)
                                   & (bFscRange || (!bFscRange & tel.fsc_range.Equals(fsc_range)))
                                   & (bAmtRange || (!bAmtRange & tel.amt_range.Equals(amt_range)))
                                select new OAP0043Model
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    fsc_range = "",
                                    amt_range = tel.amt_range,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_amt = (Decimal)m.check_amt,
                                    paid_id = m.paid_id


                                }).ToList();

                    return rows;

                }
            }

        }

        /// <summary>
        /// OAP0043電訪派件標準設定作業-查詢
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fsc_range"></param>
        /// <param name="amt_range"></param>
        /// <returns></returns>
        public List<OAP0043Model> qryForOAP0043(string type, string fsc_range, string amt_range)
        {

            bool bFscRange = false;
            if ("".Equals(StringUtil.toString(fsc_range)))
                bFscRange = true;

            bool bAmtRange = false;
            if ("".Equals(StringUtil.toString(amt_range)))
                bAmtRange = true;


            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join tel in db.FAP_TEL_CHECK.Where(x => x.data_flag == "Y" & x.tel_interview_id == "") on new { m.check_no, m.check_acct_short } equals new { tel.check_no, tel.check_acct_short }

                                where 1 == 1
                                   & tel.tel_std_type.Equals(type)
                                   & (bFscRange || (!bFscRange & tel.fsc_range.Equals(fsc_range)))
                                   & (bAmtRange || (!bAmtRange & tel.amt_range.Equals(amt_range)))
                                select new OAP0043Model
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    fsc_range = tel.fsc_range,
                                    amt_range = tel.amt_range,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_amt = (Decimal)m.check_amt,
                                    paid_id = m.paid_id


                                }).ToList();

                    return rows;

                }
            }

        }


        /// <summary>
        /// 重新派件作業-查詢
        /// </summary>
        /// <param name="tel_interview_id_o"></param>
        /// <param name="check_no"></param>
        /// <returns></returns>
        public List<OAP0044Model> qryForOAP0044(string temp_id, string tel_interview_id_o, string check_no, string paid_id) {
            bool bTempId = false;
            if ("".Equals(StringUtil.toString(temp_id)))
                bTempId = true;

            bool bInterviewId = false;
            if ("".Equals(StringUtil.toString(tel_interview_id_o)))
                bInterviewId = true;

            bool bCheckNo = false;
            if ("".Equals(StringUtil.toString(check_no)))
                bCheckNo = true;

            bool bPaidId = false;
            if ("".Equals(StringUtil.toString(paid_id)))
                bPaidId = true;


            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                join tel in db.FAP_TEL_CHECK.Where(x => x.data_flag == "Y" ) on new { m.check_no, m.check_acct_short } equals new { tel.check_no, tel.check_acct_short }

                                join telInterview in db.FAP_TEL_INTERVIEW on tel.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                                from xTelInterview in psTelInterview.DefaultIfEmpty()

                                where 1 == 1
                                   & tel.tel_std_type.Equals("tel_assign_case")
                                   & tel.data_flag == "Y"
                                   & m.status != "1"
                                   & (bInterviewId || (!bInterviewId & tel.tel_interview_id.Equals(tel_interview_id_o)))
                                   & (bCheckNo || (!bCheckNo & tel.check_no.Equals(check_no)))
                                  // & (tel.dispatch_status == "3" || (tel.dispatch_status == "0" & tel.tel_interview_id != "") || (tel.dispatch_status == "1" & xTelInterview == null))
                                   & (tel.dispatch_status == "3" || (tel.dispatch_status == "0") || (tel.dispatch_status == "1" & xTelInterview == null))
                                   & (bPaidId || (!bPaidId & m.paid_id.Equals(paid_id)))
                                select new OAP0044Model
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    tel_interview_id = tel.tel_interview_id,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    data_status = tel.data_status,
                                    tel_appr_result = xTelInterview.tel_appr_result
                                }).Where(x => (bTempId || (!bTempId && x.temp_id == temp_id))
                                & (x.tel_appr_result == null || x.tel_appr_result == "" || x.tel_appr_result == "14")
                                ).Distinct().ToList();

                    return rows;

                }
            }

        }




        /// <summary>
        /// 依"支票號碼 + 電訪標準設定類別"查詢
        /// </summary>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public FAP_TEL_CHECK qryByCheckNo(string check_no, string check_acct_short, string type)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FAP_TEL_CHECK d = db.FAP_TEL_CHECK
                        .Where(x => x.check_no == check_no & x.check_acct_short == check_acct_short & x.tel_std_type == type)
                        .OrderByDescending(x => x.data_flag).FirstOrDefault();

                    if (d != null)
                        return d;
                    else
                        return new FAP_TEL_CHECK();
                }
            }

           
        }


        public void updTelProcNo(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK
  SET tel_proc_no = @tel_proc_no
    , data_status = @data_status
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE system = @system
  AND check_no = @check_no
  AND check_acct_short = @check_acct_short
  AND tel_std_type = @tel_std_type
  AND dispatch_status <> '2'
  AND data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@data_status", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void reAssignOAP0044A(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK
  SET dispatch_status = @dispatch_status
    , tel_proc_no = ''
    , dispatch_date = @dispatch_date
    , tel_interview_id = @tel_interview_id
    , data_status = @data_status
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE  1 = 1
  AND tel_std_type = @tel_std_type
  and check_no = @check_no
  and check_acct_short = @check_acct_short
  and dispatch_status <> '2'
  AND data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                

                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@data_status", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(d.tel_interview_id));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@dispatch_date", System.Data.SqlDbType.DateTime).Value = (Object)d.dispatch_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 重新派件
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void reAssignByTelProcNo(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                //modify by daiyu 20210421
                string sql = @"
UPDATE FAP_TEL_CHECK
  SET dispatch_status = @dispatch_status
    , tel_proc_no = ''
    , dispatch_date = null
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE tel_proc_no = @tel_proc_no
  AND tel_std_type = @tel_std_type
  AND data_flag = 'Y'";
//                string sql = @"
//UPDATE FAP_TEL_CHECK
//  SET dispatch_status = @dispatch_status
//    , tel_proc_no = ''
//    , dispatch_date = null
//    , update_id = @update_id
//    , update_datetime = @update_datetime
//WHERE tel_proc_no = @tel_proc_no
//  AND tel_std_type = @tel_std_type
//  and dispatch_status <> '2'
//  AND data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void updDispatchStatusByTelProcNo(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK
  SET dispatch_status = @dispatch_status
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE tel_proc_no = @tel_proc_no
  AND tel_std_type = @tel_std_type
  AND dispatch_status in ('1', '2')
  AND data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public int updDataStatusByTelProcNo(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {
            int cnt = 0;
            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK
  SET data_status = @data_status
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE tel_proc_no = @tel_proc_no
  AND tel_std_type = @tel_std_type
  AND dispatch_status <> '2'
  AND data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@data_status", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }

            return cnt;
        }



        public void updDataStatus(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK
  SET data_status = @data_status
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE system = @system
  AND check_no = @check_no
  AND check_acct_short = @check_acct_short
  AND tel_std_type = @tel_std_type
  AND dispatch_status <> '2'
  AND data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@data_status", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 註記該筆資料是否有效
        /// </summary>
        /// <param name="data_flag"></param>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updDataFlag(string data_flag, FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK
  SET data_flag = @data_flag
WHERE system = @system
  AND check_no = @check_no
  AND check_acct_short = @check_acct_short
  AND tel_std_type = @tel_std_type";
  //AND dispatch_status <> '2'

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@data_flag", StringUtil.toString(data_flag));
                

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// OAP0043A 電訪派件標準設定覆核作業--核可
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="tel_std_type"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updForOAP0043A(string aply_no, string tel_std_type, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
update FAP_TEL_CHECK
 set FAP_TEL_CHECK.tel_interview_id = his.tel_interview_id
    , FAP_TEL_CHECK.update_id = his.update_id
    , FAP_TEL_CHECK.update_datetime = his.update_datetime
  from FAP_TEL_CHECK f join FAP_TEL_CHECK_HIS his 
    on f.system = his.system and f.check_no = his.check_no and f.check_acct_short = his.check_acct_short 
    and f.tel_std_aply_no = his.tel_std_aply_no and f.tel_std_type = his.tel_std_type
 where his.aply_no = @aply_no
   and his.tel_std_type = @tel_std_type
  AND f.dispatch_status <> '2'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(tel_std_type));


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updVeCleanFinish(FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
update FAP_TEL_CHECK
  set dispatch_status = '2'
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 where system = @system
   and check_acct_short = @check_acct_short
   and check_no = @check_no
   and dispatch_status <> '2'
   and data_flag = 'Y'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void updForDispacthStatus(string tel_interview_id, string tel_std_type, string system, string check_acct_short, string check_no, string tel_proc_no
            , DateTime dt, string status, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";
                sql += " update FAP_TEL_CHECK set ";
                if ("tel_assign_case".Equals(tel_std_type))
                {
                    sql += "  tel_interview_id = @tel_interview_id ";
                    sql += ", dispatch_date = @dispatch_date ";
                    sql += ", dispatch_status = @dispatch_status ";
                }
                else if("sms_assign_case".Equals(tel_std_type))
                {
                    sql += "  tel_interview_id = @tel_interview_id ";
                    sql += ", sms_date = @sms_date ";
                    sql += ", sms_status = @sms_status ";

                }
                sql += " where data_flag = 'Y'";
                sql += " and tel_std_type = @tel_std_type";
                sql += " and system = @system";
                sql += " and check_no = @check_no";
                sql += " and check_acct_short = @check_acct_short";
                sql += " AND dispatch_status<> '2'";
                

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(tel_interview_id));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(tel_std_type));
                cmd.Parameters.AddWithValue("@system", StringUtil.toString(system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(check_acct_short));
                if ("tel_assign_case".Equals(tel_std_type))
                {
                    //cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(tel_proc_no));
                    cmd.Parameters.AddWithValue("@dispatch_date", dt);
                    cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(status));
                }
                else if ("sms_assign_case".Equals(tel_std_type))
                {
                    cmd.Parameters.AddWithValue("@sms_date", dt);
                    cmd.Parameters.AddWithValue("@sms_status", StringUtil.toString(status));
                }

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(DateTime dt, FAP_TEL_CHECK d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_CHECK
           (system 
           ,check_no
           ,check_acct_short
           ,tel_std_aply_no
           ,tel_std_type
           ,fsc_range
           ,amt_range
           ,data_flag
           ,tel_proc_no
           ,tel_interview_id
           ,remark
           ,dispatch_date
           ,dispatch_status
           ,sms_date
           ,sms_status
           ,sec_stat
           ,data_status
           ,update_id
           ,update_datetime)
             VALUES
            (@system 
           , @check_no
           , @check_acct_short
           , @tel_std_aply_no
           , @tel_std_type
           , @fsc_range
           , @amt_range
           , @data_flag
           , @tel_proc_no
           , @tel_interview_id
           , @remark
           , @dispatch_date
           , @dispatch_status
           , @sms_date
           , @sms_status
           , @sec_stat
           , @data_status
           , @update_id
           , @update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@tel_std_aply_no", StringUtil.toString(d.tel_std_aply_no));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@fsc_range", StringUtil.toString(d.fsc_range));
                cmd.Parameters.AddWithValue("@amt_range", StringUtil.toString(d.amt_range));
                cmd.Parameters.AddWithValue("@data_flag", StringUtil.toString(d.data_flag));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(d.tel_interview_id));
                cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                cmd.Parameters.AddWithValue("@dispatch_date", System.Data.SqlDbType.DateTime).Value = (Object)d.dispatch_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@sms_date", System.Data.SqlDbType.DateTime).Value = (Object)d.sms_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@sms_status", StringUtil.toString(d.sms_status));
                cmd.Parameters.AddWithValue("@sec_stat", StringUtil.toString(d.sec_stat) == "" ? "N" : StringUtil.toString(d.sec_stat));
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        
        

    }
}