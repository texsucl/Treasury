
using System.Configuration;


namespace FRT.Web.BO
{
    public static class DbUtil
    {
        public static string GetDBFglConnStr()
        {
            var connString = ConfigurationManager.ConnectionStrings["dbFGL"].ConnectionString;

            return connString;
        }
    }
}