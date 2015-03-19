using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocAsCode.EntityModel
{
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
}
