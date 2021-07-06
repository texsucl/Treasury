using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication5
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("20190627000000000100000737102710668   " + ToWide("富邦人壽借款") + "                                                  737168559898     " + ToWide("林虹淇") + "                                                     0127370                                                                                                                                                        15 Q020800888                                                                                                                             A224406990  ");
                sb.AppendLine("20190627000000000200000737102710668   " + ToWide("富邦人壽借款") + "                                                  737168058856     " + ToWide("陳秉潔") + "                                                     0127370                                                                                                                                                        15 Q020800889                                                                                                                             F223338916  ");
                //sb.AppendLine("20190627000000000300000737102710668   " + ToWide("富邦人壽借款") + "                                                  470168113672     " + ToWide("莊惠鈞") + "                                                     0124700                                                                                                                                                        15 Q020800890                                                                                                                             F227403745  ");
                sb.AppendLine("20190627000000000400000737102710668   " + ToWide("富邦人壽借款") + "                                                  737168512581     " + ToWide("曾亦幼") + "                                                     0127370                                                                                                                                                        15 Q020800891                                                                                                                             A227668367  ");


                var _filler_20 = "20190620173907";
                string datFile = @"D:\FRT.Web\Dat"; //dat資料夾
                //string datFile = @"D:\Git\gl\FRT.Web\Dat"; //dat資料夾
                var txtFileName = $@"FBOQ11_{_filler_20.Split('*')[0]}";
                string path = Path.Combine(datFile, $@"{txtFileName}.txt");
                //System.IO.File.WriteAllText(path, sb.ToString(), Encoding.GetEncoding(950));


                string pw = "FBOTEST97041";
                ProcessStartInfo processInfo;
                processInfo = new ProcessStartInfo();
                processInfo.FileName = Path.Combine(datFile, "FBO.bat");
                var a = $"{pw} {$@"{txtFileName}.txt"}";
                processInfo.Arguments = a;
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                Process process;
                process = Process.Start(processInfo);
                process.WaitForExit();

                //byte[] big5 = Encoding.GetEncoding(950).GetBytes(ToWide("富邦人壽借款"));
                //Console.WriteLine(Encoding.GetEncoding(65001).GetString(big5));
                //byte[] big6 = Encoding.Default.GetBytes("撖鈭箏ˊ?狡");
                //Console.WriteLine(Encoding.Default.GetString(big6));

                //StringBuilder sb = new StringBuilder();
                //string source = "富邦人壽借款";

                //foreach (var e1 in Encoding.GetEncodings())
                //{
                //    foreach (var e2 in Encoding.GetEncodings())
                //    {
                //        byte[] unknow = Encoding.GetEncoding(e1.CodePage).GetBytes(source);
                //        string result = Encoding.GetEncoding(e2.CodePage).GetString(unknow);
                //        if(result == "撖鈭箏ˊ?狡")
                //        sb.AppendLine(string.Format("{0} => {1} : {2}", e1.CodePage, e2.CodePage, result));
                //        //65001 950
                //    }
                //}
                //Console.WriteLine(sb.ToString());
                //File.WriteAllText("test.txt", sb.ToString());

                Console.ReadLine();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ///字串轉全形
        ///</summary>
        ///<param name="input">任一字元串</param>
        ///<returns>全形字元串</returns>
        public static string ToWide(string input)
        {
            //半形轉全形：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                //全形空格為12288，半形空格為32
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }
    }

}
