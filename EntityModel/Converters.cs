using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


namespace DocAsCode.EntityModel
{
    public class CommentIdToYamlHeaderConverter : TypeConverter
    {
        /// <summary>
        /// Refer to http://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx
        /// ELEMENT_TYPE_PTR is represented as a '*' following the modified type.
        /// ELEMENT_TYPE_BYREF is represented as a '@' following the modified type.
        /// ELEMENT_TYPE_PINNED is represented as a '^' following the modified type. The C# compiler never generates this.
        /// ELEMENT_TYPE_CMOD_REQ is represented as a '|' and the fully qualified name of the modifier class, following the modified type.The C# compiler never generates this.
        /// ELEMENT_TYPE_CMOD_OPT is represented as a '!' and the fully qualified name of the modifier class, following the modified type.
        /// ELEMENT_TYPE_SZARRAY is represented as "[]" following the element type of the array.
        /// ELEMENT_TYPE_GENERICARRAY is represented as "[?]" following the element type of the array.The C# compiler never generates this.
        /// ELEMENT_TYPE_ARRAY is represented as [lowerbound: size, lowerbound:size] where the number of commas is the rank - 1, and the lower bounds and size of each dimension, if known, are represented in decimal. If a lower bound or size is not specified, it is simply omitted. If the lower bound and size for a particular dimension are omitted, the ':' is omitted as well.For example, a 2-dimensional array with 1 as the lower bounds and unspecified sizes is [1:,1:].
        /// ELEMENT_TYPE_FNPTR is represented as "=FUNC:type(signature)", where type is the return type, and signature is the arguments of the method.If there are no arguments, the parentheses are omitted. The C# compiler never generates this.
        /// For conversion operators only (op_Implicit and op_Explicit), the return value of the method is encoded as a '~' followed by the return type.
        /// For generic types, the name of the type will be followed by a back tick and then a number that indicates the number of generic type parameters
        /// </summary>
        public static Regex CommentIdRegex = new Regex(@"^(?<type>N|T|M|P|F|E):(?<id>((?![0-9])[\w_])+[\w\(\)\.\{\}\[\]\|\*\^~#@!`,_<>:]*)$", RegexOptions.Compiled);

        /// <summary>
        /// Yaml style: 
        /// First line: start with ---, and could append whitespaces
        /// Second line: start with method| namespace| class, could prefix whitespaces, and must append one *{space}* and one *:*
        /// Third line: start with ---, and could append whitespaces, must contain a *\n* EOL
        /// </summary>
        public static Regex YamlHeaderRegex = new Regex(@"\-\-\-((?!\n)\s)*\n((?!\n)\s)*(?<type>method|namespace|class|property|field|event): (?<id>\S*)((?!\n)\s)*\n\-\-\-((?!\n)\s)*\n", RegexOptions.Compiled | RegexOptions.Multiline);

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
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, destinationType);
        }

        /// <summary>
        /// Yaml header to CommentId
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentNullException("value", "Null or empty string is not a valid comment id.");
            }

            if (!YamlHeaderRegex.IsMatch(stringValue))
            {
                throw new ArgumentException(string.Format("Not a valid yaml header {0}", stringValue));
            }

            var match = YamlHeaderRegex.Match(stringValue);
            string type = match.Groups["type"].Value;
            string id = match.Groups["id"].Value;
            return FormatCommentId(type, id);
        }

        /// <summary>
        /// CommentId to Yaml header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            string stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentNullException("value", "Null or empty string is not a valid comment id.");
            }

            if (!CommentIdRegex.IsMatch(stringValue))
            {
                throw new ArgumentException(string.Format("Not a valid comment ID {0}", stringValue));
            }

            var match = CommentIdRegex.Match(stringValue);
            string type = match.Groups["type"].Value;
            string id = match.Groups["id"].Value;
            return FormatYamlHeader(type, id);
        }

        private string FormatYamlHeader(string type, string id)
        {
            type = GetYamlHeaderPrefix(type);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("---");
            builder.AppendFormat("{0}: {1}", type, id);
            builder.AppendLine();
            builder.AppendLine("---");
            return builder.ToString();
        }

        private string FormatCommentId(string typeInput, string id)
        {
            string type = GetCommentIdPrefix(typeInput);
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}:{1}", type, id);
            return builder.ToString();
        }

        private string GetYamlHeaderPrefix(string type)
        {
            switch (type)
            {
                case "N":
                    return "namespace";
                case "T":
                    return "class";
                case "M":
                    return "method";
                case "P":
                    return "property";
                case "F":
                    return "field";
                case "E":
                    return "event";
                default:
                    throw new NotSupportedException(type);
            }
        }

        private string GetCommentIdPrefix(string type)
        {
            switch (type)
            {
                case "namespace":
                    return "N";
                case "class":
                    return "T";
                case "method":
                    return "M";
                case "property":
                    return "P";
                case "field":
                    return "F";
                case "event":
                    return "E";
                default:
                    throw new NotSupportedException(type);
            }
        }
    }
}
