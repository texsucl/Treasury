using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace SSO.Web.BO
{
    public class FileRelated
    {
        #region 上傳檔案到指定路徑
        /// <summary>
        /// 檔案上傳
        /// </summary>
        /// <param name="path">檔案放置位置</param>
        /// <param name="file">檔案</param>
        /// <returns></returns>
        public static Utility.MSGReturnModel FileUpLoadinPath(string path, HttpPostedFileBase file)
        {
            Utility.MSGReturnModel result = new Utility.MSGReturnModel();
            result.RETURN_FLAG = true;
            try
            {
                using (var fileStream = new FileStream(path,
                     FileMode.Create, FileAccess.ReadWrite))
                {
                    file.InputStream.CopyTo(fileStream); //資料複製一份到FileUploads,存在就覆寫
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message.ToString();
            }
            return result;
        }
        #endregion

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
        #endregion


    }
}