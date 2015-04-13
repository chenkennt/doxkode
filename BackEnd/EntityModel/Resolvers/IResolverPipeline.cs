namespace DocAsCode.EntityModel
{
    public interface IResolverPipeline
    {
        ParseResult Run(YamlViewModel yaml, ResolverContext context);
    }
}
