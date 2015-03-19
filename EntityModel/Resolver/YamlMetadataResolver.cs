using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocAsCode.Utility;
namespace DocAsCode.EntityModel
{
    public static class YamlMetadataResolver
    {
        // Order matters
        static List<IResolverPipeline> pipelines = new List<IResolverPipeline>()
        {
            new LayoutCheckAndCleanup(),
            new ResolveRelativePath(),
            new BuildIndex(),
            new ResolveLink(),
            new ResolveGitPath(),
            new ResolvePath(),
            new BuildMembers(),
            new BuildToc(),
        };

        // Folder structure
        // toc.yaml # toc containing all the namespaces
        // api.yaml # id-yaml mapping
        // api/{id}.yaml # items
        public const string YamlExtension = ".yaml";

        /// <summary>
        /// TODO: input Namespace list instead
        /// </summary>
        /// <param name="allMembers"></param>
        /// <returns></returns>
        public static YamlViewModel ResolveMetadata(Dictionary<string, YamlItemViewModel> allMembers, string apiFolder)
        {
            YamlViewModel viewModel = new YamlViewModel();
            viewModel.IndexYamlViewModel = new Dictionary<string, IndexYamlItemViewModel>();
            viewModel.TocYamlViewModel = new YamlItemViewModel()
            {
                Type = MemberType.Toc,
                Items = allMembers.Where(s => s.Value.Type == MemberType.Namespace).Select(s => s.Value).ToList(),
            };
            viewModel.MemberYamlViewModelList = new List<YamlItemViewModel>();
            ResolverContext context = new ResolverContext { ApiFolder = apiFolder };
            var result = ExecutePipeline(viewModel, context);

            result.WriteToConsole();
            return viewModel;
        }

        public static ParseResult ExecutePipeline(YamlViewModel yaml, ResolverContext context)
        {
            ParseResult result = new ParseResult(ResultLevel.Success);
            foreach(var pipeline in pipelines)
            {
                result = pipeline.Run(yaml, context);
                if (result.ResultLevel == ResultLevel.Error)
                {
                    return result;
                }

                if (!string.IsNullOrEmpty(result.Message))
                {
                    result.WriteToConsole();
                }
            }

            return result;
        }

        public interface IResolverPipeline
        {
            ParseResult Run(YamlViewModel yaml, ResolverContext context);
        }

        public class BuildToc : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                yaml.TocYamlViewModel = yaml.TocYamlViewModel.ShrinkToSimpleToc();

