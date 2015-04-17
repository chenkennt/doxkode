using System.Threading.Tasks;

namespace DocAsCode.EntityModel
{
    public class BuildIndex : IResolverPipeline
    {
        public ParseResult Run(MetadataModel yaml, ResolverContext context)
        {
            TreeIterator.PreorderAsync<MetadataItem>(yaml.TocYamlViewModel, null,
                (s) =>
                {
                    if (s.IsInvalid) return null;
                    else return s.Items;
                }, (member, parent) =>
                {
                    if (member.Type != MemberType.Toc)
                    {
                        MetadataItem item;
                        if (yaml.Indexer.TryGetValue(member.Name, out item))
                        {
                            ParseResult.WriteToConsole(ResultLevel.Error, "{0} already exists in {1}, the duplicate one {2} will be ignored", member.Name, item.Href, member.Href);
                        }
                        else
                        {
                            yaml.Indexer.Add(member.Name, new MetadataItem { Name = member.Name, Href = member.Href });
                        }
                    }

                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }
    }
}
