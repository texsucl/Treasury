using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 上傳 & 下載 檔案Service
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 抓Excel 資料
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="path"></param>
        /// <param name="excelName"></param>
        /// <returns></returns>
        Tuple<string, IEnumerable<IFileModel>> getExcel(string pathType, string path, ExcelName excelName);

        /// <summary>
        /// 依照UserName 查詢 UserID
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        List<SelectOption> SearchUserID(string userName);

        /// <summary>
        /// Excel資料存到DB
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        MSGReturnModel<string> saveData(IEnumerable<IFileModel> datas, ExcelName type);
    }
}
