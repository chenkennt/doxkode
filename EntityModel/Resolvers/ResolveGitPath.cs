using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
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
