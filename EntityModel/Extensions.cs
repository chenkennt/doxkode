using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using EntityModel;
using Newtonsoft.Json.Converters;
using System.Collections;
using YamlDotNet.Serialization;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

/// <summary>
/// The utility class for docascode project
/// </summary>
namespace DocAsCode.Utility
{
    /// <summary>
    /// The converter to transform strings delimited by comma into string arrays
    /// </summary>
    public class DelimitedStringArrayConverter : TypeConverter
    {
        private readonly char[] _delimiter = { ',', };

        /// <summary>
        /// Specifies if the current type can be converted from
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            return stringValue.Split(_delimiter, StringSplitOptions.RemoveEmptyEntries);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            throw new NotImplementedException();
        }
    }

    public static class YamlUtility
    {
        public static Serializer Serializer = new Serializer();
        public static Deserializer Deserializer = new Deserializer();
    }
}
