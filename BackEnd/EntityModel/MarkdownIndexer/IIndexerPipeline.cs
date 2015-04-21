namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public interface IIndexerPipeline
    {
        ParseResult Run(MarkdownIndex yaml, IndexerContext context);
    }
}
