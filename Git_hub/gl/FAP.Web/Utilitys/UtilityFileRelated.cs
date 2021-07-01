using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;
using System.IO;
using FAP.Web.ViewModels;
using System.Reflection;

namespace FAP.Web.Utilitys
{
    public static class UtilityFileRelated
    {
        public static IEnumerable<IFileUpLoadModel> ISheetToFileModel(this ISheet sh, string name, bool Description = true)
        {
            List<IFileUpLoadModel> results = new List<IFileUpLoadModel>();
            int i = 1; //預設從第三行開始
            if (!Description)
                i = 0; //沒有Title從第二行開始
            if (sh.GetRow(0) != null)
            {
                var pros = GetInstance(name).GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                int k = 1;
                while (sh.GetRow(i + k) != null)
                {
                    var cls = GetInstance(name);
                    bool addFlag = false;
                    // write row value
                    for (int j = 0; j < sh.GetRow(i).LastCellNum; j++)
                    {
                        var cell = sh.GetRow(i).GetCell(j);
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

                        var value = sh.GetRow(i+k).GetCell(j);

                        if (value != null)
                        {
                            switch (value.CellType)
                            {
                                case CellType.Numeric:
                                    p.SetValue(cls, value.ToString());
                                    addFlag = true;
                                    break;
                                case CellType.String:
                                    var _value = value.StringCellValue;
                                    if (!_value.IsNullOrWhiteSpace())
                                        addFlag = true;
                                    p.SetValue(cls, value.StringCellValue);
                                    break;
                            }
                        }
                    }
                    k++;
                    if (addFlag)
                        results.Add(cls);
                }

            }
            return results;
        }

        public static IFileUpLoadModel GetInstance(string type)
        {
            Type t = null;
            switch (type)
            {
                case "OAP0031":
                    t = typeof(OAP0031ViewModel);
                    break;
            }          
            return (IFileUpLoadModel)Activator.CreateInstance(t);
        }
    }
}