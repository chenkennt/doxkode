namespace DocAsCode.EntityModel
{
    using System.Threading.Tasks;

    using DocAsCode.Utility;

    public class BuildToc : IResolverPipeline
    {
        public ParseResult Run(MetadataModel yaml, ResolverContext context)
        {
            yaml.TocYamlViewModel = yaml.TocYamlViewModel.ShrinkToSimpleTocWithNamespaceNotEmpty();
            // Comparing to toc files, yaml files are all in api folder
            TreeIterator.PreorderAsync<MetadataItem>(yaml.TocYamlViewModel, null,
                (s) =>
                {
                    if (s.IsInvalid) return null;
                    else return s.Items;
                }, (current, parent) =>
                {
                    if (current.Type != MemberType.Toc)
                    {
                        if (!string.IsNullOrEmpty(current.Href))
                        {
                            current.Href = context.ApiFolder.ForwardSlashCombine(current.Href);
                        }
                    }

                    return Task.FromResult(true);
                }
                ).Wait();
            return new ParseResult(ResultLevel.Success);
        }
    }
}
