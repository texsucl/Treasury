using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using SearchDB2.Utility;

namespace SearchDB2.Models
{
    interface ISql
    {
        MSGReturnModel<List<ExpandoObject>> work(string sqlStr, string type, bool transaction,string connectionString);
    }
}
