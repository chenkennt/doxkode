using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocAsCode.EntityModel
{
    public class ResolveLink : IResolverPipeline
    {
        public ParseResult Run(MetadataModel yaml, ResolverContext context)
        {
            var index = yaml.Indexer;

            TreeIterator.PreorderAsync<MetadataItem>(yaml.TocYamlViewModel, null,
                (s) =>
                {
                    if (s.IsInvalid) return null;
                    else return s.Items;
                }, (member, parent) =>
                {
                    // get all the possible places where link is possible
                    member.Remarks = ResolveText(index, member.Remarks, member.Name);
                    member.Summary = ResolveText(index, member.Summary, member.Name);
                    if (member.Syntax != null && member.Syntax.Parameters != null)
                        member.Syntax.Parameters.ForEach(s =>
                        {
                            s.Description = ResolveText(index, s.Description, member.Name);
                        });
                    if (member.Syntax != null && member.Syntax.Return != null)
                        member.Syntax.Return.Description = ResolveText(index, member.Syntax.Return.Description, member.Name);

                    // resolve parameter's Type


                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }

        private static string ResolveText(Dictionary<string, MetadataItem> dict, string input, string currentMemberName)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return LinkParser.ResolveToMarkdownLink(dict, input, currentMemberName);
        }
    }
}
