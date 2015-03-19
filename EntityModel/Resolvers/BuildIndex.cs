using System.Threading.Tasks;

namespace DocAsCode.EntityModel
{
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
                        }
                        else
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
}
