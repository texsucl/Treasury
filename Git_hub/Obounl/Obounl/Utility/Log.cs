using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Obounl.ObounlEnum.Ref;

namespace Obounl.Utility
{
    public static class Log
    {



        /// <summary>
        /// Nlog 寫入
        /// </summary>
        /// <param name="message">訊息</param>
        /// <param name="name">子資料夾</param>
        /// <param name="nlog">類型 預設為訊息</param>
        public static void NlogSet(string message, string name = null, Nlog nlog = Nlog.Info)
        {
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            if (!string.IsNullOrWhiteSpace(name))
            {
                logger = LogManager.GetLogger(name);
            }
            switch (nlog)
            {
                // 用於追蹤，可以在程式裡需要追蹤的地方將訊息以Trace傳出。
                case Nlog.Trace:
                    logger.Trace(message);
                    break;
                // 用於開發，於開發時將一些需要特別關注的訊息以Debug傳出。
                case Nlog.Debug:
                    logger.Debug(message);
                    break;
                // 訊息，記錄不影響系統執行的訊息，通常會記錄登入登出或是資料的建立刪除、傳輸等。
                case Nlog.Info:
                    logger.Info(message);
                    break;
                // 警告，用於需要提示的訊息，例如庫存不足、貨物超賣、餘額即將不足等。
                case Nlog.Warn:
                    logger.Warn(message);
                    break;
                // 錯誤，記錄系統實行所發生的錯誤，例如資料庫錯誤、遠端連線錯誤、發生例外等。
                case Nlog.Error:
                    logger.Info(message);
                    logger.Error(message);
                    break;
                // 致命，用來記錄會讓系統無法執行的錯誤，例如資料庫無法連線、重要資料損毀等。
                case Nlog.Fatal:
                    logger.Info(message);
                    logger.Fatal(message);
                    break;
            }
        }
    }
}