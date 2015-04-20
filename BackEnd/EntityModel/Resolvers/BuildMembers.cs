using System.Threading.Tasks;

namespace DocAsCode.EntityModel
{
    public class BuildMembers : IResolverPipeline
    {
        public ParseResult Run(MetadataModel yaml, ResolverContext context)
        {
            TreeIterator.PreorderAsync<MetadataItem>(yaml.TocYamlViewModel, null,
                (s) =>
                {
                    if (s.IsInvalid || (s.Type != MemberType.Namespace && s.Type != MemberType.Toc)) return null;
                    else return s.Items;
                }, (member, parent) =>
                {
                    if (member.Type != MemberType.Toc)
                    {
                        yaml.Members.Add(member);
                    }

                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }
    }
}
