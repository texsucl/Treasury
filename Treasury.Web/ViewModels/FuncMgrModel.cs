using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class FuncMgrModel
    {
        public FuncMgrModel()
        {
            this.nodes = new List<FuncMgrModel>();
        }

        public string text { get; set; }

        public int nodeId { get; set; }

        public String Id { get; set; }

        public int parentId { get; set; }

        [JsonProperty("nodes")]
        public List<FuncMgrModel> nodes { get; set; }
    }
}