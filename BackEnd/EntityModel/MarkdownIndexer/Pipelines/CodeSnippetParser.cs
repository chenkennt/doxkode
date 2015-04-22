namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public class CodeSnippetParser : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            return new ParseResult(ResultLevel.Success);
        }
    }
}
