using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class TREASURYKEYCHECK : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();
            List<TreasuryKeyCheckReport> ReportDataList = new List<TreasuryKeyCheckReport>();
            var _Parameters = new List<SqlParameter>();
            var ReportData = new TreasuryKeyCheckReport();
            var APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value;
            var APLY_DT_To = parms.Where(x => x.key == "APLY_DT_To").FirstOrDefault()?.value;
            string CONTROL_MODE = parms.Where(x => x.key == "CONTROL_MODE").FirstOrDefault()?.value;
            string CONTROL_MODE_TEXT = parms.Where(x => x.key == "CONTROL_MODE_TEXT").FirstOrDefault()?.value;
            string CUSTODY_MODE = parms.Where(x => x.key == "CUSTODY_MODE").FirstOrDefault()?.value;
            string EMP_NAME = parms.Where(x => x.key == "EMP_NAME").FirstOrDefault()?.value;
            string AGENT_NAME = parms.Where(x => x.key == "AGENT_NAME").FirstOrDefault()?.value;
            int Total = 0;
            var _datas = new List<CODE_ROLE_TREA_ITEM>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _APLY_DT_From = TypeTransfer.stringToDateTimeN(APLY_DT_From);
                DateTime? _APLY_DT_To = TypeTransfer.stringToDateTimeN(APLY_DT_To).DateToLatestTime();
                List<string> _EMP_Roles = new List<string>();
                List<string> _AGENT_Roles = new List<string>();
                List<string> _ITEM_ID_AlreadyHave = new List<string>();
                if(EMP_NAME != "All")
                    _EMP_Roles = db.CODE_USER_ROLE.AsNoTracking().Where(x=>x.USER_ID == EMP_NAME).Select(x=>x.ROLE_ID).ToList();
                if(AGENT_NAME != "All")
                    _AGENT_Roles =  db.CODE_USER_ROLE.AsNoTracking().Where(x=>x.USER_ID == AGENT_NAME).Select(x=>x.ROLE_ID).ToList();
                
                var _CRTI =db.CODE_ROLE_TREA_ITEM.AsNoTracking()//角色金庫設備資料檔-設備代碼
                    .Where(x => _APLY_DT_From <= x.LAST_UPDATE_DT  , _APLY_DT_From != null)
                    .Where(x => x.LAST_UPDATE_DT <= _APLY_DT_To , _APLY_DT_To != null)
                    //.Where(x => _EMP_Roles.Contains(x.ROLE_ID) , EMP_NAME != "All")
                    //.Where(x => _AGENT_Roles.Contains())
                    .Where(x => x.CUSTODY_MODE  == CUSTODY_MODE ) 
                    .ToList();
                if(_EMP_Roles.Any() || _AGENT_Roles.Any())
                {
                    var _CRTI_EMP = _CRTI.Where(x => x.CUSTODY_ORDER == 1)
                    .Where( x => _EMP_Roles.Contains(x.ROLE_ID), _EMP_Roles.Any()).ToList();
                    var _CRTI_AGENT = _CRTI.Where(x => x.CUSTODY_ORDER == 2 )
                    .Where( x=> _AGENT_Roles.Contains(x.ROLE_ID),_AGENT_Roles.Any()).ToList();
                    _CRTI = _CRTI_EMP;
                    _CRTI.AddRange(_CRTI_AGENT);
                }
                               
                var _TREA_EQUIP_IDs = _CRTI.Select(x=>x.TREA_EQUIP_ID).ToList();

                var _TE =db.TREA_EQUIP.AsNoTracking()//金庫設備名稱
                    .Where(x => x.IS_DISABLED == "N")
                    .Where(x=> _TREA_EQUIP_IDs.Contains(x.TREA_EQUIP_ID))
                    .Where(x => x.CONTROL_MODE ==CONTROL_MODE )
                    .ToList();

                var depts = new List<V_EMPLY2>();
                var types = new List<SYS_CODE>(); 
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    depts = dbINTRA.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
                }
                types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null).ToList();

                var curs = db.CODE_USER_ROLE.AsNoTracking().ToList();
               
                foreach(var item in _CRTI.GroupBy( x=>new {x.TREA_EQUIP_ID,x.CUSTODY_MODE}) )
                {
                    bool addFlag = false;
                    if (EMP_NAME != "All" && AGENT_NAME != "All")
                    {
                        var order1_Role_Ids = item.Where(x => x.CUSTODY_ORDER == 1).Select(x => x.ROLE_ID).ToList();
                        var order2_Role_Ids = item.Where(x => x.CUSTODY_ORDER == 2).Select(x => x.ROLE_ID).ToList();
                        if (curs.Any(x => order1_Role_Ids.Contains(x.ROLE_ID) && x.USER_ID == EMP_NAME) &&
                            curs.Any(x => order2_Role_Ids.Contains(x.ROLE_ID) && x.USER_ID == AGENT_NAME))
                            addFlag = true;
                    }
                    else if (EMP_NAME != "All")
                    {
                        var order1_Role_Ids = item.Where(x => x.CUSTODY_ORDER == 1).Select(x => x.ROLE_ID).ToList();
                        if (curs.Any(x => order1_Role_Ids.Contains(x.ROLE_ID) && x.USER_ID == EMP_NAME))
                            addFlag = true;
                    }
                    else if (AGENT_NAME != "All")
                    {
                        var order2_Role_Ids = item.Where(x => x.CUSTODY_ORDER == 2).Select(x => x.ROLE_ID).ToList();
                        if(curs.Any(x => order2_Role_Ids.Contains(x.ROLE_ID) && x.USER_ID == AGENT_NAME))
                            addFlag = true;
                    }
                    else
                    {
                        addFlag = true;
                    }
                    if (addFlag)
                    {
                        _ITEM_ID_AlreadyHave.Add(item.Key.TREA_EQUIP_ID);
                        var TREA_EQUIP_ID_Key = item.Key.TREA_EQUIP_ID;
                        var TE = _TE.FirstOrDefault(x => x.TREA_EQUIP_ID == TREA_EQUIP_ID_Key);
                        var CUSTODY_Value_1 = string.Join("、", getEmpName(depts, curs, item.Where(x => x.CUSTODY_ORDER == 1).Select(x => x.ROLE_ID).ToList()));//保管人
                        var CUSTODY_Value_2 = string.Join("、", getEmpName(depts, curs, item.Where(x => x.CUSTODY_ORDER == 2).Select(x => x.ROLE_ID).ToList())); //代理人
                        if (TE != null)
                        {
                            Total++;
                            var CUSTODY_MODE_Key = getCSMTtype(types, item.Key.CUSTODY_MODE);
                            var TREA_EQUIP_ID_Key_Name = TE?.EQUIP_NAME;
                            var MEMO = TE?.MEMO;

                            ReportData = new TreasuryKeyCheckReport()
                            {
                                ROW = Total,
                                CUSTODY_MODE = CUSTODY_MODE_Key,
                                EQUIP_NAME = TREA_EQUIP_ID_Key_Name,
                                EMP_NAME = CUSTODY_Value_1,
                                AGENT_NAME = CUSTODY_Value_2,
                                MEMO = MEMO,
                            };
                            ReportDataList.Add(ReportData);
                        }
                    }
                }
                if (EMP_NAME == "All" && AGENT_NAME == "All")
                {
                    var _TREA_EQUIP = db.TREA_EQUIP.AsNoTracking()
                        .Where(x => x.CONTROL_MODE == CONTROL_MODE)
                        .Where(x => x.IS_DISABLED == "N")
                        .Where(x => !_ITEM_ID_AlreadyHave.Contains(x.TREA_EQUIP_ID));

                    foreach(var item in _TREA_EQUIP)
                    {
                       var EQUIPData = new TreasuryKeyCheckReport()
                        {
                            ROW = ++Total,
                            CUSTODY_MODE = getCSMTtype(types, CUSTODY_MODE),
                            EQUIP_NAME = item.EQUIP_NAME,
                            EMP_NAME = "",
                            AGENT_NAME = "",
                            MEMO = "",
                        };
                        ReportDataList.Add(EQUIPData);
                    }
                }
            }
            resultsTable.Tables.Add(ReportDataList.ToDataTable());
            return resultsTable;
        }
        /// <summary>
        /// 使用 USER_ID 獲得 部門名稱
        /// </summary>
        /// <param name="depts"></param>
        /// <param name="USR_ID"></param>
        /// <returns></returns>
        private List<string> getEmpName(List<V_EMPLY2> depts,List<CODE_USER_ROLE> curs, List<string> ROL_ID)
        {
            if (ROL_ID.Any())
            {
                return curs.Where(x => ROL_ID.Contains(x.ROLE_ID) && x.USER_ID != null)
                    .Select(x => x.USER_ID).Distinct()
                    .Select(z => depts.Where(x => x.USR_ID.Trim() == z)
                        .Select(x => x.EMP_NAME + "(" + x.DPT_NAME + ")").FirstOrDefault()).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// 使用 控管方式 獲得 控管方式名稱
        /// </summary>
        /// <param name="CUSTODY_MODE"></param>
        /// <param name="CODE_VALUE"></param>
        /// <returns></returns>
        private string getCSMTtype(List<SYS_CODE> types, string CODE)
        {
            if (!CODE.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.CODE.Trim() == CODE.Trim() && x.CODE_TYPE == "CUSTODY_MODE")?.CODE_VALUE?.Trim();
            return string.Empty;
        }
        /// <summary>
        /// 使用 控管方式 獲得 控管方式名稱
        /// </summary>
        /// <param name="CONTROL_MODE"></param>
        /// <param name="CODE_VALUE"></param>
        /// <returns></returns>
        private string getCNMTtype(List<SYS_CODE> types, string CODE)
        {
            if (!CODE.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.CODE.Trim() == CODE.Trim() && x.CODE_TYPE == "CONTROL_MODE")?.CODE_VALUE?.Trim();
            return string.Empty;
        }
    }
}
