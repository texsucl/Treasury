using Dapper;
using Newtonsoft.Json;
using Obounl.Daos;
using Obounl.Infrastructure;
using Obounl.Models.Interface;
using Obounl.Models.Model;
using Obounl.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using static Obounl.ObounlEnum.Ref;
using static Obounl.Utility.Log;

namespace Obounl.Models.Repository
{
    public class WebRepository : IWebRepository
    {
        /// <summary>
        /// 目前線上等候電訪人數共計○人(類掛號人數功能)
        /// </summary>
        /// <returns></returns>
        public int Waiting_Number()
        {
            var _count = -1;
            string sql = $@"
select count(*) as c from tblRosterPolicy a (nolock),tblRosterPolicy_DTL b (nolock) 
where a.CampID='COL70' and a.LCloseCode='' and b.Appointment='0'
and a.campid=b.campid and a.ListNo=b.ListNo and a.PolicyNo=b.PolicyNo
and a.PolicySeq=b.PolicySeq and a.IDDup=b.IDDup;";
            var _data = new MSSql().Query<dynamic>(sql, null, null);
            if (_data.Item1.IsNullOrWhiteSpace())
            {
                foreach (var item in _data.Item2)
                {
                    _count = item.c;
                }
            }
            return Convert.ToInt32(_count);
        }

        /// <summary>
        /// 取得COL70即時電訪最大可供人力
        /// </summary>
        /// <returns></returns>
        public int ReturnCapacity()
        {
            var sql = "select SMemo1 from tblSysCode (nolock) where Stype='Mapp-Capacity';";
            var data = new MSSql().Query<dynamic>(sql, null, null);

            if (!String.IsNullOrWhiteSpace(data.Item1))
            {
                throw new Exception("取得COL70即時電訪最大可供人力時發生錯誤。");
            }

            var capacityStr = data.Item2.FirstOrDefault()?.SMemo1;
            var result = Convert.ToInt32(capacityStr);

            return result;
        }

        /// <summary>
        /// 取得服務時段
        /// </summary>
        /// <returns></returns>
        public tblSysCode GetServicTime()
        {
            var sql = "select * from tblSysCode where Stype='workday' and Code='workday' and DeleteFlag='N';";
            var data = new MSSql().Query<tblSysCode>(sql, null, null);

            if (!String.IsNullOrWhiteSpace(data.Item1))
            {
                throw new Exception(data.Item1);
            }

            var result = data.Item2.FirstOrDefault();

            return result;
        }

        /// <summary>
        /// 是否於服務時間
        /// </summary>
        /// <returns></returns>
        public bool IsServicing()
        {
            var now = DateTime.Now;
            var sql = "select daytype from cal where @s <= caldtime and caldtime < @e;";
            var data = new MSSql().Query<dynamic>(sql, new { s = now.Date, e = now.AddDays(1).Date }, null);

            if (!String.IsNullOrWhiteSpace(data.Item1))
            {
                throw new Exception(data.Item1);
            }

            if (data.Item2.FirstOrDefault()?.daytype != "W")
            {
                // 非工作日
                return false;
            }

            var tblSysCode = this.GetServicTime();

            if (tblSysCode == null)
            {
                throw new Exception("取得服務時段結果為空值");
            }

            var start = Convert.ToInt32(tblSysCode.SMemo1);
            var end = Convert.ToInt32(tblSysCode.SMemo2);
            var nowTime = now.Hour * 100 + now.Minute;
            var result = start <= nowTime && nowTime <= end;

            return result;
        }

        /// <summary>
        /// 預約電訪/進度查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public List<DataViewModel> GetSearchModel(DataSearchModel searchModel)
        {
            DateTime dtn = DateTime.Now;
            List<DataViewModel> resultModels = new List<DataViewModel>();
            try
            {
                string sql = $@"
Select  
 a.AssignSchDate as AssignSchDate, --要保文件上傳日期,  
 d.custid as custid, --受訪者身份證字號, 
 a.RptReceNo as RptReceNo, --要保書編號,
ISNULL(b.MPolicyNo,'') as MPolicyNo, --保單號碼,   
 d.CustName   as CustName, --受訪者姓名, 
 CASE b.Appointment 
     WHEN 0 then '即時電訪'
     WHEN 1 then '預約電訪'
     ELSE ''
END  as Appointment, --電訪方式, 
--a.AssignSchDate as AssignSchDate, --[預約日期/時段], 
c.SMemo1 as SMemo1 --電訪狀態
From tblRosterPolicy a (nolock),tblRosterPolicy_DTL b (nolock), tblSysCode c (nolock), tblroster d (nolock)
Where a.listno =b.listno 
and a.policyno= b.policyno
and a.iddup=b.iddup
and a.policySeq=b.policySeq
and a.memberid= b.memberid
and a.RptReceNo=b.RptReceNo
and a.LCloseCode =c.code 
and c.Stype ='LCloseCode'
and a.listno = d.listno 
and a.CampID ='COL70' ";


                //--以下條件式
                sql += $@" and (a.sAgentID = @agentID OR  a.sAgentID2 = @agentID ) "; //業務員ID
                if (!searchModel.Sdate.IsNullOrWhiteSpace())
                    sql += $@" and a.AssignSchDate >= @Sdate ";  //起日
                if (!searchModel.Edate.IsNullOrWhiteSpace())
                    sql += $@" and a.AssignSchDate <= @Edate ";  //迄日
                if (!searchModel.RptReceNo.IsNullOrWhiteSpace())
                    sql += $@" and a.RptReceNo = @RptReceNo "; //同意書編號
                if (!searchModel.custid.IsNullOrWhiteSpace())
                    sql += $@" and d.custid = @custid "; //要保身分證字號

                var datas = new MSSql().Query<dynamic>(sql, searchModel).Item2;

                foreach (var item in datas)
                {
                    //要保文件上傳日期 & 預約日期/時段
                    var _AssignSchDate = ((DateTime)(item.AssignSchDate)).ToString("yyyy-MM-dd");
                    //受訪者身分證字號
                    var _custid = item.custid;
                    //要保書編號
                    var _RptReceNo = item.RptReceNo;
                    //保單號碼
                    var _MPolicyNo = item.MPolicyNo;
                    //受訪者姓名
                    var _CustName = item.CustName;
                    //電訪方式
                    var _Appointment = item.Appointment;
                    //電訪狀態
                    var _SMemo1 = item.SMemo1;

                    resultModels.Add(new DataViewModel()
                    {
                        AssignSchDate = _AssignSchDate,
                        custid = _custid,
                        RptReceNo = _RptReceNo,
                        MPolicyNo = _MPolicyNo,
                        CustName = _CustName,
                        Appointment = _Appointment,
                        Reservation_Date = _AssignSchDate,
                        SMemo1 = _SMemo1
                    });
                }

                //PIA
                var _pia = new PIA().AddPIA(new PIA_LOG_MAIN()
                {
                    TRACKING_TYPE = "A", //個資存取紀錄
                    ACCESS_ACCOUNT = "", // searchModel.agentID, //使用者帳號或員工編號
                    ACCOUNT_NAME = "", //使用者姓名或員工姓名
                    FROM_IP = new Common().GetIp(), //記載來源IP或終端機ID
                    ACCESS_DATE = dtn.Date, //記載執行日期
                    ACCESS_TIME = dtn.TimeOfDay, //記載執行時間
                    PROGFUN_NAME = $@"GetSearchModel", //執行程式名稱或交易/功能之代號/名稱
                    ACCESSOBJ_NAME = $@"tblRosterPolicy,tblRosterPolicy_DTL,tblRoster", //系統檔案名稱或存取物件對象名稱(例如TABLE / VIEW名稱）
                    EXECUTION_TYPE = "Q", //查詢
                    EXECUTION_CONTENT = $@"",//記載完整的資料存取動作，如執行所使用輸入條件、檔案存取紀錄或SQL Statement紀錄等
                    AFFECT_ROWS = resultModels.Count, //記載執行動作之結果筆數或影響筆數
                                                      //PIA_OWNER1 = "??",
                    PIA_TYPE = "11111"
                });
            }
            catch (Exception ex)
            {
                var _msg = ex.exceptionMessage();
                NlogSet($@"預約電訪/進度查詢 Error , Msg => {_msg}", null);
                NlogSet($@"預約電訪/進度查詢 Error , Msg => {_msg}", null, Nlog.Error);
            }
            return resultModels;
        }

