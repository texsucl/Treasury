using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 不動產權狀
    /// </summary>
    public interface IEstate : IApply, IAgency, ICDCAction
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
        List<SelectOption> GetBuildName(string vAplyUnit = null, string aplyNo = null);

        /// <summary>
        /// 抓取冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        List<SelectOption> GetBookNo(string vAplyUnit = null, string aplyNo = null);

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <param name="EditFlag">可否修改,可以也需抓取庫存資料</param>
        /// <returns></returns>
        EstateViewModel GetDataByAplyNo(string aplyNo, bool EditFlag = false);


        /// <summary>
        /// 使用 存取項目冊號資料檔ITEM_BOOK 的 群組編號 抓取資料
        /// </summary>
        /// <param name="groupNo">群組號</param>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        List<EstateDetailViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit, string aplyNo = null);

        /// <summary>
        /// 由group No 抓取 存取項目冊號資料檔
        /// </summary>
        /// <param name="groupNo"></param>
        /// <returns></returns>
        EstateModel GetItemBook(int groupNo);

        /// <summary>
        /// 對資料庫進行大樓名稱的模糊比對
        /// </summary>
        /// <param name="building_Name">大樓名稱</param>
        /// <returns></returns>
        MSGReturnModel<string> GetCheckItemBook(string building_Name);

        /// <summary>
        /// 修改 "坐落" & "備註"
        /// </summary>
        /// <param name="updateData"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        MSGReturnModel<string> UpdateDBITEM_BOOK(CDCEstateViewModel updateData, CDCEstateViewModel viewModel, string cUserId);
    }
}
