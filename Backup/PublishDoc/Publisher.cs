using DocAsCode.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAsCode.PublishDoc
{
    public class Publisher
    {
        
        public static void PublishToGithub(string githubConfigFile)
        {
            GithubConfiguration githubFoncifguration = new GithubConfiguration();
            JsonSerializer jsonSerializer = new JsonSerializer();

            // Read configuration if exist
            if (File.Exists(githubConfigFile))
            {
                StreamReader streamReader = new StreamReader(githubConfigFile);
                Newtonsoft.Json.JsonTextReader jsonTextReader = new JsonTextReader(streamReader);
                githubFoncifguration = jsonSerializer.Deserialize<GithubConfiguration>(jsonTextReader);
                jsonTextReader.Close();
                streamReader.Close();
            }

            string remoteGitPath = githubFoncifguration.remoteGitPath;
            string localGitPath = githubFoncifguration.localGitPath;
            string publishUrl = githubFoncifguration.publishUrl;
            string userName = githubFoncifguration.userName;
            string passWord = githubFoncifguration.passWord;
            string acessUrl = githubFoncifguration.acessUrl;
            bool clearLocalGit = githubFoncifguration.clearLocalGit;
            bool openSite = githubFoncifguration.openSite;

            if (!Directory.Exists(localGitPath))
            {
                return;
            }

            //publish
            GitRepositoryPublisher.PublishToGit(remoteGitPath, localGitPath, userName, passWord);

            //open browser
            if(openSite) System.Diagnostics.Process.Start(acessUrl);
        }
    }
}