        /// <summary>
        /// 即時電訪 查詢客戶資料
        /// </summary>
        /// <param name="caseNo"></param>
        /// <param name="userId"></param>
        /// <param name="model"></param>
        public void getCust(string caseNo, string userId, InstantCallViewModel model)
        {
            try
            {
                DateTime dtn = DateTime.Now;

                string sql = $@"
select CaseNo,CaseSeq,InsDT,MData from CampMPool
where CaseNo = @CaseNo
order by SN desc
";
                var _CampMPool = new MSSql().Query_FirstOrDefault<CampMPool>(sql, new { CaseNo = caseNo }).Item2;
                if (_CampMPool != null)
                {
                    var _data = JsonConvert.DeserializeObject<Tel_Visit_InputData>(_CampMPool.MData);

                    sql = $@" 
SELECT Stype,Code,SDesc,SMemo1,SMemo2,SMemo3,DeleteFlag 
FROM [dbCTI].[dbo].[tblSysCode]  
where DeleteFlag = 'N' and stype = @stype 
";
                    var _NBSysTypeDatas = new List<tblSysCode>();
                    var _NBSysType = new MSSql().Query<tblSysCode>(sql, new { stype = "Custcode" });
                    if (_NBSysType.Item1.IsNullOrWhiteSpace())
                        _NBSysTypeDatas = _NBSysType.Item2.ToList();
                    List<InstantCallSubCallViewModel> allData = new List<InstantCallSubCallViewModel>();
                    List<InstantCallSubCallViewModel> fixedData = new List<InstantCallSubCallViewModel>();
                    List<InstantCallSubCallViewModel> fixedData2 = new List<InstantCallSubCallViewModel>();
                    foreach (var item in _data.PolicyCases.First().Customers)
                    {
                        allData.Add(new InstantCallSubCallViewModel()
                        {
                            CustContGen = item.CustContGen,
                            CustID = item.CustID,
                            Cust_Name = item.CustName,
                            Order = _NBSysTypeDatas.FirstOrDefault(x => x.Code == item.CustContGen)?.SMemo2 ?? "0"
                        });
                    }
                    #region 要保人 & 要保人法代
                    List<string> CustContGen_1_4 = new List<string>() { "1", "4" };
                    var _CustContGen_1_4 = allData.Where(x => CustContGen_1_4.Contains(x.CustContGen))
                        .OrderByDescending(x => x.Order).FirstOrDefault();
                    if (_CustContGen_1_4 != null)
                        fixedData.Add(new InstantCallSubCallViewModel()
                        {
                            CustID = _CustContGen_1_4.CustID,
                            CustContGen = _NBSysTypeDatas.FirstOrDefault(z => z.Code == _CustContGen_1_4.CustContGen)?.SDesc ?? _CustContGen_1_4.CustContGen,
                            Cust_Name = _CustContGen_1_4.Cust_Name,
                            Order = _CustContGen_1_4.Order
                        });
                    #endregion
                    #region 被保險人 & 被保險人法代
                    List<string> CustContGen_2_5 = new List<string>() { "2", "5" };
                    var _CustContGen_2_5 = allData.Where(x => CustContGen_2_5.Contains(x.CustContGen))
                       .OrderByDescending(x => x.Order).FirstOrDefault();
                    if (_CustContGen_2_5 != null && !fixedData.Any(x => x.CustID == _CustContGen_2_5.CustID))
                        fixedData.Add(new InstantCallSubCallViewModel()
                        {
                            CustID = _CustContGen_2_5.CustID,
                            CustContGen = _NBSysTypeDatas.FirstOrDefault(z => z.Code == _CustContGen_2_5.CustContGen)?.SDesc ?? _CustContGen_2_5.CustContGen,
                            Cust_Name = _CustContGen_2_5.Cust_Name,
                            Order = _CustContGen_2_5.Order
                        });
                    #endregion
                    #region 實際繳款人 & 實際繳款人法代
                    List<string> CustContGen_3_6 = new List<string>() { "3", "6" };
                    var _CustContGen_3_6 = allData.Where(x => CustContGen_3_6.Contains(x.CustContGen))
                       .OrderByDescending(x => x.Order).FirstOrDefault();
                    if (_CustContGen_3_6 != null && !allData
                            .Where(x => new List<string>() { "1", "2", "4", "5" }.Contains(x.CustContGen))
                            .Any(x => x.CustID == _CustContGen_3_6.CustID))
                        fixedData.Add(new InstantCallSubCallViewModel()
                        {
                            CustID = _CustContGen_3_6.CustID,
                            CustContGen = _NBSysTypeDatas.FirstOrDefault(z => z.Code == _CustContGen_3_6.CustContGen)?.SDesc ?? _CustContGen_3_6.CustContGen,
                            Cust_Name = _CustContGen_3_6.Cust_Name,
                            Order = _CustContGen_3_6.Order
                        });
                    #endregion
                    foreach (var item in fixedData.GroupBy(x => x.CustID))
                    {
                        var _f = item.OrderByDescending(x => x.Order).First();
                        fixedData2.Add(new InstantCallSubCallViewModel()
                        {
                            CustID = _f.CustID.AESEncrypt().Item2,
                            CustContGen = _f.CustContGen,
                            Cust_Name = _f.Cust_Name,
                            Order = _f.Order
                        });
                    }

                    model.CallData = fixedData2.OrderByDescending(x => x.Order).ToList();

                    //PIA
                    var _pia = new PIA().AddPIA(new PIA_LOG_MAIN()
                    {
                        TRACKING_TYPE = "A", //個資存取紀錄
                        ACCESS_ACCOUNT = "", //使用者帳號或員工編號
                        ACCOUNT_NAME = "", //使用者姓名或員工姓名
                        FROM_IP = new Common().GetIp(), //記載來源IP或終端機ID
                        ACCESS_DATE = dtn.Date, //記載執行日期
                        ACCESS_TIME = dtn.TimeOfDay, //記載執行時間
                        PROGFUN_NAME = $@"getCust", //執行程式名稱或交易/功能之代號/名稱
                        ACCESSOBJ_NAME = $@"tblRosterPolicy,tblRosterPolicy_DTL,tblRoster", //系統檔案名稱或存取物件對象名稱(例如TABLE / VIEW名稱）
                        EXECUTION_TYPE = "Q", //查詢
                        EXECUTION_CONTENT = $@"",//記載完整的資料存取動作，如執行所使用輸入條件、檔案存取紀錄或SQL Statement紀錄等
                        AFFECT_ROWS = model.CallData.Count, //記載執行動作之結果筆數或影響筆數
                                                            //PIA_OWNER1 = "??",
                        PIA_TYPE = "11111"
                    });
                }
            }
            catch (Exception ex)
            {
                var _msg = ex.exceptionMessage();
                NlogSet($@"即時電訪 查詢客戶資料 Error , Msg => {_msg}", null);
                NlogSet($@"即時電訪 查詢客戶資料 Error , Msg => {_msg}", null, Nlog.Error);
            }
        }

