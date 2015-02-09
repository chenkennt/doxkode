﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocAsCode.Utility;
using System.IO;
using LibGit2Sharp;

namespace DocAsCode.PublishDoc
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string githubConfigFile = "GithubPublish.config";

            var options = new Option[]
                {
                    new Option("githubConfigFile", s => githubConfigFile = s, defaultValue: githubConfigFile, helpText: "Specify the configuration file of github for publish."),
                };

            if (!ConsoleParameterParser.ParseParameters(options, args))
            {
                Console.WriteLine("Error while parsing parameters!");
                return ;
            }

            Publisher.PublishToGithub(githubConfigFile);
        }
    }
}
