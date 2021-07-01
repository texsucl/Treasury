using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;


namespace FAP.Web.BO
{
    public static class ObjectUtil
    {
        public static TConvert CopyPropertiesTo<TConvert>(this object entity) where TConvert : new()
        {
            var convertProperties = TypeDescriptor.GetProperties(typeof(TConvert)).Cast<PropertyDescriptor>();
            var entityProperties = TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>();

            var convert = new TConvert();

            foreach (var entityProperty in entityProperties)
            {
                var property = entityProperty;
                var convertProperty = convertProperties.FirstOrDefault(prop => prop.Name == property.Name);
                if (convertProperty != null)
                {
                    convertProperty.SetValue(convert, Convert.ChangeType(entityProperty.GetValue(entity), convertProperty.PropertyType));
                }
            }

            return convert;
        }

        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    PropertyInfo p = destProps.First(x => x.Name == sourceProp.Name);

                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        //p.SetValue(dest, sourceProp.GetValue(source, null), null);
                        
                        //p.SetValue(dest, ChangeType(sourceProp.GetValue(source, null), p.PropertyType), null);


                        p.SetValue(dest, ChangeType(sourceProp.GetValue(source, null), Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType), null);
                        //propertyInfo.SetValue(ship, Convert.ChangeType(value, propertyInfo.PropertyType), null);

                        //convertProperty.SetValue(convert, Convert.ChangeType(sourceProp.GetValue(entity), convertProperty.PropertyType));
                    }
                }

            }
        }

        public static object ChangeType(object value, Type conversion)
        {
            if (value == null)
            {
                return null;
            }


            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {

                //if (value == null)
                //{
                //    return null;
                //}
                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }

    }
}