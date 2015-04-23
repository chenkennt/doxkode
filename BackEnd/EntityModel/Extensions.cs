using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using YamlDotNet.Serialization;

/// <summary>
/// The utility class for docascode project
/// </summary>
namespace DocAsCode.EntityModel
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
        private static readonly ThreadLocal<Serializer> serializer = new ThreadLocal<Serializer>(() => new Serializer());
        private static readonly ThreadLocal<Deserializer> deserializer = new ThreadLocal<Deserializer>(() => new Deserializer());

        public static void Serialize(TextWriter writer, object graph)
        {
            serializer.Value.Serialize(writer, graph);
        }

        public static void Serialize(string path, object graph)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamWriter writer = new StreamWriter(path))
            {
                YamlUtility.Serialize(writer, graph);
            }
        }

        public static T Deserialize<T>(TextReader reader)
        {
            return deserializer.Value.Deserialize<T>(reader);
        }
    }
}
