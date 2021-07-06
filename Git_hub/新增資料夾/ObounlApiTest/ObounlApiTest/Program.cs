using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObounlApiTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            testApi();
        }

        private static void testApi()
        {

            using (var client1 = new HttpClient())
            {
                
                client1.BaseAddress = new Uri("http://10.241.17.48:8080/");
                client1.DefaultRequestHeaders.Accept.Clear();
                client1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                
                string json2 = "{'id':'','inputType':'A','unitCode':'TP788','unitSeq':'0000','workYmStart':'202001','workYmEnd':'202009','agentId':['C221185665'] }";
                 client1.DefaultRequestHeaders.Add("System-Name", "SAV");

                StringContent content = new StringContent(json2, Encoding.UTF8, "application/json");
                // HTTP POST
                HttpResponseMessage response = client1.PostAsync("lydia/rest/personnel/queryStarBinary", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    //string data = response.Content.ReadAsStringAsync().Result;
                    var a = response.Content.ReadAsByteArrayAsync().Result;
                    var s = Encoding.UTF8.GetString(a);
                }
            }


            HttpClient client = new HttpClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
            client.BaseAddress = new System.Uri("https://cs.fbl.com.tw/cswr/Obounl/");
            //client.BaseAddress = new System.Uri("https://localhost:44369/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //資料可以自行改
            var _data = new Tel_Visit_InputData()
            {
                CaseNo = "1234567890",
                UploadDate = "2020/02/03",
                SysID = "1",
                PolicyCases = new List<Contract>() {
                    new Contract(){
                        Context_C = "商品中文名稱",
                        PlanDesc = "VEGN",
                        YAmtType = "Y/N",
                        UAmtType = "true",
                        PolicyYear = 6,
                        PolicyCurType = "幣別代碼",
                        ProductPay = "繳別代碼",
                        PolicyAmt = 1000000,
                        PolicyCost = 10000,
                        PayMethod = "繳款方式",
                        SubmitDate = "2020/01/01",
                        ContactMemo = "Q",
                        CampCondition = "3",
                        CustLevelLoan = true,
                        CustLevelChangeItem = true,
                        PayAmountSourceQ = "1",
                        PayAmountSourceA = "2",
                        InsurancePolicyQ = "3",
                        InsurancePolicyA = "4",
                        Customers = new List<Customer>(){
                               new Customer(){
                                 CustID = "C123456789",
                                 CustName = "tett",
                                 CustGen = "M",
                                 CustBirthday = "2020/01/01",
                                 CustEmail = "abcde.fgh@fubon.com",
                                 CustAddr = "台北市建國北路二段234號",
                                 CustRecvAddr = "台北市建國北路二段123號",
                                 CustMoblie = "0987654321",
                                 CustTEL = "012345678",
                                 CustRecvTEL = "012345",
                                 CustContGen = "6",
                                 CustMenuValue = "母",
                                 CustVisitTimeS = 9,
                                 CustVisitTimeE = 13,
                                 CustIsfriendly = true,
                                 PolicyBenfit = new List<PolicyBenfit>(){
                                     new PolicyBenfit(){
                                         TrustSeq = "1",
                                         TrustName = "XXX",
                                         TrustRate = "100",
                                         TrustRelate = "父子",
                                         TrustType = "0",
                                     },
                                     new PolicyBenfit(){
                                         TrustSeq = "2",
                                         TrustName = "OXX",
                                         TrustRate = "100",
                                         TrustRelate = "母子",
                                         TrustType = "0",
                                     },
                                 }
                            },
                            // new Customer(){
                            //     CustID = "D123456789",
                            //     CustName = "tsss",
                            //     CustGen = "M",
                            //     CustBirthday = "2020/01/01",
                            //     CustEmail = "abcde.fgh@fubon.com",
                            //     CustAddr = "台北市建國北路二段234號",
                            //     CustRecvAddr = "台北市建國北路二段123號",
                            //     CustMoblie = "0987654321",
                            //     CustTEL = "012345678",
                            //     CustRecvTEL = "012345",
                            //     CustContGen = "1",
                            //     CustMenuValue = "母",
                            //     CustVisitTimeS = 9,
                            //     CustVisitTimeE = 13,
                            //     CustIsfriendly = true
                            //},
                            //new Customer(){
                            //     CustID = "A123456789",
                            //     CustName = "ooyy",
                            //     CustGen = "M",
                            //     CustBirthday = "2020/01/01",
                            //     CustEmail = "abcde.fgh@fubon.com",
                            //     CustAddr = "台北市建國北路二段234號",
                            //     CustRecvAddr = "台北市建國北路二段123號",
                            //     CustMoblie = "0987654321",
                            //     CustTEL = "012345678",
                            //     CustRecvTEL = "012345",
                            //     CustContGen = "5",
                            //     CustMenuValue = "父",
                            //     CustVisitTimeS = 9,
                            //     CustVisitTimeE = 13,
                            //     CustIsfriendly = true
                            //},
                            // new Customer(){
                            //     CustID = "B123456789",
                            //     CustName = "test2",
                            //     CustGen = "性別代碼",
                            //     CustBirthday = "2020/01/01",
                            //     CustEmail = "abcde.fgh@fubon.com",
                            //     CustAddr = "台北市建國北路二段234號",
                            //     CustRecvAddr = "台北市建國北路二段123號",
                            //     CustMoblie = "0987654321",
                            //     CustTEL = "012345678",
                            //     CustRecvTEL = "012345",
                            //     CustContGen = "2",
                            //     CustMenuValue = "兄弟",
                            //     CustVisitTimeS = 9,
                            //     CustVisitTimeE = 13,
                            //     CustIsfriendly = true
                            //},
                        }
                    }
                },
                Agent = new List<Agent>() {
                    new Agent(){
                        AgentID = "A111111111",
                        AgentName = "業務員一號",
                        AgentUnitCode = "A001",
                        AgentUnitName = "AAAAA"
                    },
                    new Agent(){
                        AgentID = "B222222222",
                        AgentName = "業務員二號",
                        AgentUnitCode = "A002",
                        AgentUnitName = "BBBBB"
                    }
                }
            };
            //var sss = AESEncrypt(JsonConvert.SerializeObject(_data));
            //var _data2 = new Tel_Visit_InputData()
            //{
            //    CaseNo = "ee77b4b4-34c4-491f-ba8f-0d17b38b67cc",
            //    UploadDate = "2020/10/19",
            //    SysID = "1",
            //    PolicyCases = new List<Contract>()
            //     {
            //        new Contract(){
            //        Context_C = "安福一生終身健康保險",
            //        PlanDesc = "HIX1",
            //        YAmtType = "H",
            //        UAmtType = "H",
            //        PolicyYear = 20,
            //        PolicyCurType = "NTD",
            //        ProductPay = "A",
            //        PolicyAmt = 800,
            //        PolicyCost = 1000,
            //        PayMethod = "5",
            //        SubmitDate = "",
            //        ContactMemo = "",
            //        CampCondition = "1",
            //        CustLevelLoan = true,
            //        CustLevelChangeItem = false,
            //        PayAmountSourceQ = "保險費支出來源",
            //        PayAmountSourceA = "1.薪資",
            //        InsurancePolicyQ = "要保人及被保險人投保目的及保險需求",
            //        InsurancePolicyA = "1.保障",
            //        Customers = new List<Customer>(){
            //           new Customer(){
            //           CustID = "A123456789",
            //           CustName = "EASON",
            //           CustGen = "M",
            //           CustBirthday = "1998/08/06",
            //           CustEmail = "",
            //           CustAddr = "６５３，雲林縣，口湖鄉，１１１號１樓",
            //           CustRecvAddr = null,
            //           CustMoblie = "0999999995",
            //           CustTEL = "02-25158050#",
            //           CustRecvTEL = "",
            //           CustContGen = "1",
            //           CustMenuValue = "本人",
            //           CustVisitTimeS = 0,
            //           CustVisitTimeE = 0,
            //           CustIsfriendly = false,
            //           PolicyBenfit = null
            //           },
            //           new Customer()
            //           {
            //              CustID = "A123456789",
            //              CustName = "EASON",
            //              CustGen = "M",
            //              CustBirthday = "1998/08/06",
            //              CustEmail = "",
            //              CustAddr = "６５３，雲林縣，口湖鄉，１１１號１樓",
            //              CustRecvAddr = "６５３，雲林縣，口湖鄉，１１１號１樓",
            //              CustMoblie = "0999999995",
            //              CustTEL = "02-25158050#",
            //              CustRecvTEL = "",
            //              CustContGen = "2",
            //              CustMenuValue = "本人",
            //              CustVisitTimeS = 0,
            //              CustVisitTimeE = 0,
            //              CustIsfriendly = false,
            //              PolicyBenfit = new List<PolicyBenfit>(){
            //                  new PolicyBenfit(){
            //                     TrustSeq = null,
            //                     TrustRate = null,
            //                     TrustRelate = null,
            //                     TrustType = null,
            //                     TrustName = null
            //                  }
            //              }
            //           },
            //           new Customer(){
            //              CustID = "A123456789",
            //              CustName = "EASON",
            //              CustGen = "M",
            //              CustBirthday = "1998/08/06",
            //              CustEmail = "",
            //              CustAddr = "６５３，雲林縣，口湖鄉，１１１號１樓",
            //              CustRecvAddr = null,
            //              CustMoblie = "0999999995",
            //              CustTEL = "02-25158050#",
            //              CustRecvTEL = "",
            //              CustContGen = "3",
            //              CustMenuValue = "本人",
            //              CustVisitTimeS = 0,
            //              CustVisitTimeE = 0,
            //              CustIsfriendly = false,
            //              PolicyBenfit = null
            //               }
            //           }
            //        }
            //    },
            //      Agent = new List<Agent>() {
            //       new Agent(){
            //       AgentID = "U220171224",
            //       AgentName = "范Ｘ萍",
            //       AgentUnitCode = "TP906",
            //       AgentUnitName = "富好通訊處"
            //       }
            //     }
            //};

            //CaseNo => CASE_ID  PolicyAmt 800 元 , TrustName => TrustKind

            //client.DefaultRequestHeaders.Add("token", getToken()); //token
            //var response = client.PostAsJsonAsync(@"Api/ObounlApi/Contract", AESEncrypt(JsonConvert.SerializeObject(_data)), CancellationToken.None).Result;

            //var response = client.PostAsJsonAsync(@"Api/ObounlApi/Contract", "tokenkey").Result;

            //Console.WriteLine($@"Contract IsSuccessStatusCode:{response.IsSuccessStatusCode}");
            //if (response.IsSuccessStatusCode)
            //{
            //    Tel_Visit_OutputData data = JsonConvert.DeserializeObject<Tel_Visit_OutputData>(response.Content.ReadAsStringAsync().Result);
            //    Console.WriteLine($@"Status:{data.Status}");
            //    Console.WriteLine($@"Code:{data.Code}");
            //    Console.WriteLine($@"Msg:{data.Msg}");
            //    Console.WriteLine($@"TokenKey:{data.TokenKey}");
            //}
            //else
            //{
            //    var message = string.Format("Status Code : {0}", response.StatusCode);
            //    Console.WriteLine($@"StatusCode:{response.StatusCode}");
            //}
            Console.ReadLine();
        }

        private static string getToken()
        {
            var token = string.Empty;
            HttpClient client = new HttpClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
            //client.BaseAddress = new System.Uri("https://cs.fbl.com.tw/cswr/Obounl/");
            client.BaseAddress = new System.Uri("https://localhost:44369/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Add("token", "fbim"); //token
            var response = client.PostAsJsonAsync(@"Api/ObounlApi/GetT", "").Result;

            Console.WriteLine($@"GET_Token IsSuccessStatusCode:{response.IsSuccessStatusCode}");
            if (response.IsSuccessStatusCode)
            {
                Token_OutputData data = JsonConvert.DeserializeObject<Token_OutputData>(response.Content.ReadAsStringAsync().Result);
                Console.WriteLine($@"Status:{data.Status}");
                if (data.Status == "success")
                    token = data.TokenKey;
            }
            else
            {
                var message = string.Format("Status Code : {0}", response.StatusCode);
                Console.WriteLine($@"StatusCode:{response.StatusCode}");
            }
            return token;
        }

        public class Token_OutputData
        {
            /// <summary>
            /// 處理狀態  (success：完成、fail：失敗)
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// TokenKey
            /// </summary>
            public string TokenKey { get; set; }
        }

        public class Tel_Visit_InputData
        {
            /// <summary>
            /// 要保書編號
            /// </summary>
            public string CaseNo { get; set; }

            /// <summary>
            /// 電訪抽件日 要保書上傳日Date Format：yyyy/MM/dd
            /// </summary>
            public string UploadDate { get; set; }

            /// <summary>
            /// 系統別 通路來源代碼 1：業務服展 2：整銷 3：保經代
            /// </summary>
            public string SysID { get; set; }

            /// <summary>
            /// 合約 要保書資料多筆
            /// </summary>
            public List<Contract> PolicyCases { get; set; }

            /// <summary>
            /// 業務員 業務員資料多筆
            /// </summary>
            public List<Agent> Agent { get; set; }
        }

        /// <summary>
        /// 合約內容
        /// </summary>
        public class Contract
        {
            /// <summary>
            /// 商品名稱 ProductName
            /// </summary>
            public string Context_C { get; set; }

            /// <summary>
            /// 險種 險種代碼
            /// </summary>
            public string PlanDesc { get; set; }

            /// <summary>
            /// 年金險/壽險 A：意外險、D：死亡險、E：養老險、H：健康險、P：生存險、Y：年金險
            /// </summary>
            public string YAmtType { get; set; }

            /// <summary>
            /// 投資型/非投資型 U：投資型、其它則為非投資型
            /// </summary>
            public string UAmtType { get; set; }

            /// <summary>
            /// 繳費年期 年期
            /// </summary>
            public int PolicyYear { get; set; }

            /// <summary>
            /// 幣別 幣別代碼
            /// </summary>
            public string PolicyCurType { get; set; }

            /// <summary>
            /// 繳別 繳別代碼
            /// </summary>
            public string ProductPay { get; set; }

            /// <summary>
            /// 總保額
            /// </summary>
            public int PolicyAmt { get; set; }

            /// <summary>
            /// 應繳保費
            /// </summary>
            public int PolicyCost { get; set; }

            /// <summary>
            /// 繳款方式
            /// </summary>
            public string PayMethod { get; set; }

            /// <summary>
            /// 契約始期 Date Format：yyyy/MM/dd
            /// </summary>
            public string SubmitDate { get; set; }

            /// <summary>
            /// 電訪特殊註記 業報書上業務員的特殊註記
            /// </summary>
            public string ContactMemo { get; set; }

            /// <summary>
            /// 電訪條件 1(貸款/借款: True, 解約:  False), 2(貸款/借款: False, 解約: True), 3(貸款/借款: True, 解約: True)
            /// </summary>
            public string CampCondition { get; set; }

            /// <summary>
            /// 貸款/借款
            /// </summary>
            public bool CustLevelLoan { get; set; }

            /// <summary>
            /// 解約
            /// </summary>
            public bool CustLevelChangeItem { get; set; }

            /// <summary>
            /// 保費來源題目
            /// </summary>
            public string PayAmountSourceQ { get; set; }

            /// <summary>
            /// 保費來源回應
            /// </summary>
            public string PayAmountSourceA { get; set; }

            /// <summary>
            /// 投保目的題目
            /// </summary>
            public string InsurancePolicyQ { get; set; }

            /// <summary>
            /// 投保目的回應
            /// </summary>
            public string InsurancePolicyA { get; set; }

            /// <summary>
            /// 客戶資料
            /// </summary>
            public List<Customer> Customers { get; set; }
        }

        /// <summary>
        /// 客戶資料
        /// </summary>
        public class Customer
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public string CustID { get; set; }

            /// <summary>
            /// 客戶ID
            /// </summary>
            public string CustName { get; set; }

            /// <summary>
            /// 客戶姓別
            /// </summary>
            public string CustGen { get; set; }

            /// <summary>
            /// 客戶生日 Date Format：yyyy/MM/dd
            /// </summary>
            public string CustBirthday { get; set; }

            /// <summary>
            /// Email
            /// </summary>
            public string CustEmail { get; set; }

            /// <summary>
            /// 戶籍地址
            /// </summary>
            public string CustAddr { get; set; }

            /// <summary>
            /// 收費地址
            /// </summary>
            public string CustRecvAddr { get; set; }

            /// <summary>
            /// 行動電話
            /// </summary>
            public string CustMoblie { get; set; }

            /// <summary>
            /// 行動電話
            /// </summary>
            public string CustTEL { get; set; }

            /// <summary>
            /// 收費電話
            /// </summary>
            public string CustRecvTEL { get; set; }

            /// <summary>
            /// 角色代碼  CodeDesc角色名稱（要保人、被保險人、實際繳款人、要保人法代、被保險人法代、實際繳款人法代）
            /// </summary>
            public string CustContGen { get; set; }

            /// <summary>
            /// 受訪者與要被保人關係 MenuText關係名稱
            /// </summary>
            public string CustMenuValue { get; set; }

            /// <summary>
            /// 電訪時間(起)
            /// </summary>
            public int CustVisitTimeS { get; set; }

            /// <summary>
            /// 電訪時間(迄)
            /// </summary>
            public int CustVisitTimeE { get; set; }

            /// <summary>
            /// 有善服務
            /// </summary>
            public bool CustIsfriendly { get; set; }

            /// <summary>
            /// 受益人
            /// </summary>
            public List<PolicyBenfit> PolicyBenfit { get; set; }
        }

        /// <summary>
        /// 業務員內容
        /// </summary>
        public class Agent
        {
            /// <summary>
            /// 業務員ID
            /// </summary>
            public string AgentID { get; set; }

            /// <summary>
            /// 業務員姓名
            /// </summary>
            public string AgentName { get; set; }

            /// <summary>
            /// 單位代號
            /// </summary>
            public string AgentUnitCode { get; set; }

            /// <summary>
            /// 單位名稱
            /// </summary>
            public string AgentUnitName { get; set; }
        }

        /// <summary>
        /// 受益人資料
        /// </summary>
        public class PolicyBenfit
        {
            /// <summary>
            /// 受益人順位
            /// </summary>
            public string TrustSeq { get; set; }

            /// <summary>
            /// 受益人姓名
            /// </summary>
            public string TrustName { get; set; }

            /// <summary>
            /// 受益人比例
            /// </summary>
            public string TrustRate { get; set; }

            /// <summary>
            /// 受益人關係
            /// </summary>
            public string TrustRelate { get; set; }

            /// <summary>
            /// 受益人類型
            /// </summary>
            public string TrustType { get; set; }
        }

        public class Tel_Visit_OutputData
        {
            /// <summary>
            /// 處理狀態  (success：完成、fail：失敗、error：錯誤)
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// 結果代碼 (提供AP判斷錯誤點的代碼)
            /// </summary>
            public string Code { get; set; }

            /// <summary>
            /// 回傳訊息
            /// </summary>
            public string Msg { get; set; }

            public string TokenKey { get; set; }
        }

        public static string AESEncrypt(string str)
        {
            string _key = "fbim";
            str = str ?? "test";
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(_key.Substring(0, (_key.Length > 32) ? 32 : _key.Length).PadLeft(32, '0'));
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(str);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            var encryptStr = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            encryptStr = Uri.UnescapeDataString(encryptStr);
            return encryptStr;
        }
    }
}