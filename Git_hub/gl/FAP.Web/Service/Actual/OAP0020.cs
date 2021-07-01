using FAP.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FAP.Web.ViewModels;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;
using System.Data.Entity.Infrastructure;

namespace FAP.Web.Service.Actual
{
    public class OAP0020 : Common, IOAP0020
    {
        public Tuple<bool, string>  CheckSameData(OAP0020InsertModel model, string mod)
        {
            bool hasSameData = false;
            string reason = "";
            if (model != null)
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    if(model.type == "M")
                    {
                        var result = db.FAP_NOTES_PAYABLE_RECEIVED.AsNoTracking()
                            .Any(x => x.dep_id == model.dep_id);
                        var ddresult = db.FAP_NOTES_PAYABLE_RECEIVED_D.AsNoTracking()
                            .Any(x => x.division == model.dep_id);
                        hasSameData = result || ddresult;
                        reason = MessageType.Already_Same_Data.GetDescription();
                    }
                    else
                    {
                        var dresult = db.FAP_NOTES_PAYABLE_RECEIVED_D.AsNoTracking()
                            .Any(x => x.division == model.division);
                        hasSameData = dresult;

                        if (hasSameData)
                        {
                            var _data = db.FAP_NOTES_PAYABLE_RECEIVED_D.AsNoTracking()
                            .First(x => x.division == model.division);
                            reason = $"此筆出錯誤訊息，因{model.division}已歸入{_data.dep_id}下，不能再歸入{model.dep_id}中";
                        } 
                    }
                    
                }
            }
            return new Tuple<bool, string>(hasSameData, reason);
        }

        public List<OAP0020ViewModel> GetSearchData()
        {
            List<OAP0020ViewModel> result = new List<OAP0020ViewModel>();
            var emps = GetEmps();
            var depts = GetDepts();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                result = db.FAP_NOTES_PAYABLE_RECEIVED.AsNoTracking()
                       .AsEnumerable().Select(x => new OAP0020ViewModel()
                       {
                           dep_id = x.dep_id,
                           //dep_name = depts.Where(z => z.DEP_ID == x.dep_id)?.Select(z => z.DEP_NAME).FirstOrDefault(),
                           apt_id = x.apt_id,
                           apt_name = x.apt_name,
                           email = x.email
                           //apt_name = emps.Where(z => z.MEM_MEMO1 == x.apt_id)?.Select(m => { return $@"{m.MEM_NAME}"; }).FirstOrDefault(),
                           //email = emps.Where(z => z.MEM_MEMO1 == x.apt_id)?.Select(m => { return $@"{m.MEM_EMAIL}"; }).FirstOrDefault(),
                       }).ToList();
                var fullDatas = getFullDepName(result.Select(x => x.dep_id).Distinct());
                foreach (var item in result)
                {
                    item.dep_name = fullDatas.FirstOrDefault(x => x.Item1 == item.dep_id)?.Item2;
                }
            }

            return result;
        }

        public List<OAP0020ViewModel> GetSearchDetail(string dep_id, string dept_Name)
        {
            List<OAP0020ViewModel> result = new List<OAP0020ViewModel>();
            var emps = GetEmps();
            var depts = GetDepts();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                result = db.FAP_NOTES_PAYABLE_RECEIVED_D.AsNoTracking()
                       .Where(x => x.dep_id == dep_id)
                       .AsEnumerable().Select(x => new OAP0020ViewModel()
                       {
                           division = x.division,
                           //division_name = depts.Where(z => z.DEP_ID == x.division)?.Select(z => z.DEP_NAME).FirstOrDefault(),
                           dep_id = dep_id,
                           dep_name = dept_Name
                       }).ToList();
                var fullDatas = getFullDepName(result.Select(x => x.division).Distinct());
                foreach (var item in result)
                {
                    item.division_name = fullDatas.FirstOrDefault(x => x.Item1 == item.division)?.Item2;
                }
            }

            return result;
        }

        public MSGReturnModel<string> InsertData(OAP0020InsertModel saveData, string userId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = MessageType.insert_Fail.GetDescription();
            DateTime now = DateTime.Now;

            if (saveData != null)
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    if (saveData.type == "M")
                    {

                        db.FAP_NOTES_PAYABLE_RECEIVED.Add(new FAP_NOTES_PAYABLE_RECEIVED()
                        {
                            dep_id = saveData.dep_id,
                            apt_id = saveData.apt_id,
                            apt_name = saveData.apt_name,
                            email = saveData.email,
                            create_id = userId,
                            create_datetime = now,
                            update_id = userId,
                            update_datetime = now,
                        });

                        db.FAP_NOTES_PAYABLE_RECEIVED_D.Add(new FAP_NOTES_PAYABLE_RECEIVED_D()
                        {
                            division = saveData.dep_id,
                            dep_id = saveData.dep_id,
                            create_id = userId,
                            create_datetime = now
                        });
                    }
                    else
                    {
                        db.FAP_NOTES_PAYABLE_RECEIVED_D.Add(new FAP_NOTES_PAYABLE_RECEIVED_D()
                        {
                            division = saveData.division,
                            dep_id = saveData.dep_id,
                            create_id = userId,
                            create_datetime = now
                        });
                    }

                   

                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (!validateMessage.IsNullOrWhiteSpace())
                    {
                        result.DESCRIPTION = validateMessage;
                    }
                    else
                    {
                        try
                        {
                            db.SaveChanges();

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.insert_Success.GetDescription();

                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                    }
                }
            }

            return result;
        }

        public MSGReturnModel<string> UpdateData(OAP0020InsertModel saveData, string userId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = MessageType.update_Fail.GetDescription();
            DateTime now = DateTime.Now;

            if (saveData != null)
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    if (saveData.type == "M")
                    {
                        var dbM = db.FAP_NOTES_PAYABLE_RECEIVED.First(x => x.dep_id == saveData.dep_id);

                        dbM.apt_id = saveData.apt_id;
                        dbM.apt_name = saveData.apt_name;
                        dbM.email = saveData.email;
                        dbM.update_id = userId;
                        dbM.update_datetime = now;
                    }

                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (!validateMessage.IsNullOrWhiteSpace())
                    {
                        result.DESCRIPTION = validateMessage;
                    }
                    else
                    {
                        try
                        {
                            db.SaveChanges();

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.update_Success.GetDescription();

                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                    }
                }
            }
            return result;
        }

        public MSGReturnModel<string> DeleteData(OAP0020InsertModel saveData, string userId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = MessageType.update_Fail.GetDescription();
            DateTime now = DateTime.Now;

            if (saveData != null)
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    if (saveData.type == "M")
                    {
                        var dbM = db.FAP_NOTES_PAYABLE_RECEIVED.First(x => x.dep_id == saveData.dep_id);
                        db.FAP_NOTES_PAYABLE_RECEIVED.Remove(dbM);

                        var dbD = db.FAP_NOTES_PAYABLE_RECEIVED_D.Where(x => x.dep_id == saveData.dep_id);
                        db.FAP_NOTES_PAYABLE_RECEIVED_D.RemoveRange(dbD);
                    }
                    else
                    {
                        var dbD = db.FAP_NOTES_PAYABLE_RECEIVED_D.First(x => x.division == saveData.division);
                        db.FAP_NOTES_PAYABLE_RECEIVED_D.Remove(dbD);
                    }

                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (!validateMessage.IsNullOrWhiteSpace())
                    {
                        result.DESCRIPTION = validateMessage;
                    }
                    else
                    {
                        try
                        {
                            db.SaveChanges();

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.delete_Success.GetDescription();

                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                    }
                }
            }
            return result;
        }
    }
}