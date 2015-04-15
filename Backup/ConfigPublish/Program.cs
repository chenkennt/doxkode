using DocAsCode.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAsCode.ConfigPublish
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            string githubConfigFile = "GithubPublish.config";

            var options = new Option[]
                {
                    new Option("githubConfigFile", s => githubConfigFile = s, defaultValue: githubConfigFile, helpText: "Specify the configuration file of github for publish."),
                };

            if (!ConsoleParameterParser.ParseParameters(options, args))
            {
                Console.WriteLine("Error while parsing parameters!");
                return -1;
            }

            return ConfigPublishToGithub.Configure(githubConfigFile);
        }
    }
}
