﻿namespace DocAsCode.Utility
{
    using System;

    using LibGit2Sharp;

    using YamlDotNet.Serialization;

    public class GitDetail
    {
        [YamlMember(Alias = "branch")]
        public string RemoteBranch { get; set; }

        [YamlMember(Alias = "repo")]
        public string RemoteRepositoryUrl { get; set; }

        [YamlIgnore]
        //[YamlDotNet.Serialization.YamlMember(Alias = "local")]
        public string LocalWorkingDirectory { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (this.GetType() != obj.GetType()) return false;

            return Equals(this.ToString(), obj.ToString());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("branch: {0}, url: {1}, local: {2}, desc: {3}", RemoteBranch, RemoteRepositoryUrl, LocalWorkingDirectory, Description);
        }
    }
    public static class GitUtility
    {
        /// <summary>
        /// TODO: only get GitDetail on Project level?
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

                // Convert to forward slash
                detail.LocalWorkingDirectory = repo.Info.WorkingDirectory.BackSlashToForwardSlash();
                if (repo.Head == null) return detail;
                var remote = repo.Head.Remote;
                if (remote == null) return detail;
                detail.RemoteRepositoryUrl = remote.Url;
                detail.RemoteBranch = repo.Head.UpstreamBranchCanonicalName;
                var hrefHeadIndex = detail.RemoteBranch.IndexOf("refs/heads/");
                if (hrefHeadIndex > -1)
                {
                    detail.RemoteBranch = detail.RemoteBranch.Substring(hrefHeadIndex + 11);
                }

                detail.Description = repo.Describe(repo.Lookup<Commit>(repo.Head.Tip.Sha));
            }
            catch (Exception e)
            {
                // SWALLOW exception?
                // Console.Error.WriteLine(e.Message);
            }

            return detail;
        }
    }
}
