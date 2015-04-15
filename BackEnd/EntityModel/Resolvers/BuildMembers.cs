using System.Threading.Tasks;

namespace DocAsCode.EntityModel
{
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
}
