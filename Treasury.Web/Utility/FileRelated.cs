using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

//using MExcel = Microsoft.Office.Interop.Excel;
namespace Treasury.WebUtility
{
    public static class FileRelated
    {
        #region 上傳檔案到指定路徑

        /// <summary>
        /// 檔案上傳
        /// </summary>
        /// <param name="path">檔案放置位置</param>
        /// <param name="file">檔案</param>
        /// <returns></returns>
        public static MSGReturnModel<T> FileUpLoadinPath<T>(string path, HttpPostedFileBase file)
        {
            MSGReturnModel<T> result = new MSGReturnModel<T>();
            result.RETURN_FLAG = true;
            try
            {
                using (var fileStream = new FileStream(path,
                     FileMode.Create, FileAccess.Write,FileShare.Write))
                {
                    file.InputStream.CopyTo(fileStream); //資料複製一份到FileUploads,存在就覆寫
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = MessageType.upload_Fail
                    .GetDescription(null, ex.Message);
            }
            return result;
        }

        #endregion 上傳檔案到指定路徑

        #region Create 資料夾

        /// <summary>
        /// Create 資料夾(判斷如果沒有的話就新增)
        /// </summary>
        /// <param name="projectFile">資料夾位置</param>
        public static void createFile(string projectFile)
        {
            try
            {
                bool exists = Directory.Exists(projectFile);
                if (!exists) Directory.CreateDirectory(projectFile);
            }
            catch
            { }
        }

        #endregion Create 資料夾

        #region Download Excel

        /// <summary>
        /// 將 DataTable 資料轉換至 Excel
        /// </summary>
        /// <param name="thisTable">欲轉換之DataTable</param>
        /// <param name="path">檔案放置位置</param>
        /// <param name="sheetName">寫入之sheet名稱</param>
        /// <returns>失敗時回傳錯誤訊息</returns>
        public static string DataTableToExcel(DataTable dt, string path, ExcelName type, List<FormateTitle> titles = null)
        {
            string result = string.Empty;

            try
            {
                string version = "2003"; //default 2003
                IWorkbook wb = null;
                ISheet ws;
                string configVersion = ConfigurationManager.AppSettings["ExcelVersion"];
                if (!configVersion.IsNullOrWhiteSpace())
                    version = configVersion;

                //建立Excel 2003檔案
                if ("2003".Equals(version))
                    wb = new HSSFWorkbook();

                if ("2007".Equals(version))
                    wb = new XSSFWorkbook();

                ws = wb.CreateSheet(type.GetDescription());

                ExcelSetValue(ws, dt, type, titles);

                FileStream file = new FileStream(path, FileMode.Create);//產生檔案
                wb.Write(file);
                file.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                //關閉文件
                //oXL.Quit();
            }
            return result;
        }

        #endregion Download Excel

        #region ExcelSetValue

        private static void ExcelSetValue(ISheet ws, DataTable dt, ExcelName type, List<FormateTitle> titles = null)
        {
            ws.CreateRow(0);//第一行為欄位名稱
            
            //if (type == Excel_DownloadName.A95)
            //{
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        ws.GetRow(0).CreateCell(i).SetCellValue((dt.Columns[i].ColumnName).formateTitle(titles));
            //    }
            //}
            //if (type == Excel_DownloadName.C07AdvancedSum)
            //{
            //    ws.GetRow(0).CreateCell(0).SetCellValue(   
            //                                              (dt.Columns[0].ColumnName).formateTitle(titles) + "："
            //                                               + dt.Rows[0]["報導日"].ToString()
            //                                               + "    "
            //                                               + (dt.Columns[1].ColumnName).formateTitle(titles) + "："
            //                                               + dt.Rows[0]["版本"].ToString()
            //                                               + "    "
            //                                               + "產品：" 
            //                                               + dt.Rows[0]["產品群代碼"].ToString() + " "  + dt.Rows[0]["產品群名稱"].ToString()
            //                                           );
            //    ws.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

            //    ws.CreateRow(1);
            //    ws.GetRow(1).CreateCell(0).SetCellValue("預期信用損失統計_台幣(報導日匯率)");
            //    ws.AddMergedRegion(new CellRangeAddress(1, 1, 0, 3));

            //    ws.CreateRow(2);
            //    for (int i = 0; i < dt.Columns.Count - 4; i++)
            //    {
            //        ws.GetRow(2).CreateCell(i).SetCellValue((dt.Columns[i + 4].ColumnName).formateTitle(titles));
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        ws.GetRow(0).CreateCell(i).SetCellValue((dt.Columns[i].ColumnName).formateTitle(titles));
            //    }
            //}


            //if (type == Excel_DownloadName.A59)
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 1);
            //        for (int j = 0; j < dt.Columns.Count; j++)
            //        {
            //            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
            //        }
            //    }
            //}
            //else if (type == Excel_DownloadName.A72 || type == Excel_DownloadName.A73)
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 1);
            //        for (int j = 0; j < dt.Columns.Count; j++)
            //        {
            //            if (0.Equals(j)) //第一行固定為 string
            //            {
            //                ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
            //            }
            //            else //後面皆為 double
            //            {
            //                ws.GetRow(i + 1).CreateCell(j).SetCellValue(Convert.ToDouble(dt.Rows[i][j]));
            //            }
            //        }
            //    }
            //}
            //else if (type == Excel_DownloadName.A95)
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 1);
            //        for (int j = 0; j < dt.Columns.Count - 2; j++) //Bond_Type,Assessment_Sub_Kind 由程式產生
            //        {
            //            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
            //        }
            //    }
            //}
            //else if (type == Excel_DownloadName.C07Mortgage || type == Excel_DownloadName.C07Bond)
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 1);
            //        for (int j = 0; j < dt.Columns.Count; j++)
            //        {
            //            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
            //        }
            //    }
            //}
            //else if (type == Excel_DownloadName.D62 || type == Excel_DownloadName.D64)
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 1);
            //        for (int j = 0; j < dt.Columns.Count; j++)
            //        {
            //            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
            //        }
            //    }
            //}
            //else if (type == Excel_DownloadName.C07AdvancedSum)
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 3);
            //        for (int j = 0; j < dt.Columns.Count - 4; j++)
            //        {
            //            ws.GetRow(i + 3).CreateCell(j).SetCellValue(dt.Rows[i][j + 4].ToString());
            //        }
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        ws.CreateRow(i + 1);
            //        for (int j = 0; j < dt.Columns.Count; j++)
            //        {
            //            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
            //        }
            //    }
            //}

            //for (int i = 0; i < dt.Columns.Count; i++)
            //{
            //    ws.AutoSizeColumn(i);
            //}
        }

        #endregion ExcelSetValue

        #region ISheet to DataTable

        public static DataTable ISheetToDataTable(this ISheet sh, bool title = false)
        {
            DataTable DT = new DataTable();
            DT.Rows.Clear();
            DT.Columns.Clear();
            int i = 0;
            if (title)
            {
                if (sh.GetRow(i) != null)
                {
                    // add neccessary columns
                    if (DT.Columns.Count < sh.GetRow(i).LastCellNum)
                    {
                        for (int j = 0; j < sh.GetRow(i).LastCellNum; j++)
                        {
                            var t = sh.GetRow(i).GetCell(j);
                            DT.Columns.Add(t != null ? t.ToString() : " ", typeof(string));
                        }
                    }
                    i += 1;
                }
            }
            while (sh.GetRow(i) != null)
            {
                // add row
                DT.Rows.Add();

                // write row value
                for (int j = 0; j < sh.GetRow(i).LastCellNum; j++)
                {
                    var cell = sh.GetRow(i).GetCell(j);

                    if (cell != null)
                    {
                        switch (cell.CellType)
                        {
                            case CellType.Numeric:
                                DT.Rows[i - (title ? 1 : 0)][j] = sh.GetRow(i).GetCell(j).NumericCellValue;
                                break;

                            case CellType.String:
                                DT.Rows[i - (title ? 1 : 0)][j] = sh.GetRow(i).GetCell(j).StringCellValue;
                                break;
                        }
                    }
                }
                i++;
            }
            return DT;
        }

        public static IEnumerable<IFileModel> ISheetToFileModel(this ISheet sh, ExcelName name,bool Despotion = true)
        {
            List<IFileModel> results = new List<IFileModel>();
            int i = 2; //預設從第三行開始
            if (!Despotion)
                i = 1; //沒有Title從第二行開始
            if (sh.GetRow(0) != null)
            {
                var pros = FactoryRegistry.GetInstance(name).GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                while (sh.GetRow(i) != null)
                {
                    var cls = FactoryRegistry.GetInstance(name);
                    bool addFlag = false;
                    // write row value
                    for (int j = 0; j < sh.GetRow(i).LastCellNum; j++)
                    {
                        var cell = sh.GetRow(0).GetCell(j);
                        var _name = cell?.ToString().ToUpper().Trim();

                        var p = pros.FirstOrDefault(y => y.Name.ToUpper() == _name);

                        if (p == null) { continue; }

                        // If not writable then cannot null it; if not readable then cannot check it's value
                        if (!p.CanWrite || !p.CanRead) { continue; }

                        MethodInfo mget = p.GetGetMethod(false);
                        MethodInfo mset = p.GetSetMethod(false);

                        // Get and set methods have to be public
                        if (mget == null) { continue; }
                        if (mset == null) { continue; }

                        var value = sh.GetRow(i).GetCell(j);

                        if (value != null)
                        {
                            switch (value.CellType)
                            {
                                case CellType.Numeric:
                                    p.SetValue(cls, value.NumericCellValue.ToString());
                                    addFlag = true;
                                    break;
                                case CellType.String:
                                    var _value = value.StringCellValue;
                                    if(!_value.IsNullOrWhiteSpace())
                                        addFlag = true;
                                    p.SetValue(cls, value.StringCellValue);
                                    break;
                            }
                        }                    
                    }
                    i++;
                    if(addFlag)
                        results.Add(cls);
                }
             
            }         
            return results;
        }

        #endregion ISheet to DataTable
    }
}