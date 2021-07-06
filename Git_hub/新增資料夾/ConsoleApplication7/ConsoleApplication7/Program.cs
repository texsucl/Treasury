using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication7
{
    class Program
    {
        public class a { 
        public string b { get; set; }
        public string c { get; set; }
        }

        public class z {
            public string y { get; set; }
            public List<a> x { get; set; }
        }

        static void Main(string[] args)
        {
            TimeSpan ts = new TimeSpan();
            ts = new TimeSpan(23, 52, 0);
            Console.WriteLine(ts);
            Console.WriteLine(ts.ToString(@"hh\:mm"));




            //DateTime dtn = DateTime.Now;
            //dtn = dtn.AddYears(5);
            //DateTime dtn = DateTime.Parse("1900/01/01");
            //Console.WriteLine(dtn.ToString("yyMMdd"));


            //int a = 3;
            //for (var i = a; i > 0; i--)
            //{
            //    Console.WriteLine(i);
            //}

            //List<a> qq = new List<a>() {
            //new a(){ b = "1",c="2"},
            //new a(){ b = "2",c="3"}
            //};
            //z zz = new z() { y = "1" };
            //zz.x = qq;

            //Console.WriteLine(Convert.ToInt16(Math.Floor((decimal)8100 / 82)));
            //string aa = "4695AB6F";
            //var bb = (aa.Substring(0, aa.Length - 1));
            //bb += "A";
            //Console.WriteLine(bb);
            //List<string> a = new List<string>() {null,"12","234", "KG9876543","KG9876542","KG9976012" };
            //Console.WriteLine(string.Join(",", a.Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 3)
            //    .Take(10)
            //    .Select(x => x.Substring(x.Length - 3, 3))));

            //var a = formateThousand("-0.76");

            //List<a> _a = new List<a>() { new a() { b = "b", c = "c" }, new a() { b = "b1", c = "c1" } };

            //foreach (var item in _a)
            //{
            //    _a.Remove(item);
            //}
            //Console.WriteLine(_a.Count);

            //SNIC 組合方法 yyyyMMdd + 轉出帳號 + SNIC + '0000'
            //CHREncoding($@"{"20191127"}{"0000737102710668"}{"7154151ILIQ07082700B"}{"0000"}");
            //Console.WriteLine(DateTime.Now.ToString("HHmmssf"));
            //List<string> s = new List<string>() { "1", "2", "3", "4" };
            //var q =  new List<string>();

            //for (var i = 0; i < s.Count; i++)
            //{
            //    q.Add(s[i]);
            //    Console.WriteLine(s[i]);
            //}
            //q.Add("5");
            //foreach (var t in s)
            //{
            //    Console.WriteLine(t);
            //}
            //foreach (var t in q)
            //{
            //    Console.WriteLine(t);
            //}
            //int _ds = 201810;
            //int _de = 201903;
            //List<string> _dv = new List<string>();
            //for (var i = _ds; i <= _de; i++)
            //{
            //    var _i = i.ToString();
            //    if (_i.Substring(_i.Length - 2, 2) == "13")
            //    {
            //        i = i - 12 + 100;
            //    }
            //    _dv.Add(i.ToString());
            //    Console.WriteLine(i.ToString());
            //}
            //TaskContinueDemo();
            //Task.Delay(5000).Wait();
            //foreach (var item in (new Int32[] { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 }))
            //{
            //    numbers.Add(item);
            //}
            //TaskContinueDemo();
            //Task.Delay(20000).Wait();
            //foreach (var item in (new Int32[] { 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 }))
            //{
            //    numbers.Add(item);
            //}
            //TaskContinueDemo();
            //Task.Delay(1000).Wait();
            //foreach (var item in (new Int32[] { 40, 41, 42, 43, 44, 45, 46, 47, 48, 49 }))
            //{
            //    numbers.Add(item);
            //    TaskContinueDemo();
            //}
            //Console.WriteLine(a);
            //Console.WriteLine(DateTime.Now.AddDays(-1).Date);
            //var a = Test();
            //a.Wait();
            //Console.WriteLine("End" + a.Result);
            Console.ReadLine();
        }

        private static Task<bool> Test()
        {
            //
            Console.WriteLine("wait 10s");
            Task.Delay(10000).Wait();
            if (currentCount != 0)
            {
                Console.WriteLine("繼續等待");
                return Test();
            }
            else
                return new Task<bool>(() => false);
        }

        public static string formateThousand(string value)
        {
            decimal d = 0;
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            try
            {
                if (decimal.TryParse(value, out d))
                {
                    value = Math.Round(d, 2).ToString();
                    if (value.IndexOf(".") > -1)
                    {
                        var _val = value.Substring(0, value.IndexOf("."));
                        var _valFlag = string.Empty;
                        if (_val == "-0")
                            _valFlag = "-";
                        Int64 strNumberWithoutDecimals = Convert.ToInt64(_val.Replace(",", ""));
                        string strNumberDecimals = value.Substring(value.IndexOf("."));
                        return $@"{_valFlag}{strNumberWithoutDecimals.ToString("#,##0")}{strNumberDecimals}";
                    }
                    return Convert.ToInt64(value.Replace(",", "")).ToString("#,##0");
                }
            }
            catch (Exception ex)
            {

            }

            return value;
        }


        static object lockObj = new object();
        static object lockObj2 = new object();
        static int maxTask = 5;
        static int currentCount = 0;
        //假设要处理的数据源
        static List<int> numbers = Enumerable.Range(5, 10).ToList();
        private static void TaskContinueDemo()
        {
            while ((currentCount < maxTask && numbers.Count > 0 ) || (currentCount != 0))
            {
                lock (lockObj)
                {
                    if (currentCount < maxTask && numbers.Count > 0)
                    {                                                                          
                        Interlocked.Increment(ref currentCount);
                        var task = Task.Factory.StartNew(() =>
                        {              
                            if (numbers.Any())
                            {
                                var number = 0;
                                lock (lockObj2)
                                {
                                    number = numbers.FirstOrDefault();
                                    numbers.Remove(number);
                                }
                                var sleepTime = Rand(5) + 1;
                                Console.WriteLine($@"start dealNumber:{number} , sleepTime:{sleepTime}s , currentCount => {currentCount} , dtn:{DateTime.Now} ");
                                Task.Delay(sleepTime * 1000).Wait();
                                Console.WriteLine($@"End dealNumber:{number} , sleepTime:{sleepTime}s , currentCount => {currentCount} , dtn:{DateTime.Now}");
                            }
                        }, TaskCreationOptions.LongRunning).ContinueWith(t =>
                        {//在ContinueWith中恢复计数
                            Interlocked.Decrement(ref currentCount);
                            Console.WriteLine($@"ContinueWith  currentCount => {currentCount} , dtn:{DateTime.Now}");
                            //Console.WriteLine("Continue Task id {0} Time{1} currentCount{2}", Task.CurrentId, DateTime.Now, currentCount);
                            TaskContinueDemo();
                        });
                    }
                }
            }
        }

        private static int Rand(int maxNumber = 5)
        {
            return Math.Abs(Guid.NewGuid().GetHashCode()) % maxNumber;
        }

        public static string CHREncoding(string value)
        {
            string str = string.Empty;
            char[] NumChar = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            foreach (char C in value)
            {
                if (NumChar.Contains(C))  //數字
                {
                    var _str = (BitConverter.ToString(Encoding.ASCII.GetBytes(C.ToString()))).Replace("-", "");
                    Console.WriteLine($@"{C} => {_str}");
                    str += _str;
                }
                else  //文字
                {
                    var _str = (BitConverter.ToString(Encoding.GetEncoding(500).GetBytes(C.ToString()))).Replace("-", "");
                    Console.WriteLine($@"{C} => {_str}");
                    str += _str;
                }
            }
            return str;
        }
    }
}