        /// <summary>
        /// 即時電訪服務 確認送出功能
        /// </summary>
        /// <param name="caseNo"></param>
        /// <param name="userId"></param>
        /// <param name="custID"></param>
        /// <returns></returns>
        public MSGReturnModel<string> InstantCall_Confirm(string caseNo, string userId, List<string> custID)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = "找不到該筆資料";
            try
            {
                List<tblRosterPolicy> tblRosterPolicys = new List<tblRosterPolicy>();
                List<tblRosterPolicy_DTL> tblRosterPolicy_DTLs = new List<tblRosterPolicy_DTL>();
                List<tblRoster> tblRosters = new List<tblRoster>();
                List<tblRosterPolicyBenfit> tblRosterPolicyBenfits = new List<tblRosterPolicyBenfit>();
                List<FPMJLSR0> FPMJLSR0s = new List<FPMJLSR0>();
                List<FNBPMLD0> FNBPMLD0s = new List<FNBPMLD0>();
                List<string> CustContGen_4_5 = new List<string>() { "4", "5" };
                DateTime dtn = DateTime.Now;

                string sql = $@"
select CaseNo,CaseSeq,InsDT,MData from CampMPool
where CaseNo = @CaseNo
order by SN desc
";
                var _CampMPool = new MSSql().Query_FirstOrDefault<CampMPool>(sql, new { CaseNo = caseNo }).Item2;
                if (_CampMPool != null)
                {
                    var _data = JsonConvert.DeserializeObject<Tel_Visit_InputData>(_CampMPool.MData);

                    List<string> _items = _data.PolicyCases.Select(x => x.PlanDesc).ToList();
                    List<int> _prem_years = _data.PolicyCases.Select(x => x.PolicyYear).ToList();

                    if (_items.Any())
                    {
                        using (EacConnection conn = new EacConnection(EasyCom.GetEasycomConn()))
                        {
                            conn.Open();
                            if (_prem_years.Any())
                            {
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
select ITEM, PREM_YEAR from FPMJLSR0
where ITEM in ( ";
                                    string c = string.Empty;
                                    int j = 0;
                                    foreach (var item in _items)
                                    {
                                        sql += $@" {c} :ITEM_{j} ";
                                        com.Parameters.Add($@"ITEM_{j}", item);
                                        c = " , ";
                                        j += 1;
                                    }
                                    sql += " ) and PREM_YEAR in ( ";
                                    c = string.Empty;
                                    j = 0;
                                    foreach (var _prem_year in _prem_years)
                                    {
                                        sql += $@" {c} :PREM_YEAR_{j} ";
                                        com.Parameters.Add($@" PREM_YEAR_{j}", _prem_year);
                                        c = " , ";
                                        j += 1;
                                    }
                                    sql += $@" ) group by ITEM, PREM_YEAR ";
                                    com.CommandText = sql;
                                    com.Prepare();
                                    DbDataReader dbresult = com.ExecuteReader();
                                    while (dbresult.Read())
                                    {
                                        var _ITEM = dbresult["ITEM"]?.ToString()?.Trim(); //險種代號
                                        int _PREM_YEAR = TypeTransfer.stringToInt(dbresult["PREM_YEAR"]?.ToString()); //繳費年期
                                        FPMJLSR0s.Add(new FPMJLSR0()
                                        {
                                            ITEM = _ITEM,
                                            PREM_YEAR = _PREM_YEAR,
                                        });
                                    }
                                    com.Dispose();
                                }
                            }
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
select  ITEM  from FNBPMLD0
where ITEM in ( ";
                                string c = string.Empty;
                                int j = 0;
                                foreach (var item in _items)
                                {
                                    sql += $@" {c} :ITEM_{j} ";
                                    com.Parameters.Add($@"ITEM_{j}", item);
                                    c = " , ";
                                    j += 1;
                                }
                                sql += $@" ) group by ITEM; ";
                                com.CommandText = sql;
                                com.Prepare();
                                DbDataReader dbresult = com.ExecuteReader();
                                while (dbresult.Read())
                                {
                                    var _ITEM = dbresult["ITEM"]?.ToString()?.Trim(); //險種代號
                                    FNBPMLD0s.Add(new FNBPMLD0()
                                    {
                                        ITEM = _ITEM
                                    });
                                }
                                com.Dispose();
                            }
                        }
                    }

                    var _CampID = "COL70"; //固定 'COL70'

                    sql = $@" 
SELECT Stype,Code,SDesc,SMemo1,SMemo2,SMemo3,DeleteFlag 
FROM [dbCTI].[dbo].[tblSysCode]  
where DeleteFlag = 'N' and stype = @stype and Code = @Code";
                    var _NBSysType = new MSSql().Query_FirstOrDefault<tblSysCode>(sql, new { stype = "sysID", Code = _data.SysID })?.Item2?.SMemo1;


                    sql = $@"select CampName  from [CampTable] where Campid = @Campid";
                    string _CampName = new MSSql().Query_FirstOrDefault<dynamic>(sql, new { Campid = _CampID })?.Item2?.CampName ?? string.Empty;

                    sql = $@"
select CustLevelCode,CustLevelName,CampID,CampCondition,CustContGen,UAmtType,CampPromo,ProductPay,PolicyCurType,YAmtType,PlanYearSuFee,PlanSuFee from CampCustLevel
where DeleteFlag = 'N'";
                    var _CampCustLevels = new MSSql().Query<CampCustLevel>(sql).Item2.ToList();

                    sql = $@"
SELECT Stype,Code,SDesc,SMemo1,SMemo2,SMemo3,DeleteFlag 
FROM [dbCTI].[dbo].[tblSysCode]  
where DeleteFlag = 'N' and stype = @stype";
                    var _CustTypes = new MSSql().Query<tblSysCode>(sql, new { stype = "Custcode" }).Item2.ToList();

                    var dicCustTypeForPay = new MSSql().Query<tblSysCode>("SELECT * FROM [dbCTI].[dbo].[tblSysCode] where Stype='保費用關係代碼' and DeleteFlag = 'N';").Item2
                        .ToDictionary(x => x.Code, y => y.SDesc);

                    int i = 0;
                    string _sAgentID = null; //業務員ID_1
                    string _sAgentName = null; //業務員姓名_1
                    string _sAgentUnitName = null; //單位代號_1
                    string _sAgentUnitNo = null; //單位名稱_1
                    string _sAgentID2 = null; //業務員ID_2
                    string _sAgentName2 = null; //業務員姓名_2
                    string _sAgentUnitName2 = null; //單位代號_2
                    string _sAgentUnitNo2 = null; //單位名稱_2
                    string agentEmail = _data.Agent.FirstOrDefault()?.AgentEmail;
                    _data.Agent.ForEach(Agent =>
                    {
                        if (_sAgentID == null)
                        {
                            _sAgentID = Agent.AgentID; //業務員ID
                            _sAgentName = Agent.AgentName; //業務員姓名
                            _sAgentUnitNo = Agent.AgentUnitCode; //單位名稱
                            _sAgentUnitName = Agent.AgentUnitName; //單位代號
                        }
                        else if (_sAgentID2 == null)
                        {
                            _sAgentID2 = Agent.AgentID; //業務員ID
                            _sAgentName2 = Agent.AgentName; //業務員姓名
                            _sAgentUnitNo2 = Agent.AgentUnitCode; //單位名稱
                            _sAgentUnitName2 = Agent.AgentUnitName; //單位代號
                        }
                    });

                    _data.PolicyCases.ForEach(_PolicyCase =>
                    {

                        var _CustID = string.Empty; //要保人客戶ID
                        var _LegalPID = string.Empty; //法代客戶ID
                        var _APPID = string.Empty; //被保人客戶ID
                        var _OriCustName = string.Empty; //要保人客戶姓名
                        var _LegalPName = string.Empty; //法代客戶姓名
                        var _APPName = string.Empty; //被保人客戶姓名
                        var _CustBirth = string.Empty; //要保人客戶生日
                        var _APPBirth = string.Empty; //被保人客戶生日
                        var _APPEmailAddr = string.Empty; //被保人Email
                        var _FADDR = string.Empty; //要保人戶籍地址
                        var _APPAddr = string.Empty; //被保人戶籍地址
                        var _FRecvAddr = string.Empty; //要保人收費地址
                        var _FMoblie = string.Empty; //要保人行動電話
                        var _APPMobile = string.Empty; //被保人行動電話
                        var _FTEL = string.Empty; //要保人戶籍電話(住家電話)
                        var _FRecvTEL = string.Empty; //要保人收費電話(公司電話)
                        var _APPTel1 = string.Empty;
                        var _APPTel2 = string.Empty;
                        _PolicyCase.Customers.ForEach(_Customer =>
                        {
                            switch (_Customer.CustContGen)
                            {
                                case "1":
                                    _CustID = _Customer.CustID;
                                    _OriCustName = _Customer.CustName;
                                    _CustBirth = _Customer.CustBirthday;
                                    _FADDR = _Customer.CustAddr;
                                    _FRecvAddr = _Customer.CustRecvAddr;
                                    _FMoblie = _Customer.CustMoblie;
                                    _FTEL = String.IsNullOrWhiteSpace(_Customer.CustTELext) ?
                                        _Customer.CustTEL :
                                        _Customer.CustTEL + "#" + _Customer.CustTELext;
                                    _FRecvTEL = String.IsNullOrWhiteSpace(_Customer.CustRecvTELext) ?
                                        _Customer.CustRecvTEL :
                                        _Customer.CustRecvTEL + "#" + _Customer.CustRecvTELext;
                                    break;
                                case "2":
                                    _APPID = _Customer.CustID;
                                    _APPName = _Customer.CustName;
                                    _APPBirth = _Customer.CustBirthday;
                                    _APPEmailAddr = _Customer.CustEmail;
                                    _APPAddr = _Customer.CustAddr;
                                    _APPMobile = _Customer.CustMoblie;
                                    break;
                                case "4":
                                case "5":
                                case "6":
                                    _LegalPID = _Customer.CustID;
                                    _LegalPName = _Customer.CustName;
                                    break;
                            }

                            if (new string[] { "2", "5" }.Contains(_Customer.CustContGen))
                            {
                                _APPTel1 = String.IsNullOrWhiteSpace(_Customer.CustTELext) ? _Customer.CustTEL : $"{_Customer.CustTEL}#{_Customer.CustTELext}";
                                _APPTel2 = String.IsNullOrWhiteSpace(_Customer.CustRecvTELext) ? _Customer.CustRecvTEL : $"{_Customer.CustRecvTEL}#{_Customer.CustRecvTELext}";
                            }
                        });

                        var custometList = new List<Customer>();
                        // 1.先取出要保人 / 被保人，若有法代則取法代
                        // 2.判斷上述兩個ID是否相同，如相同取要保人(法代)
                        var custApl = _PolicyCase.Customers
                            .Where(x => x.CustContGen == "1" || x.CustContGen == "4")
                            .OrderByDescending(x => x.CustContGen)
                            .FirstOrDefault();

                        if (custApl != null)
                        {
                            custometList.Add(custApl);
                        }

                        var custIns = _PolicyCase.Customers
                            .Where(x => x.CustContGen == "2" || x.CustContGen == "5")
                            .OrderByDescending(x => x.CustContGen)
                            .FirstOrDefault();

                        if (custIns != null && !custometList.Any(x => x.CustID == custIns.CustID))
                        {
                            custometList.Add(custIns);
                        }

                        // 3.實際繳款人(法代)如符合要 / 被保人及法代ID，則不顯示，反之顯示。
                        var custPay = _PolicyCase.Customers
                            .Where(x => x.CustContGen == "3" || x.CustContGen == "6")
                            .OrderByDescending(x => x.CustContGen)
                            .FirstOrDefault();

                        if (custPay != null && !_PolicyCase.Customers.Any(x => Array.IndexOf(new string[] { "1", "2", "4", "5" }, x.CustContGen) > 0 && x.CustID == custPay.CustID))
                        {
                            custometList.Add(custPay);
                        }

                        // 4.移除頁面未勾選ID
                        custometList.Where(m => custID.Contains(m.CustID)).ToList().ForEach(customer =>
                        {
                            i += 1;
                            var p = new DynamicParameters();
                            p.Add("@lsUser", userId);
                            p.Add("@lsSeqNo", null, dbType: DbType.String, direction: ParameterDirection.Output, 15);
                            new MSSql().Execute("spGetMPoolSEQ", p, null, CommandType.StoredProcedure);
                            Int64 _lsSeqNo = 0L; //spGetMPoolSEQ(storePercedure計數器每日reset)
                            Int64.TryParse($@"{dtn.ToString("yyMMdd")}999{p.Get<string>("@lsSeqNo")}", out _lsSeqNo);
                            var _UploadDate = TypeTransfer.stringToDateTime(_data.UploadDate);
                            var _MemberID = "39I";
                            var custType = (_CustTypes.FirstOrDefault(x => x.Code == customer.CustContGen)?.SMemo1).stringSetRequired();

                            tblRosterPolicys.Add(new tblRosterPolicy()
                            {
                                ListNo = _lsSeqNo,
                                CampID = _CampID.stringSetRequired(),
                                MemberID = _MemberID.stringSetRequired(),
                                ListSource = "mApp",
                                AssignSchDate = dtn,
                                RptReceNo = _data.CaseNo.stringSetRequired(), //要保書編號
                                CustLevel = get_CustLevel(
                                    _CampCustLevels,
                                    _PolicyCase.CampCondition, //電訪條件
                                    customer.CustContGen, //受訪對象
                                    _PolicyCase.UAmtType == "U" ? "Y" : "N", //投資型/非投資型 If UAmtType = U 為 Y(= True) else N(= False)
                                    "N", //mApp通路都非電銷件 固定 N
                                    _PolicyCase.ProductPay == "D" ? "Y" : "N", //繳別(躉繳:D) if ProductPay = D躉繳 then Y else N
                                    _PolicyCase.PolicyCurType == "NTD" ? "Y" : "N", //幣別(NTD) if PolicyCurType = NTD then Y else N
                                    _PolicyCase.YAmtType == "Y" ? "Y" : "N", //年金/非年金[危險保費] IF YAmtType = Y then Y else N
                                    get_PlanYearSuFee(FPMJLSR0s, _PolicyCase.PlanDesc, _PolicyCase.PolicyYear), //解約費用
                                    get_PlanSuFee(FNBPMLD0s, _PolicyCase.PlanDesc) //保費費用
                                ), //取客戶群組中文名稱
                                PolicyNo = _data.CaseNo, //要保書編號
                                IDDup = "",//(i - 1 == 0) ? "" : (i - 1).ToString(),
                                PolicySeq = i.ToString().PadLeft(2, '0'),
                                CreateDate = _UploadDate, //電訪抽件日
                                NBSysType = _NBSysType.stringSetRequired(), //系統別
                                CONTEXT_C = _PolicyCase.Context_C.stringSetRequired(), //商品名稱
                                PlanDesc = _PolicyCase.PlanDesc.stringSetRequired(), //險種
                                PolicyYear = _PolicyCase.PolicyYear.ToString(), //繳費年期
                                PolicyCurType = _PolicyCase.PolicyCurType.stringSetRequired(), //幣別
                                PayType = _PolicyCase.ProductPay, //繳別
                                PolicyAmt = _PolicyCase.PolicyAmt.stringSetRequired(), //總保額
                                PolicyCost = _PolicyCase.PolicyCost.stringSetRequired(), //應繳保費
                                PayMethodN_Org = _PolicyCase.PayMethod.stringSetRequired(), //繳款方式
                                SubmitDate = _PolicyCase.SubmitDate.stringSetRequired(), //契約始期
                                ContactSF = _PolicyCase.ContactMemo.stringSetRequired(), //電訪特殊註記
                                                                                         //CustID = _Customer.CustContGen == "1" ? _Customer.CustID : "", //要保人客戶ID
                                                                                         //LegalPID = CustContGen_4_5.Contains(_Customer.CustContGen) ? _Customer.CustID : "", //客戶ID
                                                                                         //APPID = _Customer.CustContGen == "2" ? _Customer.CustID : "", //被保人客戶ID
                                                                                         //OriCustName = _Customer.CustContGen == "1" ? _Customer.CustName : "", //客戶姓名
                                                                                         //LegalPName = CustContGen_4_5.Contains(_Customer.CustContGen) ? _Customer.CustName : "", //客戶姓名
                                                                                         //APPName = _Customer.CustContGen == "2" ? _Customer.CustName : "", //客戶姓名
                                                                                         //CustBirth = _Customer.CustContGen == "1" ? _Customer.CustBirthday : "", //客戶生日
                                                                                         //APPBirth = _Customer.CustContGen == "2" ? _Customer.CustBirthday : "", //客戶生日
                                                                                         //APPEmailAddr = _Customer.CustContGen == "2" ? _Customer.CustEmail : "", //Email
                                                                                         //FADDR = _Customer.CustContGen == "1" ? _Customer.CustAddr : "", //戶籍地址
                                                                                         //APPAddr = _Customer.CustContGen == "2" ? _Customer.CustAddr : "", //戶籍地址
                                                                                         //FRecvAddr = _Customer.CustContGen == "1" ? _Customer.CustRecvAddr : "", //收費地址
                                                                                         //FMoblie = _Customer.CustContGen == "1" ? _Customer.CustMoblie : "", //行動電話
                                                                                         //APPMobile = _Customer.CustContGen == "2" ? _Customer.CustMoblie : "", //行動電話
                                                                                         //FTEL = _Customer.CustContGen == "1" ? _Customer.CustTEL : "", //戶籍電話(住家電話)
                                                                                         //FRecvTEL = _Customer.CustContGen == "1" ? _Customer.CustRecvTEL : "", //收費電話(公司電話)
                                CustID = _CustID.stringSetRequired(), //要保人客戶ID
                                LegalPID = _LegalPID.stringSetRequired(), //法代客戶ID
                                APPID = _APPID.stringSetRequired(), //被保人客戶ID
                                OriCustName = _OriCustName.stringSetRequired(), //要保人客戶姓名
                                LegalPName = _LegalPName.stringSetRequired(), //法代客戶姓名
                                APPName = _APPName.stringSetRequired(), //被保人客戶姓名
                                CustBirth = _CustBirth.stringSetRequired(), //要保人客戶生日
                                APPBirth = _APPBirth.stringSetRequired(), //被保人客戶生日
                                APPEmailAddr = _APPEmailAddr.stringSetRequired(), //被保人Email
                                FADDR = _FADDR.stringSetRequired(), //要保人戶籍地址
                                APPAddr = _APPAddr, //被保人戶籍地址
                                FRecvAddr = _FRecvAddr, //要保人收費地址
                                FMoblie = _FMoblie, //要保人行動電話
                                APPMobile = _APPMobile, //被保人行動電話
                                FTEL = _FTEL, //要保人戶籍電話(住家電話)
                                FRecvTEL = _FRecvTEL, //要保人收費電話(公司電話)
                                PayRelation = customer.CustMenuValue.stringSetRequired(), //受訪者與要被保人關係
                                VisitTime = $@"{customer.CustVisitTimeS.ToString().PadLeft(2, '0')}-{customer.CustVisitTimeE.ToString().PadLeft(2, '0')}",
                                sAgentID = _sAgentID.stringSetRequired(), //業務員ID_1
                                sAgentName = _sAgentName.stringSetRequired(), //業務員姓名_1
                                sAgentUnitNo = _sAgentUnitNo.stringSetRequired(), //單位名稱_1
                                sAgentUnitName = _sAgentUnitName.stringSetRequired(), //單位代號_1
                                sAgentID2 = _sAgentID2.stringSetRequired(), //業務員ID_2
                                sAgentName2 = _sAgentName2.stringSetRequired(), //業務員姓名_2
                                sAgentUnitNo2 = _sAgentUnitNo2.stringSetRequired(), //單位名稱_2
                                sAgentUnitName2 = _sAgentUnitName2.stringSetRequired(), //單位代號_2
                                sAgentEmail = agentEmail,
                                TrustInfo = "受益人清單",
                                APPTel1 = _APPTel1,
                                APPTel2 = _APPTel2,
                                LegalPRel = new string[] { "4", "5", "6" }.Contains(customer.CustContGen) ?
                                    dicCustTypeForPay.ContainsKey(customer.CustMenuValue) ?
                                        dicCustTypeForPay[customer.CustMenuValue] :
                                        String.Empty :
                                    String.Empty,
                                sAgentPhone = String.Empty
                            });
                            tblRosterPolicy_DTLs.Add(new tblRosterPolicy_DTL()
                            {
                                ListNo = _lsSeqNo,
                                campid = _CampID,
                                MemberID = _MemberID.stringSetRequired(),
                                RptReceNo = _data.CaseNo.stringSetRequired(), //要保書編號
                                Appointment = "0", //'0' : 即時電訪,  '1' : 預約電訪
                                PolicyNo = _data.CaseNo.stringSetRequired(), //要保書編號
                                IDDup = "",//(i - 1 == 0) ? "" : (i - 1).ToString(),
                                PolicySeq = i.ToString().PadLeft(2, '0'),
                                CreateTime = _UploadDate, //電訪抽件日
                                PayAmountSourceQ = _PolicyCase.PayAmountSourceQ, //保費來源題目
                                PayAmountSourceA = _PolicyCase.PayAmountSourceA, //保費來源回應
                                InsurancePolicyQ = _PolicyCase.InsurancePolicyQ, //投保目的題目
                                InsurancePolicyA = _PolicyCase.InsurancePolicyA, //投保目的回應
                                CustEmailAddr = customer.CustContGen == "1" ? customer.CustEmail : "", //Email
                                PAYER_LR_Id = customer.CustContGen == "6" ? customer.CustID : String.Empty,
                                PAYER_LR_Name = customer.CustContGen == "6" ? customer.CustName : String.Empty,
                                PAYER_Moblie = customer.CustContGen == "3" ? customer.CustMoblie : String.Empty,
                                PAYER_TelO = customer.CustContGen == "3" ?
                                    String.IsNullOrWhiteSpace(customer.CustRecvTELext) ?
                                        customer.CustRecvTEL :
                                        $"{customer.CustRecvTEL}#{customer.CustRecvTELext}" :
                                    String.Empty,
                                PAYER_Relate = new string[] { "3", "6" }.Contains(customer.CustContGen) ?
                                    dicCustTypeForPay.ContainsKey(customer.CustMenuValue) ?
                                        dicCustTypeForPay[customer.CustMenuValue] :
                                        String.Empty :
                                    String.Empty
                            });
                            tblRosters.Add(new tblRoster()
                            {
                                ListNo = _lsSeqNo,
                                CampID = _CampID.stringSetRequired(),
                                CampName = _CampName,
                                CreateDate = _UploadDate, //電訪抽件日
                                CustID = customer.CustID.stringSetRequired(), //客戶ID
                                CustName = customer.CustName.stringSetRequired(), //客戶姓名
                                Gender = customer.CustGen.stringSetRequired(), //客戶姓別
                                CustBirth = customer.CustBirthday.stringSetRequired(), //客戶生日
                                EmailAddr = customer.CustEmail.stringSetRequired(), //Email
                                ContactAddr = customer.CustAddr, //戶籍地址
                                TelM_Aft = customer.CustMoblie, //行動電話
                                                                //TelH_Aft = String.IsNullOrWhiteSpace(_Customer.CustTELext) ?
                                                                //     _Customer.CustTEL :
                                                                //     _Customer.CustTEL + "#" + _Customer.CustTELext, //戶籍電話(住家電話)+分機
                                TelH_Aft = customer.CustTEL,
                                TelH_Ext = customer.CustTELext,
                                TelO_Aft = customer.CustRecvTEL, //收費電話(公司電話)
                                TelO_Ext = customer.CustRecvTELext,
                                CustType = custType, //角色代碼
                                CustGroup = custType,
                                CampType = "R",
                                AssignSchDate = dtn
                            });
                            _PolicyCase.PolicyBenfit?.ForEach(_tblRosterPolicyBenfit =>
                            {
                                tblRosterPolicyBenfits.Add(new tblRosterPolicyBenfit()
                                {
                                    ListNo = _lsSeqNo,
                                    CampID = "COL70",
                                    PolicyNo = _data.CaseNo.stringSetRequired(),
                                    IDDup = "",//(i - 1 == 0) ? "" : (i - 1).ToString(),
                                    PolicySeq = i.ToString().PadLeft(2, '0'),
                                    MemberID = _MemberID.stringSetRequired(),
                                    RptReceNo = _data.CaseNo.stringSetRequired(),
                                    TrustSeq = _tblRosterPolicyBenfit.TrustSeq.stringSetRequired(),
                                    TrustName = _tblRosterPolicyBenfit.TrustName.stringSetRequired(),
                                    TrustRate = _tblRosterPolicyBenfit.TrustRate.stringSetRequired(),
                                    TrustRelate = _tblRosterPolicyBenfit.TrustRelate.stringSetRequired(),
                                    TrustType = _tblRosterPolicyBenfit.TrustType.stringSetRequired()
                                });
                            });
                        });
                    });

                    if (tblRosterPolicys.Any())
                    {
                        List<string> _ValidationMsg = new List<string>();
                        var _tblRosterPolicy_pros = new tblRosterPolicy().GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                        var _tblRosterPolicy_DTL_pros = new tblRosterPolicy_DTL().GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                        var _tblRoster_pros = new tblRoster().GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                        var _tblRosterPolicyBenfit_pros = new tblRosterPolicyBenfit().GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                        foreach (var _tblRosterPolicy in tblRosterPolicys)
                        {
                            var _tblRosterPolicy_context = new ValidationContext(_tblRosterPolicy, null, null);
                            var _tblRosterPolicy_ValidationResult = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(_tblRosterPolicy, _tblRosterPolicy_context, _tblRosterPolicy_ValidationResult, true))
                            {
                                _tblRosterPolicy_ValidationResult.ForEach(x =>
                                {
                                    var m = x.MemberNames.FirstOrDefault()?.ToString();
                                    var p = _tblRosterPolicy_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                    var val = (p == null) ? null : (p.GetValue(_tblRosterPolicy))?.ToString();
                                    if (!val.IsNullOrWhiteSpace())
                                        _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                    else
                                        _ValidationMsg.Add(x.ErrorMessage);
                                });
                            }
                        }
                        foreach (var _tblRosterPolicy_DTL in tblRosterPolicy_DTLs)
                        {
                            var _tblRosterPolicy_DTL_context = new ValidationContext(_tblRosterPolicy_DTL, null, null);
                            var _tblRosterPolicy_DTL_ValidationResult = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(_tblRosterPolicy_DTL, _tblRosterPolicy_DTL_context, _tblRosterPolicy_DTL_ValidationResult, true))
                            {
                                _tblRosterPolicy_DTL_ValidationResult.ForEach(x =>
                                {
                                    var m = x.MemberNames.FirstOrDefault()?.ToString();
                                    var p = _tblRosterPolicy_DTL_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                    var val = (p == null) ? null : (p.GetValue(_tblRosterPolicy_DTL))?.ToString();
                                    if (!val.IsNullOrWhiteSpace())
                                        _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                    else
                                        _ValidationMsg.Add(x.ErrorMessage);
                                });
                            }
                        }
                        foreach (var _tblRoster in tblRosters)
                        {
                            var _tblRoster_context = new ValidationContext(_tblRoster, null, null);
                            var _tblRoster_ValidationResult = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(_tblRoster, _tblRoster_context, _tblRoster_ValidationResult, true))
                            {
                                _tblRoster_ValidationResult.ForEach(x =>
                                {
                                    var m = x.MemberNames.FirstOrDefault()?.ToString();
                                    var p = _tblRoster_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                    var val = (p == null) ? null : (p.GetValue(_tblRoster))?.ToString();
                                    if (!val.IsNullOrWhiteSpace())
                                        _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                    else
                                        _ValidationMsg.Add(x.ErrorMessage);
                                });
                            }
                        }
                        foreach (var _tblRosterPolicyBenfit in tblRosterPolicyBenfits)
                        {
                            var _tblRosterPolicyBenfit_context = new ValidationContext(_tblRosterPolicyBenfit, null, null);
                            var _tblRosterPolicyBenfit_ValidationResult = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(_tblRosterPolicyBenfit, _tblRosterPolicyBenfit_context, _tblRosterPolicyBenfit_ValidationResult, true))
                            {
                                _tblRosterPolicyBenfit_ValidationResult.ForEach(x =>
                                {
                                    var m = x.MemberNames.FirstOrDefault()?.ToString();
                                    var p = _tblRosterPolicyBenfit_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                    var val = (p == null) ? null : (p.GetValue(_tblRosterPolicyBenfit))?.ToString();
                                    if (!val.IsNullOrWhiteSpace())
                                        _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                    else
                                        _ValidationMsg.Add(x.ErrorMessage);
                                });
                            }
                        }
                        if (_ValidationMsg.Any())
                        {
                            result.DESCRIPTION = $@"資料檢核失敗 => {string.Join(",", _ValidationMsg)}";
                            return result;
                        }

