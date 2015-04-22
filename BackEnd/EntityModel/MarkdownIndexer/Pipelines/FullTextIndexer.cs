namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public class FullTextIndexer : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            return new ParseResult(ResultLevel.Success);
        }
    }
}
