using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebDaos;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Actual
{
    public class TreaDefinitionAppr : Common, ITreaDefinitionAppr
    {
        public MSGReturnModel<List<TDAApprSearchDetailViewModel>> Approved(TDAApprSearchViewModel searchData, List<TDAApprSearchDetailViewModel> viewModels)
        {
            var result = new MSGReturnModel<List<TDAApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            List<string> TREA_ITEM_APLY = new List<string>();
            List<string> TREA_EQUIP_APLY = new List<string>();
            List<string> MAIL_CONTENT_APLY = new List<string>();
            List<string> MAIL_TIME_APLY = new List<string>();
            List<string> ITEM_CHARGE_UNIT_APLY = new List<string>();
            List<string> DEP_CHK_ITEM_APLY = new List<string>();
            if (!viewModels.Any())
            {
                return result;
            }
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();
                foreach (var item in viewModels.Where(x => x.vCheckFlag))
                {
                    switch (item.vTDA_Id)
                    {
                        case "TREA_ITEM":
                            TREA_ITEM_APLY.Add(item.vAply_No);
                            break;
                        case "TREA_EQUIP":
                            TREA_EQUIP_APLY.Add(item.vAply_No);
                            break;
                        case "MAIL_CONTENT":
                            MAIL_CONTENT_APLY.Add(item.vAply_No);
                            break;
                        case "MAIL_TIME":
                            MAIL_TIME_APLY.Add(item.vAply_No);
                            break;
                        case "ITEM_CHARGE_UNIT":
                            ITEM_CHARGE_UNIT_APLY.Add(item.vAply_No);
                            break;
                        case "DEP_CHK_ITEM":
                            DEP_CHK_ITEM_APLY.Add(item.vAply_No);
                            break;
                    }
                    aplynos.Add(item.vAply_No);
                }
                #region 對應資料檔-核可
                var sampleFactory = new SampleFactory();

                if (TREA_ITEM_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "TREA_ITEM"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinApproved(db, TREA_ITEM_APLY, logStr, dt, searchData.vCreateUid);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (TREA_EQUIP_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "TREA_EQUIP"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinApproved(db, TREA_EQUIP_APLY, logStr, dt, searchData.vCreateUid);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (MAIL_CONTENT_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "MAIL_CONTENT"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinApproved(db, MAIL_CONTENT_APLY, logStr, dt, searchData.vCreateUid);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (MAIL_TIME_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "MAIL_TIME"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinApproved(db, MAIL_TIME_APLY, logStr, dt, searchData.vCreateUid);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (ITEM_CHARGE_UNIT_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "ITEM_CHARGE_UNIT"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinApproved(db, ITEM_CHARGE_UNIT_APLY, logStr, dt, searchData.vCreateUid);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (DEP_CHK_ITEM_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "DEP_CHK_ITEM"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinApproved(db, DEP_CHK_ITEM_APLY, logStr, dt, searchData.vCreateUid);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    } 
                }
                #endregion

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

                        #region LOG
                        //新增LOG
                        Log log = new Log();
                        log.CFUNCTION = "覆核-定義檔覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 覆核成功!";
                        result.Datas = GetApprSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }

        public List<TDAApprSearchDetailViewModel> GetApprSearchDetail(TDAApprSearchViewModel data)
        {
            List<TDAApprSearchDetailViewModel> result = new List<TDAApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _TREA_ITEM_HIS = db.TREA_ITEM_HIS.AsNoTracking(); //金庫存取作業設定異動檔
                var _TREA_EQUIP_HIS = db.TREA_EQUIP_HIS.AsNoTracking(); //金庫設備設定異動檔
                var _MAIL_CONTENT_HIS = db.MAIL_CONTENT_HIS.AsNoTracking(); //mail發送內文設定異動檔
                var _MAIL_TIME_HIS = db.MAIL_TIME_HIS.AsNoTracking(); //發送時間定義異動檔
                var _ITEM_CHARGE_UNIT_HIS = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking(); //保管單位設定異動檔
                var _DEP_CHK_ITEM_HIS = db.DEP_CHK_ITEM_HIS.AsNoTracking(); //定存檢核表項目設定異動檔
                var aplyStatus = ((int)Ref.ApplyStatus._1).ToString(); // 狀態 => 表單申請

                var _TREA_ITEM = (Ref.DefinitionType.TREA_ITEM).ToString(); // 設定檔項目 => 金庫存取作業設定檔
                var _TREA_EQUIP = (Ref.DefinitionType.TREA_EQUIP).ToString(); // 設定檔項目 => 金庫設備設定檔
                var _MAIL_CONTENT = (Ref.DefinitionType.MAIL_CONTENT).ToString(); // 設定檔項目 => mail發送內文設定檔
                var _MAIL_TIME = (Ref.DefinitionType.MAIL_TIME).ToString(); // 設定檔項目 => 發送時間定義檔
                var _ITEM_CHARGE_UNIT = (Ref.DefinitionType.ITEM_CHARGE_UNIT).ToString(); // 設定檔項目 => 保管單位設定檔
                var _DEP_CHK_ITEM = (Ref.DefinitionType.DEP_CHK_ITEM).ToString(); // 設定檔項目 => 定存檢核表項目設定檔

                if (data.vTDA_Id == "All" || data.vTDA_Id == _TREA_ITEM)
                {
                    //金庫存取作業設定異動檔
                    var _TIH = _TREA_ITEM_HIS
                        .Where(x => x.APPR_STATUS == aplyStatus)
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _TREA_ITEM,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _TREA_ITEM).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vApprFlag = x.APLY_UID != data.vCreateUid
                        }).ToList();
                    result.AddRange(_TIH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _TREA_EQUIP)
                {
                    //金庫設備設定異動檔
                    var _TEH = _TREA_EQUIP_HIS
                        .Where(x => x.APPR_STATUS == aplyStatus)
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _TREA_EQUIP,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _TREA_EQUIP).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vApprFlag = x.APLY_UID != data.vCreateUid
                        }).ToList();
                    result.AddRange(_TEH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _MAIL_CONTENT)
                {
                    //mail發送內文設定異動檔
                    var _MCH = _MAIL_CONTENT_HIS
                       .Where(x => x.APPR_STATUS == aplyStatus)
                       .AsEnumerable()
                       .Select(x => new TDAApprSearchDetailViewModel()
                       {
                           vTDA_Id = _MAIL_CONTENT,
                           vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _MAIL_CONTENT).GetDescription(),
                           vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                           vAply_No = x.APLY_NO,
                           vAply_Uid = x.APLY_UID,
                           vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                           vApprFlag = x.APLY_UID != data.vCreateUid
                       }).ToList();
                    result.AddRange(_MCH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _MAIL_TIME)
                {
                    //發送時間定義異動檔


                    var _MTH = _MAIL_TIME_HIS
                        .Where(x => x.APPR_STATUS == aplyStatus)
                        .AsEnumerable()     
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _MAIL_TIME,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _MAIL_TIME).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vApprFlag = x.APLY_UID != data.vCreateUid
                        }).ToList();
                    result.AddRange(_MTH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _ITEM_CHARGE_UNIT)
                {
                    //保管單位設定異動檔
                    var _ICUH = _ITEM_CHARGE_UNIT_HIS
                        .Where(x => x.APPR_STATUS == aplyStatus)
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _ITEM_CHARGE_UNIT,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _ITEM_CHARGE_UNIT).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vApprFlag = x.APLY_UID != data.vCreateUid
                        }).ToList();
                    result.AddRange(_ICUH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _DEP_CHK_ITEM)
                {
                    //定存檢核表項目設定異動檔
                    var _DCIH = _DEP_CHK_ITEM_HIS
                        .Where(x => x.APPR_STATUS == aplyStatus)
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _DEP_CHK_ITEM,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _DEP_CHK_ITEM).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vApprFlag = x.APLY_UID != data.vCreateUid
                        }).ToList();
                    result.AddRange(_DCIH);
                }
            }
            result = result.Distinct(new TDAApprSearchDetailViewModel_Comparer()).OrderBy(x => x.vAply_No).ToList();
            return result;
        }

        public MSGReturnModel<List<TDAApprSearchDetailViewModel>> Reject(TDAApprSearchViewModel searchData, List<TDAApprSearchDetailViewModel> viewModels, string apprDesc)
        {
            var result = new MSGReturnModel<List<TDAApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            List<string> TREA_ITEM_APLY = new List<string>();
            List<string> TREA_EQUIP_APLY = new List<string>();
            List<string> MAIL_CONTENT_APLY = new List<string>();
            List<string> MAIL_TIME_APLY = new List<string>();
            List<string> ITEM_CHARGE_UNIT_APLY = new List<string>();
            List<string> DEP_CHK_ITEM_APLY = new List<string>();
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();
                foreach (var item in viewModels.Where(x => x.vCheckFlag))
                {
                    switch (item.vTDA_Id)
                    {
                        case "TREA_ITEM":
                            TREA_ITEM_APLY.Add(item.vAply_No);
                            break;
                        case "TREA_EQUIP":
                            TREA_EQUIP_APLY.Add(item.vAply_No);
                            break;
                        case "MAIL_CONTENT":
                            MAIL_CONTENT_APLY.Add(item.vAply_No);
                            break;
                        case "MAIL_TIME":
                            MAIL_TIME_APLY.Add(item.vAply_No);
                            break;
                        case "ITEM_CHARGE_UNIT":
                            ITEM_CHARGE_UNIT_APLY.Add(item.vAply_No);
                            break;
                        case "DEP_CHK_ITEM":
                            DEP_CHK_ITEM_APLY.Add(item.vAply_No);
                            break;
                    }
                    aplynos.Add(item.vAply_No);
                }
                #region 對應資料檔-駁回
                var sampleFactory = new SampleFactory();

                if (TREA_ITEM_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "TREA_ITEM"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinReject(db, TREA_ITEM_APLY, logStr, dt, searchData.vCreateUid, apprDesc);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (TREA_EQUIP_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "TREA_EQUIP"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinReject(db, TREA_EQUIP_APLY, logStr, dt, searchData.vCreateUid, apprDesc);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (MAIL_CONTENT_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "MAIL_CONTENT"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinReject(db, MAIL_CONTENT_APLY, logStr, dt, searchData.vCreateUid, apprDesc);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (MAIL_TIME_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "MAIL_TIME"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinReject(db, MAIL_TIME_APLY, logStr, dt, searchData.vCreateUid, apprDesc);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (ITEM_CHARGE_UNIT_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "ITEM_CHARGE_UNIT"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinReject(db, ITEM_CHARGE_UNIT_APLY, logStr, dt, searchData.vCreateUid, apprDesc);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                }
                if (DEP_CHK_ITEM_APLY.Any())
                {
                    var getTDAAction = sampleFactory.GetTDAAction(EnumUtil.GetValues<Ref.DefinitionType>().First(x => x.ToString() == "DEP_CHK_ITEM"));
                    if (getTDAAction != null)
                    {
                        var _recover = getTDAAction.TinReject(db, DEP_CHK_ITEM_APLY, logStr, dt, searchData.vCreateUid, apprDesc);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }    
                }
                #endregion
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

                        #region LOG
                        //新增LOG
                        Log log = new Log();
                        log.CFUNCTION = "駁回-定義檔覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 已駁回!";
                        result.Datas = GetApprSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }

        public List<TDAApprSearchDetailViewModel> GetSearchDetail(TDAApprSearchViewModel data)
        {
            List<TDAApprSearchDetailViewModel> result = new List<TDAApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _TREA_ITEM_HIS = db.TREA_ITEM_HIS.AsNoTracking(); //金庫存取作業設定異動檔
                var _TREA_EQUIP_HIS = db.TREA_EQUIP_HIS.AsNoTracking(); //金庫設備設定異動檔
                var _MAIL_CONTENT_HIS = db.MAIL_CONTENT_HIS.AsNoTracking(); //mail發送內文設定異動檔
                var _MAIL_TIME_HIS = db.MAIL_TIME_HIS.AsNoTracking(); //發送時間定義異動檔
                var _ITEM_CHARGE_UNIT_HIS = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking(); //保管單位設定異動檔
                var _DEP_CHK_ITEM_HIS = db.DEP_CHK_ITEM_HIS.AsNoTracking(); //定存檢核表項目設定異動檔

                var _TREA_ITEM = (Ref.DefinitionType.TREA_ITEM).ToString(); // 設定檔項目 => 金庫存取作業設定檔
                var _TREA_EQUIP = (Ref.DefinitionType.TREA_EQUIP).ToString(); // 設定檔項目 => 金庫設備設定檔
                var _MAIL_CONTENT = (Ref.DefinitionType.MAIL_CONTENT).ToString(); // 設定檔項目 => mail發送內文設定檔
                var _MAIL_TIME = (Ref.DefinitionType.MAIL_TIME).ToString(); // 設定檔項目 => 發送時間定義檔
                var _ITEM_CHARGE_UNIT = (Ref.DefinitionType.ITEM_CHARGE_UNIT).ToString(); // 設定檔項目 => 保管單位設定檔
                var _DEP_CHK_ITEM = (Ref.DefinitionType.DEP_CHK_ITEM).ToString(); // 設定檔項目 => 定存檢核表項目設定檔

                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();

                Dictionary<string, string> DefinitionTypes = new Dictionary<string, string>() { };
                EnumUtil.GetValues<Ref.DefinitionType>().ToList()
                    .ForEach(x =>
                    {
                        DefinitionTypes.Add(x.ToString(), x.GetDescription());
                    });

                var apprStatus = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();

                if (data.vTDA_Id == "All" || data.vTDA_Id == _TREA_ITEM)
                {
                    //金庫存取作業設定異動檔
                    var _TIH = _TREA_ITEM_HIS
                        .Where(x => x.APLY_DATE >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                        .Where(x => x.APLY_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                        .Where(x => x.APLY_NO == data.vAplyNo, !data.vAplyNo.IsNullOrWhiteSpace()) //申請單號
                        .Where(x => x.APLY_UID == data.vAPLY_ID, !data.vAPLY_ID.IsNullOrWhiteSpace()) //申請人
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _TREA_ITEM,
                            vTDA_Desc = DefinitionTypes[_TREA_ITEM],
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vApprFlag = x.APLY_UID != data.vCreateUid,
                            vAppr_Desc = x.APPR_DESC,
                            vAppr_Status = apprStatus.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        }).ToList();
                    result.AddRange(_TIH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _TREA_EQUIP)
                {
                    //金庫設備設定異動檔
                    var _TEH = _TREA_EQUIP_HIS
                        .Where(x => x.APLY_DATE >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                        .Where(x => x.APLY_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                        .Where(x => x.APLY_NO == data.vAplyNo, !data.vAplyNo.IsNullOrWhiteSpace()) //申請單號
                        .Where(x => x.APLY_UID == data.vAPLY_ID, !data.vAPLY_ID.IsNullOrWhiteSpace()) //申請人
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _TREA_EQUIP,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _TREA_EQUIP).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vApprFlag = x.APLY_UID != data.vCreateUid,
                            vAppr_Desc = x.APPR_DESC,
                            vAppr_Status = apprStatus.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        }).ToList();
                    result.AddRange(_TEH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _MAIL_CONTENT)
                {
                    //mail發送內文設定異動檔
                    var _MCH = _MAIL_CONTENT_HIS
                        .Where(x => x.APLY_DATE >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                        .Where(x => x.APLY_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                        .Where(x => x.APLY_NO == data.vAplyNo, !data.vAplyNo.IsNullOrWhiteSpace()) //申請單號
                        .Where(x => x.APLY_UID == data.vAPLY_ID, !data.vAPLY_ID.IsNullOrWhiteSpace()) //申請人
                       .AsEnumerable()
                       .Select(x => new TDAApprSearchDetailViewModel()
                       {
                           vTDA_Id = _MAIL_CONTENT,
                           vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _MAIL_CONTENT).GetDescription(),
                           vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                           vAply_No = x.APLY_NO,
                           vAply_Uid = x.APLY_UID,
                           vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                           vApprFlag = x.APLY_UID != data.vCreateUid,
                           vAppr_Desc = x.APPR_DESC,
                           vAppr_Status = apprStatus.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                       }).ToList();
                    result.AddRange(_MCH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _MAIL_TIME)
                {
                    //發送時間定義異動檔
                    var _MTH = _MAIL_TIME_HIS
                        .Where(x => x.APLY_DATE >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                        .Where(x => x.APLY_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                        .Where(x => x.APLY_NO == data.vAplyNo, !data.vAplyNo.IsNullOrWhiteSpace()) //申請單號
                        .Where(x => x.APLY_UID == data.vAPLY_ID, !data.vAPLY_ID.IsNullOrWhiteSpace()) //申請人
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _MAIL_TIME,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _MAIL_TIME).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vApprFlag = x.APLY_UID != data.vCreateUid,
                            vAppr_Desc = x.APPR_DESC,
                            vAppr_Status = apprStatus.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        }).ToList();
                    result.AddRange(_MTH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _ITEM_CHARGE_UNIT)
                {
                    //保管單位設定異動檔
                    var _ICUH = _ITEM_CHARGE_UNIT_HIS
                        .Where(x => x.APLY_DATE >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                        .Where(x => x.APLY_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                        .Where(x => x.APLY_NO == data.vAplyNo, !data.vAplyNo.IsNullOrWhiteSpace()) //申請單號
                        .Where(x => x.APLY_UID == data.vAPLY_ID, !data.vAPLY_ID.IsNullOrWhiteSpace()) //申請人
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _ITEM_CHARGE_UNIT,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _ITEM_CHARGE_UNIT).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vApprFlag = x.APLY_UID != data.vCreateUid,
                            vAppr_Desc = x.APPR_DESC,
                            vAppr_Status = apprStatus.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        }).ToList();
                    result.AddRange(_ICUH);
                }

                if (data.vTDA_Id == "All" || data.vTDA_Id == _DEP_CHK_ITEM)
                {
                    //定存檢核表項目設定異動檔
                    var _DCIH = _DEP_CHK_ITEM_HIS
                        .Where(x => x.APLY_DATE >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                        .Where(x => x.APLY_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                        .Where(x => x.APLY_NO == data.vAplyNo, !data.vAplyNo.IsNullOrWhiteSpace()) //申請單號
                        .Where(x => x.APLY_UID == data.vAPLY_ID, !data.vAPLY_ID.IsNullOrWhiteSpace()) //申請人
                        .AsEnumerable()
                        .Select(x => new TDAApprSearchDetailViewModel()
                        {
                            vTDA_Id = _DEP_CHK_ITEM,
                            vTDA_Desc = EnumUtil.GetValues<Ref.DefinitionType>().FirstOrDefault(y => y.ToString() == _DEP_CHK_ITEM).GetDescription(),
                            vAply_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vApprFlag = x.APLY_UID != data.vCreateUid,
                            vAppr_Desc = x.APPR_DESC,
                            vAppr_Status = apprStatus.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        }).ToList();
                    result.AddRange(_DCIH);
                }
            }
            result = result.Distinct(new TDAApprSearchDetailViewModel_Comparer()).OrderBy(x => x.vAply_No).ToList();
            return result;
        }
    }
}