                        using (SqlConnection conn = new SqlConnection(
                            System.Configuration.ConfigurationManager.
                        ConnectionStrings["DefaultConnection"].ConnectionString))
                        {
                            conn.Open();
                            using (SqlTransaction tran = conn.BeginTransaction())
                            {
                                var _insert_tblRosterPolicyBenfit_c = new MSSql().Execute(insert_tblRosterPolicyBenfit(), tblRosterPolicyBenfits, null, null, tran, conn, false).Item2;
                                var _insert_tblRosterPolicy_c = new MSSql().Execute(insert_tblRosterPolicy(), tblRosterPolicys, null, null, tran, conn, false).Item2;
                                var _insert_tblRosterPolicy_DTL_c = new MSSql().Execute(insert_tblRosterPolicy_DTL(), tblRosterPolicy_DTLs, null, null, tran, conn, false).Item2;
                                var _insert_tblRoster_c = new MSSql().Execute(insert_tblRoster(), tblRosters, null, null, tran, conn).Item2;
                                if (_insert_tblRosterPolicy_c == tblRosterPolicys.Count &&
                                    _insert_tblRosterPolicy_DTL_c == tblRosterPolicy_DTLs.Count &&
                                    _insert_tblRoster_c == tblRosters.Count
                                     && _insert_tblRosterPolicyBenfit_c == tblRosterPolicyBenfits.Count
                                    )
                                {
                                    tran.Commit();
                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = "執行成功";

                                    using (EacConnection EacConn = new EacConnection(EasyCom.GetEasycomConn()))
                                    {
                                        EacConn.Open();
                                        using (EacCommand com = new EacCommand(EacConn))
                                        {
                                            com.CommandType = CommandType.StoredProcedure;
                                            com.CommandText = "*PGM/SNBTLRP";
                                            com.Parameters.Clear();
                                            //傳送參數
                                            var _parm1 = _data.CaseNo; //要保書編號 X(10)
                                            var _parm2 = "";  //保單號碼 X(10) => 20
                                            var _parm3 = "";  //保單序號 9(02) => 22
                                            var _parm4 = "";  //身分證重複別 X(01) => 23
                                            var _parm5 = "A";  //預約電訪狀態 X(01) => 24
                                            var _parm6 = "BNB39I0";  //來源程式 X(10) => 34
                                            var _parm7 = string.Join("|", tblRosters.Where(x => !x.CustType.IsNullOrWhiteSpace())
                                                .Select(x => x.CustType).Distinct());  //電訪對象 X(50) => 84
                                            var _parm8 = string.Join("|", tblRosters.Where(x => !x.CustID.IsNullOrWhiteSpace())
                                                .Select(x => x.CustID).Distinct());  //電訪對象ID X(100) => 184
                                            var _parm9 = tblRosters.Where(x => !x.CustName.IsNullOrWhiteSpace()) //電訪對象姓名 X(150)
                                                .Select(x => x.CustName)
                                                .Distinct()
                                                .Aggregate("", (names, name) => names + (name.Length > 5 ? name.Substring(0, 5) : name.PadRight(5, '　')))
                                                .PadRight(74, '　')
                                                .ToWide();
                                            var _parm10 = "39I"; //X(03) => 337
                                            var _parm11 = ""; //X(25) => 362
                                            var _parm12 = "mApp"; //X(10) => 372
                                            var _parm13 = (dtn.Year * 10000 + dtn.Month * 100 + dtn.Day).ToString(); //預計電訪日期 9(08) => 380
                                            var _parm14 = dtn.ToString("hhmmssff"); //預計電訪時間 X(08) => 388
                                            var _parm15 = "".PadRight(1024, ' '); //補充內容 X(1024) => 1412
                                                                                  //接收參數
                                            var _parm16 = ""; //回傳內容前置符號 X(02) => 1414  ,  接收應為 '||'
                                            var _parm17 = ""; //處理回傳 X(02) => 1416
                                            var _parm18 = ""; //回傳補充內容 X(1024) => 2440

                                            NlogSet($@"即時電訪服務_確認送出功能 AS400 參數 , IAPPLY-NO(要保書編號):{_parm1} , OSURY-STAT(預約電訪狀態):{_parm5} , SRC-PGM(來源程式):{_parm6} , CUSTTYPE(電訪對象):{_parm7} , OSURY-ID(電訪對象ID):{_parm8} , OSURY-NAME(電訪對象姓名):{_parm9} , OSURY-DATE(預計電訪日期):{_parm13} , OSURY-TIME(預計電訪時間):{_parm14} ", null);

                                            var _parms = $@"{_parm1.PadRight(10, ' ')}{_parm2.PadRight(10, ' ')}{_parm3.PadLeft(2, '0')}{_parm4.PadRight(1, ' ')}{_parm5.PadRight(1, ' ')}{_parm6.PadRight(10, ' ')}{_parm7.PadRight(50, ' ')}{_parm8.PadRight(100, ' ')}{_parm9}{_parm10.PadRight(3, ' ')}{_parm11.PadRight(25, ' ')}{_parm12.PadRight(10, ' ')}{_parm13.PadLeft(8, '0')}{_parm14.PadRight(8, ' ')}{_parm15}{_parm16.PadRight(2, ' ')}{_parm17.PadRight(2, ' ')}{_parm18.PadRight(1024, ' ')}";

                                            NlogSet($@"即時電訪服務_確認送出功能 AS400 參數 , LINK-AREA:{_parms} ", null);

                                            EacParameter EP1 = new EacParameter(); //LINK-AREA
                                            EP1.ParameterName = "LINK-AREA";
                                            EP1.DbType = DbType.String;
                                            EP1.Size = 2438;
                                            EP1.Direction = ParameterDirection.InputOutput;
                                            EP1.Value = _parms;

                                            com.Parameters.Add(EP1);

                                            try
                                            {
                                                com.Prepare();
                                                com.ExecuteNonQuery();
                                                var _r = EP1.Value?.ToString() ?? string.Empty;
                                                int _split = _r.IndexOf("||");
                                                var _RTN_CODE = string.Empty;
                                                var _OREMARK = string.Empty;
                                                if (_split > -1)
                                                {
                                                    _split += 2;
                                                    _RTN_CODE = _r.Length >= _split + 2 ? _r.Substring(_split, 2) : string.Empty;
                                                    _OREMARK = _r.Length >= _split + 3 ? _r.Substring(_split + 2, _r.Length - (_split + 2)) : string.Empty;
                                                }
                                                NlogSet($@"即時電訪服務_確認送出功能 AS400 參數 , RTN_CODE(處理回傳):{_RTN_CODE} , OREMARK(補充內容):{_OREMARK} ", null);
                                            }
                                            catch (Exception ex)
                                            {
                                                var _ex = ex.exceptionMessage();
                                                NlogSet($@"即時電訪服務_確認送出功能 AS400 Error , Msg => {_ex}", null);
                                                NlogSet($@"即時電訪服務_確認送出功能 AS400 Error , Msg => {_ex}", null, Nlog.Error);
                                            }
                                            com.Dispose();
                                        }
                                    }

                                    var _pia = new PIA().AddPIA(new PIA_LOG_MAIN()
                                    {
                                        TRACKING_TYPE = "A", //個資存取紀錄
                                        ACCESS_ACCOUNT = "", //使用者帳號或員工編號
                                        ACCOUNT_NAME = "", //使用者姓名或員工姓名
                                        FROM_IP = new Common().GetIp(), //記載來源IP或終端機ID
                                        ACCESS_DATE = dtn.Date, //記載執行日期
                                        ACCESS_TIME = dtn.TimeOfDay, //記載執行時間
                                        PROGFUN_NAME = $@"InstantCall_Confirm", //執行程式名稱或交易/功能之代號/名稱
                                        ACCESSOBJ_NAME = $@"tblRosterPolicy,tblRosterPolicy_DTL,tblRoster", //系統檔案名稱或存取物件對象名稱(例如TABLE / VIEW名稱）
                                        EXECUTION_TYPE = "A", //新增
                                        EXECUTION_CONTENT = $@"",//記載完整的資料存取動作，如執行所使用輸入條件、檔案存取紀錄或SQL Statement紀錄等
                                        AFFECT_ROWS = (_insert_tblRosterPolicy_c + _insert_tblRosterPolicy_DTL_c + _insert_tblRoster_c), //記載執行動作之結果筆數或影響筆數
                                                                                                                                         //PIA_OWNER1 = "??",
                                        PIA_TYPE = "11111"
                                    });
                                }
                                else
                                {
                                    var _msg = $@"
tblRosterPolicy 應新增筆數 : {tblRosterPolicys.Count} , 實際新增筆數 : {_insert_tblRosterPolicy_c} ;
tblRosterPolicy_DTL 應新增筆數 : {tblRosterPolicy_DTLs.Count} , 實際新增筆數 : {_insert_tblRosterPolicy_DTL_c} ;
tblRoster 應新增筆數 : {tblRosters.Count} , 實際新增筆數 : {_insert_tblRoster_c} ;
tblRosterPolicyBenfits 應新增筆數 : {tblRosterPolicyBenfits.Count} , 實際新增筆數 : {_insert_tblRosterPolicyBenfit_c} ;
";
                                    //                                    var _msg = $@"
                                    //tblRosterPolicy 應新增筆數 : {tblRosterPolicys.Count} , 實際新增筆數 : {_insert_tblRosterPolicy_c} ;
                                    //tblRosterPolicy_DTL 應新增筆數 : {tblRosterPolicy_DTLs.Count} , 實際新增筆數 : {_insert_tblRosterPolicy_DTL_c} ;
                                    //tblRoster 應新增筆數 : {tblRosters.Count} , 實際新增筆數 : {_insert_tblRoster_c} ;
                                    //";
                                    NlogSet($@"即時電訪服務_確認送出功能 Error , Msg => {_msg}", null);
                                    NlogSet($@"即時電訪服務_確認送出功能 Error , Msg => {_msg}", null, Nlog.Error);
                                    tran.Rollback();
                                    result.DESCRIPTION = _msg;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                NlogSet($@"即時電訪服務_確認送出功能 Error , Msg => {ex}", null);
                NlogSet($@"即時電訪服務_確認送出功能 Error , Msg => {ex}", null, Nlog.Error);
                result.DESCRIPTION = "很抱歉!目前系統發生錯誤，暫時無法申請即時電訪<br>將等待案件受理後，另行安排電訪";
            }
            return result;
        }

        /// <summary>
        /// 判斷 解約費用
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="Item"></param>
        /// <param name="Prem_Year"></param>
        /// <returns></returns>
        private static string get_PlanYearSuFee(List<FPMJLSR0> datas, string Item, int Prem_Year)
        {
            return datas.Any(x => x.ITEM == Item && x.PREM_YEAR == Prem_Year) ? "Y" : "N";
        }

        /// <summary>
        /// 判斷 保費費用 
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        private static string get_PlanSuFee(List<FNBPMLD0> datas, string Item)
        {
            return datas.Any(x => x.ITEM == Item) ? "Y" : "N";
        }

        /// <summary>
        /// 取客戶群組中文名稱
        /// </summary>
        /// <param name="datas">CampCustLevel</param>
        /// <param name="CampCondition">電訪條件</param>
        /// <param name="CustContGen">受訪對象</param>
        /// <param name="UAmtType">是否投資型</param>
        /// <param name="CampPromo">是否為電銷件</param>
        /// <param name="ProductPay">繳別(躉繳:D)</param>
        /// <param name="PolicyCurType">幣別(NTD)</param>
        /// <param name="YAmtType">年金/非年金[危險保費]</param>
        /// <param name="PlanYearSuFee">解約費用</param>
        /// <param name="PlanSuFee">保費費用</param>
        /// <returns></returns>
        private static string get_CustLevel(
            List<CampCustLevel> datas,
            string CampCondition,
            string CustContGen,
            string UAmtType,
            string CampPromo,
            string ProductPay,
            string PolicyCurType,
            string YAmtType,
            string PlanYearSuFee,
            string PlanSuFee
            )
        {
            return datas.Where(x =>
            x.CampCondition == CampCondition &&
            x.CustContGen == CustContGen &&
            x.UAmtType == UAmtType &&
            (x.CampPromo.IsNullOrWhiteSpace() || x.CampPromo == CampPromo) &&
            (x.ProductPay.IsNullOrWhiteSpace() || x.ProductPay == ProductPay) &&
            (x.PolicyCurType.IsNullOrWhiteSpace() || x.PolicyCurType == PolicyCurType) &&
            (x.YAmtType.IsNullOrWhiteSpace() || x.YAmtType == YAmtType) &&
            (x.PlanYearSuFee.IsNullOrWhiteSpace() || x.PlanYearSuFee == PlanYearSuFee) &&
            (x.PlanSuFee.IsNullOrWhiteSpace() || x.PlanSuFee == PlanSuFee))
                .OrderByDescending(x => x.CustLevelCode)
                .Select(x => x.CustLevelName)
                .FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// insert into tblRosterPolicy sql
        /// </summary>
        /// <returns></returns>
        private static string insert_tblRosterPolicy()
        {
            return $@"
INSERT INTO [dbo].[tblRosterPolicy]
           ([ListNo]
           ,[CampID]
           ,[ListSource]
           ,[PolicyNo]
           ,[IDDup]
           ,[PolicySeq]
           ,[MemberID]
           ,[RptReceNo]
           ,[CustLevel]
           ,[PolicyGoods]
           ,[CustID]
           ,[APPID]
           ,[APPName]
           ,[APPBirth]
           ,[AssignSchDate]
           ,[VisitDate]
           ,[VisitTimeScale]
           ,[VisitTime]
           ,[RecvZip]
           ,[RecvAddr]
           ,[RecvTEL]
           ,[ApplyDate]
           ,[ChangeCloseDate]
           ,[FileNO]
           ,[ChangeItem]
           ,[sAgentID]
           ,[sAgentName]
           ,[sAgentPhone]
           ,[sAgentMobile]
           ,[sAgentEmail]
           ,[sAgentUnitName]
           ,[sAgentUnitNo]
           ,[AccAgent]
           ,[ProceUnitNo]
           ,[ProceID]
           ,[ProceName]
           ,[CCustID]
           ,[CCustName]
           ,[CZipCode]
           ,[CADDR]
           ,[CTEL]
           ,[AcceptCNTime]
           ,[AcceptCNTEL]
           ,[ContactMemo]
           ,[AreaCode]
           ,[CardType]
           ,[CustAddr]
           ,[Payment]
           ,[DCRNO]
           ,[CTNO]
           ,[BookValue]
           ,[SharesValue]
           ,[ChkNO]
           ,[ChkAccount]
           ,[PlanDesc]
           ,[AssetsNM]
           ,[PayItem]
           ,[OTel]
           ,[VerifyAgent]
           ,[CNTFaxNO]
           ,[CNTOP]
           ,[CNTDP]
           ,[CNTFL]
           ,[RiskControlFactor]
           ,[SubmitDate]
           ,[PolicyNum]
           ,[PolicyYear]
           ,[PolicyAmt]
           ,[SourceCamp]
           ,[PolicyCurType]
           ,[PayType]
           ,[PayMethod]
           ,[PolicyCost]
           ,[AmtOfSigned]
           ,[AmtOfAppro]
           ,[AmtOfNTChk]
           ,[AmtOfAct]
           ,[AmtOfEQNT]
           ,[CntOfSelf]
           ,[AmtOfKind]
           ,[Trf_File]
           ,[CreateDate]
           ,[BringDate]
           ,[BatchNo]
           ,[AssignDate]
           ,[AgentID]
           ,[Dept_ID]
           ,[Dept_Name]
           ,[LAgentID]
           ,[FContactDate]
           ,[LContactDate]
           ,[LTermCode]
           ,[LCloseCode]
           ,[LCloseDate]
           ,[SYSID]
           ,[LAGTermCode]
           ,[LAGContactDate]
           ,[LAGFollowDate]
           ,[AGID]
           ,[AGName]
           ,[LTarget]
           ,[LAGContent]
           ,[NullifyReason]
           ,[ProcDay]
           ,[UpdID]
           ,[UpdDT]
           ,[CMoblie]
           ,[FZipCode]
           ,[FADDR]
           ,[FTEL]
           ,[FMoblie]
           ,[FRecvZip]
           ,[FRecvAddr]
           ,[FRecvTEL]
           ,[RiskDispPhone1]
           ,[RiskDispPPNo1]
           ,[RiskDispPPSO1]
           ,[RiskDispPIDDUP1]
           ,[RiskDispPPSREC1]
           ,[RiskDispMob1]
           ,[RiskDispMPNo1]
           ,[RiskDispMPSO1]
           ,[RiskDispMIDDUP1]
           ,[RiskDispMPSREC1]
           ,[RiskDispPhone2]
           ,[RiskDispPPNo2]
           ,[RiskDispPPSO2]
           ,[RiskDispPIDDUP2]
           ,[RiskDispPPSREC2]
           ,[RiskDispMob2]
           ,[RiskDispMPNo2]
           ,[RiskDispMPSO2]
           ,[RiskDispMIDDUP2]
           ,[RiskDispMPSREC2]
           ,[RiskDispPhone3]
           ,[RiskDispPPNo3]
           ,[RiskDispPPSO3]
           ,[RiskDispPIDDUP3]
           ,[RiskDispPPSREC3]
           ,[RiskDispMob3]
           ,[RiskDispMPNo3]
           ,[RiskDispMPSO3]
           ,[RiskDispMIDDUP3]
           ,[RiskDispMPSREC3]
           ,[sAgentID2]
           ,[sAgentName2]
           ,[PolicySdType]
           ,[BnkCampCond]
           ,[SecretGrp]
           ,[CONTEXT_C]
           ,[ChgDateReason]
           ,[OldAssignSchDate]
           ,[LegalPID]
           ,[LegalPName]
           ,[LegalPRel]
           ,[sAgentUnitNo2]
           ,[sAgentUnitName2]
           ,[ContactSF]
           ,[SourceType]
           ,[PayType_Aft]
           ,[PayTypeN_Org]
           ,[PayTypeN_Aft]
           ,[PayMethod_Aft]
           ,[PayMethodN_Org]
           ,[PayMethodN_Aft]
           ,[ChangeItem_rest]
           ,[AppGen]
           ,[CustBirth]
           ,[CustBirth_Aft]
           ,[FRecvTEL2]
           ,[RecvTEL2]
           ,[FTEL2]
           ,[CTEL2]
           ,[AutoBatchNo]
           ,[TotallPolicyCost]
           ,[TrustInfo]
           ,[PrePolicyCost]
           ,[PolicyCostDate]
           ,[AuthFlag]
           ,[OCustFingPrt]
           ,[NCustFingPrt]
           ,[APPFingPrt]
           ,[CustSERR]
           ,[APPSERR]
           ,[APPZipCode]
           ,[APPAddr]
           ,[APPTel1]
           ,[APPTel2]
           ,[APPMobile]
           ,[APPEmailAddr]
           ,[NAPPZipCode]
           ,[NAPPAddr]
           ,[NAPPTel1]
           ,[NAPPTel2]
           ,[NAPPMobile]
           ,[NAPPEmailAddr]
           ,[OriCustName]
           ,[NTrustInfo]
           ,[LawRefer]
           ,[TAEffet_Date]
           ,[TAEffet_Time]
           ,[TAEnd_Date]
           ,[TATravelDays]
           ,[TATravelPlac1]
           ,[TATravelPlac2]
           ,[TATravelPlac3]
           ,[TAAddUnit]
           ,[TAMrUnit]
           ,[TAOhsUnit]
           ,[TAOhs1Unit]
           ,[TAAddPrem]
           ,[TAMrPrem]
           ,[TAOhsPrem]
           ,[TAOhs1Prem]
           ,[TABankCode]
           ,[TACardNo]
           ,[TACardEndYM]
           ,[CPlanDesc]
           ,[CPolicyYear]
           ,[CCONTEXT_C]
           ,[CAPPID]
           ,[CAPPNAME]
           ,[CAPPBirth]
           ,[EffcntFlag]
           ,[RemitTotal]
           ,[RemitAmt]
           ,[PayMode]
           ,[PayRelation]
           ,[RemitDate]
           ,[RchkFrom]
           ,[RchkPNO]
           ,[RchkIDDUP]
           ,[RchkPSO]
           ,[RchkCustName]
           ,[RchkAppName]
           ,[RchkAppMop]
           ,[RchkAppMopName]
           ,[RchkAppPayK]
           ,[RchkAppPayKName]
           ,[RepyPType]
           ,[RepyChgAmt]
           ,[RepyNotyAmt]
           ,[RepyTolPayAmt]
           ,[RepyInfo]
           ,[EasyClmArea]
           ,[EasyRevID]
           ,[EasyRevName]
           ,[EasyPayType]
           ,[EasyPrClmMemo]
           ,[EasyPrClmAmt]
           ,[EasyClmillDate]
           ,[EasyClmAcpDate]
           ,[EasyClmTime]
           ,[EasyBankName]
           ,[EasyBankCode]
           ,[EasyBankNo]
           ,[NBSysType]
           ,[NBPayAmtperY]
           ,[PSChgType]
           ,[CustTrustPayFlag]
           ,[CustTrustPayInfo]
           ,[ChangeItemR_Code]
           ,[NBCPType]
           ,[IGTFlag]
           ,[TRSFlag]
           ,[CoverState]
           ,[ClsAgentID]
           ,[TrustFlagYN]
           ,[HearingYN]
           ,[LanguageYN]
           ,[YAmtType]
           ,[LegalCustodian]
           ,[RITDID]
           ,[Status]
           ,[investment])
     VALUES
           (@ListNo, 
            @CampID, 
            @ListSource,
            @PolicyNo,
            @IDDup, 
            @PolicySeq,
            @MemberID, 
            @RptReceNo,
            @CustLevel,
            @PolicyGoods,
            @CustID,
            @APPID, 
            @APPName, 
            @APPBirth,
            @AssignSchDate, 
            @VisitDate,
            @VisitTimeScale, 
            @VisitTime,
            @RecvZip, 
            @RecvAddr,
            @RecvTEL, 
            @ApplyDate, 
            @ChangeCloseDate, 
            @FileNO, 
            @ChangeItem, 
            @sAgentID, 
            @sAgentName, 
            @sAgentPhone,
            @sAgentMobile,
            @sAgentEmail, 
            @sAgentUnitName, 
            @sAgentUnitNo,
            @AccAgent, 
            @ProceUnitNo, 
            @ProceID,
            @ProceName, 
            @CCustID, 
            @CCustName, 
            @CZipCode,
            @CADDR, 
            @CTEL, 
            @AcceptCNTime,
            @AcceptCNTEL, 
            @ContactMemo, 
            @AreaCode,
            @CardType,
            @CustAddr,
            @Payment, 
            @DCRNO,
            @CTNO, 
            @BookValue,
            @SharesValue, 
            @ChkNO, 
            @ChkAccount,
            @PlanDesc,
            @AssetsNM,
            @PayItem, 
            @OTel, 
            @VerifyAgent, 
            @CNTFaxNO, 
            @CNTOP, 
            @CNTDP, 
            @CNTFL, 
            @RiskControlFactor, 
            @SubmitDate,
            @PolicyNum, 
            @PolicyYear,
            @PolicyAmt, 
            @SourceCamp,
            @PolicyCurType, 
            @PayType,
            @PayMethod, 
            @PolicyCost,
            @AmtOfSigned,
            @AmtOfAppro, 
            @AmtOfNTChk, 
            @AmtOfAct, 
            @AmtOfEQNT,
            @CntOfSelf,
            @AmtOfKind,
            @Trf_File, 
            @CreateDate,
            @BringDate, 
            @BatchNo,
            @AssignDate,
            @AgentID,
            @Dept_ID,
            @Dept_Name,
            @LAgentID, 
            @FContactDate,
            @LContactDate,
            @LTermCode, 
            @LCloseCode,
            @LCloseDate,
            @SYSID,
            @LAGTermCode, 
            @LAGContactDate,
            @LAGFollowDate, 
            @AGID, 
            @AGName, 
            @LTarget,
            @LAGContent, 
            @NullifyReason,
            @ProcDay,
            @UpdID, 
            @UpdDT, 
            @CMoblie, 
            @FZipCode,
            @FADDR,
            @FTEL, 
            @FMoblie, 
            @FRecvZip,
            @FRecvAddr,
            @FRecvTEL, 
            @RiskDispPhone1,
            @RiskDispPPNo1, 
            @RiskDispPPSO1, 
            @RiskDispPIDDUP1, 
            @RiskDispPPSREC1, 
            @RiskDispMob1, 
            @RiskDispMPNo1,
            @RiskDispMPSO1,
            @RiskDispMIDDUP1,
            @RiskDispMPSREC1,
            @RiskDispPhone2,
            @RiskDispPPNo2, 
            @RiskDispPPSO2, 
            @RiskDispPIDDUP2,
            @RiskDispPPSREC2,
            @RiskDispMob2, 
            @RiskDispMPNo2,
            @RiskDispMPSO2,
            @RiskDispMIDDUP2,
            @RiskDispMPSREC2,
            @RiskDispPhone3, 
            @RiskDispPPNo3, 
            @RiskDispPPSO3, 
            @RiskDispPIDDUP3, 
            @RiskDispPPSREC3, 
            @RiskDispMob3, 
            @RiskDispMPNo3,
            @RiskDispMPSO3,
            @RiskDispMIDDUP3, 
            @RiskDispMPSREC3, 
            @sAgentID2, 
            @sAgentName2, 
            @PolicySdType,
            @BnkCampCond, 
            @SecretGrp,
            @CONTEXT_C,
            @ChgDateReason,
            @OldAssignSchDate, 
            @LegalPID, 
            @LegalPName,
            @LegalPRel, 
            @sAgentUnitNo2, 
            @sAgentUnitName2, 
            @ContactSF, 
            @SourceType,
            @PayType_Aft, 
            @PayTypeN_Org, 
            @PayTypeN_Aft, 
            @PayMethod_Aft,
            @PayMethodN_Org, 
            @PayMethodN_Aft, 
            @ChangeItem_rest,
            @AppGen,
            @CustBirth, 
            @CustBirth_Aft, 
            @FRecvTEL2, 
            @RecvTEL2,
            @FTEL2, 
            @CTEL2, 
            @AutoBatchNo, 
            @TotallPolicyCost, 
            @TrustInfo, 
            @PrePolicyCost, 
            @PolicyCostDate,
            @AuthFlag,
            @OCustFingPrt,
            @NCustFingPrt,
            @APPFingPrt,
            @CustSERR,
            @APPSERR, 
            @APPZipCode,
            @APPAddr,
            @APPTel1,
            @APPTel2,
            @APPMobile, 
            @APPEmailAddr,
            @NAPPZipCode, 
            @NAPPAddr, 
            @NAPPTel1, 
            @NAPPTel2, 
            @NAPPMobile, 
            @NAPPEmailAddr, 
            @OriCustName, 
            @NTrustInfo,
            @LawRefer, 
            @TAEffet_Date, 
            @TAEffet_Time, 
            @TAEnd_Date, 
            @TATravelDays, 
            @TATravelPlac1,
            @TATravelPlac2,
            @TATravelPlac3,
            @TAAddUnit, 
            @TAMrUnit, 
            @TAOhsUnit,
            @TAOhs1Unit, 
            @TAAddPrem,
            @TAMrPrem, 
            @TAOhsPrem,
            @TAOhs1Prem, 
            @TABankCode, 
            @TACardNo, 
            @TACardEndYM,
            @CPlanDesc, 
            @CPolicyYear, 
            @CCONTEXT_C, 
            @CAPPID, 
            @CAPPNAME, 
            @CAPPBirth,
            @EffcntFlag, 
            @RemitTotal, 
            @RemitAmt,
            @PayMode, 
            @PayRelation,
            @RemitDate, 
            @RchkFrom, 
            @RchkPNO, 
            @RchkIDDUP, 
            @RchkPSO, 
            @RchkCustName,
            @RchkAppName, 
            @RchkAppMop, 
            @RchkAppMopName, 
            @RchkAppPayK, 
            @RchkAppPayKName, 
            @RepyPType, 
            @RepyChgAmt,
            @RepyNotyAmt, 
            @RepyTolPayAmt,
            @RepyInfo, 
            @EasyClmArea,
            @EasyRevID, 
            @EasyRevName, 
            @EasyPayType, 
            @EasyPrClmMemo, 
            @EasyPrClmAmt, 
            @EasyClmillDate,
            @EasyClmAcpDate,
            @EasyClmTime, 
            @EasyBankName,
            @EasyBankCode,
            @EasyBankNo,
            @NBSysType, 
            @NBPayAmtperY,
            @PSChgType,
            @CustTrustPayFlag,
            @CustTrustPayInfo,
            @ChangeItemR_Code,
            @NBCPType,
            @IGTFlag, 
            @TRSFlag, 
            @CoverState, 
            @ClsAgentID, 
            @TrustFlagYN,
            @HearingYN, 
            @LanguageYN,
            @YAmtType, 
            @LegalCustodian, 
            @RITDID, 
            @Status, 
            @investment)
";
        }

        /// <summary>
        /// insert into tblRosterPolicy_DTL sql
        /// </summary>
        /// <returns></returns>
        private static string insert_tblRosterPolicy_DTL()
        {
            return $@"
INSERT INTO [dbo].[tblRosterPolicy_DTL]
           ([ListNo]
           ,[PolicyNo]
           ,[IDDup]
           ,[PolicySeq]
           ,[MemberID]
           ,[RptReceNo]
           ,[PAYER_Id]
           ,[PAYER_Name]
           ,[PAYER_Moblie]
           ,[PAYER_Tel]
           ,[PAYER_BirthDate]
           ,[PAYER_Relate]
           ,[PAYER_LR_Id]
           ,[PAYER_LR_Name]
           ,[PAYER_LR_BirthDate]
           ,[FPrem_Auth_BankName]
           ,[FPrem_Auth_Way]
           ,[LetterNM]
           ,[SendType]
           ,[SendDT]
           ,[ReturnDT]
           ,[ReturnRS]
           ,[RecentPayDT]
           ,[Pay_Kind]
           ,[Pay_Type]
           ,[Pay_AMT]
           ,[RepayDT]
           ,[RepayType]
           ,[RepayAMT]
           ,[ContractType]
           ,[CustEmailAddr]
           ,[campid]
           ,[LK_GRACE_DATE]
           ,[LK_GEN_DATE]
           ,[LK_STOP_DATE]
           ,[PAYER_TelO]
           ,[Appointment]
           ,[CreateTime]
           ,[PayAmountSourceQ]
           ,[PayAmountSourceA]
           ,[InsurancePolicyQ]
           ,[InsurancePolicyA]
           ,[MPolicyNo])
     VALUES
           (@ListNo, 
            @PolicyNo,
            @IDDup, 
            @PolicySeq,
            @MemberID, 
            @RptReceNo,
            @PAYER_Id, 
            @PAYER_Name, 
            @PAYER_Moblie,
            @PAYER_Tel,
            @PAYER_BirthDate, 
            @PAYER_Relate,
            @PAYER_LR_Id, 
            @PAYER_LR_Name, 
            @PAYER_LR_BirthDate,
            @FPrem_Auth_BankName, 
            @FPrem_Auth_Way, 
            @LetterNM,
            @SendType,
            @SendDT, 
            @ReturnDT,
            @ReturnRS,
            @RecentPayDT, 
            @Pay_Kind,
            @Pay_Type,
            @Pay_AMT, 
            @RepayDT, 
            @RepayType,
            @RepayAMT, 
            @ContractType, 
            @CustEmailAddr,
            @campid,
            @LK_GRACE_DATE,
            @LK_GEN_DATE, 
            @LK_STOP_DATE,
            @PAYER_TelO, 
            @Appointment,
            @CreateTime, 
            @PayAmountSourceQ, 
            @PayAmountSourceA, 
            @InsurancePolicyQ, 
            @InsurancePolicyA, 
            @MPolicyNo)
";
        }

        /// <summary>
        /// insert into tblRoster sql
        /// </summary>
        /// <returns></returns>
        private static string insert_tblRoster()
        {
            return $@"
INSERT INTO [dbo].[tblRoster]
           ([ListNo]
           ,[CampID]
           ,[CampType]
           ,[CustKey]
           ,[CustID]
           ,[CustName]
           ,[ContactName]
           ,[Gender]
           ,[Status]
           ,[CustType]
           ,[CustBirth]
           ,[Marry]
           ,[EmailAddr]
           ,[Email2]
           ,[ZipCode]
           ,[ContactAddr]
           ,[TelM_Org]
           ,[TelM_Aft]
           ,[TelO_Org]
           ,[TelO_Aft]
           ,[TelO_Ext]
           ,[TelH_Org]
           ,[TelH_Aft]
           ,[TelH_Ext]
           ,[OfficeZipCode]
           ,[OfficeAddress]
           ,[FaxNo]
           ,[CustMemo]
           ,[AssignSchDate]
           ,[ListSource]
           ,[SaleAgentID]
           ,[Urgent]
           ,[Trf_File]
           ,[DialTM]
           ,[LCallBackDate]
           ,[FContactDate]
           ,[LContactDate]
           ,[LTermCode]
           ,[LCloseCode]
           ,[LCloseDate]
           ,[SYSID]
           ,[LAGTermCode]
           ,[LAGFollowDate]
           ,[LAGContactDate]
           ,[AGID]
           ,[AGName]
           ,[LTarget]
           ,[LAGContent]
           ,[NullifyReason]
           ,[LAgentID]
           ,[LContent]
           ,[LTelNo]
           ,[CreateDate]
           ,[BatchNo]
           ,[AssignDate]
           ,[AgentID]
           ,[AgentName]
           ,[GroupID]
           ,[GroupName]
           ,[DialFlag]
           ,[DataCatchFlag]
           ,[DialTMFlag]
           ,[TicketNo]
           ,[isContact]
           ,[TranAgent]
           ,[TranDate]
           ,[ReclaimDate]
           ,[CampName]
           ,[TelM_State]
           ,[TelH_State]
           ,[TelO_State]
           ,[InsertFlag]
           ,[Deadline]
           ,[Dept_ID]
           ,[Dept_Name]
           ,[InsID]
           ,[InsDT]
           ,[UpdID]
           ,[UpdDT]
           ,[CustType_Aft]
           ,[CustTypeN_Org]
           ,[CustTypeN_Aft]
           ,[AutoBatchNo]
           ,[QueueTime]
           ,[AbnCount]
           ,[CustGroup]
           ,[AppFlag]
           ,[PSContCustID]
           ,[CCSFlag]
           ,[CCSFDate]
           ,[EVaiFlag]
           ,[AS400Nflg]
           ,[AssiReason]
           ,[ClsAgentID]
           ,[CustAge]
           ,[UseTime]
           ,[Suggesttime1]
           ,[Suggesttime2]
           ,[Suggesttime3])
     VALUES
           (@ListNo,
            @CampID,
            @CampType,
            @CustKey,
            @CustID,
            @CustName, 
            @ContactName,
            @Gender,
            @Status,
            @CustType,
            @CustBirth, 
            @Marry,
            @EmailAddr,
            @Email2,
            @ZipCode,
            @ContactAddr,
            @TelM_Org,
            @TelM_Aft,
            @TelO_Org,
            @TelO_Aft,
            @TelO_Ext,
            @TelH_Org,
            @TelH_Aft,
            @TelH_Ext,
            @OfficeZipCode, 
            @OfficeAddress,
            @FaxNo,
            @CustMemo, 
            @AssignSchDate,
            @ListSource, 
            @SaleAgentID,
            @Urgent, 
            @Trf_File, 
            @DialTM, 
            @LCallBackDate,
            @FContactDate, 
            @LContactDate, 
            @LTermCode, 
            @LCloseCode,
            @LCloseDate,
            @SYSID, 
            @LAGTermCode, 
            @LAGFollowDate, 
            @LAGContactDate,
            @AGID, 
            @AGName, 
            @LTarget,
            @LAGContent, 
            @NullifyReason, 
            @LAgentID, 
            @LContent, 
            @LTelNo, 
            @CreateDate,
            @BatchNo,
            @AssignDate,
            @AgentID, 
            @AgentName, 
            @GroupID, 
            @GroupName,
            @DialFlag,
            @DataCatchFlag, 
            @DialTMFlag,
            @TicketNo, 
            @isContact,
            @TranAgent,
            @TranDate, 
            @ReclaimDate, 
            @CampName, 
            @TelM_State, 
            @TelH_State, 
            @TelO_State, 
            @InsertFlag, 
            @Deadline,
            @Dept_ID,
            @Dept_Name,
            @InsID, 
            @InsDT, 
            @UpdID, 
            @UpdDT, 
            @CustType_Aft, 
            @CustTypeN_Org,
            @CustTypeN_Aft,
            @AutoBatchNo,
            @QueueTime,
            @AbnCount,
            @CustGroup, 
            @AppFlag, 
            @PSContCustID,
            @CCSFlag,
            @CCSFDate, 
            @EVaiFlag, 
            @AS400Nflg, 
            @AssiReason,
            @ClsAgentID,
            @CustAge,
            @UseTime,
            @Suggesttime1,
            @Suggesttime2,
            @Suggesttime3)
";
        }

        private static string insert_tblRosterPolicyBenfit()
        {
            return $@"
INSERT INTO [dbo].[tblRosterPolicyBenfit]
           ([ListNo]
           ,[CampID]
           ,[PolicyNo]
           ,[IDDup]
           ,[PolicySeq]
           ,[MemberID]
           ,[RptReceNo]
           ,[TrustSeq]
           ,[TrustName]
           ,[TrustRate]
           ,[TrustRelate]
           ,[TrustType])
     VALUES
           (@ListNo,
            @CampID, 
            @PolicyNo,
            @IDDup, 
            @PolicySeq, 
            @MemberID,
            @RptReceNo,
            @TrustSeq, 
            @TrustName,
            @TrustRate, 
            @TrustRelate, 
            @TrustType)
";
        }
    }
}
