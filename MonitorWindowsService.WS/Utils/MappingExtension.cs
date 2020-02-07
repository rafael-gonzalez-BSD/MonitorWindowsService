using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace MonitorWindowsService.WS.Utils
{
    public static class MappingExtension
    {
        public static T ToObject<T>(this IDictionary<string, dynamic> source) where T : class, new()
        {
            T obj = new T();
            Type type = obj.GetType();

            foreach (KeyValuePair<string, dynamic> item in source)
            {
                type.GetProperty(item.Key).SetValue(obj, item.Value, null);
            }

            return obj;
        }

        public static Dictionary<string, dynamic> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }

        public static String ToXMLString<T>(this List<T> list) where T : class, new()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "iso-8859-1", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement D = doc.CreateElement(string.Empty, "D", string.Empty);
            doc.AppendChild(D);

            XmlElement R;

            foreach (T item in list)
            {
                R = doc.CreateElement(string.Empty, "R", string.Empty);

                List<PropertyInfo> properties = item.GetType().GetProperties().ToList();

                foreach (PropertyInfo property in properties)
                {
                    object value = property.GetValue(item, null);
                    R.SetAttribute(property.Name, value != null ? value.ToString() : "null");
                }

                D.AppendChild(R);
            }

            return doc.OuterXml;
        }
    }
}