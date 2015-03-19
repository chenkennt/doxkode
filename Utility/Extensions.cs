using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
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
    public static class TypeExtension
    {
        public static bool IsA(this Type thisType, Type targetType)
        {
            return targetType.IsAssignableFrom(thisType);
        }
    }
    public static class StringExtension
    {
        public static string ForwardSlashCombine(this string baseAddress, string relativeAddress)
        {
            return baseAddress + "/" + relativeAddress;
        }

        public static string BackSlashToForwardSlash(this string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return input.Replace('\\', '/');
        }

        public static string[] ToArray(this string input, StringSplitOptions option, params char[] delimiter)
        {
            if (input == null)
            {
                return null;
            }

            return input.Split(delimiter, option);
        }

        public static string ToDelimitedString(this IEnumerable<string> input, string delimiter = ",")
        {
            if (input == null)
            {
                return null;
            }

            return string.Join(delimiter, input);
        }
    }

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

    /// <summary>
    /// 
    /// </summary>
    public static class FileExtensions
    {
        private static char[] InvalidFilePathChars = Path.GetInvalidFileNameChars();
        public static string ToValidFilePath(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Path.GetRandomFileName();
            }

            return new string(input.Select(s => InvalidFilePathChars.Contains(s) ? '_' : s).ToArray());
        }

        public static void SaveFileListToFile(List<string> fileList, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || fileList == null || fileList.Count == 0)
            {
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                fileList.ForEach(s => writer.WriteLine(s));
            }
        }

        public const string ListFileExtension = ".list";

        public static List<string> GetFileListFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            List<string> fileList = new List<string>();

            if (Path.GetExtension(filePath) == ListFileExtension)
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        fileList.Add(reader.ReadLine());
                    }
                }
            }

            return fileList;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="basePath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="absolutePath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(string basePath, string absolutePath)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(absolutePath)) throw new ArgumentNullException("toPath");

            // Append / to the directory
            if (basePath[basePath.Length - 1] != '/')
            {
                basePath = basePath + "/";
            }

            Uri fromUri = new Uri(Path.GetFullPath(basePath));
            Uri toUri = new Uri(Path.GetFullPath(absolutePath));

            if (fromUri.Scheme != toUri.Scheme) { return absolutePath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        /// <summary>
        /// Also change backslash to forward slash
        /// </summary>
        /// <param name="path"></param>
        /// <param name="kind"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static string FormatPath(this string path, UriKind kind, string basePath = null)
        {
            if (kind == UriKind.RelativeOrAbsolute)
            {
                return path.BackSlashToForwardSlash();
            }
            if (kind == UriKind.Absolute)
            {
                return Path.GetFullPath(path).BackSlashToForwardSlash();
            }
            if (kind == UriKind.Relative)
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    return path.BackSlashToForwardSlash();
                }

                return MakeRelativePath(basePath, path).BackSlashToForwardSlash();
            }

            return null;
        }
    }
}
