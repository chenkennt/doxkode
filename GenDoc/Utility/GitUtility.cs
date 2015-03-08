﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
namespace Utility
{
    public class GitDetail
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "branch")]
        public string RemoteBranch { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "repo")]
        public string RemoteRepositoryUrl { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "local")]
        public string LocalWorkingDirectory { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "sha1")]
        public string LatestSha1 { get; set; }
    }
    public static class GitUtility
    {
        
        public static GitDetail GetGitDetail(string path)
        {
            GitDetail detail = null;
            if (string.IsNullOrEmpty(path)) return detail;
            try
            {
                var repoPath = Repository.Discover(path);
                if (string.IsNullOrEmpty(repoPath)) return detail;

                var repo = new Repository(repoPath);

                detail = new GitDetail();
                detail.LocalWorkingDirectory = repo.Info.WorkingDirectory;
                if (repo.Head == null) return detail;
                var remote = repo.Head.Remote;
                if (remote == null) return detail;
                detail.RemoteRepositoryUrl = remote.Url;
                detail.RemoteBranch = repo.Head.UpstreamBranchCanonicalName;
                detail.LatestSha1 = repo.Commits.FirstOrDefault().Sha;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            return detail;
        }
    }
}
