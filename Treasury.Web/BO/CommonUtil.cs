﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Treasury.WebBO
{
    public class CommonUtil
    {

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


    }
}