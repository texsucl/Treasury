using System.Collections.Generic;
using System.Web.Mvc;

namespace Treasury.WebUtility
{
    public class jqGridParam
    {
        public bool _search { get; set; }
        public int page { get; set; }
        public int rows { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
    }

    public class jqGridData
    {
        public jqGridData()
        {
            colNames = new List<string>();
            colModel = new List<jqGridColModel>();
        }

        public List<string> colNames { get; set; }
        public List<jqGridColModel> colModel { get; set; }
    }

    public class jqGridData<T>
    {
        public jqGridData()
        {
            colNames = new List<string>();
            colModel = new List<jqGridColModel>();
            Datas = new List<T>();
        }

        public List<string> colNames { get; set; }
        public List<jqGridColModel> colModel { get; set; }
        public List<T> Datas { get; set; }
    }

    public class jqGridColModel
    {
        public jqGridColModel()
        {
            align = "left";
            hidden = false;
        }

        public string name { get; set; }
        public string index { get; set; }
        public string align { get; set; }
        public int? width { get; set; }
        public bool? sortable { get; set; }
        public bool hidden { get; set; }
    }
}