﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAsCode.PublishDoc
{
    class GitRepositoryPublisher
    {
        public static bool PublishToGit(string remoteGitPath, string localGitPath, string userName, string passWord)
        {
            //TODO: This function may throw exception, should I catch them and return false?
            //init
            Repository.Init(localGitPath);
            Repository repo = new Repository(localGitPath);
            string nameKey = "user.name";
            string emailKey = "user.email";
            string name = repo.Config.GetValueOrDefault(nameKey, (string)null);

            if (name == null)
            {
                name = userName;
                repo.Config.Set(nameKey, name);
            }

            string email = repo.Config.GetValueOrDefault(emailKey, (string)null);
            
            if (email == null)
            {
                email = userName + "@" + Environment.MachineName;
                repo.Config.Set(emailKey, email);
            }

            //stage & commit
            repo.Stage("*");
            try
            {
                repo.Commit("Auto-generated by PublishDoc");
            }
            catch (Exception e)
            {
                if (e is LibGit2Sharp.EmptyCommitException)
                {
                    Console.WriteLine("Warning! Empty commit!");
                }
                else throw e;
            }
            //set remote
            Remote remote = repo.Network.Remotes["origin"];
            if (remote == null)
            {
                repo.Network.Remotes.Add("origin", remoteGitPath);
                remote = repo.Network.Remotes["origin"];
            }
            else
            {
                repo.Network.Remotes.Update(remote, r => r.Url = remoteGitPath);
            }
            //force push
            string pushRefSpec = string.Format("+{0}:{0}", repo.Head.CanonicalName);
            var pushOptions = new PushOptions()
            {
                CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials
                {
                    Username = userName,
                    Password = passWord
                }
            };
            repo.Network.Push(remote, pushRefSpec, pushOptions);
            return true;
        }
    }
}
