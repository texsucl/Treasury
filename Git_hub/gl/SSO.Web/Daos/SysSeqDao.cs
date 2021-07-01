
using SSO.Web.Models;
using System;
using System.Linq;

namespace SSO.Web.Daos
{
    public class SysSeqDao
    {
        public int qrySeqNo(string sysCd, string cType, string cPreCode)
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

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    try
                    {
                        if ("".Equals(cPreCode))
                        {
                            SYS_SEQ sysDeq = db.SYS_SEQ.Where(x =>  x.SYS_CD == sysCd & x.SEQ_TYPE == cType).FirstOrDefault<SYS_SEQ>();

                        if (sysDeq == null)
                        {
                            sysDeq = new SYS_SEQ();
                            intseq = 1;
                            sysDeq.SYS_CD = sysCd;
                            sysDeq.SEQ_TYPE = cType;
                            sysDeq.PRECODE = "";
                            sysDeq.CURR_VALUE = intseq + 1;
                            
                            db.SYS_SEQ.Add(sysDeq);
                            cnt = db.SaveChanges();
                        }
                        else {
                            intseq = sysDeq.CURR_VALUE;

                            sysDeq.CURR_VALUE = intseq + 1;
                            
                            cnt = db.SaveChanges();
                        }
                            
                        }
                        else
                        {


                            SYS_SEQ sysDeq = db.SYS_SEQ.Where(x => x.SYS_CD == sysCd & x.SEQ_TYPE == cType & x.PRECODE == cPreCode).FirstOrDefault<SYS_SEQ>();

                            if (sysDeq == null)
                            {

                                sysDeq = new SYS_SEQ();
                                intseq = 1;
                                sysDeq.SYS_CD = sysCd;
                                sysDeq.SEQ_TYPE = cType;
                                sysDeq.PRECODE = cPreCode;
                                sysDeq.CURR_VALUE = intseq + 1;
                                
                                db.SYS_SEQ.Add(sysDeq);
                                cnt = db.SaveChanges();

                            }
                            else
                            {
                                intseq = sysDeq.CURR_VALUE;
                                sysDeq.CURR_VALUE = intseq + 1;
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