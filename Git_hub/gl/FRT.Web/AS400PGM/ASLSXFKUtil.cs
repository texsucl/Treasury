using FRT.Web.AS400Models;
using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400PGM
{
    public class ASLSXFKUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<string, ASLSXFKModel> callASLSXFKUtil(EacConnection con, EacCommand cmd
            , string usrId, Dictionary<string, ASLSXFKModel> userMap) {

            try
            {
                cmd.Parameters.Clear();
                EacParameter p1 = new EacParameter();
                p1.ParameterName = "P0RTN";
                p1.DbType = DbType.String;
                p1.Size = 7;
                p1.Direction = ParameterDirection.Input;
                p1.Value = "       ";

                EacParameter p2 = new EacParameter();
                p2.ParameterName = "WP0001";
                p2.DbType = DbType.String;
                p2.Size = 1;
                p2.Direction = ParameterDirection.Input;
                p2.Value = "3";

                EacParameter p3 = new EacParameter();
                p3.ParameterName = "WP0002";
                p3.DbType = DbType.String;
                p3.Size = 1;
                p3.Direction = ParameterDirection.Input;
                p3.Value = usrId.PadRight(10, ' ');

                EacParameter p4 = new EacParameter();
                p4.ParameterName = "WP0003";
                p4.DbType = DbType.String;
                p4.Size = 1;
                p4.Direction = ParameterDirection.Input;
                p4.Value = " ";

                EacParameter p5 = new EacParameter();
                p5.ParameterName = "P4PARM";
                p5.DbType = DbType.String;
                p5.Size = 901;
                p5.Direction = ParameterDirection.Input;
                p5.Value = " ";


                ASLSXFKModel iASLSXFKModel = new ASLSXFKModel();
                iASLSXFKModel.wp0001 = "3";
                iASLSXFKModel.wp0002 = usrId;
                


                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                cmd.Parameters.Add(p4);
                cmd.Parameters.Add(p5);

                cmd.Prepare();
                cmd.ExecuteNonQuery();

                iASLSXFKModel.p4parm = stringToModel(cmd.Parameters["P4PARM"].Value.ToString());
                //string strRtn = cmd.Parameters["P4PARM"].Value.ToString();
                //ASLSXFKModel user = stringToModel(strRtn);
                userMap.Add(usrId, new ASLSXFKModel());

            }
            catch (Exception e) {
                logger.Error("usrId:" + usrId + "=" + e.ToString());
                userMap.Add(usrId, new ASLSXFKModel());
            }
            

            return userMap;
        }


        public ASLSXFKModel.P4PARM stringToModel(string input)
        {
            ASLSXFKModel.P4PARM d = new ASLSXFKModel.P4PARM();
            //input = StringUtil.toString(input).PadRight(901, ' ');


            d.p4a9wf = input.Substring(0, 6);
            d.p4a8wf = input.Substring(6, 10);
            d.p4bawf = input.Substring(16, 10);
            d.p4k5ig = input.Substring(26, 4);  //10->4 姓名
            d.p4bbwf = input.Substring(30, 1);
            d.p4bcwf = input.Substring(31, 1);
            d.p4bdwf = input.Substring(32, 8);
            d.p4bewf = input.Substring(40, 8);
            d.p4bfwf = input.Substring(48, 8);
            d.p4k6ig = input.Substring(56, 9); //20->9 職稱
            d.p4bgwf = input.Substring(65, 1);
            d.p4k7ig = input.Substring(66, 24); //50->24    學歷
            d.p4bhwf = input.Substring(90, 10);
            d.p4biwf = input.Substring(100, 18);
            d.p4k8ig = input.Substring(118, 39);    //80->39 戶籍地址
            d.p4k9ig = input.Substring(157, 39);    //80->39 通訊地址
            d.p4bjwf = input.Substring(196, 50);
            d.p4bkwf = input.Substring(246, 30);
            d.p4blwf = input.Substring(276, 5);
            d.p4bmwf = input.Substring(281, 12);
            d.p4bnwf = input.Substring(293, 12);
            d.p4buwf = input.Substring(305, 255);
            d.p4a3wf = input.Substring(560, 10);
            d.p4bpwf = input.Substring(570, 50);
            d.p4laig = input.Substring(620, 30);    //62->30 上班地點
            d.p4bqwf = input.Substring(650, 10);
            d.p4brwf = input.Substring(660, 50);
            d.p4bswf = input.Substring(710, 10);
            d.p4btwf = input.Substring(720, 6);
            d.p4a6wf = input.Substring(726, 8);
            d.p4a7wf = input.Substring(734, 10);

            return d;
        }

        //private string modelTostring(ASLSXFKModel d) {
        //    string output = "";

        //    output += d.portn.PadRight(6, ' ') +

        //  d.P4A9WF.PadRight(6, ' ') +
        //  d.P4A8WF.PadRight(10, ' ') +
        //  d.P4BAWF.PadRight(10, ' ') +
        //  d.P4K5IG.PadRight(10, ' ') +
        //  d.P4BBWF.PadRight(1, ' ') +
        //  d.P4BCWF.PadRight(1, ' ') +
        //  d.P4BDWF.PadRight(8, ' ') +
        //  d.P4BEWF.PadRight(8, ' ') +
        //  d.P4BFWF.PadRight(8, ' ') +
        //  d.P4K6IG.PadRight(20, ' ') +
        //  d.P4BGWF.PadRight(1, ' ') +
        //  d.P4K7IG.PadRight(50, ' ') +
        //  d.P4BHWF.PadRight(10, ' ') +
        //  d.P4BIWF.PadRight(18, ' ') +
        //  d.P4K8IG.PadRight(80, ' ') +
        //  d.P4K9IG.PadRight(80, ' ') +
        //  d.P4BJWF.PadRight(50, ' ') +
        //  d.P4BKWF.PadRight(30, ' ') +
        //  d.P4BLWF.PadRight(5, ' ') +
        //  d.P4BMWF.PadRight(12, ' ') +
        //  d.P4BNWF.PadRight(12, ' ') +
        //  d.P4BUWF.PadRight(255, ' ') +
        //  d.P4A3WF.PadRight(10, ' ') +
        //  d.P4BPWF.PadRight(50, ' ') +
        //  d.P4LAIG.PadRight(62, ' ') +
        //  d.P4BQWF.PadRight(10, ' ') +
        //  d.P4BRWF.PadRight(50, ' ') +
        //  d.P4BSWF.PadRight(10, ' ') +
        //  d.P4BTWF.PadRight(6, ' ') +
        //  d.P4A6WF.PadRight(8, ' ') +
        //  d.P4A7WF.PadRight(10, ' ') +

        //  d.WP0001.PadRight(1, ' ') +
        //  d.WP0002.PadRight(10, ' ') +
        //  d.WP0003.PadRight(1, ' ');
        //    return output;
        //}
    }
}