                return new ParseResult(ResultLevel.Success);
            }
        }

        public class BuildMembers : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                    (s) =>
                    {
                        if (s.IsInvalid || (s.Type != MemberType.Namespace && s.Type != MemberType.Toc)) return null;
                        else return s.Items;
                    }, (member, parent) =>
                    {
                        if (member.Type != MemberType.Toc)
                        {
                            yaml.MemberYamlViewModelList.Add(member.ShrinkChildren());
                        }

                        return Task.FromResult(true);
                    }
                    ).Wait();

                return new ParseResult(ResultLevel.Success);
            }
        }

        public class BuildIndex : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                    (s) =>
                    {
                        if (s.IsInvalid) return null;
                        else return s.Items;
                    }, (member, parent) =>
                    {
                        if (member.Type != MemberType.Toc)
                        {
                            IndexYamlItemViewModel item;
                            if (yaml.IndexYamlViewModel.TryGetValue(member.Name, out item))
                            {
                                ParseResult.WriteToConsole(ResultLevel.Error, "{0} already exists in {1}, the duplicate one {2} will be ignored", member.Name, item.YamlPath, member.YamlPath);
                            }else
                            {
                                yaml.IndexYamlViewModel.Add(member.Name, new IndexYamlItemViewModel { Name = member.Name, YamlPath = member.YamlPath, Href = member.Href });
                            }
                        }

                        return Task.FromResult(true);
                    }
                    ).Wait();

                return new ParseResult(ResultLevel.Success);
            }
        }

        public class LayoutCheckAndCleanup : IResolverPipeline
        {
            /// <summary>
            /// The yaml layout should be 
            /// namespace -- class level -- method level
            /// </summary>
            /// <param name="allMembers"></param>
            /// <returns></returns>
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                ParseResult overall = new ParseResult(ResultLevel.Success);
                StringBuilder message = new StringBuilder();
                foreach (var member in yaml.TocYamlViewModel.Items)
                {
                    CheckNamespaces(member);
                }

                if (message.Length > 0)
                {
                    overall.ResultLevel = ResultLevel.Warn;
                    overall.Message = message.ToString();
                }

                return overall;
            }

            private ParseResult CheckNamespaces(YamlItemViewModel member)
            {
                ParseResult overall = new ParseResult(ResultLevel.Success);
                StringBuilder message = new StringBuilder();

                // Skip if it is already invalid
                if (member.Items == null || member.IsInvalid)
                {
                    return overall;
                }

                foreach (var i in member.Items)
                {
                    Debug.Assert(i.Type.IsPageLevel());
                    if (!i.Type.IsPageLevel())
                    {
                        ParseResult.WriteToConsole(ResultLevel.Error, "Invalid item inside yaml metadata: {0} is not allowed inside {1}. Will be ignored.", i.Type.ToString(), member.Type.ToString());
                        message.AppendFormat("{0} is not allowed inside {1}.", i.Type.ToString(), member.Type.ToString());
                        i.IsInvalid = true;
                    }
                    else
                    {
                        ParseResult result = CheckNamespaceMembers(i);
                        if (!string.IsNullOrEmpty(result.Message))
                        {
                            message.AppendLine(result.Message);
                        }
                    }
                }

                if (message.Length > 0)
                {
                    overall.ResultLevel = ResultLevel.Warn;
                    overall.Message = message.ToString();
                }

                return overall;
            }

            /// <summary>
            /// e.g. Classes
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            private ParseResult CheckNamespaceMembers(YamlItemViewModel member)
            {
                ParseResult overall = new ParseResult(ResultLevel.Success);
                StringBuilder message = new StringBuilder();

                // Skip if it is already invalid
                if (member.Items == null || member.IsInvalid)
                {
                    return overall;
                }


                foreach (var i in member.Items)
                {
                    Debug.Assert(!i.Type.IsPageLevel());
                    if (i.Type.IsPageLevel())
                    {
                        ParseResult.WriteToConsole(ResultLevel.Error, "Invalid item inside yaml metadata: {0} is not allowed inside {1}. Will be ignored.", i.Type.ToString(), member.Type.ToString());
                        message.AppendFormat("{0} is not allowed inside {1}.", i.Type.ToString(), member.Type.ToString());
                        i.IsInvalid = true;
                    }
                    else
                    {
                        ParseResult result = CheckNamespaceMembersMembers(i);
                        if (!string.IsNullOrEmpty(result.Message))
                        {
                            message.AppendLine(result.Message);
                        }
                    }
                }

                if (message.Length > 0)
                {
                    overall.ResultLevel = ResultLevel.Warn;
                    overall.Message = message.ToString();
                }

                return overall;
            }


            /// <summary>
            /// e.g. Methods
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            private ParseResult CheckNamespaceMembersMembers(YamlItemViewModel member)
            {
                ParseResult overall = new ParseResult(ResultLevel.Success);
                StringBuilder message = new StringBuilder();
                if (member.IsInvalid)
                {
                    return overall;
                }

                // does method has members?
                Debug.Assert(member.Items == null);
                if (member.Items != null)
                {
                    foreach (var i in member.Items)
                    {
                        i.IsInvalid = true;
                    }

                    ParseResult.WriteToConsole(ResultLevel.Error, "Invalid item inside yaml metadata: {0} should not contain items. Will be ignored.", member.Type.ToString());
                    message.AppendFormat("{0} should not contain items.", member.Type.ToString());
                }

                if (message.Length > 0)
                {
                    overall.ResultLevel = ResultLevel.Warn;
                    overall.Message = message.ToString();
                }

                return overall;
            }
        }

        public class ResolveRelativePath : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                    (s) =>
                    {
                        if (s.IsInvalid) return null;
                        else return s.Items;
                    }, (current, parent) =>
                    {
                        if (current.Type != MemberType.Toc)
                        {
                            if (current.Type.IsPageLevel())
                            {
                                current.YamlPath = context.ApiFolder.ForwardSlashCombine(current.Name + YamlExtension);
                                current.Href = context.ApiFolder.ForwardSlashCombine(current.Name);
                            }
                            else
                            {
                                current.YamlPath = parent.YamlPath;
                                current.Href = parent.Href;
                            }
                        }

                        return Task.FromResult(true);
                    }
                    ).Wait();

                return new ParseResult(ResultLevel.Success);
            }
        }

        public class ResolvePath : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                    (s) =>
                    {
                        if (s.IsInvalid) return null;
                        else return s.Items;
                    }, (current, parent) =>
                    {
                        if (current.Inheritance != null && current.Inheritance.Count > 0)
                        {
                            current.Inheritance.ForEach(s =>
                            {
                                if (!s.IsExternalPath)
                                {
                                    s.Href = ResolveInternalLink(yaml.IndexYamlViewModel, s.Name);
                                }
                            });
                        }
                        
                        if (current.Syntax != null && current.Syntax.Parameters != null)
                        {
                            current.Syntax.Parameters.ForEach(s =>
                            {
                                Debug.Assert(s.Type != null);
                                if (s.Type != null && !s.Type.IsExternalPath)
                                {
                                    s.Type.Href = ResolveInternalLink(yaml.IndexYamlViewModel, s.Type.Name);
                                }
                            });
                        }

                        if (current.Syntax != null && current.Syntax.Return != null && current.Syntax.Return.Type != null)
                        {
                            current.Syntax.Return.Type.Href = ResolveInternalLink(yaml.IndexYamlViewModel, current.Syntax.Return.Type.Name);
                        }

                        return Task.FromResult(true);
                    }
                    ).Wait();

                return new ParseResult(ResultLevel.Success);
            }

            private static string ResolveInternalLink(Dictionary<string, IndexYamlItemViewModel> index, string name)
            {
                Debug.Assert(!string.IsNullOrEmpty(name));
                if (string.IsNullOrEmpty(name)) return name;

                IndexYamlItemViewModel item;
                if (index.TryGetValue(name, out item))
                {
                    return item.Href;
                }

                return name;
            }
        }

        public class ResolveLink : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                var index = yaml.IndexYamlViewModel;

                TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                    (s) =>
                    {
                        if (s.IsInvalid) return null;
                        else return s.Items;
                    }, (member, parent) =>
                    {
                        // get all the possible places where link is possible
                        member.Remarks = ResolveText(index, member.Remarks);
                        member.Summary = ResolveText(index, member.Summary);
                        if (member.Syntax != null && member.Syntax.Parameters != null)
                            member.Syntax.Parameters.ForEach(s =>
                            {
                                s.Description = ResolveText(index, s.Description);
                            });
                        if (member.Syntax != null && member.Syntax.Return != null)
                            member.Syntax.Return.Description = ResolveText(index, member.Syntax.Return.Description);

                        // resolve parameter's Type
                        

                        return Task.FromResult(true);
                    }
                    ).Wait();

                return new ParseResult(ResultLevel.Success);
            }

            private static string ResolveText(Dictionary<string, IndexYamlItemViewModel> dict, string input)
            {
                if (string.IsNullOrEmpty(input)) return null;
                return LinkParser.ResolveToMarkdownLink(dict, input);
            }
        }

        public class ResolveGitPath : IResolverPipeline
        {
            public ParseResult Run(YamlViewModel yaml, ResolverContext context)
            {
                TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                    (s) =>
                    {
                        if (s.IsInvalid) return null;
                        else return s.Items;
                    }, (member, parent) =>
                    {
                        Debug.Assert(member.Type == MemberType.Toc || member.Source != null);
                        if (member.Source != null)
                        {
                            // Debug.Assert(member.Source.Remote != null);
                            if (member.Source.Remote != null)
                            {
                                member.Source.Path = member.Source.Path.FormatPath(UriKind.Relative, member.Source.Remote.LocalWorkingDirectory);
                            }
                        }

                        return Task.FromResult(true);
                    }
                    ).Wait();

                return new ParseResult(ResultLevel.Success);
            }
        }
    }
}
