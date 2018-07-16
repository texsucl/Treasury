using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface IEstate : IApply
    {
        /// <summary>
        /// 抓取狀別 土地,建物,他項權利,其他
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetEstateFromNo();

        /// <summary>
        /// 抓取大樓名稱
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        List<SelectOption> GetBuildName(string vAplyUnit = null);

        /// <summary>
        /// 抓取冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        List<SelectOption> GetBookNo(string vAplyUnit = null);

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        EstateViewModel GetDataByAplyNo(string aplyNo);

        /// <summary>
        /// 使用 存取項目冊號資料檔ITEM_BOOK 的 群組編號 抓取資料
        /// </summary>
        /// <param name="groupNo"></param>
        /// <returns></returns>
        List<EstateDetailViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit);

        /// <summary>
        /// 由group No 抓取 存取項目冊號資料檔
        /// </summary>
        /// <param name="groupNo"></param>
        /// <returns></returns>
        EstateModel GetItemBook(int groupNo);
    }
}
