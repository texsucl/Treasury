using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAP.Web.Service.Interface
{
    interface IFileService
    {
        /// <summary>
        /// Excel 資料轉成 iewModel
        /// </summary>
        /// <param name="stream">stream 資料</param>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="excelName">帶轉換Excel Model</param>
        /// <returns></returns>
        Tuple<string, IEnumerable<IFileUpLoadModel>> getExcel(Stream stream, string pathType, string excelName);
    }
}
