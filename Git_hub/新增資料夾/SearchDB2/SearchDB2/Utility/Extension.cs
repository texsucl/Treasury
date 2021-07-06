using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SearchDB2.Utility
{
    public static class Extension
    {
        public static jqGridData TojqGridData(this System.Dynamic.ExpandoObject obj)
        {
            var jqgridParams = new jqGridData();

            var tTarget = obj as System.Dynamic.IDynamicMetaObjectProvider;
            if (tTarget != null)
            {
                bool flag = false;
                int len = 0;
                int widthIndex = 0;
                int? widthParam = 200;

                bool alignFlag = false;
                int alignLen = 0;
                int alignIndex = 0;
                string alignParam = "left";
                var names = tTarget.GetMetaObject(System.Linq.Expressions.Expression.Constant(tTarget)).GetDynamicMemberNames();
                foreach (string name in names)
                {
                    var str = name;
                    jqgridParams.colModel.Add(new jqGridColModel()
                    {
                        name = str,
                        index = str,
                        width = widthParam,
                        align = alignParam,
                    });
                    jqgridParams.colNames.Add(str);
                    widthIndex += 1;
                    alignIndex += 1;
                }
            }
            return jqgridParams;
        }

        public static string exceptionMessage(this Exception ex)
        {
            return $"message: {ex.Message}" +
                   $", inner message {ex.InnerException}";
            //$", inner message {ex.InnerException?.InnerException?.Message}";
        }

        public static string getConnectionStringSetting(this string ConnectionString)
        { 
            return ConfigurationManager.ConnectionStrings[ConnectionString]?.ConnectionString;
        }

        public class ExpandedObjectFromApi : DynamicObject
        {
            private Dictionary<string, object> _customProperties = new Dictionary<string, object>();
            private object _currentObject;

            public ExpandedObjectFromApi(dynamic sealedObject)
            {
                _currentObject = sealedObject;
            }

            private PropertyInfo GetPropertyInfo(string propertyName)
            {
                return _currentObject.GetType().GetProperty(propertyName);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var prop = GetPropertyInfo(binder.Name);
                if (prop != null)
                {
                    result = prop.GetValue(_currentObject);
                    return true;
                }
                result = _customProperties[binder.Name];
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                var prop = GetPropertyInfo(binder.Name);
                if (prop != null)
                {
                    prop.SetValue(_currentObject, value);
                    return true;
                }
                if (_customProperties.ContainsKey(binder.Name))
                    _customProperties[binder.Name] = value;
                else
                    _customProperties.Add(binder.Name, value);
                return true;
            }
        }
    }
}