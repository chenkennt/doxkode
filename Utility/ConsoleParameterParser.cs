using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DocAsCode.Utility
{
    /// <summary>
    /// The console parameters
    /// </summary>
    public class Option
    {
        public string Name { get; set; }
        public string HelpName { get; set; }
        public string DefaultValue { get; set; }
        public Action<string> Setter { get; set; }
        public bool Required { get; set; }
        public string HelpText { get; set; }

        public Option(string name, Action<string> setter, string helpName = null, bool required = false, string defaultValue = null, string helpText = null)
        {
            Name = name;
            Setter = setter;
            HelpName = helpName;
            DefaultValue = defaultValue;
            Required = required;
            HelpText = helpText;
        }
    }

    /// <summary>
    /// The console parameter parser
    /// </summary>
    public class ConsoleParameterParser
    {
        static string[] helpCommand = new[] { "/?", "?", "-?", "/help", "/h", "--help", "-h", "--?" };

        /// <summary>
        /// Parse the console parameters
        /// </summary>
        /// <param name="options"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool ParseParameters(IEnumerable<Option> options, string[] args)
        {
            if (args.Length == 1 && helpCommand.Contains(args[0], StringComparer.OrdinalIgnoreCase))
            {
                PrintUsage(options);
                return false;
            }

            // Set default value first
            foreach (var o in options)
            {
                o.Setter(o.DefaultValue);
            }

            Regex paramRegex = new Regex(@"^/(\w+):(.*)$");
            var optionDict = options.ToDictionary(o => o.Name ?? string.Empty, o => o);
            try
            {
                foreach (var a in args)
                {
                    var match = paramRegex.Match(a);
                    string key;
                    string value;
                    if (match.Success && optionDict.ContainsKey(match.Groups[1].Value))
                    {
                        key = match.Groups[1].Value;
                        value = match.Groups[2].Value;
                    }
                    else if (optionDict.ContainsKey(string.Empty))
                    {
                        key = string.Empty;
                        value = a;
                    }
                    else throw new ArgumentException(string.Format("Unrecognized parameter: {0}.", a));
                    optionDict[key].Setter(value);
                    optionDict.Remove(key);
                }

                foreach (var o in optionDict.Values)
                {
                    if (o.Required) throw new ArgumentException(string.Format("{0} is not specified.", o.HelpName));
                }

                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                PrintUsage(options);
                return false;
            }
        }

        static void PrintUsage(IEnumerable<Option> options)
        {
            if (options == null)
            {
                return;
            }

            // Write usage
            Console.Write(Process.GetCurrentProcess().ProcessName);
            foreach (var o in options)
            {
                Console.Write(" ");
                if (!o.Required) Console.Write("[");
                if (o.Name != null) Console.Write("/{0}:", o.Name);
                Console.Write(o.HelpName);
                if (!o.Required) Console.Write("]");
            }

            Console.WriteLine();
            Console.WriteLine("Available parameters:");
            foreach (var o in options)
            {
                if (o.Name != null) Console.Write("/{0}:", o.Name);
                Console.WriteLine(o.HelpName);
                if (o.HelpText != null) Console.WriteLine("  " + o.HelpText.Replace(Environment.NewLine, Environment.NewLine + "  "));
            }
        }
    }
}
