using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace FAP.Web.BO
{
    public class  CommonUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetFormIp()
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);


            System.Web.HttpContext context = System.Web.HttpContext.Current;
            logger.Info("HTTP_X_FORWARDED_FOR:" + context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);

            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string ip = "";
            string IPAddrMachine = "";

            logger.Info("ipAddress:" + ipAddress);
            if (!string.IsNullOrEmpty(ipAddress))
            {
                
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                    ip = addresses[0];
            }
            else
                ip = context.Request.ServerVariables["REMOTE_ADDR"];

            try
            {
                logger.Info("ip:" + ip.Trim());
                IPHostEntry ipHostName = Dns.GetHostEntry(ip.Trim());
                IPAddrMachine = ipHostName.HostName.ToString();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());

            }


            return ip + "," + IPAddrMachine;

        }

        /// <summary>
        /// 取得使用者IP
        /// </summary>
        /// <returns></returns>
        public string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }


        /// <summary>
        /// 取得easycom連線資訊
        /// </summary>
        /// <returns></returns>
        public static String GetEasycomConn()
        {
            var connString = ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString;

            return connString;

        }


        /// <summary>
        /// print出物件的屬性及其值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string PropertyList(object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
            }
            return sb.ToString();
        }


        /// <summary>
        /// print出物件的屬性及其值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string printPropertyList(object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.Append(p.Name + ": " + p.GetValue(obj, null) + "|");
            }
            return sb.ToString();
        }


        /// <summary>
        /// 將LIST轉為dataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }



        public ADModel qryEmp(string usr_id)
        {
            ADModel adModel = new ADModel();
            adModel.user_id = StringUtil.toString(usr_id);

            if ("".Equals(adModel.user_id))
                return adModel;

            try
            {
                //先從DB_INTRA查員工檔
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    OaEmpDao oaEmpDao = new OaEmpDao();

                    V_EMPLY2 u = oaEmpDao.qryByUsrId(usr_id, dbIntra);

                    if (u != null) {
                        adModel.user_id = usr_id;
                        adModel.name = StringUtil.toString(u.EMP_NAME);
                        adModel.e_mail = StringUtil.toString(u.EMAIL);
                        adModel.department = StringUtil.toString(u.DPT_CD);
                    }

                }

                if (!"".Equals(adModel.name))
                    return adModel;


                //不在員工檔的人員，改查AD
                string ADPath = System.Configuration.ConfigurationManager.AppSettings.Get("ADPath");
                string ADUsr = System.Configuration.ConfigurationManager.AppSettings.Get("ADUsr");
                string ADPass = System.Configuration.ConfigurationManager.AppSettings.Get("ADPass");
                DirectoryEntry entry = new DirectoryEntry(ADPath, ADUsr, ADPass);


                string propUsername = "samaccountname";
                string propFirstName = "givenName";
                string propLastName = "sn";
                string propDisplayName = "cn";
                string propMail = "mail";
                string propDepartment = "department";
                string propCompany = "company";
                string propDepartmentNumber = "departmentNumber";
                string propDisplayname = "displayname";
                string propDescription = "description";
                string propMemberOf = "memberOf";

                using (DirectorySearcher search = new DirectorySearcher(entry))
                {
                    //set the list of properties you are interested in
                    //DOMAIN PATH PATTERN: CN=Name,CN=Directory,DC=Domain,DC=com or simply server name
                    //directorySearcher.PropertiesToLoad.Add(propUsername);
                    //directorySearcher.PropertiesToLoad.Add(propDisplayName);
                    //directorySearcher.PropertiesToLoad.Add(propFirstName);
                    //directorySearcher.PropertiesToLoad.Add(propLastName);
                    //directorySearcher.PropertiesToLoad.Add(propMail);
                    //directorySearcher.PropertiesToLoad.Add(propDepartmentNumber);
                    //directorySearcher.PropertiesToLoad.Add(propCompany);
                    //directorySearcher.PropertiesToLoad.Add(propDepartment);
                    //directorySearcher.PropertiesToLoad.Add(propDisplayname);
                    //directorySearcher.PropertiesToLoad.Add(propDescription);
                    //directorySearcher.PropertiesToLoad.Add(propMemberOf);



                   // DirectorySearcher search = new DirectorySearcher(entry);
                    //search.Filter = "(&(objectClass=user)(objectCategory=person))";
                    //search.Filter = "(&(objectClass=user))";
                    search.Filter = string.Format("({0})", "&(objectClass=user)(cn=" + usr_id + ")");
                    search.PropertiesToLoad.Add("samaccountname");
                    search.PropertiesToLoad.Add("displayname");
                    search.PropertiesToLoad.Add("mail");
                    search.PropertiesToLoad.Add("telephoneNumber");
                    search.PropertiesToLoad.Add("department");
                    search.PropertiesToLoad.Add("title");
                    search.PropertiesToLoad.Add("departmentNumber");
                    search.PropertiesToLoad.Add("memberOf");

                    SearchResultCollection results1 = search.FindAll();
                    if (results1 != null)
                    {
                        foreach (SearchResult result in results1)
                        {
                            foreach (DictionaryEntry property in result.Properties)
                            {
                                  //  logger.Info(property.Key + ": " );
                                //  Debug.Write(property.Key + ": ");
                                foreach (var val in (property.Value as ResultPropertyValueCollection))
                                {
                                    adModel.user_id = usr_id;
                                    switch (property.Key.ToString()) {
                                        case "displayname":
                                            adModel.name = val.ToString();
                                            break;
                                        case "mail":
                                            adModel.e_mail = val.ToString();
                                            break;
                                        case "department":
                                            adModel.department = val.ToString();
                                            break;
                                    }

                                    logger.Info(property.Key + ": " + " - " + val + ": ");
                                    // Debug.Write(val + "; ");
                                }


                            }
                        }
                        logger.Info("==============================");
                    }


                    //////Set a filters to get only the active users from AD
                    //search.Filter = "(&(objectClass=user)(objectCategory=person))";
                    //directorySearcher.Filter = string.Format("({0})", "&(objectClass=user)(cn=" + usr_id + ")");

                    ////Set Search Options
                    //directorySearcher.SearchScope = SearchScope.Subtree;
                    //directorySearcher.SearchRoot.AuthenticationType = AuthenticationTypes.Secure;
                    //directorySearcher.PageSize = 100;

                    ////run the search and and it will a collection of the entries that are found.
                    //using (SearchResultCollection results = directorySearcher.FindAll())
                    //{
                    //    foreach (SearchResult result in results)
                    //    {
                    //        //get poperties and write them to the console
                    //        if (result.Properties.Contains(propUsername))
                    //            logger.Info("User Name: " + result.Properties[propUsername][0]);

                    //        if (result.Properties.Contains(propDepartmentNumber))
                    //            logger.Info("propDepartmentNumber: " + result.Properties[propDepartmentNumber][0]);

                    //        if (result.Properties.Contains(propMail))
                    //        {
                    //            adModel.e_mail = result.Properties[propMail][0].ToString();
                    //            logger.Info("Mail ID: " + result.Properties[propMail][0]);
                    //        }

                    //        if (result.Properties.Contains(propDisplayName))
                    //        {
                    //            adModel.name = result.Properties[propDisplayName][0].ToString();
                    //            logger.Info("DisplayName: " + result.Properties[propDisplayName][0]);
                    //        }

                    //        if (result.Properties.Contains(propCompany))
                    //            logger.Info("propCompany: " + result.Properties[propCompany][0]);


                    //        if (result.Properties.Contains(propDepartment))
                    //        {
                    //            adModel.department = result.Properties[propDepartment][0].ToString();
                    //            logger.Info("department: " + result.Properties[propDepartment][0]);
                    //        }


                    //        if (result.Properties.Contains(propDescription))
                    //            logger.Info("propDescription: " + result.Properties[propDescription][0]);


                    //        if (result.Properties.Contains(propMemberOf))
                    //            logger.Info("propMemberOf: " + result.Properties[propMemberOf][0]);


                    //    }
                    //}
                    //release resources
                    search.Dispose();
                    entry.Dispose();
                }

                return adModel;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                //throw e;
                return adModel;
            }

        }


    }
}