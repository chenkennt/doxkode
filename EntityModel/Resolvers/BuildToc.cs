namespace DocAsCode.EntityModel
{
    public class BuildToc : IResolverPipeline
    {
        public ParseResult Run(YamlViewModel yaml, ResolverContext context)
        {
            yaml.TocYamlViewModel = yaml.TocYamlViewModel.ShrinkToSimpleTocWithNamespaceNotEmpty();

            return new ParseResult(ResultLevel.Success);
        }
    }
}
