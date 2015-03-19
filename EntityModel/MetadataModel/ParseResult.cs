using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public enum ResultLevel
    {
        Success,
        Info,
        Warn,
        Error
    }

    public class ParseResult
    {
        public ResultLevel ResultLevel { get; set; }
        public string Message { get; set; }
        public ParseResult(ResultLevel resultLevel, string message, params string[] arg)
        {
            this.ResultLevel = resultLevel;
            this.Message = string.Format(message, arg);
        }

        public ParseResult(ResultLevel resultLevel)
        {
            this.ResultLevel = resultLevel;
        }

        public void WriteToConsole()
        {
            Console.Error.WriteLine(this.ToString());
        }

        public static void WriteToConsole(ResultLevel resultLevel, string message, params string[] arg)
        {
            Console.Error.WriteLine(resultLevel + ": " + message, arg);
        }

        public override string ToString()
        {
            return ResultLevel + ": " + Message;
        }
    }
}
