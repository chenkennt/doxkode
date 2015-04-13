using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Logging;

namespace MicrosoftIT.DocProject
{
    class PublishToGithubOperation
    {
        public static void operate(Project selectedProject)
        {
            var solutionBuild = selectedProject.DTE.Solution.SolutionBuild;
            string currentConfigName = solutionBuild.ActiveConfiguration.Name;
            string publishConfigName = "PublishDoc";
            object[] k = (object[])selectedProject.ConfigurationManager.ConfigurationRowNames;
            var configCount = solutionBuild.SolutionConfigurations.Count;
            var list = new List<string>();
            for (int i = 1; i <= configCount; i++)
            {
                list.Add(solutionBuild.SolutionConfigurations.Item(i).Name);
            }

            SolutionConfiguration item;
            int index = list.IndexOf(publishConfigName);
            if (index == -1)
            {
                solutionBuild.SolutionConfigurations.Add(publishConfigName, currentConfigName, true);
                var countNew = solutionBuild.SolutionConfigurations.Count;
                item = solutionBuild.SolutionConfigurations.Item(countNew);
            }
            else
            {
                item = solutionBuild.SolutionConfigurations.Item(index + 1);
            }

            var contextCount = item.SolutionContexts.Count;
            for (int j = 1; j <= contextCount; j++)
            {
                var projectName = item.SolutionContexts.Item(j).ProjectName;
                if (projectName == selectedProject.UniqueName)
                {
                    item.SolutionContexts.Item(j).ShouldBuild = true;
                    item.SolutionContexts.Item(j).ConfigurationName = publishConfigName;
                }
                else
                {
                    item.SolutionContexts.Item(j).ShouldBuild = false;
                }
            }

            solutionBuild.BuildProject(publishConfigName, selectedProject.UniqueName);
        }
    }
}
