using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using FAP.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FAP.Web.ViewModels;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using FAP.Web.BO;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;
using FAP.Web.Daos;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace FAP.Web.Service.Actual
{
    public class FileService : Common, IFileService
    {
        #region Excel 資料轉成  ViewModel

        /// <summary>
        /// Excel 資料轉成 iewModel
        /// </summary>
        /// <param name="stream">stream 資料</param>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="excelName">帶轉換Excel Model</param>
        /// <returns></returns>
        public Tuple<string, IEnumerable<IFileUpLoadModel>> getExcel(Stream stream, string pathType, string excelName)
        {
            IEnumerable<IFileUpLoadModel> dataModel = new List<IFileUpLoadModel>();
            IWorkbook wb = null;
            string msg = string.Empty;
            try
            {
                switch (pathType) //判斷型別
                {
                    case "xls":
                        wb = new HSSFWorkbook(stream);
                        break;

                    case "xlsx":
                        wb = new XSSFWorkbook(stream);
                        break;
                }
                ISheet sheet = wb.GetSheetAt(0);

                dataModel = (sheet.ISheetToFileModel(excelName));
                if (!dataModel.Any()) //判斷有無資料
                {
                    return new Tuple<string, IEnumerable<IFileUpLoadModel>>(MessageType.not_Find_Any.GetDescription(), dataModel);
                }
                List<string> errors = new List<string>();
                var pros = UtilityFileRelated.GetInstance(excelName).GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                foreach (var item in dataModel)
                {
                    var context = new ValidationContext(item, null, null);
                    var result = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(item, context, result, true))
                    {
                        result.ForEach(x =>
                        {
                            var m = x.MemberNames.FirstOrDefault()?.ToString();
                            var p = pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                            var val = (p == null) ? null : (p.GetValue(item))?.ToString();
                            if (!val.IsNullOrWhiteSpace())
                                errors.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                            else
                                errors.Add(x.ErrorMessage);
                        });
                    }
                }
                if (errors.Any())
                {
                    msg = string.Join(",", errors.Distinct());
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                msg = ex.exceptionMessage();
            }
            return new Tuple<string, IEnumerable<IFileUpLoadModel>>(msg, dataModel);
        }

        #endregion Excel 資料轉成  ViewModel
    }
}