using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Treasury.Web;
using Treasury.Web.ViewModels;
using static Treasury.Web.Enum.Ref;

namespace Treasury.WebUtility
{
    public class FactoryRegistry
    {
        public static IFileModel GetInstance(ExcelName type)
        {
            Type t = TableTypeHelper.GetInstanceType(type);
            return (IFileModel)Activator.CreateInstance(t);
        }

        internal class TableTypeHelper
        {
            internal static Type GetInstanceType(ExcelName type)
            {
                FieldInfo data = typeof(ExcelName).GetField(type.ToString());
                Attribute attribute = Attribute.GetCustomAttribute(data, typeof(CommucationAttribute));
                CommucationAttribute result = (CommucationAttribute)attribute;
                return result.InstanceType;
            }
        }
    }
}