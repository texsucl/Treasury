using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 回傳訊息格式統一
        /// </summary>
        public enum MessageType
        {
            /// <summary>
            /// 資料已經儲存過了
            /// </summary>
            [Description("資料已經儲存過了!")]
            already_Save,

            /// <summary>
            /// 資料已異動(請重新查詢)
            /// </summary>
            [Description("資料已異動(請重新查詢)")]
            already_Change,

            /// <summary>
            /// 新增成功
            /// </summary>
            [Description("新增成功!")]
            insert_Success,

            /// <summary>
            /// 新增失敗
            /// </summary>
            [Description("新增失敗!")]
            insert_Fail,

            /// <summary>
            /// 儲存成功
            /// </summary>
            [Description("儲存成功!")]
            save_Success,

            /// <summary>
            /// 儲存失敗
            /// </summary>
            [Description("儲存失敗!")]
            save_Fail,

            /// <summary>
            /// 修改成功
            /// </summary>
            [Description("修改成功!")]
            update_Success,

            /// <summary>
            /// 修改失敗
            /// </summary>
            [Description("修改失敗!")]
            update_Fail,

            /// <summary>
            /// 刪除成功
            /// </summary>
            [Description("刪除成功!")]
            delete_Success,

            /// <summary>
            /// 刪除失敗
            /// </summary>
            [Description("刪除失敗!")]
            delete_Fail,

            /// <summary>
            /// 沒有找到資料
            /// </summary>
            [Description("沒有找到資料!")]
            not_Find_Any,

            /// <summary>
            /// 無更新資料
            /// </summary>
            [Description("無更新資料!")]
            not_Find_Update_Data,

            /// <summary>
            /// 無申請覆核資料
            /// </summary>
            [Description("無申請覆核資料!")]
            not_Find_Audit_Data,

            /// <summary>
            /// 沒有找到搜尋的資料
            /// </summary>
            [Description("沒有找到搜尋的資料!")]
            query_Not_Find,

            /// <summary>
            /// 下載成功
            /// </summary>
            [Description("下載成功!")]
            download_Success,

            /// <summary>
            /// 下載失敗
            /// </summary>
            [Description("下載失敗!")]
            download_Fail,

            /// <summary>
            /// 上傳成功
            /// </summary>
            [Description("上傳成功!")]
            upload_Success,

            /// <summary>
            /// 上傳失敗
            /// </summary>
            [Description("上傳失敗!")]
            upload_Fail,

            /// <summary>
            /// 請選擇上傳檔案
            /// </summary>
            [Description("請選擇上傳檔案!")]
            upload_Not_Find,

            /// <summary>
            /// 無比對到資料!
            /// </summary>
            [Description("無比對到資料!")]
            data_Not_Compare,

            /// <summary>
            /// 傳入參數錯誤!
            /// </summary>
            [Description("傳入參數錯誤!")]
            parameter_Error,

            /// <summary>
            /// 時間停滯太久請重新上一動作!
            /// </summary>
            [Description("時間停滯太久請重新上一動作!")]
            time_Out,

            /// <summary>
            /// 申請複核成功
            /// </summary>
            [Description("申請複核成功!")]
            Apply_Audit_Success,

            /// <summary>
            /// 申請複核失敗
            /// </summary>
            [Description("申請複核失敗!")]
            Apply_Audit_Fail,

            /// <summary>
            /// 呈送複核成功
            /// </summary>
            [Description("呈送複核成功!")]
            send_To_Audit_Success,

            /// <summary>
            /// 呈送複核失敗
            /// </summary>
            [Description("呈送複核失敗!")]
            send_To_Audit_Fail,

            /// <summary>
            /// 複核成功
            /// </summary>
            [Description("複核成功!")]
            Audit_Success,

            /// <summary>
            /// 複核失敗
            /// </summary>
            [Description("複核失敗!")]
            Audit_Fail,

            /// <summary>
            /// 無呈送複核權限
            /// </summary>
            [Description("無呈送複核權限!")]
            none_Send_Audit_Authority,

            /// <summary>
            /// 無複核權限
            /// </summary>
            [Description("無複核權限!")]
            none_Audit_Authority,

            /// <summary>
            /// 請輸入正確的帳號或密碼!
            /// </summary>
            [Description("請輸入正確的帳號或密碼!")]
            login_Fail,

            /// <summary>
            /// 閒置太久請重新登入!
            /// </summary>
            [Description("閒置太久請重新登入!")]
            login_Time_Out,

            /// <summary>
            /// 帳號已失效!
            /// </summary>
            [Description("帳號已失效!")]
            login_Effective_Fail,

            /// <summary>
            /// 帳號登入中!
            /// </summary>
            [Description("帳號登入中!")]
            login_Flag_Fail,

            /// <summary>
            /// 請輸入正確的驗證碼!
            /// </summary>
            [Description("請輸入正確的驗證碼!")]
            login_Captcha_Fail,

            /// <summary>
            /// 檢核錯誤!
            /// </summary>
            [Description("檢核錯誤!")]
            Check_Fail,

            /// <summary>
            /// 暫存資料尚未申請覆核,是否繼續動作?
            /// </summary>
            [Description("暫存資料尚未申請覆核,是否繼續動作?")]
            application_Audit_Confirm,

            /// <summary>
            /// 輸入null為清空該欄位
            /// </summary>
            [Description("輸入null為清空該欄位")]
            null_Input,

            /// <summary>
            /// 冊號為保管科必要欄位
            /// </summary>
            [Description("冊號為保管科必要欄位")]
            book_No_Error,

            /// <summary>
            /// excel檔案驗證失敗
            /// </summary>
            [Description("excel檔案驗證失敗")]
            excel_Validate,

            /// <summary>
            /// 存單於2018/12/10到期,敬請確認是否展期?
            /// </summary>
            [Description("存單於 @Time 到期,敬請確認是否展期?")]
            auto_Trans,

            /// <summary>
            /// 存單已到展期次數,無法繼續展期,此筆存單記錄將轉至金庫內=N
            /// </summary>
            [Description("存單已到展期次數,無法繼續展期,此筆存單記錄將轉至金庫內=N")]
            already_Max_Trans,

            /// <summary>
            /// Mail內文設定替換規則
            /// </summary>
            [Description(@"針對部分功能權限有權者作發送,需有相關程式配合,才能執行
替換規則
每日例行性排程:
1. @_TREA_OPEN_TIME_ => (開關庫時間) 
2. @_EXEC_TIME_E_ => (系統(迄)時間)
流程檢核排程:
1. @_TREA_REGISTER_ID_ => (金庫開庫申請單單號)
2. @_STATUS_ => (流程階段)
出入庫流程完成通知:
1. @_APLYNO_ => (申請單單號)
季報表通知
1. @_DATE_ => (在庫日期)
2. @_ITEM_ => (查詢項目)
")]
            Mail_Content_Message,

            /// <summary>
            /// 取代變數說明
            /// </summary>
            [Description(@"取代變數
            P1：新作存單
            G1：到期存單
            ")]
            Replace_Desc,
        }
    }

}