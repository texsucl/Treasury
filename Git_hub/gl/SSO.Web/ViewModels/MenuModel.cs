using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;


namespace SSO.Web.ViewModels
{
    [Serializable]
    public class MenuModel
    {
        public MenuModel()
        {
            this.SubMenu = new List<MenuModel>();
        }

        public String MenuID { get; set; }
        public string Title { get; set; }
        public string SYS_CD { get; set; }
        public string PARENT_FUNC_ID { get; set; }
        
        public int FUNC_LEVEL { get; set; }
        public int FUNC_ORDER { get; set; }
        public bool IsAction { get; set; }
        public string Link { get; set; }
        public string Class { get; set; }
        public string AUTH_UNIT { get; set; }
        public List<MenuModel> SubMenu { get; set; }
    }
}