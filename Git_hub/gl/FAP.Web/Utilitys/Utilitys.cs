using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.Utilitys
{
    public class SelectOption
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public string Code { get; set; }
    }

    public class OAP0018AViewModel_Comparer : IEqualityComparer<OAP0018AViewModel>
    {
        public bool Equals(OAP0018AViewModel x, OAP0018AViewModel y)
        {
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return (x.aply_no == y.aply_no);
        }

        public int GetHashCode(OAP0018AViewModel obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            return obj.aply_no.GetHashCode();
        }
    }
}