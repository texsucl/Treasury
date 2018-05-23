//using Treasury.WebModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace Treasury.WebDaos
//{
//    public class SysParaDao
//    {
//        public String qryByParaId(String cParaId)
//        {
//            String cParaValue = "";
//            DbAccountEntities context = new DbAccountEntities();

//            var result = context.SysPara.Where(x => x.para_id == cParaId ).FirstOrDefault();
//            if (result != null)
//                cParaValue = result.para_value;

//            return cParaValue;
//        }
//    }
//}