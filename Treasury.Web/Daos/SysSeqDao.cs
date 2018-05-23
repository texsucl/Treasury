using Treasury.WebBO;
using Treasury.WebModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Transactions;
using Treasury.Web.Models;

namespace Treasury.WebDaos
{
    public class SysSeqDao
    {
        public int qrySeqNo(String cType, String cPreCode)
        {
            int intseq = 0;
            int cnt = 0;

            //using (new TransactionScope(
            //       TransactionScopeOption.Required,
            //       new TransactionOptions
            //       {
            //           IsolationLevel = IsolationLevel.ReadUncommitted
            //       }))
            //{

                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    try
                    {
                        if ("".Equals(cPreCode))
                        {
                            SYS_SEQ sysDeq = db.SYS_SEQ.Where(x => x.SEQ_TYPE == cType).FirstOrDefault<SYS_SEQ>();
                        if (sysDeq == null)
                        {
                            sysDeq = new SYS_SEQ();
                            intseq = 1;
                            sysDeq.SEQ_TYPE = cType;
                            sysDeq.PRECODE = "";
                            sysDeq.CURR_VALUE = intseq + 1;
                            sysDeq.LAST_UPDATE_DT = DateTime.Now;

                            db.SYS_SEQ.Add(sysDeq);
                            cnt = db.SaveChanges();
                        }
                        else {
                            intseq = sysDeq.CURR_VALUE;

                            sysDeq.CURR_VALUE = intseq + 1;
                            sysDeq.LAST_UPDATE_DT = DateTime.Now;

                            cnt = db.SaveChanges();
                        }
                            
                        }
                        else
                        {


                            SYS_SEQ sysDeq = db.SYS_SEQ.Where(x => x.SEQ_TYPE == cType & x.PRECODE == cPreCode).FirstOrDefault<SYS_SEQ>();

                            if (sysDeq == null)
                            {

                                sysDeq = new SYS_SEQ();
                                intseq = 1;
                                sysDeq.SEQ_TYPE = cType;
                                sysDeq.PRECODE = cPreCode;
                                sysDeq.CURR_VALUE = intseq + 1;
                                sysDeq.LAST_UPDATE_DT = DateTime.Now;

                                db.SYS_SEQ.Add(sysDeq);
                                cnt = db.SaveChanges();

                            }
                            else
                            {
                                intseq = sysDeq.CURR_VALUE;

                                sysDeq.CURR_VALUE = intseq + 1;
                                sysDeq.LAST_UPDATE_DT = DateTime.Now;

                                cnt = db.SaveChanges();

                            }



                        }


                        return intseq;
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }

                }
            //}



          

        }
    }
}