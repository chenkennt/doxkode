using DocAsCode.PublishDoc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAsCode.ConfigPublish
{
    public class ConfigPublishToGithub
    {
        static public int Configure(string githubConfigFile)
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
            
            // Show dialog
            GithubConfigurationForm githubConfigurationForm = new GithubConfigurationForm(githubFoncifguration);
            if (githubConfigurationForm.ShowDialog() == false)
            {
                return -1;
            }

            // Write configuration
            githubFoncifguration = githubConfigurationForm.ExportGithubConfiguration();
            StreamWriter streamWriter = new StreamWriter(githubConfigFile);
            jsonSerializer.Serialize(streamWriter, githubFoncifguration);
            streamWriter.Close();

            return 0;
        }
    }
}
