using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class NormalizeSyntax : IResolverPipeline
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
                    if (member.Syntax != null && member.Syntax.Content != null)
                    {
                        SyntaxLanguage[] keys = new SyntaxLanguage[member.Syntax.Content.Count];
                        member.Syntax.Content.Keys.CopyTo(keys, 0);
                        foreach(var key in keys)
                        {
                            member.Syntax.Content[key] = NormalizeLines(member.Syntax.Content[key]);
                        }
                    }

                    return Task.FromResult(true);
                }
                ).Wait();
            return new ParseResult(ResultLevel.Success);
        }
        private static string NormalizeLines(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            return string.Join(" ", content.Split(new[] { ' ', '\r', '\n'  }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
