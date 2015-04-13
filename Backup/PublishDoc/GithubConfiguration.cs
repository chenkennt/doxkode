using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAsCode.PublishDoc
{
    public class GithubConfiguration
    {
        public string remoteGitPath { get; set; }
        public string localGitPath { get; set; }
        public string publishUrl { get; set; }
        public string userName { get; set; }
        public string passWord { get; set; }
        public string acessUrl { get; set; }
        public bool clearLocalGit { get; set; }
        public bool openSite { get; set; }

        public GithubConfiguration()
        {
            remoteGitPath = "https://github.com/openauthor/openauthor.github.io.git";
            localGitPath = "bin/PublishDoc";
            publishUrl = "/";
            userName = "openauthor";
            passWord = "open123";
            acessUrl = "http://openauthor.github.io";
            clearLocalGit = true;
            openSite = true;
        }
    }
}
