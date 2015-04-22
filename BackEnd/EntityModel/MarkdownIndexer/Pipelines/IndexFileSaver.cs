namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public class IndexFileSaver : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            return new ParseResult(ResultLevel.Success);
        }
    }
}
