using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebBO;
using Treasury.WebDaos;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{
    public class DailyRoutine
    {
        public void Routine(string dateTime, DailyRoutineType type)
        {
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dtn = DateTime.Now;
                var _split = dateTime.Split(':');
                var hh = _split[0];
                var mm = _split.Length > 1 ?  _split[1] : string.Empty;
                var dt = new DateTime(dtn.Year,dtn.Month,dtn.Day, Convert.ToInt32(hh),Convert.ToInt32(mm),0);
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
             
                switch (type)
                {
                    case DailyRoutineType.Routine_Fisrt:
                    case DailyRoutineType.Routine_Second:
                        var _Mail_Time = db.MAIL_TIME.AsNoTracking()
                            .FirstOrDefault(x =>
                            x.SEND_TIME != null &&
                            x.SEND_TIME == dateTime);
                        if (_Mail_Time != null &&
                            !db.SYS_JOB_REC.AsNoTracking()
                            .Any(x => x.JOB_ID == _Mail_Time.FUNC_ID &&
                                      x.CREATE_TIME == dt))
                        {
                            var MailTId = sysSeqDao.qrySeqNo("MailT", qPreCode).ToString().PadLeft(3, '0');
                            #region 新增排程工作紀錄檔
                            db.SYS_JOB_REC.Add(new SYS_JOB_REC()
                            {
                                JOB_ITEM_ID = $@"{qPreCode}W{MailTId}",
                                JOB_ID = _Mail_Time.FUNC_ID,
                                CREATE_TIME = dt,
                                STATUS = "2",
                                START_TIME = dtn
                            });
                            db.SaveChanges();
                            #endregion

                            #region 新增 開庫紀錄檔
                            var _TORId = sysSeqDao.qrySeqNo("W", qPreCode).ToString().PadLeft(2, '0');
                            var _TOR = new TREA_OPEN_REC() {
                                TREA_REGISTER_ID = _TORId, //開庫工作單號(金庫登記簿單號)
                                OPEN_TREA_TYPE = "1", //開庫類型
                                OPEN_TREA_REASON = "例行性開庫", //開庫原因
                                OPEN_TREA_TIME = _Mail_Time.TREA_OPEN_TIME, //開庫時間
                                EXEC_TIME_B = _Mail_Time.EXEC_TIME_B, //系統區間(起)
                                EXEC_TIME_E = _Mail_Time.EXEC_TIME_E, //系統區間(迄)
                                CREATE_DT = dtn,
                                OPEN_TYPE = "Y" //Y為例行性
                            };
                            db.TREA_OPEN_REC.Add(_TOR);
                            #endregion

                            #region 取得例行出入庫作業項目

                            #region 自【金庫存取作業設定檔】查詢例行出入庫作業項目
                            var _Trea_Item = db.TREA_ITEM.AsNoTracking()
                                        .Where(x =>
                                        x.DAILY_FLAG == "Y" &&
                                        x.IS_DISABLED == "N").ToList();
                            #endregion

                            #region 清空【申請單紀錄暫存檔】
                            db.TREA_APLY_TEMP.RemoveRange(db.TREA_APLY_TEMP);
                            #endregion

                            #region 將例行作業項目寫入【申請單紀錄暫存檔】
                            _Trea_Item.ForEach(x =>
                            {
                                db.TREA_APLY_TEMP.Add(new TREA_APLY_TEMP() {
                                    ITEM_ID = x.ITEM_ID,
                                    
                                });
                            });
                        
                            #endregion

                            #endregion

                            #region 申請單紀錄暫存檔

                            #endregion

                        }


                        break;
                }
                //db.MAIL_TIME
            }
        }
    }
}