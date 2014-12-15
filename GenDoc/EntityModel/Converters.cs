﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DocAsCode.EntityModel
{
    public class CommentIdToYamlHeaderConverter : TypeConverter
    {
        /// <summary>
        /// TODO: currently only support N|T|M|P
        /// </summary>
        public static Regex CommentIdRegex = new Regex(@"^(?<type>N|T|M|P):(?<id>[0-9a-zA-Z\(\)\.,_<>:]+)$", RegexOptions.Compiled);

        /// <summary>
        /// Yaml style: 
        /// First line: start with ---, and could append whitespaces
        /// Second line: start with method| namespace| class, could prefix whitespaces, and must append one *{space}* and one *:*
        /// Third line: start with ---, and could append whitespaces, must contain a *\n* EOL
        /// </summary>
        public static Regex YamlHeaderRegex = new Regex(@"\-\-\-((?!\n)\s)*\n((?!\n)\s)*(?<type>method|namespace|class|property): (?<id>\S*)((?!\n)\s)*\n\-\-\-((?!\n)\s)*\n", RegexOptions.Compiled | RegexOptions.Multiline);

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
                default:
                    throw new NotSupportedException(type);
            }
        }
    }
}
