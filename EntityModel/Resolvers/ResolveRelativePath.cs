using System.Threading.Tasks;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
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
                            current.Href = current.Name + Constants.YamlExtension;
                        }
                        else
                        {
                            current.Href = parent.Href;
                        }
                    }

                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }
    }
}
