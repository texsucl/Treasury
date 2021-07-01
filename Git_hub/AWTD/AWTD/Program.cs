using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWTD.Utility.Log;
using static AWTD.Enum.Ref;
using AWTD.project;

namespace AWTD
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = string.Empty;
            if (args.Length > 0 && args[0] != null)
                type = args[0]?.Trim();
            type = "W514";
            switch (type)
            {
                case "W514":
                    NlogSet("執行 AS400 資料 (FPMWTDP0 保單所屬核保部門代號檔) 轉入 總帳中介 (XXX279350734514_FPMWTDP0) ", null);
                    new W514_FPMWTDP0().start();
                    NlogSet("結束 AS400 資料 (FPMWTDP0 保單所屬核保部門代號檔) 轉入 總帳中介 (XXX279350734514_FPMWTDP0) ", null);
                    break;
            }
        }
    }
}
