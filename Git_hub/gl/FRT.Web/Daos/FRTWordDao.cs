using FRT.Web.BO;
using FRT.Web.Enum;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace FRT.Web.Daos
{
    public class FRTWordDao
    {
        public MSGReturnModel<List<ORTB016Model>> qryForORTB016()
        {
            MSGReturnModel<List<ORTB016Model>> results = new MSGReturnModel<List<ORTB016Model>>();
            results.Datas = new List<ORTB016Model>();
            var emplys = new List<V_EMPLY2>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emplys = db.V_EMPLY2.AsNoTracking().Where(x=>x.USR_ID != null).ToList();
            }

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var words = db.FRT_WORD.AsNoTracking().ToList();
                var word_his = db.FRT_WORD_HIS.AsNoTracking().Where(x => x.appr_stat == "1").ToList(); //申請覆核中
                var sysCode_Data_Status = db.SYS_CODE.AsNoTracking().Where(x => x.SYS_CD == "RT" && x.CODE_TYPE == "DATA_STATUS").ToList();
                foreach (var item in words)
                {
                    results.Datas.Add(new ORTB016Model()
                    {
                        frt_word_Id = item.frt_word_id,
                        frt_sys_type = item.frt_sys_type,
                        frt_srce_from = item.frt_srce_from,
                        frt_srce_kind = item.frt_srce_kind,
                        frt_memo_apx = item.frt_memo_apx,
                        frt_achcode = item.frt_achcode,
                        apprId = item.appr_id,
                        apprName = (emplys.FirstOrDefault(x => x.USR_ID == item.appr_id)?.EMP_NAME?.Trim()) ?? item.appr_id,
                        apprDt = item.approve_datetime?.ToString("yyyy/MM/dd HH:mm:ss"),
                        dataStatus = item.data_status,
                        updId = item.update_id,
                        updateUName = (emplys.FirstOrDefault(x => x.USR_ID == item.update_id)?.EMP_NAME?.Trim()) ?? item.update_id,
                        updDatetime = item.update_datetime?.ToString("yyyy/MM/dd HH:mm:ss"),
                    });
                }
            }
            results.RETURN_FLAG = true;
            if (!results.Datas.Any())
                results.DESCRIPTION = Ref.MessageType.data_Empty.GetDescription();
            else
                results.DESCRIPTION = string.Empty;
            return results;
        }

        public MSGReturnModel<string> updateORTB016(List<ORTB016Model> models)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            string msg = string.Empty;
            if (!models.Any())
            {
                result.DESCRIPTION = "無覆核資料";
                return result;
            }
            DateTime dtn = DateTime.Now;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var words = db.FRT_WORD.ToList();
                SysSeqDao sysSeqDao = new SysSeqDao();
                string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
                String qPreCode = curDateTime[0];            
                var aplyNo = string.Empty; 
                foreach (var item in models.Where(x => !string.IsNullOrWhiteSpace(x.status)))
                {
                    if (item.frt_word_Id != null)
                    {
                        if (db.FRT_WORD_HIS.Any(x => x.appr_stat == "1" && x.frt_word_id == item.frt_word_Id))
                        {
                            result.DESCRIPTION = "資料已異動請重新查詢!!";
                            return result;
                        }
                    }
                    if (item.status == "A")
                    {
                        if (db.FRT_WORD_HIS.Any(x =>
                        x.appr_stat == "1" &&
                        x.frt_sys_type == item.frt_sys_type &&
                        x.frt_srce_from == item.frt_srce_from &&
                        x.frt_srce_kind == item.frt_srce_kind &&
                        x.frt_memo_apx == item.frt_memo_apx &&
                        x.frt_achcode == item.frt_achcode))
                        {
                            result.DESCRIPTION = "有相同的新增資料在申請覆核中!!";
                            return result;
                        }
                    }
                    if (aplyNo == string.Empty)
                    {
                        var cId = sysSeqDao.qrySeqNo("RT", "B016", qPreCode).ToString();
                        aplyNo = qPreCode + cId.ToString().PadLeft(3, '0');
                    }
                    var _new_his = new FRT_WORD_HIS()
                    {
                        aply_no = aplyNo,
                        frt_sys_type = item.frt_sys_type,
                        frt_srce_from = item.frt_srce_from,
                        frt_srce_kind = item.frt_srce_kind,
                        frt_memo_apx = item.frt_memo_apx,
                        frt_achcode = item.frt_achcode,
                        exec_action = item.status,
                        appr_stat = "1",
                        update_id = item.updId,
                        update_datetime = dtn
                    };
                    switch (item.status)
                    {
                        case "A":
                            _new_his.frt_word_id = sysSeqDao.qrySeqNo("RT", "WORD", string.Empty).ToString().PadLeft(10, '0');
                            break;
                        case "D":
                        case "U":
                            var _word = words.First(x => x.frt_word_id == item.frt_word_Id);
                            _word.data_status = "2";
                            _word.update_id = item.updId;
                            _word.update_datetime = dtn;
                            _new_his.frt_word_id = item.frt_word_Id;
                            break;
                    }
                    db.FRT_WORD_HIS.Add(_new_his);
                }
                try
                {
                    db.SaveChanges();
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.send_To_Audit_Success.GetDescription(null,$@"申請單號碼:{aplyNo}");
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($@"updateORTB016 : {ex.Message}");
                    result.DESCRIPTION = Ref.MessageType.sys_Error.GetDescription();
                }
            }
            return result;
        }

        /// <summary>
        /// 核可資料異動
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public string appr(List<ORTB016Model> procData, dbFGLEntities db, DateTime dtn)
        {
            var his = db.FRT_WORD_HIS.Where(x => x.appr_stat == "1").ToList();
            var words = db.FRT_WORD.ToList();
            foreach (var item in procData)
            {
                var _his = his.FirstOrDefault(x => x.aply_no == item.aplyNo && x.frt_word_id == item.frt_word_Id);
                var _word = words.FirstOrDefault(x => x.frt_word_id == item.frt_word_Id);
                if (_his == null)
                    return "資料已異動!!";
                switch (_his.exec_action)
                {
                    case "A":                        
                        db.FRT_WORD.Add(new FRT_WORD()
                        {
                            frt_word_id = _his.frt_word_id,
                            frt_sys_type = _his.frt_sys_type,
                            frt_srce_from = _his.frt_srce_from,
                            frt_srce_kind = _his.frt_srce_kind,
                            frt_memo_apx = _his.frt_memo_apx,
                            frt_achcode = _his.frt_achcode,
                            update_id = _his.update_id,
                            update_datetime = _his.update_datetime,
                            data_status = "1",
                            appr_id = item.apprId,
                            approve_datetime = dtn
                        });
                        break;
                    case "U":
                        if (_word == null)
                            return "資料已異動!!";
                        _word.frt_sys_type = _his.frt_sys_type;
                        _word.frt_srce_from = _his.frt_srce_from;
                        _word.frt_srce_kind = _his.frt_srce_kind;
                        _word.frt_memo_apx = _his.frt_memo_apx;
                        _word.frt_achcode = _his.frt_achcode;
                        _word.update_id = _his.update_id;
                        _word.update_datetime = _his.update_datetime;
                        _word.data_status = "1";
                        _word.appr_id = item.apprId;
                        _word.approve_datetime = dtn;
                        break;
                    case "D":
                        if (_word == null)
                            return "資料已異動!!";
                        db.FRT_WORD.Remove(_word);
                        break;
                }
            }
            return string.Empty;
        }

        public string reject(List<ORTB016Model> procData, dbFGLEntities db)
        {
            var words = db.FRT_WORD.ToList();
            foreach (var item in procData.Where(x=>x.status != "A"))
            {
                var _word = words.FirstOrDefault(x => x.frt_word_id == item.frt_word_Id);
                if (_word == null)
                    return "資料已異動!!";
                else _word.data_status = "1";
            }
            return string.Empty;
        }
    }
}